using Bulky.DataAccess.Repositories.Abstractions;
using Bulky.Models.Identity;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility.OrderUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAllBy(u => u.ApplicationUserId == userId, "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(ShoppingCartVM);
        }


        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetBy(o => o.Id==id, includeProperties: "ApplicationUser");

            if(orderHeader.PaymentStatus != PaymentStatus.DelayedPayment)
            {
                var service = new SessionService();

                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, OrderStatus.Approved, PaymentStatus.Approved);
                    _unitOfWork.Save();
                }
            }

            var shoppingCarts = _unitOfWork.ShoppingCart
                .GetAllBy(u=> u.ApplicationUserId == orderHeader.ApplicationUserId)
                .ToList();
      
            _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        private void CreateOrderDetails()
        {
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
        }

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAllBy(u => u.ApplicationUserId == userId,
				includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetBy(u => u.Id == userId);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;



			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
        [ActionName(nameof(Summary))]   
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity!;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAllBy(u => u.ApplicationUserId == userId,
				includeProperties: "Product");

			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			var applicationUser = _unitOfWork.ApplicationUser.GetBy(u => u.Id == userId);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}

			AssignOrderAndPaymentStatus(applicationUser);

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			CreateOrderDetails();

			if (IsCustomerAccount(applicationUser))
            {
                //stripe payment
                var options = new SessionCreateOptions
                {
                    SuccessUrl =  $"http://localhost:5140/customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = "http://localhost:5140/customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                

                var service = new SessionService();
                var session = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetBy(c => c.Id == cartId);
            cartFromDb.Count += 1;

            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetBy(c => c.Id == cartId);

            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Delete(cartFromDb);
            }
            else 
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetBy(c => c.Id == cartId);

            _unitOfWork.ShoppingCart.Delete(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;

            if(shoppingCart.Count <= 100)
                return shoppingCart.Product.Price50;

            return shoppingCart.Product.Price100;
        }

		private bool IsCompanyAccount(ApplicationUser user) => user.CompanyId.GetValueOrDefault() != 0;
		private bool IsCustomerAccount(ApplicationUser user) => user.CompanyId.GetValueOrDefault() == 0;
		private void AssignOrderAndPaymentStatus(ApplicationUser user)
		{
			if (IsCompanyAccount(user))
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.DelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.Approved;
				return;
			}
			//Regular customer account
			ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.Pending;
			ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.Pending;
		}
	}
}
