using Microsoft.AspNetCore.Identity;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
	public interface IAppUserService
	{
		public Task<BaseResponse<AppUser>> GetByName(string name);

		public Task<BaseResponse<bool>> Update(AppUser user);

		public Task<BaseResponse<List<AppUser>>> GetUsers(bool isModerator, int page, int pageSize);

		public Task<BaseResponse<List<AppUser>>> GetUsers(string email);

		public Task<BaseResponse<List<IdentityRole<Guid>>>> GetRoles();

		public Task<BaseResponse<AppUser>> ChangeUserRole(Guid userId, string roleName);

	}
}
