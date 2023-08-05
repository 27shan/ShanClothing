using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Interfaces;
using ShanClothing.Domain.Enum;

namespace ShanClothing.Controllers 
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IOrderService _orderService;
        private readonly IAppUserService _appUserService;
        private readonly IClothService _clothService;

        public AccountController(IAccountService accountService, IOrderService orderService, IAppUserService appUserService,
            IClothService clothService)
        {
            _accountService = accountService;
            _orderService = orderService;
            _appUserService = appUserService;
            _clothService = clothService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var responseUser = await _accountService.Register(model);
            
            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.CreateBasket(responseUser.Data.Id);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("SendEmailConfirmation", "Account", new {userEmail = responseUser.Data.Email});
                }
                return View("Error", $"{responseOrder.Description}");
            }
            else if(responseUser.StatusCode != Domain.Enum.StatusCode.InternalServerError)
            {
                TempData["ErrorMessage"] = responseUser.Description;
                return View();
            }
            return View("Error", $"{responseUser.Description}");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var response = await _accountService.Login(model);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }
            else if(response.StatusCode == Domain.Enum.StatusCode.EmailNotConfirmed)
            {
                return RedirectToAction("SendEmailConfirmation", "Account", new { userEmail = model.Email });
            }
            else if(response.StatusCode != Domain.Enum.StatusCode.InternalServerError)
            {
                TempData["ErrorMessage"] = response.Description;
                return View();
            }
            return View("Error", $"{response.Description}");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await _accountService.Logout();

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("Error", $"{response.Description}");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var response = await _accountService.GetProfile(User.Identity.Name);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return View(response.Data);
            }
			return View("Error", $"{response.Description}");
		}

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Basket()
        {
            var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.GetBasket(responseUser.Data.Id);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return View(responseOrder.Data);
                }
				return View("Error", $"{responseOrder.Description}");
			}
            return View("Error", $"{responseUser.Description}");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.GetUserOrders(responseUser.Data.Id);

                if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK || responseOrder.StatusCode == Domain.Enum.StatusCode.EntityNotFound)
                {
                    return View(responseOrder.Data);
                }
				return View("Error", $"{responseOrder.Description}");
			}
			return View("Error", $"{responseUser.Description}");
		}

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Order(Guid id)
        {
            var response = await _orderService.Get(id);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return View(response.Data);
            }
            return View("Error", $"{response.Description}");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var response = await _accountService.GetProfile(User.Identity.Name);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return View(response.Data);
            }
            return View("Error", $"{response.Description}");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var response = await _accountService.UpdateUser(model, User.Identity.Name);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return RedirectToAction("Profile", "Account");
            }
            return View("Error", $"{response.Description}");
        }

        public async Task<IActionResult> ChangePassword(UpdatePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Error", "Некорректные данные.");
            }

            var response = await _accountService.ChangePassword(model, User.Identity.Name);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseLogout = await _accountService.Logout();

                if(responseLogout.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return RedirectToAction("Login", "Account");
                }
                return View("Error", $"{responseLogout.Description}");
            }
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
        public async Task<IActionResult> SendEmailConfirmation(string userEmail)
        {
            var responseToken = await _accountService.GenerateEmailConfirmationToken(userEmail);

            if (responseToken.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var confirmEmailUrl = Url.Action("ConfirmEmail", "Account", new { userEmail = userEmail, token = responseToken.Data }, Request.Scheme);

                var responseEmail = await _accountService.SendEmailConfirmation(userEmail, confirmEmailUrl);

                if (responseEmail.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return View("SendEmailConfirmation", userEmail);
                }
                return View("Error", $"{responseEmail.Description}");
            }
            return View("Error", $"{responseToken.Description}");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userEmail, string token)
        {
            var response = await _accountService.ConfirmEmail(userEmail, token);

            if (response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return View("ConfirmEmail", userEmail);
            }
            return View("Error", $"{response.Description}");
        }

        [HttpGet]
        public IActionResult PasswordResetEmail() => View();

        [HttpPost]
        public async Task<IActionResult> SendPasswordResetEmail(string userEmail)
        {
            var responseToken = await _accountService.GeneratePasswordResetToken(userEmail);

            if(responseToken.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var resetPasswordUrl = Url.Action("ResetPassword", "Account", new { userEmail = userEmail, token = responseToken.Data }, Request.Scheme);

                var responseEmail = await _accountService.SendPasswordReset(userEmail, resetPasswordUrl);

                if(responseEmail.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    return View("SendPasswordResetEmail", userEmail);
                }
                return View("Error", $"{responseEmail.Description}");
            }
            return View("Error", $"{responseToken.Description}");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userEmail, string token)
        {
            var model = new ResetPasswordTokenViewModel
            {
                Email = userEmail,
                Token = token
            };
            return View("ResetPassword", model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var response = await _accountService.ResetPassword(model);

            if(response.StatusCode == Domain.Enum.StatusCode.OK)
            {
                return View("ResetPasswordResult");
            }
            return View("Error", $"{response.Description}");
        }
	}
}