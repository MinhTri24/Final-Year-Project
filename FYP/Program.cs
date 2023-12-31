using FYP.Data;
using FYP.Data.Repository.IRepository;
using FYP.Data.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FYP.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); ;
builder.Services.AddControllersWithViews();
builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "334535615994875";
    options.AppSecret = "d029e3ba958734183ca85d8da8ef73e9";
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "525899625528-m5ne0oq1onvl2i28qfq2hjiavhoscfi6.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

var cloudName = builder.Configuration.GetSection("Cloudinary:CloudName").Get<string>();
var apiKey = builder.Configuration.GetSection("Cloudinary:ApiKey").Get<string>();
var apiSecret = builder.Configuration.GetSection("Cloudinary:ApiSecret").Get<string>();

builder.Services.Configure<CloudinarySetting>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton<Cloudinary>(x => new Cloudinary(new CloudinaryDotNet.Account(cloudName, apiKey, apiSecret)));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseAuthentication();
app.UseRouting();
app.MapRazorPages();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
