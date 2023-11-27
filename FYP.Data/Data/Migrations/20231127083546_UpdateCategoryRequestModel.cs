using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "CategoryRequests");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CategoryRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CategoryRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "CategoryRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
