using Bulky.DataAccess.Data;
using Bulky.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _repository;
        public CategoryController(ICategoryRepository repository)
        {
            _repository = repository;
        }
        public IActionResult Index()
        {
            var categoryList = _repository.GetAllBy();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _repository.Add(category);
            _repository.Save();

            TempData["success"] = "Category created successfully!";

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (id is null)
                return NotFound();

            var category = _repository.GetBy(c => c.Id == id);
            if (category is null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if(ModelState.IsValid)
            {
                _repository.Update(category);
                _repository.Save();

				TempData["success"] = "Category edited successfully!";

				return RedirectToAction("Index");
            }
            return View();
        }

		public IActionResult Delete(int? id)
		{
			if (id is null)
				return NotFound();

			var category = _repository.GetBy(c => c.Id == id);
			if (category is null)
				return NotFound();

			return View(category);
		}

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var category = _repository.GetBy(c => c.Id == id);
            if (category is null)
                return NotFound();

            _repository.Remove(category);
            _repository.Save();

			TempData["success"] = "Category deleted successfully!";

			return RedirectToAction("Index");
        }
					
	}
}
