using FYP.Data.Repository.IRepository;
using FYP.Models;
using FYP.Models.ViewModels;
using FYP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace FYP.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                Order = _unitOfWork.Order.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = StaticVariables.Role_Admin)]
        public IActionResult UpdateOrderDetail()
        {
            var OrderFromDb = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id);
            OrderFromDb.Name = OrderVM.Order.Name;
            OrderFromDb.PhoneNumber = OrderVM.Order.PhoneNumber;
            OrderFromDb.StreetAddress = OrderVM.Order.StreetAddress;
            OrderFromDb.City = OrderVM.Order.City;
            _unitOfWork.Order.Update(OrderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = OrderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticVariables.Role_Admin)]
        public IActionResult ShipOrder()
        {
            var Order = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id);
            Order.OrderStatus = StaticVariables.StatusShipped;
            Order.ShippingDate = DateTime.Now;
            _unitOfWork.Order.Update(Order);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticVariables.Role_Admin)]
        public IActionResult CancelOrder()
        {

            var Order = _unitOfWork.Order.Get(u => u.Id == OrderVM.Order.Id);

            if (Order.PaymentStatus == StaticVariables.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = Order.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.Order.UpdateStatus(Order.Id, StaticVariables.StatusCancelled, StaticVariables.StatusRefunded);
            }
            else
            {
                _unitOfWork.Order.UpdateStatus(Order.Id, StaticVariables.StatusCancelled, StaticVariables.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.Order.Id });

        }

        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<Order> objOrders;


            if (User.IsInRole(StaticVariables.Role_Admin))
            {
                objOrders = _unitOfWork.Order.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrders = _unitOfWork.Order
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrders = objOrders.Where(u => u.PaymentStatus == StaticVariables.StatusPending).ToList();
                    break;
                case "completed":
                    objOrders = objOrders.Where(u => u.OrderStatus == StaticVariables.StatusShipped).ToList();
                    break;
                case "approved":
                    objOrders = objOrders.Where(u => u.OrderStatus == StaticVariables.StatusApproved).ToList();
                    break;
                default:
                    break;

            }

            return Json(new { data = objOrders });
		}


		#endregion
	}
}
