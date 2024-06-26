using Bulky.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.IdentityUtils;
using Microsoft.AspNetCore.Authorization;

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

            var product = id is null ? new Product() : _unitOfWork.Product.GetBy(p => p.Id == id);

            var productVM = new ProductVM() { Product = product, CategoryList = CategoryList };

			return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
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

            if (file is not null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    string oldImagePath =
                        Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
						System.IO.File.Delete(oldImagePath);
                    }

                }
                using (var fileStream = new FileStream(Path.Combine(productPath,fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = Constants.ProductImagesPath + fileName;
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

            return RedirectToAction("Index");
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
            var productToDelete = _unitOfWork.Product.GetBy(p=>p.Id == id);
            if (productToDelete is null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }

            var oldImagePath = 
                Path.Combine(_webHostEnvironment.WebRootPath,
                    productToDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Delete(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull!" });
        }
        #endregion
    }
}
