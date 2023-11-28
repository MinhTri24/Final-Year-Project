using FYP.Data.Repository.IRepository;
using FYP.Models;
using FYP.Models.ViewModels;
using FYP.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace FYP.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public CartVM CartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartVM = new()
            {
                CartList = _unitOfWork.Cart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                Order = new()
            };

            foreach (var cart in CartVM.CartList)
            {
                cart.Price = cart.Product.Price;
                CartVM.Order.OrderTotal += (cart.Price * cart.Quantity);
            }

            return View(CartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            CartVM = new()
            {
                CartList = _unitOfWork.Cart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                Order = new()
            };

            CartVM.Order.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            CartVM.Order.Name = CartVM.Order.ApplicationUser.Name;
            CartVM.Order.PhoneNumber = CartVM.Order.ApplicationUser.PhoneNumber;
            CartVM.Order.StreetAddress = CartVM.Order.ApplicationUser.StreetAddress;
            CartVM.Order.City = CartVM.Order.ApplicationUser.City;



            foreach (var cart in CartVM.CartList)
            {
                cart.Price = cart.Product.Price;
                CartVM.Order.OrderTotal += (cart.Price * cart.Quantity);
            }
            return View(CartVM);
        }

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			CartVM.CartList = _unitOfWork.Cart.GetAll(u => u.ApplicationUserId == userId,
				includeProperties: "Product");

			CartVM.Order.OrderDate = System.DateTime.Now;
			CartVM.Order.ApplicationUserId = userId;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var cart in CartVM.CartList)
			{
				cart.Price = cart.Product.Price;
				CartVM.Order.OrderTotal += (cart.Price * cart.Quantity);
			}



			CartVM.Order.PaymentStatus = StaticVariables.PaymentStatusPending;
			CartVM.Order.OrderStatus = StaticVariables.StatusPending;


			_unitOfWork.Order.Add(CartVM.Order);
			_unitOfWork.Save();
			foreach (var cart in CartVM.CartList)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = CartVM.Order.Id,
					Price = cart.Price,
					Quantity = cart.Quantity
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}

            if (!ModelState.IsValid)
            {
				var domain = "https://localhost:7228/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={CartVM.Order.Id}",
					CancelUrl = domain + "customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in CartVM.CartList)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price), // $20.50 => 2050
							Currency = "vnd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}
						},
						Quantity = item.Quantity
					};
					options.LineItems.Add(sessionLineItem);
				}


				var service = new SessionService();
				Session session = service.Create(options);
				_unitOfWork.Order.UpdateStripePaymentID(CartVM.Order.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = CartVM.Order.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			return View(id);
		}

		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.Cart.Get(u => u.Id == cartId);
            cartFromDb.Quantity += 1;
            _unitOfWork.Cart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.Cart.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb.Quantity <= 1)
            {
                //remove that from cart
                _unitOfWork.Cart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Quantity -= 1;
                _unitOfWork.Cart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.Cart.Get(u => u.Id == cartId, tracked: true);
            _unitOfWork.Cart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
