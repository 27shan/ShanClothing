using Microsoft.EntityFrameworkCore;
using ShanClothing.DAL.Interfaces;
using ShanClothing.DAL.Repositories;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShanClothing.Service.Implementations
{
	public class ClothService : IClothService
	{
		private readonly IBaseRepository<Cloth> _clothRepository;

		public ClothService(IBaseRepository<Cloth> clothRepository)
		{
            _clothRepository = clothRepository;
		}

        public async Task<BaseResponse<Cloth>> Create(CreateClothViewModel model)
        {
            try
            {
                if(model.Price < 0 || model.Discount > 100)
                {
                    return new BaseResponse<Cloth>()
                    {
                        Data = null,
                        Description = "Неккоректные данные.",
                        StatusCode = StatusCode.IncorrectData
                    };
                }

                var cloth = new Cloth()
                {
                    Name = model.Name.ToLower(),
                    Price = model.Price,
                    Discount = model.Discount,
                    Description = model.Description,
                    NumberS = model.NumberS,
                    NumberM = model.NumberM,
                    NumberL = model.NumberL,
                    TimeCreation = DateTime.Now,
                    TypeClothId = model.TypeId,
                    IsVisible = model.IsVisible,
                    NumberSold = 0,
                    PriceSold = 0
                };
                
                await _clothRepository.Create(cloth);

                return new BaseResponse<Cloth>()
                {
                    Data = cloth,
                    Description = "Товар создан.",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Cloth>()
                {
                    Data = null,
                    Description = $"[CreateCloth]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

		public async Task<BaseResponse<Cloth>> Get(int id)
		{
			try
			{
				var cloth = await _clothRepository.Get()
					.Include(x => x.TypeCloth)
					.Include(x => x.ImagesCloth)
					.FirstOrDefaultAsync(x => x.Id == id);

				if(cloth == null)
				{
					return new BaseResponse<Cloth>()
					{
						Data = null,
						Description = "Товар не найден.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<Cloth>()
				{
					Data = cloth,
					Description = "Товар получен.",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<Cloth>()
				{
					Data = null,
					Description = $"[GetCloth]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

        public async Task<BaseResponse<List<Cloth>>> GetClothes(int typeId, SortType sortType, bool showHidden, int page, int pageSize)
        {
            try
            {
                IQueryable<Cloth> query = _clothRepository.Get()
                    .Include(c => c.ImagesCloth)
                    .Include(c => c.TypeCloth);


                if (typeId != 0)
                {
                    query = query.Where(c => c.TypeClothId == typeId);
                }

                if (!showHidden)
                {
                    query = query.Where(c => c.IsVisible);
                }

                switch (sortType)
                {
                    case SortType.DateAscending:
                        query = query.OrderBy(c => c.TimeCreation);
                        break;
                    case SortType.DateDescending:
                        query = query.OrderByDescending(c => c.TimeCreation);
                        break;
                    case SortType.PriceAscending:
                        query = query.OrderBy(c => c.Price);
                        break;
                    case SortType.PriceDescending:
                        query = query.OrderByDescending(c => c.Price);
                        break;
                    case SortType.DiscountAscending:
                        query = query.OrderBy(c => c.Discount);
                        break;
                    case SortType.DiscountDescending:
                        query = query.OrderByDescending(c => c.Discount);
                        break;
                    case SortType.PopularAscending:
                        query = query.OrderBy(c => c.NumberSold);
                        break;
                    case SortType.PopularDescending:
                        query = query.OrderByDescending(c => c.NumberSold);
                        break;
                }

                query = query.Skip((page - 1) * pageSize).Take(pageSize);

                var clothes = await query.ToListAsync();

                if (!clothes.Any())
                {
                    return new BaseResponse<List<Cloth>>
                    {
                        Data = clothes,
                        Description = "Товары не найдены.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                return new BaseResponse<List<Cloth>>()
                {
                    Data = clothes,
                    Description = "Товары получены",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<List<Cloth>>()
                {
                    Data = null,
                    Description = $"[GetClothes]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<List<Cloth>>> GetClothes(string name)
        {
            try
            {
                var clothes = await _clothRepository.Get()
                    .Include(c => c.ImagesCloth)
                    .Include(c => c.TypeCloth)
                    .Where(c => c.Name == name.ToLower())
                    .ToListAsync();

                if (!clothes.Any())
                {
                    return new BaseResponse<List<Cloth>>
                    {
                        Data = clothes,
                        Description = "Товары не найдены.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                return new BaseResponse<List<Cloth>>()
                {
                    Data = clothes,
                    Description = "Товары получены",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<List<Cloth>>()
                {
                    Data = null,
                    Description = $"[GetClothes]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<Cloth>> MinorUpdate(MinorEditClothViewModel model)
        {
            try
            {
                if(model.Discount > 100)
                {
                    return new BaseResponse<Cloth>()
                    {
                        Data = null,
                        Description = "Неккоректные данные.",
                        StatusCode = StatusCode.IncorrectData
                    };
                }

                var cloth = await _clothRepository.Get().FirstOrDefaultAsync(c => c.Id == model.Id);

                if (cloth == null)
                {
                    return new BaseResponse<Cloth>()
                    {
                        Data = null,
                        Description = "Товар не найден.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                cloth.Discount = model.Discount;
                cloth.NumberS = model.NumberS;
                cloth.NumberM = model.NumberM;
                cloth.NumberL = model.NumberL;
                
                if(cloth.NumberS == 0 && cloth.NumberM == 0 && cloth.NumberL == 0)
                    cloth.IsVisible = false;
                else
                    cloth.IsVisible = true;

                await _clothRepository.Update(cloth);

                return new BaseResponse<Cloth>
                {
                    Data = cloth,
                    Description = "Товар обновлен",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<Cloth>()
                {
                    Data = null,
                    Description = $"[MinorUpdate]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<Cloth>> Update(EditClothViewModel model)
        {
            try
            {
                if(model.Price < 0 || model.Discount > 100)
                {
                    return new BaseResponse<Cloth>()
                    {
                        Data = null,
                        Description = "Неккоректные данные.",
                        StatusCode = StatusCode.IncorrectData
                    };
                }
                
                var cloth = await _clothRepository.Get().FirstOrDefaultAsync(c => c.Id == model.Id);

                if (cloth == null)
                {
                    return new BaseResponse<Cloth>()
                    {
                        Data = null,
                        Description = "Товар не найден.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                cloth.Name = model.Name.ToLower();
                cloth.Price = model.Price;
                cloth.Discount = model.Discount;
                cloth.Description = model.Description;
                cloth.NumberS = model.NumberS;
                cloth.NumberM = model.NumberM;
                cloth.NumberL = model.NumberL;
                cloth.TypeClothId = model.TypeId;
                cloth.IsVisible = model.IsVisible;

                await _clothRepository.Update(cloth);

                return new BaseResponse<Cloth>
                {
                    Data = cloth,
                    Description = "Товар обновлен",
                    StatusCode = StatusCode.OK
                };
            }
            catch(Exception ex)
            {
                return new BaseResponse<Cloth>()
                {
                    Data = null,
                    Description = $"[Update]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}
