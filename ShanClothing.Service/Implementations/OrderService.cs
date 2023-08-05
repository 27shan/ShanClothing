using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShanClothing.DAL.Interfaces;
using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using ShanClothing.Service.Helpers;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IBaseRepository<Order> _orderRepository;

        private readonly IBaseRepository<OrderCloth> _orderClothRepository;

		private readonly IBaseRepository<Cloth> _clothRepository;

		private readonly SmtpClient _smtpClient;

        public OrderService(IBaseRepository<Order> orderRepository, IBaseRepository<OrderCloth> orderClothRepository,
			IBaseRepository<Cloth> clothRepository, SmtpClient smtpClient)
        {
            _orderRepository = orderRepository;
            _orderClothRepository = orderClothRepository;
			_clothRepository = clothRepository;
			_smtpClient = smtpClient;
        }

        public async Task<BaseResponse<Order>> CreateBasket(Guid userId)
        {
            try
            {
                var order = new Order()
                {
                    TimeCreation = DateTime.Now,
                    Status = Domain.Enum.StatusOrder.Basket,
                    Price = 0,
                    Discount = 0,
                    PriceTotal = 0,
                    AppUserId = userId,
                };

                await _orderRepository.Create(order);

                return new BaseResponse<Order>()
                {
                    Data = order,
					Description = "Корзина создана",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Order>()
                {
					Data = null,
                    Description = $"[Create]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<Order>> Get(Guid id)
        {
            try
            {
                var order = await _orderRepository.Get()
                    .Include(o => o.OrderClothes)
                        .ThenInclude(oc => oc.Cloth)
                            .ThenInclude(c => c.ImagesCloth)
					.Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if(order == null)
                {
                    return new BaseResponse<Order>()
                    {
						Data = null,
                        Description = "Заказ не найден.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                return new BaseResponse<Order>()
                {
                    Data = order,
					Description = "Заказ получен",
                    StatusCode = Domain.Enum.StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Order>()
                {
					Data = null,
                    Description = $"[GetOrder]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

		public async Task<BaseResponse<List<Order>>> GetOrders(SortType sortType, StatusOrder statusOrder, int page, int pageSize)
		{
			try
			{
				IQueryable<Order> query = _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.ThenInclude(oc => oc.Cloth)
					.ThenInclude(c => c.ImagesCloth)
					.Where(o => o.Status == statusOrder);

				switch(sortType)
				{
                    case SortType.DateAscending:
						query = query.OrderBy(o => o.TimeCreation);
							
                    break;

                    case SortType.DateDescending:
						query = query.OrderByDescending(o => o.TimeCreation);
              
                    break;

                    case SortType.PriceAscending:
						query = query.OrderBy(o => o.PriceTotal);
                            
                        break;

                    case SortType.PriceDescending:
						query = query.OrderByDescending(o => o.PriceTotal);
                        break;
                }

                query = query.Skip((page - 1) * pageSize).Take(pageSize);

				var orders = await query.ToListAsync();

                if (!orders.Any())
				{
					return new BaseResponse<List<Order>>()
					{
						Data = orders,
						Description = "Заказы не найдены.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<List<Order>>()
				{
					Data = orders,
					Description = "Заказы получены",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex)
			{
                return new BaseResponse<List<Order>>()
                {
                    Data = null,
                    Description = $"[GetOrders]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}

        public async Task<BaseResponse<List<Order>>> GetOrders(Guid orderId)
        {
            try
            {
                var orders = new List<Order>();

                orders = await _orderRepository.Get()
                    .Include(o => o.OrderClothes)
                    .ThenInclude(oc => oc.Cloth)
                    .ThenInclude(c => c.ImagesCloth)
                    .Where(o => o.Id == orderId)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return new BaseResponse<List<Order>>()
                    {
                        Data = orders,
                        Description = "Заказы не найдены.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                return new BaseResponse<List<Order>>
                {
                    Data = orders,
                    Description = "Заказы получены",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<Order>>()
                {
                    Data = null,
                    Description = $"[GetOrders]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<List<Order>>> GetUserOrders(Guid userId)
		{
			try
			{
				var orders = new List<Order>();
					
				orders = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.ThenInclude(oc => oc.Cloth)
					.ThenInclude(c => c.ImagesCloth)
					.Where(o => o.AppUserId == userId)
                    .OrderByDescending(o => o.TimeCreation)
                    .ToListAsync();

                if (!orders.Any())
				{
					return new BaseResponse<List<Order>>()
					{
						Data = orders,
						Description = "Заказы не найдены.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<List<Order>>
				{
					Data = orders,
					Description = "Заказы получены",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex) 
			{
				return new BaseResponse<List<Order>>()
				{
					Data = null,
					Description = $"[GetUserOrders]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<Order>> GetOnlyBasket(Guid userId)
		{
			try
			{
				var order = await _orderRepository.Get().FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<Order>()
					{
						Data = null,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				return new BaseResponse<Order>
				{
					Data = order,
					Description = "Корзина получена.",
					StatusCode = StatusCode.OK,
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<Order>()
				{
					Data = null,
					Description = $"[GetOnlyBasket]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

        public async Task<BaseResponse<Order>> GetBasket(Guid userId)
        {
            try
            {
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
						.ThenInclude(oc => oc.Cloth)
							.ThenInclude(c => c.ImagesCloth)
					.FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
                {
                    return new BaseResponse<Order>()
                    {
                        Data = null,
                        Description = "Корзина не найдена.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }
                
                decimal price = 0;
                decimal discount = 0;
				bool isOutOfStock = false;
				foreach (var oc in order.OrderClothes)
                {
                    switch(oc.Size)
                    {
                        case 's':
                            if (oc.Cloth.NumberS < 1)
                            {
								await _orderClothRepository.Delete(oc);
                                isOutOfStock = true;
							}
                            else if(oc.Number > oc.Cloth.NumberS)
                            {
                                oc.Number = oc.Cloth.NumberS;
                                await _orderClothRepository.Update(oc);
                            }
                            else if(oc.Number < 1)
                            {
                                oc.Number = 1;
                                await _orderClothRepository.Update(oc);
                            }
                            break;

                        case 'm':
							if (oc.Cloth.NumberM < 1)
							{
								await _orderClothRepository.Delete(oc);
								isOutOfStock = true;
							}
							else if (oc.Number > oc.Cloth.NumberM)
							{
								oc.Number = oc.Cloth.NumberM;
								await _orderClothRepository.Update(oc);
							}
							else if (oc.Number < 1)
							{
								oc.Number = 1;
								await _orderClothRepository.Update(oc);
							}
							break;

                        case 'l':
							if (oc.Cloth.NumberL < 1)
							{
								await _orderClothRepository.Delete(oc);
								isOutOfStock = true;
							}
							else if (oc.Number > oc.Cloth.NumberL)
							{
								oc.Number = oc.Cloth.NumberL;
								await _orderClothRepository.Update(oc);
							}
							else if (oc.Number < 1)
							{
								oc.Number = 1;
								await _orderClothRepository.Update(oc);
							}
							break;
                    }

                    if(!isOutOfStock)
                    {
						price += oc.Cloth.Price * oc.Number;
						discount += oc.Cloth.Price * (oc.Cloth.Discount / 100M) * oc.Number;
					}
                }

                if(isOutOfStock)
                {
					return new BaseResponse<Order>
					{
						Data = null,
						Description = "Товар закончился на складе.",
						StatusCode = StatusCode.EntityNotFound,
					};
				}

                order.Price = price;
                order.Discount = discount;
                order.PriceTotal = price - discount;
                await _orderRepository.Update(order);

                return new BaseResponse<Order>
                {
                    Data = order,
                    Description = "Корзина получена.",
                    StatusCode = StatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Order>()
                {
                    Data = null,
                    Description = $"[GetBasket]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

		public async Task<BaseResponse<bool>> AddClothInBasket(Guid userId, int clothId, char size)
		{
			try
			{
				if(size != 's' && size != 'm' && size != 'l')
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Некорректные  данные.",
						StatusCode = StatusCode.IncorrectData
					};
				}

				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				foreach (var oc in order.OrderClothes)
				{
					if (oc.ClothId == clothId && oc.Size == size)
					{
						oc.Number++;
						await _orderClothRepository.Update(oc);
						return new BaseResponse<bool>()
						{
							Data = true,
							Description = "Товар добавлен в корзину.",
							StatusCode = StatusCode.OK,
						};
					}
				}

				var orderCloth = new OrderCloth()
				{
					OrderId = order.Id,
					ClothId = clothId,
					Size = size,
					Number = 1
				};

				await _orderClothRepository.Create(orderCloth);

				return new BaseResponse<bool>()
				{
					Data = true,
					Description = "Товар добавлен в корзину.",
					StatusCode = StatusCode.OK,
				};
			}
			catch (Exception ex)
			{
				return new BaseResponse<bool>()
				{
					Data = false,
					Description = $"[AddCloth]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<bool>> CleanBasket(Guid userId)
		{
			try
            {
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<bool>()
					{
                        Data = false,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				foreach (var oc in order.OrderClothes)
                {
                    await _orderClothRepository.Delete(oc);
                }

				return new BaseResponse<bool>()
				{
					Data = true,
                    Description = "Корзина очищена",
                    StatusCode = StatusCode.OK
				};
			}
            catch(Exception ex)
            {
				return new BaseResponse<bool>()
				{
                    Data = false,
					Description = $"[CleanBasket]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

        public async Task<BaseResponse<bool>> RemoveClothFromBasket(Guid userId, int clothId, char size)
        {
            try
            {
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

                OrderCloth? orderCloth = null;

                foreach(var oc in order.OrderClothes)
                {
                    if(oc.ClothId == clothId && oc.Size == size)
                    {
                        orderCloth = oc;
                        break;
                    }
                }

                if(orderCloth == null)
                {
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Товар не найден.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				await _orderClothRepository.Delete(orderCloth);

				return new BaseResponse<bool>()
				{
					Data = true,
					Description = "Товар удален из корзины.",
					StatusCode = StatusCode.OK
				};
			}
            catch(Exception ex)
            {
				return new BaseResponse<bool>()
				{
                    Data = false,
					Description = $"[RemoveClothFromBasket]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
        }

		public async Task<BaseResponse<bool>> СhangeNumberClothInBasket(Guid userId, int clothId, char size, int number)
		{
			try
            {
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.FirstOrDefaultAsync(o => o.AppUserId == userId && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				OrderCloth? orderCloth = null;
				
				foreach (var oc in order.OrderClothes)
				{
					if (oc.ClothId == clothId && oc.Size == size)
					{
						orderCloth = oc;
						break;
					}
				}

				if (orderCloth == null)
				{
					return new BaseResponse<bool>()
					{
						Data = false,
						Description = "Товар не найден.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				orderCloth.Number += number;

				if(orderCloth.Number < 1)
					orderCloth.Number = 1;

                await _orderClothRepository.Update(orderCloth);

				return new BaseResponse<bool>()
				{
					Data = true,
					Description = "Количество товара изменено.",
					StatusCode = StatusCode.OK
				};
			}
            catch(Exception ex)
            {
				return new BaseResponse<bool>()
				{
					Data = false,
					Description = $"[ChangeClothFromBasket]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<Order>> SaveAddress(AppUser user, DeliveryType deliveryType)
        {
            try
            {
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
						.ThenInclude(oc => oc.Cloth)
					.FirstOrDefaultAsync(o => o.AppUserId == user.Id && o.Status == StatusOrder.Basket);

				if (order == null)
				{
					return new BaseResponse<Order>()
					{
						Data = null,
						Description = "Корзина не найдена.",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				decimal price = 0;
				decimal discount = 0;
				bool isOutOfStock = false;

				foreach (var oc in order.OrderClothes)
				{
					switch (oc.Size)
					{
						case 's':
							if (oc.Cloth.NumberS < 1)
							{
								await _orderClothRepository.Delete(oc);
								isOutOfStock = true;
							}
							else if (oc.Number > oc.Cloth.NumberS)
							{
								oc.Number = oc.Cloth.NumberS;
								await _orderClothRepository.Update(oc);
							}
							else if (oc.Number < 1)
							{
								oc.Number = 1;
								await _orderClothRepository.Update(oc);
							}
							break;

						case 'm':
							if (oc.Cloth.NumberM < 1)
							{
								await _orderClothRepository.Delete(oc);
								isOutOfStock = true;
							}
							else if (oc.Number > oc.Cloth.NumberM)
							{
								oc.Number = oc.Cloth.NumberM;
								await _orderClothRepository.Update(oc);
							}
							else if (oc.Number < 1)
							{
								oc.Number = 1;
								await _orderClothRepository.Update(oc);
							}
							break;

						case 'l':
							if (oc.Cloth.NumberL < 1)
							{
								await _orderClothRepository.Delete(oc);
								isOutOfStock = true;
							}
							else if (oc.Number > oc.Cloth.NumberL)
							{
								oc.Number = oc.Cloth.NumberL;
								await _orderClothRepository.Update(oc);
							}
							else if (oc.Number < 1)
							{
								oc.Number = 1;
								await _orderClothRepository.Update(oc);
							}
							break;
					}

					if (!isOutOfStock)
					{
						price += oc.Cloth.Price * oc.Number;
						discount += oc.Cloth.Price * (oc.Cloth.Discount / 100M) * oc.Number;
					}
				}

				if (isOutOfStock)
				{
					return new BaseResponse<Order>
					{
						Data = null,
						Description = "Товар закончился на складе.",
						StatusCode = StatusCode.EntityNotFound,
					};
				}

				order.Price = price;
				order.Discount = discount;
				order.PriceTotal = price - discount;

				order.DeliveryType = deliveryType;

				if(order.DeliveryType == DeliveryType.PickupInStore)
				{
					order.Email = user.Email;
				}
				else if(deliveryType == DeliveryType.Post)
				{
					if(user.FirstName == null || user.LastName == null || user.PhoneNumber == null
						|| user.Address == null || user.Postcode == null)
					{
						return new BaseResponse<Order>
						{
							Data = order,
							Description = "Платежный адрес не найден.",
							StatusCode = StatusCode.EntityNotFound,
						};
					}
					order.FirstName = user.FirstName;
					order.LastName = user.LastName;
					order.Email = user.Email;
					order.PhoneNumber = user.PhoneNumber;
					order.Address = user.Address;
					order.Postcode = user.Postcode;
				}

				await _orderRepository.Update(order);

				return new BaseResponse<Order>
				{
					Data = order,
					Description = "Платежный адрес сохранен.",
					StatusCode = StatusCode.OK,
				};
			}
            catch(Exception ex)
            {
				return new BaseResponse<Order>()
				{
					Data = null,
					Description = $"[SaveAddress]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
        }

		public async Task<BaseResponse<Order>> Confirm(Guid orderId, decimal amountPayment)
		{
			try
			{
				var order = await _orderRepository.Get()
					.Include(o => o.OrderClothes)
					.ThenInclude(oc => oc.Cloth)
					.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return new BaseResponse<Order>()
                    {
                        Data = null,
                        Description = "Заказ не найден.",
                        StatusCode = StatusCode.EntityNotFound
                    };
                }

                if (order.Status != StatusOrder.Basket)
				{
					return new BaseResponse<Order>()
					{
						Data = order,
						Description = "Заказ уже подтвержден.",
						StatusCode = StatusCode.EntityExists
					};
				}

				if(order.PriceTotal != amountPayment)
				{
                    return new BaseResponse<Order>()
                    {
                        Data = order,
                        Description = "Неккоректная сумма платежа.",
                        StatusCode = StatusCode.IncorrectAmount
                    };
                }

				foreach(var oc in order.OrderClothes)
				{
					switch(oc.Size)
					{
						case 's':
							if(oc.Cloth.NumberS > oc.Number)
							{
								oc.Cloth.NumberS -= oc.Number;
							}
							else
							{
								oc.Cloth.NumberS = 0;
							}
						break;
						case 'm':
							if (oc.Cloth.NumberM > oc.Number)
							{
								oc.Cloth.NumberM -= oc.Number;
							}
							else
							{
								oc.Cloth.NumberM = 0;
							}
						break;
						case 'l':
							if (oc.Cloth.NumberL > oc.Number)
							{
								oc.Cloth.NumberL -= oc.Number;
							}
							else
							{
								oc.Cloth.NumberL = 0;
							}
						break;
					}

					if (oc.Cloth.NumberS == 0 && oc.Cloth.NumberM == 0 && oc.Cloth.NumberL == 0)
						oc.Cloth.IsVisible = false;

                    oc.Cloth.NumberSold += oc.Number;
                    oc.Cloth.PriceSold += oc.Number * (oc.Cloth.Price - oc.Cloth.Discount / 100 * oc.Cloth.Price);

                    await _clothRepository.Update(oc.Cloth);
                }

				order.Status = StatusOrder.Paid;
				order.TimeCreation = DateTime.Now;

				await _orderRepository.Update(order);

				return new BaseResponse<Order>()
				{
					Data = order,
					Description = "Заказ подтвержден.",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex)
			{
				return new BaseResponse<Order>()
				{
					Data = null,
					Description = $"[Confirm]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
                };
            }
        }

		public async Task<BaseResponse<Order>> SendTracker(Guid orderId, string tracker)
		{
			try
			{
				var order = await _orderRepository.Get().FirstOrDefaultAsync(o => o.Id == orderId);

				if(order == null)
				{
					return new BaseResponse<Order>()
					{
						Data = null,
						Description = "Заказ не найден",
						StatusCode = StatusCode.EntityNotFound
					};
				}

				if(order.Status != StatusOrder.Paid)
				{
					return new BaseResponse<Order>()
					{
						Data = null,
						Description = "Заказ не оплачен или уже отправлен.",
						StatusCode = StatusCode.EntityExists
					};
				}
				
				if(order.DeliveryType == DeliveryType.PickupInStore)
				{
                    order.Status = StatusOrder.Sent;
                    await _orderRepository.Update(order);

                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("shanclothing@outlook.com");
                    mailMessage.To.Add(new MailAddress(order.Email));
                    mailMessage.Subject = "Заказ SHAN CLOTHING";
                    mailMessage.Body = $"Ваш заказ №{order.Id} на сумму {order.PriceTotal}₽ готов к получению.";
                    mailMessage.IsBodyHtml = true;

                    await _smtpClient.SendMailAsync(mailMessage);
                }
				else if (order.DeliveryType == DeliveryType.Post)
				{
                    order.Tracker = tracker;
                    order.Status = StatusOrder.Sent;
                    await _orderRepository.Update(order);

                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("shanclothing@outlook.com");
                    mailMessage.To.Add(new MailAddress(order.Email));
                    mailMessage.Subject = "Заказ SHAN CLOTHING";
                    mailMessage.Body = $"Ваш заказ №{order.Id} на сумму {order.PriceTotal}₽ был отправлен, вы можете отслеживать его по трек-номеру: {order.Tracker}.";
                    mailMessage.IsBodyHtml = true;

                    await _smtpClient.SendMailAsync(mailMessage);
				}

				return new BaseResponse<Order>
				{
					Data = order,
					Description = "Заказ отправлен",
					StatusCode = StatusCode.OK
				};
			}
			catch (Exception ex)
			{
                return new BaseResponse<Order>()
                {
                    Data = null,
                    Description = $"[SendTracker]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}
    }
}
