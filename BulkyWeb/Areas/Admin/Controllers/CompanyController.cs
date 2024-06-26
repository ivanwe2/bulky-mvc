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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
        {
            var CompanyList = _unitOfWork.Company.GetAllBy().ToList();
            return View(CompanyList);
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

            var company = id is null ? new Company() : _unitOfWork.Company.GetBy(p => p.Id == id);

			return View(company);
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {

            if (!ModelState.IsValid)
            {
				return View(company);
            }

            if (company.Id == 0)
            {
                _unitOfWork.Company.Add(company);
                SuccessNotification("Company created");
            }
            else
            {
                _unitOfWork.Company.Update(company);
                SuccessNotification("Company updated");
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
            List<Company> Companys = _unitOfWork.Company.GetAllBy().ToList();

            return Json(new {data =  Companys});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyToDelete = _unitOfWork.Company.GetBy(p=>p.Id == id);
            if (companyToDelete is null)
            {
                return Json(new { success = false, message = "Error while deleting!" });
            }

            _unitOfWork.Company.Delete(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull!" });
        }
        #endregion
    }
}
