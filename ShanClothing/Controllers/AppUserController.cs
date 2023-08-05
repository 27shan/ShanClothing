using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Interfaces;

namespace ShanClothing.Controllers
{
    public class AppUserController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IAppUserService _appUserService;

        public AppUserController(IOrderService orderService, IAppUserService appUserService)
        {
            _orderService = orderService;
            _appUserService = appUserService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddClothInBasket(int clothId, char size)
        {
            var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.AddClothInBasket(responseUser.Data.Id, clothId, size);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return Json(true);
                }
                return Json(false);
            }
            return Json(false);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveClothFromBasket(int clothId, char size)
        {
			var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.RemoveClothFromBasket(responseUser.Data.Id, clothId, size);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("Basket", "Account");
                }
				return View("Error", $"{responseOrder.Description}");
			}
			return View("Error", $"{responseUser.Description}");
		}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> СhangeNumberClothInBasket(int clothId, char size, int number)
        {
            var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.СhangeNumberClothInBasket(responseUser.Data.Id, clothId, size, number);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("Basket", "Account");
                }
                return View("Error", $"{responseOrder.Description}");
            }
            return View("Error", $"{responseUser.Description}");
        }

        [Authorize]
        public async Task<IActionResult> GetAppUserInfo()
        {
            var response = await _appUserService.GetByName(User.Identity.Name);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var userInfo = new AppUserInfoViewModel()
                {
                    FirstName = response.Data.FirstName,
                    LastName = response.Data.LastName,
                    Email = response.Data.Email,
                    PhoneNumber = response.Data.PhoneNumber,
                    Address = response.Data.Address,
                    Postcode = response.Data.Postcode
                };

                return Json(new {isFound = true, userInfo });
            }

            return Json(new { isFound = false });
        }
    }
}
