using Microsoft.AspNetCore.Identity;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShanClothing.Domain.ViewModels;
using System.Data;

namespace ShanClothing.Service.Implementations
{
	public class AppUserService : IAppUserService
	{
		private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

		public AppUserService(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
		{
			_userManager = userManager;
            _roleManager = roleManager;
		}

		public async Task<BaseResponse<AppUser>> GetByName(string name)
		{
			try
			{
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == name);
                if (user == null)
                {
                    return new BaseResponse<AppUser>()
                    {
						Data = null,
                        Description = "Пользователь не найден.",
                        StatusCode = Domain.Enum.StatusCode.UserNotFound,
                    };
                }

                return new BaseResponse<AppUser>()
                {
                    Data = user,
					Description = "Пользователь найден",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
			catch (Exception ex)
			{
                return new BaseResponse<AppUser>()
                {
					Data = null,
                    Description = $"[GetByNameUser]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<bool>> Update(AppUser user)
		{
			try
			{
				var result = await _userManager.UpdateAsync(user);
				
				if(result.Succeeded)
				{
					return new BaseResponse<bool>
					{
						Data = true,
						StatusCode = Domain.Enum.StatusCode.OK
					};
				}
				return new BaseResponse<bool>
				{
					Data = false,
					StatusCode = Domain.Enum.StatusCode.InternalServerError
				};

			}
			catch (Exception ex)
			{
				return new BaseResponse<bool>()
				{
					Description = $"[Update]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<List<AppUser>>> GetUsers(bool isModerator, int page, int pageSize)
		{
			try
			{
                var users  = await _userManager.Users
                .Where(u => u.IsModerator == isModerator)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

                if (!users.Any())
                {
                    return new BaseResponse<List<AppUser>>
                    {
                        Data = users,
                        Description = "Пользователи не найденны.",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

                return new BaseResponse<List<AppUser>>()
                {
                    Data = users,
                    Description = "Пользователи полученны.",
                    StatusCode = StatusCode.OK
                };
            }
			catch(Exception ex)
			{
                return new BaseResponse<List<AppUser>>()
                {
                    Data = null,
                    Description = $"[GetUsers]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}

		public async Task<BaseResponse<List<AppUser>>> GetUsers(string email)
		{
			try
			{
				var users = await _userManager.Users.Where(u => u.NormalizedEmail == email.ToUpper()).ToListAsync();

                if (!users.Any())
                {
                    return new BaseResponse<List<AppUser>>
                    {
                        Data = users,
                        Description = "Пользователи не найденны.",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

                return new BaseResponse<List<AppUser>>()
                {
                    Data = users,
                    Description = "Пользователи полученны.",
                    StatusCode = StatusCode.OK
                };
            }
			catch(Exception ex)
			{
                return new BaseResponse<List<AppUser>>()
                {
                    Data = null,
                    Description = $"[GetUsers]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}

        public async Task<BaseResponse<List<IdentityRole<Guid>>>> GetRoles()
        {
			try
			{
				var roles = await _roleManager.Roles.ToListAsync();

                if(!roles.Any())
                {
                    return new BaseResponse<List<IdentityRole<Guid>>>()
                    {
                        Data = roles,
                        Description = "Роли не найденны.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

				return new BaseResponse<List<IdentityRole<Guid>>>()
				{
					Data = roles,
					Description = "Роли полученны.",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<List<IdentityRole<Guid>>>()
				{
					Data = null,
					Description = $"[GetRoles]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

        public async Task<BaseResponse<AppUser>> ChangeUserRole(Guid userId, string roleName)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if(user == null)
                {
                    return new BaseResponse<AppUser>()
                    {
                        Data = null,
                        Description = "Пользователь не найденн.",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

                if(await _roleManager.RoleExistsAsync(roleName))
                {
                    if(roleName == "User")
                    {
                        if(!user.IsModerator)
                        {
							return new BaseResponse<AppUser>()
							{
								Data = null,
								Description = "Пользователь уже имеет роль User",
								StatusCode = StatusCode.IncorrectData
							};
						}

                        user.IsModerator = false;

						var resultUser = await _userManager.UpdateAsync(user);

						if (resultUser.Succeeded)
						{
							var resultRole = await _userManager.RemoveFromRoleAsync(user, "Moderator");

                            if(resultRole.Succeeded)
                            {
								return new BaseResponse<AppUser>()
								{
									Data = user,
									Description = "Пользователю присвоенна роль User.",
									StatusCode = StatusCode.OK
								};
							}
                            else
                            {
								return new BaseResponse<AppUser>()
								{
									Data = null,
									Description = resultRole.ToString(),
									StatusCode = StatusCode.InternalServerError
								};
							}
						}
						else
						{
							return new BaseResponse<AppUser>()
							{
								Data = null,
								Description = resultUser.ToString(),
								StatusCode = StatusCode.InternalServerError
							};
						}
					}
                    else if(roleName == "Moderator")
                    {
						if (user.IsModerator)
						{
							return new BaseResponse<AppUser>()
							{
								Data = null,
								Description = "Пользователь уже имеет роль Moderator",
								StatusCode = StatusCode.IncorrectData
							};
						}

						user.IsModerator = true;

						var resultUser = await _userManager.UpdateAsync(user);

						if (resultUser.Succeeded)
						{
							var resultRole = await _userManager.AddToRoleAsync(user, "Moderator");

							if (resultRole.Succeeded)
							{
								return new BaseResponse<AppUser>()
								{
									Data = user,
									Description = "Пользователю присвоенна роль Moderator.",
									StatusCode = StatusCode.OK
								};
							}
							else
							{
								return new BaseResponse<AppUser>()
								{
									Data = null,
									Description = resultRole.ToString(),
									StatusCode = StatusCode.InternalServerError
								};
							}
						}
						else
						{
							return new BaseResponse<AppUser>()
							{
								Data = null,
								Description = resultUser.ToString(),
								StatusCode = StatusCode.InternalServerError
							};
						}
					}
                    else
                    {
						return new BaseResponse<AppUser>()
						{
							Data = null,
							Description = "Некорректные данные",
							StatusCode = StatusCode.IncorrectData
						};
					}
                }
                else
                {
                    return new BaseResponse<AppUser>()
                    {
                        Data = null,
                        Description = "Некорректные данные",
                        StatusCode = StatusCode.IncorrectData
                    };
                }
            }
            catch( Exception ex)
            {
				return new BaseResponse<AppUser>()
				{
					Data = null,
					Description = $"[ChangeUserRole]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
        }
    }
}
