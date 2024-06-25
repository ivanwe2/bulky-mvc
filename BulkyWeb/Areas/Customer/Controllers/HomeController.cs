using Bulky.DataAccess.Repositories.Abstractions;
using Bulky.Models.Models;
using Bulky.Utility.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim is not null)
            {
                HttpContext.Session.SetInt32(SessionConstants.SessionCart,
                    _unitOfWork.ShoppingCart.GetAllBy(s => s.ApplicationUserId == claim.Value).Count());
            }

            var products = _unitOfWork.Product.GetAllBy(null, "Category").ToList();
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            var product = _unitOfWork.Product.GetBy(p=>p.Id == productId, "Category");
            var shoppingCart = new ShoppingCart()
            {
                Product = product,
                Count = 1,
                ProductId = productId
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            shoppingCart.ApplicationUserId = userId;

            var cartFromDb = _unitOfWork.ShoppingCart.GetBy(s => s.ApplicationUserId == userId && 
                s.ProductId == shoppingCart.ProductId);

            if (cartFromDb is null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();


                HttpContext.Session.SetInt32(SessionConstants.SessionCart,
                    _unitOfWork.ShoppingCart.GetAllBy(s => s.ApplicationUserId == userId).Count());
            }
            else
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }

            TempData["success"] = "Items added to cart successfully!";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
