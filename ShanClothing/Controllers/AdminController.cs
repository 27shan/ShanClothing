using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace ShanClothing.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITypeClothService _typeClothService;
        private readonly IImageClothService _imageClothService;
        private readonly IClothService _clothService;
        private readonly IOrderService _orderService;
        private readonly IAppUserService _appUserService;

        public AdminController(ITypeClothService typeClothService, IImageClothService imageClothService, IClothService clothService,
            IOrderService orderService, IAppUserService appUserService)
        {
            _typeClothService = typeClothService;
            _imageClothService = imageClothService;
            _clothService = clothService;
            _orderService = orderService;
            _appUserService = appUserService;
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> Clothes(int typeId, SortType sortType, bool showHidden, string name)
        {
            var responseType = await _typeClothService.GetAll();

            if(responseType.StatusCode == Domain.Enum.StatusCode.OK || responseType.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
            {
                if (name != "0")
                {
                    var response = await _clothService.GetClothes(name);

                    if (response.StatusCode == Domain.Enum.StatusCode.OK || response.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
                    {
                        var model = new AdminClothesViewModel
                        {
                            TypesClothes = responseType.Data,
                            Clothes = response.Data
                        };
                        return View(model);
                    }
                    return View("Error", $"{response.Description}");
                }

                var responseTwo = await _clothService.GetClothes(typeId, sortType, showHidden, 1, 16);

                if (responseTwo.StatusCode == Domain.Enum.StatusCode.OK || responseTwo.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
                {
                    var model = new AdminClothesViewModel
                    {
                        TypesClothes = responseType.Data,
                        Clothes = responseTwo.Data
                    };
                    return View(model);
                }
                return View("Error", $"{responseTwo.Description}");
            }
            return View("Error", $"{responseType.Description}");
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> GetClothesJson(int typeId, SortType sortType, bool showHidden, int page)
        {
            var response = await _clothService.GetClothes(typeId, sortType, showHidden, page, 16);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new {isFound = true, Clothes = response.Data});
            }
            return Json(new { isFound = false });
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> GetClothJson(int id)
        {
            var response = await _clothService.Get(id);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { isFound = true, cloth = response.Data });
            }
            return Json(new { isFound = false });
        }

        [HttpPost]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> CreateCloth(CreateClothViewModel model)
        {
            var response = await _clothService.Create(model);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                if(model.Files == null || !model.Files.Any())
                    return RedirectToAction("Clothes", "Admin",
                        new { typeId = model.TypeId, sortType = SortType.DateDescending, showHidden = true, name = "0" });

                var responseImage = await _imageClothService.Create(model.Files, response.Data.Id);

                if(responseImage.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("Clothes", "Admin",
                        new { typeId = model.TypeId, sortType = SortType.DateDescending, showHidden = true, name = "0" });
                }
                return View("Error", $"{responseImage.Description}");
            }
            return View("Error", $"{response.Description}");
        }

        [HttpPost]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> MinorEditCloth(MinorEditClothViewModel model)
        {
            var response = await _clothService.MinorUpdate(model);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(true);
            }
            return Json(false);
        }

        [HttpPost]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> EditCloth(EditClothViewModel model)
        {
            var response = await _clothService.Update(model);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                if(model.ImagesClothId != null)
                {
                    for (int i = 0; i < model.ImagesClothId.Length; i++)
                    {
                        var responseDeleteImage = await _imageClothService.Delete(model.ImagesClothId[i]);

                        if (responseDeleteImage.StatusCode != Domain.Enum.StatusCode.OK)
                            return View("Error", $"{responseDeleteImage.Description}");
                    }
                }

                if(model.Files == null || !model.Files.Any())
                    return RedirectToAction("Clothes", "Admin",
                        new { typeId = model.TypeId, sortType = SortType.DateDescending, showHidden = true, name = "0" });

                var responseCreateImage = await _imageClothService.Create(model.Files, response.Data.Id);

                if(responseCreateImage.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("Clothes", "Admin",
                        new { typeId = model.TypeId, sortType = SortType.DateDescending, showHidden = true, name = "0" });
                }
                return View("Error", $"{responseCreateImage.Description}");
            }
            return View("Error", $"{response.Description}");
        }

        [HttpPost]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> CreateTypeCloth(string Name)
        {
            var response = await _typeClothService.Create(Name);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Clothes", "Admin",
                        new { typeId = 0, sortType = SortType.DateDescending, showHidden = true, name = "0" });
            }
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> Orders(SortType sortType, StatusOrder statusOrder, Guid userId, Guid orderId)
        {
            if (orderId != Guid.Empty)
            {
                var response = await _orderService.GetOrders(orderId);

                if (response.StatusCode == Domain.Enum.StatusCode.OK || response.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
                {
                    return View(response.Data);
                }
                return View("Error", $"{response.Description}");
            }

            if (userId != Guid.Empty)
            {
                var response = await _orderService.GetUserOrders(userId);

                if (response.StatusCode == Domain.Enum.StatusCode.OK || response.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
                {
                    return View(response.Data);
                }
                return View("Error", $"{response.Description}");
            }
            
            var responseTwo = await _orderService.GetOrders(sortType, statusOrder, 1, 16);

            if (responseTwo.StatusCode == Domain.Enum.StatusCode.OK || responseTwo.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
            {
                return View(responseTwo.Data);
            }
            return View("Error", $"{responseTwo.Description}");
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> GetOrdersJson(SortType sortType, StatusOrder statusOrder, int page)
        {
            var response = await _orderService.GetOrders(sortType, statusOrder, page, 16);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { isFound = true, orders = response.Data });
            }
            return Json(new { isFound = false });
        }

        [HttpGet]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> GetOrderJson(Guid id)
        {
            var response = await _orderService.Get(id);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { isFound = true, order = response.Data });
            }
            return Json(new { isFound = false });
        }

        [HttpPost]
		[Authorize(Roles = "Moderator")]
		public async Task<IActionResult> SendOrder(Guid orderId, string tracker)
        {
            var response = await _orderService.SendTracker(orderId, tracker);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Orders", "Admin", new { sortType = SortType.DateDescending, statusOrder = StatusOrder.Paid, userId = Guid.Empty, orderId = Guid.Empty });
            }
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Users(bool isModerator, string email)
        {
            if(email != "0")
            {
                var responseName = await _appUserService.GetUsers(email);

                if (responseName.StatusCode == Domain.Enum.StatusCode.OK || responseName.StatusCode == Domain.Enum.StatusCode.UserNotFound)
                {
                    return View(responseName.Data);
                }
                return View("Error", $"{responseName.Description}");
            }

            var response = await _appUserService.GetUsers(isModerator, 1, 16);

            if(response.StatusCode == Domain.Enum.StatusCode.OK || response.StatusCode == Domain.Enum.StatusCode.UserNotFound)
            {
				return View(response.Data);
            }
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetUsersJson(bool isModerator, int page)
        {
            var response = await _appUserService.GetUsers(isModerator, page, 16);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return Json(new { isFound = true, users = response.Data });
            }
            return Json(new { isFound = false });
        }

        [HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ChangeUserRole(Guid userId, string roleName)
        {
            var response = await _appUserService.ChangeUserRole(userId, roleName);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Users", "Admin", new { isModerator = false, email = response.Data.Email  });
            }
			return View("Error", $"{response.Description}");
		}

	}
}
