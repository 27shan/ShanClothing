using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
    public interface IOrderService
    {
        public Task<BaseResponse<Order>> CreateBasket(Guid userId);

        public Task<BaseResponse<Order>> Get(Guid id);

        public Task<BaseResponse<List<Order>>> GetOrders(SortType sortType, StatusOrder statusOrder, int page, int pageSize);

        public Task<BaseResponse<List<Order>>> GetOrders(Guid orderId);

        public Task<BaseResponse<List<Order>>> GetUserOrders(Guid userId);

        public Task<BaseResponse<Order>> GetOnlyBasket(Guid userId);

        public Task<BaseResponse<Order>> GetBasket(Guid userId);

		public Task<BaseResponse<bool>> AddClothInBasket(Guid userId, int clothId, char size);

		public Task<BaseResponse<bool>> CleanBasket(Guid userId);

        public Task<BaseResponse<bool>> RemoveClothFromBasket(Guid userId, int clothId, char size);

        public Task<BaseResponse<bool>> СhangeNumberClothInBasket(Guid userId, int clothId, char size, int number);

        public Task<BaseResponse<Order>> SaveAddress(AppUser user, DeliveryType deliveryType);

        public Task<BaseResponse<Order>> Confirm(Guid orderId, decimal amountPayment);

        public Task<BaseResponse<Order>> SendTracker(Guid orderId, string tracker);

    }
}
