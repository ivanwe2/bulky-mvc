using Bulky.DataAccess.Repositories.Abstractions;
using Bulky.Models.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> products = _unitOfWork.OrderHeader.GetAllBy(null, "ApplicationUser").ToList();

            return Json(new { data = products });
        }
        
        #endregion
    }
}
