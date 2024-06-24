using Bulky.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;
using Bulky.Utility;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var ProductList = _unitOfWork.Product.GetAllBy();
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

                using (var fileStream = new FileStream(Path.Combine(productPath,fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = Constants.ProductImagesPath + fileName;
            }

            _unitOfWork.Product.Add(productVM.Product);
            _unitOfWork.Save();

            SuccessNotification("Product created");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (id is null)
                return NotFound();

            var product = _unitOfWork.Product.GetBy(c => c.Id == id);
            if (product is null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var product = _unitOfWork.Product.GetBy(c => c.Id == id);
            if (product is null)
                return NotFound();

            _unitOfWork.Product.Delete(product);
            _unitOfWork.Save();

            SuccessNotification("Product deleted");

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
	}
}
