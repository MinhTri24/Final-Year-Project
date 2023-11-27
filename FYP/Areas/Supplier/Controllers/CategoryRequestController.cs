using FYP.Data;
using FYP.Data.Repository.IRepository;
using FYP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP.Areas.Supplier.Controllers
{
    [Area("Supplier")]
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
        public IActionResult Approve(int id)
        {
            var obj = _unitOfWork.CategoryRequest.Get(u => u.Id == id);
            if (obj == null)
            {
                TempData["error"] = "Request not found";
                return RedirectToAction("Index");
            }
            var category = _unitOfWork.Category.Get(c => c.Name == obj.Name);
            var lastCategory = _unitOfWork.Category.OrderByDescending().FirstOrDefault();
            int lastCategoryId = lastCategory != null ? lastCategory.Id : 0;
            if (category == null)
            {
                Category newCategory = new Category()
                {
                    Name = obj.Name,
                };
                obj.IsApproved = true;
                _unitOfWork.Category.Add(newCategory);
                _unitOfWork.Save();
                TempData["success"] = "Request approved successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.IsApproved = false;
                TempData["error"] = "Category already exists";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var obj = _unitOfWork.CategoryRequest.Get(u => u.Id == id);
            if (obj != null)
            {
                obj.IsApproved = false;
                _unitOfWork.Save();
                TempData["success"] = "Request rejected successfully";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Request not found";
                return RedirectToAction("Index");
            }
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
