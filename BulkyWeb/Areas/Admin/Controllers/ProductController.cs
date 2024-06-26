using Bulky.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.IdentityUtils;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
		public IActionResult Index()
        {
            var ProductList = _unitOfWork.Product.GetAllBy(null, "Category").ToList();
            return View(ProductList);
        }

        public IActionResult Upsert(int? id)//Update + insert
        {
			IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
				.GetAllBy().Select(c => new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				});

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;

            var product = id is null ? new Product() : _unitOfWork.Product.GetBy(p => p.Id == id,"ProductImages");

            var productVM = new ProductVM() { Product = product, CategoryList = CategoryList };

			return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files)
        {

            if (!ModelState.IsValid)
            {
                productVM.CategoryList = _unitOfWork.Category
				.GetAllBy().Select(c => new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				});

				return View(productVM);
            }


            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
                SuccessNotification("Product created");
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
                SuccessNotification("Product updated");
            }
            _unitOfWork.Save();

            if (files is not null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                foreach (IFormFile file in files)
                {

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images\products\product-" + productVM.Product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    var productImage = new ProductImage()
                    {
                        ImageUrl = @"\" + productPath + @"\" + fileName,
                        ProductId = productVM.Product.Id,
                    };

                    if (productVM.Product.ProductImages is null)
                    {
                        productVM.Product.ProductImages = new List<ProductImage>();
                    }

                    productVM.Product.ProductImages.Add(productImage);
                }

                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }
		public IActionResult DeleteImage(int imageId)
		{
			var imageToBeDeleted = _unitOfWork.ProductImage.GetBy(u => u.Id == imageId);
			int productId = imageToBeDeleted.ProductId;
			if (imageToBeDeleted != null)
			{
				if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
				{
					var oldImagePath =
								   Path.Combine(_webHostEnvironment.WebRootPath,
								   imageToBeDeleted.ImageUrl.TrimStart('\\'));

					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}

				_unitOfWork.ProductImage.Delete(imageToBeDeleted);
				_unitOfWork.Save();

				TempData["success"] = "Image deleted";
			}

			return RedirectToAction(nameof(Upsert), new { id = productId });
		}

		private void SuccessNotification(string message)
        {
            TempData["success"] = message + " successfully!";
        }

		private void ErrorNotification(string message)
		{
			TempData["error"] = message;
		}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAllBy(null, "Category").ToList();

            return Json(new {data =  products});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
			var productToBeDeleted = _unitOfWork.Product.GetBy(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			string productPath = @"images\products\product-" + id;
			string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

			if (Directory.Exists(finalPath))
			{
				string[] filePaths = Directory.GetFiles(finalPath);
				foreach (string filePath in filePaths)
				{
					System.IO.File.Delete(filePath);
				}

				Directory.Delete(finalPath);
			}


			_unitOfWork.Product.Delete(productToBeDeleted);
			_unitOfWork.Save();

			return Json(new { success = true, message = "Delete Successful" });
		}
        #endregion
    }
}
