using FYP.Data;
using FYP.Data.Repository.IRepository;
using FYP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP.Areas.Supplier.Controllers
{
    [Area("Supplier")]
    [Authorize(Roles = "Supplier")]
    public class CategoryRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryRequestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<CategoryRequest> objList = _unitOfWork.CategoryRequest.GetAll().ToList();
            return View(objList);
        }

        // GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryRequest obj)
        {
            if (ModelState.IsValid)
            {
                CategoryRequest newObj = new CategoryRequest()
                {
                    Name = obj.Name,
                    IsApproved = null,
                    CreateAt = DateTime.Now
                };
                _unitOfWork.CategoryRequest.Add(newObj);
                _unitOfWork.Save();
                TempData["success"] = "Request created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.CategoryRequest.Get(u => u.Id == id);
            if (obj != null)
            {
                _unitOfWork.CategoryRequest.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Request deleted successfully";
                return RedirectToAction("Index");

            }
            TempData["error"] = "Request not found";
            return RedirectToAction("Index");

        }
    }
}
