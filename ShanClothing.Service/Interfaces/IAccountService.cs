using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ShanClothing.Service.Interfaces
{
    public interface IAccountService
    {
        public Task<BaseResponse<AppUser>> Register(RegisterViewModel model);

        public Task<BaseResponse<bool>> Login(LoginViewModel model);

        public Task<BaseResponse<bool>> Logout();

        public Task<BaseResponse<ProfileViewModel>> GetProfile(string userName);

        public Task<BaseResponse<AppUser>> UpdateUser(EditProfileViewModel model, string userName);

        public Task<BaseResponse<bool>> ChangePassword(UpdatePasswordViewModel model, string userName);

        public Task<BaseResponse<string>> GenerateEmailConfirmationToken(string userEmail);

        public Task<BaseResponse<bool>> SendEmailConfirmation(string userEmail, string confirmEmailUrl);

        public Task<BaseResponse<bool>> ConfirmEmail(string userEmail, string token);

        public Task<BaseResponse<string>> GeneratePasswordResetToken(string userEmail);

        public Task<BaseResponse<bool>> SendPasswordReset(string userEmail, string passwordResetUrl);

        public Task<BaseResponse<bool>> ResetPassword(ResetPasswordViewModel model);
    }
}
