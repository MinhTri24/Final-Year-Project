using FYP.Data;
using FYP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP.Controllers
{
    public class CategoryRequestController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryRequestController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<CategoryRequest> objList = _db.CategoryRequests.ToList();
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
                _db.CategoryRequests.Add(newObj);
                _db.SaveChanges();
                TempData["success"] = "Request created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var obj = _db.CategoryRequests.Find(id);
            if (obj == null)
            {
                TempData["error"] = "Request not found";
                return RedirectToAction("Index");
            }
            var category = _db.Categories.FirstOrDefault(c => c.Name == obj.Name);
            var lastCategory = _db.Categories.OrderByDescending(c => c.Id).FirstOrDefault();
            int lastCategoryId = lastCategory != null ? lastCategory.Id : 0;
            if (category == null)
            {
                Category newCategory = new Category()
                {
                    Name = obj.Name,
                };
                obj.IsApproved = true;
                _db.Categories.Add(newCategory);
                _db.SaveChanges();
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
            var obj = _db.CategoryRequests.Find(id);
            if (obj != null)
            {
                obj.IsApproved = false;
                _db.SaveChanges();
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
            var obj = _db.CategoryRequests.Find(id);
            if (obj != null)
            {
                _db.CategoryRequests.Remove(obj);
                _db.SaveChanges();
                TempData["success"] = "Request deleted successfully";
                return RedirectToAction("Index");
                
            }
            TempData["error"] = "Request not found";
            return RedirectToAction("Index");

        }
    }
}
