using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ShanClothing.DAL.Interfaces;
using ShanClothing.DAL.Repositories;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Implementations
{
	public class TypeClothService : ITypeClothService
	{
		private readonly IBaseRepository<TypeCloth> _typeClothRepository;

		public TypeClothService(IBaseRepository<TypeCloth> typeClothRepository)
		{
            _typeClothRepository = typeClothRepository;
		}

		public async Task<BaseResponse<TypeCloth>> Create(string name)
		{
			try
			{
				if((await _typeClothRepository.Get().FirstOrDefaultAsync(t => t.Name == name.ToLower())) != null)
				{
                    return new BaseResponse<TypeCloth>()
                    {
						Data = null,
                        Description = "Категория уже существует",
                        StatusCode = StatusCode.EntityExists
                    };
                }

				var typeCloth = new TypeCloth()
				{
					Name = name.ToLower(),
				};

				await _typeClothRepository.Create(typeCloth);

				return new BaseResponse<TypeCloth>()
				{
					Data = typeCloth,
					Description = "Категория создана.",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<TypeCloth>()
				{
					Data = null,
					Description = $"[CreateType]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<bool>> Delete(int id)
		{
			try
			{
				var typeCloth = await _typeClothRepository.Get().FirstOrDefaultAsync(t => t.Id == id);

				if (typeCloth == null)
				{
					return new BaseResponse<bool>
					{
						Data = false,
						Description = "Категория не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}
				await  _typeClothRepository.Delete(typeCloth);

				return new BaseResponse<bool>
				{
					Data = true,
					Description = "Категория удалена.",
					StatusCode = StatusCode.OK
				};

			}
			catch (Exception ex)
			{
				return new BaseResponse<bool>
				{
					Data = false,
					Description = $"[Delete]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<TypeCloth>> Get(int id)
		{
			try
			{
				var typeCloth = await _typeClothRepository.Get()
					.Include(t => t.Clothes)
					.ThenInclude(c => c.ImagesCloth)
					.FirstOrDefaultAsync(t => t.Id == id);

				if (typeCloth == null)
				{
					return new BaseResponse<TypeCloth>
					{
						Description = "Категория не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<TypeCloth>
				{
					Data = typeCloth,
					StatusCode = StatusCode.OK
				};

			}
			catch(Exception ex)
			{
				return new BaseResponse<TypeCloth>
				{
					Description = $"[GetType]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<List<TypeCloth>>> GetAll()
		{
			try
			{
				var typesCloth = await _typeClothRepository.Get().ToListAsync();

				if (!typesCloth.Any())
				{
					return new BaseResponse<List<TypeCloth>>()
					{
						Data = typesCloth,
						Description = "Категорий не найденно.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<List<TypeCloth>>()
				{
					Data = typesCloth,
					Description = "Категории получены.",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<List<TypeCloth>>()
				{
					Data = null,
					Description = $"[GetAll]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}
	}
}
