using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Service.Interfaces
{
	public interface IPaymentService
	{
		public Task<BaseResponse<Payment>> CreatePaymentEthereum(string returnUrl, decimal amount, Guid orderId);

		public Task<BaseResponse<Payment>> CheckPaymentEthereum(Guid ordreId);

		public Task<BaseResponse<Payment>> CreatePaymentCard(string returnUrl, decimal amount, Guid orderId);

		public Task<BaseResponse<Payment>> CheckPaymentCard(Guid orderId);

    }
}
