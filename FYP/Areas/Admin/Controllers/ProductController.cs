using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FYP.Data.Repository;
using FYP.Data.Repository.IRepository;
using FYP.Models;
using FYP.Models.ViewModels;
using FYP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FYP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticVariables.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Cloudinary _cloudinary;
        public ProductController(IUnitOfWork unitOfWork, Cloudinary cloudinary)
        {
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category", includeSecondProperties: "ApplicationUser").ToList();

            return View(products);
        }

        public IActionResult Create()
        {
               ProductVM productVM = new()
               {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create([FromForm] ProductVM productVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (userId != null)
            {
                var file = productVM.Image;
                var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };

                var uploadResult = _cloudinary.Upload(uploadParams);

                productVM.Product.ImageUrl = uploadResult.SecureUri.AbsoluteUri;

                if (productVM.Product.ImageUrl == null)
                {
                    productVM.Product.ImageUrl = "https://www.thermaxglobal.com/wp-content/uploads/2020/05/image-not-found.jpg";
                }

                if (productVM.Product.Stock > 0)
                {
                    productVM.Product.IsAvailable = true;
                }
                else
                {
                    productVM.Product.IsAvailable = false;
                }

                productVM.Product.ApplicationUserId = userId;
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                TempData["error"] = "Product created unsuccessfully";
                return View(productVM);
            }
        }

        public IActionResult Edit(int? id)
        {             
            if (id == null || id == 0)
            {
                return NotFound();
            }

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);

            if (productVM == null)
            {
                return NotFound();
            }
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Edit([FromForm] ProductVM productVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (userId != null)
            {
                var file = productVM.Image;
                var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };

                var uploadResult = _cloudinary.Upload(uploadParams);

                productVM.Product.ImageUrl = uploadResult.SecureUri.AbsoluteUri;

                if (productVM.Product.ImageUrl == null)
                {
                    productVM.Product.ImageUrl = "https://www.thermaxglobal.com/wp-content/uploads/2020/05/image-not-found.jpg";
                }

                if (productVM.Product.Stock > 0)
                {
                    productVM.Product.IsAvailable = true;
                }
                else
                {
                    productVM.Product.IsAvailable = false;
                }

                productVM.Product.ApplicationUserId = userId;
                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                TempData["error"] = "Product updated unsuccessfully";
                return View(productVM);
            }
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category", includeSecondProperties: "ApplicationUser").ToList();
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var imageUrl = productToBeDeleted.ImageUrl;
            if (imageUrl != null)
            {
                var publicId = imageUrl.Substring(imageUrl.LastIndexOf('/') + 1, imageUrl.LastIndexOf('.') - imageUrl.LastIndexOf('/') - 1);
                var deleteParams = new DeletionParams(publicId);
                var result = _cloudinary.Destroy(deleteParams);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }      

        #endregion
    }
}