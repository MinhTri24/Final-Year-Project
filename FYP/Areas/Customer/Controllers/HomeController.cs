using FYP.Data.Repository.IRepository;
using FYP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace FYP.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category", includeSecondProperties: "ApplicationUser");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            Cart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category", includeSecondProperties: "ApplicationUser"),
                Quantity = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;

            Cart cartFromDb = _unitOfWork.Cart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == cart.ProductId);

            Product productFromDb = _unitOfWork.Product.Get(u => u.Id == cart.ProductId);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Quantity += cart.Quantity;
                _unitOfWork.Cart.Update(cartFromDb);
            }
            else
            {
                //add cart record
                _unitOfWork.Cart.Add(cart);
            }

            if (cartFromDb == null && productFromDb.IsAvailable == true)
            {
                if (productFromDb.Stock > 0)
                {
                    if (productFromDb.Stock >= cart.Quantity)
                    {
                        productFromDb.Stock -= cart.Quantity;
                        if (productFromDb.Stock == 0)
                        {
                            productFromDb.IsAvailable = false;
                        }
                        _unitOfWork.Product.Update(productFromDb);
                    }
                    else
                    {
                        TempData["error"] = "Product is not available";
                        return RedirectToAction(nameof(Index));
                    }
                }
                _unitOfWork.Cart.Add(cart);
            }
            else if (cartFromDb != null && productFromDb.IsAvailable == true)
            {
                if (productFromDb.Stock > 0)
                {
                    if (productFromDb.Stock >= cart.Quantity)
                    {
                        productFromDb.Stock -= cart.Quantity;
                        if (productFromDb.Stock == 0)
                        {
                            productFromDb.IsAvailable = false;
                        }
                        _unitOfWork.Product.Update(productFromDb);
                    }
                    else
                    {
                        TempData["error"] = "Product is not available";
                        return RedirectToAction(nameof(Index));
                    }
                }
                _unitOfWork.Cart.Update(cartFromDb);
            }
            else
            {
                TempData["error"] = "Product is not available";
                return RedirectToAction(nameof(Index));
            }

            TempData["success"] = "Cart updated successfully";
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}