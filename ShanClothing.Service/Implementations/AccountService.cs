using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using ShanClothing.Service.Helpers;
using System.Web;
using System.Net;
using Org.BouncyCastle.Cms;
using System.Net.Http;

namespace ShanClothing.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly SmtpClient _smtpClient;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            SmtpClient smtpClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _smtpClient = smtpClient;
        }

        public async Task<BaseResponse<AppUser>> Register(RegisterViewModel model)
        {
            try
            {
                if(model.Password != model.PasswordConfirm)
                {
                    return new BaseResponse<AppUser>()
                    {
                        Data = null,
                        Description = "Пароли не совпадают.",
                        StatusCode = StatusCode.PasswordsDontMatch
                    };
                }

                if ((await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email)) != null)
                {
                    return new BaseResponse<AppUser>
                    {
                        Data = null,
                        Description = "Email уже используется.",
                        StatusCode = StatusCode.UserExists
                    };
                }

                var user = new AppUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    return new BaseResponse<AppUser>
                    {
                        Data = user,
                        Description = "Регистрация прошла успешно",
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }

                return new BaseResponse<AppUser>
                {
                    Data = null,
                    Description = "Ошибка при регистрации.",
                    StatusCode = Domain.Enum.StatusCode.InternalServerError
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<AppUser>()
                {
                    Data = null,
                    Description = $"[Register]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> Login(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if(user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                if(!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Вход невозможен. Почта не подтверждена.",
                        StatusCode = Domain.Enum.StatusCode.EmailNotConfirmed
                    };
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

                if(result.Succeeded)
                {
                    return new BaseResponse<bool>
                    {
                        Data = true,
                        Description = "Вход выполнен успешно.",
                        StatusCode = Domain.Enum.StatusCode.OK
                    };
                }
                else
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Неверный Email или пароль", 
                        StatusCode = Domain.Enum.StatusCode.UserNotFound
                    };
                }
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[Login]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();

                return new BaseResponse<bool>
                {
                    Data = true,
                    Description = "Выход выполнен успешно.",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[Logout]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<ProfileViewModel>> GetProfile(string userName)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    return new BaseResponse<ProfileViewModel>()
                    {
                        Data = null,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                var profile = new ProfileViewModel()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Postcode = user.Postcode,
                };

                return new BaseResponse<ProfileViewModel>()
                {
                    Data = profile,
                    Description = "Профиль получен",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            catch (Exception ex)
            {
				return new BaseResponse<ProfileViewModel>()
				{
                    Data = null,
					Description = $"[GetProfile]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
        }

        public async Task<BaseResponse<AppUser>> UpdateUser(EditProfileViewModel model, string userName)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                if (user == null)
                {
                    return new BaseResponse<AppUser>()
                    {
                        Data = null,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Postcode = model.Postcode;

                await _userManager.UpdateAsync(user);

                return new BaseResponse<AppUser>()
                {
                    Data = user,
                    Description = "Данные пользователя обновлены.",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<AppUser>()
                {
                    Data = null,
                    Description = $"[UpdateUser]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> ChangePassword(UpdatePasswordViewModel model, string userName)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                if(user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

                if(model.NewPassword != model.ConfirmPassword)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пароли не совпадают.",
                        StatusCode = StatusCode.PasswordsDontMatch
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if(result.Succeeded)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = true,
                        Description = "Пароль успешно обновлен.",
                        StatusCode = StatusCode.OK
                    };
                }
                else
                {
                    if(result.Errors.Any(error => error.Code == "PasswordMismatch"))
                    {
                        return new BaseResponse<bool>()
                        {
                            Data = false,
                            Description = "Неверный пароль",
                            StatusCode = StatusCode.IncorrectPassword
                        };
                    }
                    else
                    {
                        return new BaseResponse<bool>
                        {
                            Data = false,
                            Description = string.Join(", ", result.Errors.Select(e => e.Description)),
                            StatusCode = StatusCode.InternalServerError
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[ChangePassword]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<string>> GenerateEmailConfirmationToken(string userEmail)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return new BaseResponse<string>()
                    {
                        Data = null,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new BaseResponse<string>
                    {
                        Data = null,
                        Description = "Почта уже подтверждена.",
                        StatusCode = StatusCode.EntityExists
                    };
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return new BaseResponse<string>
                {
                    Data = emailConfirmationToken,
                    Description = "Токен сгенерирован.",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<string>()
                {
                    Data = null,
                    Description = $"[GenerateEmailConfirmationToken]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> SendEmailConfirmation(string userEmail, string confirmEmailUrl)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Почта уже подтверждена.",
                        StatusCode = StatusCode.EntityExists
                    };
                }

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("shanclothing@outlook.com");
                mailMessage.To.Add(new MailAddress(user.Email));
                mailMessage.Subject = "Подтверждение почты";
                mailMessage.Body = $"Пожалуйста, подтвердите свою почту, перейдя по ссылке: <a href='{confirmEmailUrl}'>Подтвердить</a>";
                mailMessage.IsBodyHtml = true;

                await _smtpClient.SendMailAsync(mailMessage);

                return new BaseResponse<bool>
                {
                    Data = true,
                    Description = "Письмо отправлено.",
                    StatusCode = StatusCode.OK
                };
            }
            catch( Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[SendEmailConfirmation]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> ConfirmEmail(string userEmail, string token)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Почта уже подтверждена.",
                        StatusCode = StatusCode.EntityExists
                    };
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return new BaseResponse<bool>
                    {
                        Data = true,
                        Description = "Почта подтверждена.",
                        StatusCode = StatusCode.OK
                    };
                }
                else
                {
                    var errorDescription = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = errorDescription,
                        StatusCode = StatusCode.InternalServerError
                    };
                }
            }
            catch( Exception ex )
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[ConfirmEmail]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<string>> GeneratePasswordResetToken(string userEmail)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return new BaseResponse<string>()
                    {
                        Data = null,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                return new BaseResponse<string>
                {
                    Data = token,
                    Description = "Токен сгенерирован.",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<string>()
                {
                    Data = null,
                    Description = $"[GeneratePasswordResetToken]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> SendPasswordReset(string userEmail, string passwordResetUrl)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("shanclothing@outlook.com");
                mailMessage.To.Add(new MailAddress(user.Email));
                mailMessage.Subject = "Подтверждение почты";
                mailMessage.Body = $"Пожалуйста, чтобы сбросить пароль, перейдите по ссылке: <a href='{passwordResetUrl}'>Подтвердить</a>";
                mailMessage.IsBodyHtml = true;

                await _smtpClient.SendMailAsync(mailMessage);

                return new BaseResponse<bool>
                {
                    Data = true,
                    Description = "Письмо отправлено.",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[SendPasswordReset]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if(model.Password != model.PasswordConfirm)
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Пароли не совпадают.",
                        StatusCode = StatusCode.PasswordsDontMatch
                    };
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Data = false,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (!result.Succeeded)
                {
                    return new BaseResponse<bool>
                    {
                        Data = false,
                        Description = "Не удалось сбросить пароль.",
                        StatusCode = StatusCode.InternalServerError
                    };
                }

                return new BaseResponse<bool>
                {
                    Data = true,
                    Description = "Пароль успешно сброшен.",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[ResetPassword]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}
