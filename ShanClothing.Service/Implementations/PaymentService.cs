using ShanClothing.Domain.Entity;
using ShanClothing.Domain.Enum;
using ShanClothing.Domain.Response;
using ShanClothing.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Crypto.Digests;
using ShanClothing.Service.Helpers;
using ShanClothing.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ShanClothing.Service.Implementations
{
	public class PaymentService : IPaymentService
	{
		private readonly IBaseRepository<Payment> _paymentRepository;

		public PaymentService(IBaseRepository<Payment> paymentRepository)
		{
			_paymentRepository = paymentRepository;
		}

		public async Task<BaseResponse<Payment>> CreatePaymentEthereum(string returnUrl, decimal amount, Guid orderId)
		{
			try
			{
				BigInteger amountWei = await EthereumHelper.ConvertRubToWei(amount);

				if(amount <= 0 || amountWei <= BigInteger.Zero)
				{
					return new BaseResponse<Payment>()
					{
						Data = null,
						Description = "Неккоректная сумма.",
						StatusCode = StatusCode.InternalServerError
					};
				}

				var payment = new Payment()
				{
					PaymentType = PaymentType.Ethereum,
					Amount = amount,
					AmountWei = amountWei.ToString(),
					ReturnUrl = returnUrl,
					TimeCreation = DateTime.Now,
					OrderId = orderId
				};

				await _paymentRepository.Create(payment);

				return new BaseResponse<Payment>()
				{
					Data = payment,
					Description = "Платеж создан.",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex)
			{
				return new BaseResponse<Payment>()
				{
					Data = null,
					Description = $"[CreatePaymentEthereum]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<Payment>> CheckPaymentEthereum(Guid orderId)
		{
			try
			{
				var payment = await _paymentRepository.Get()
					.Include(p => p.Order)
					.OrderByDescending(p => p.TimeCreation)
				.FirstOrDefaultAsync(p => p.OrderId == orderId);

				if (payment == null)
				{
					return new BaseResponse<Payment>()
					{
						Data = null,
						Description = "Платеж не найден",
						StatusCode = StatusCode.EntityNotFound,
					};
				}

				if(payment.TimePayment != null)
				{
					return new BaseResponse<Payment>()
					{
						Data = null,
						Description = "Платеж уже подтвержден.",
						StatusCode = StatusCode.EntityExists
					};
				}

				var paymentEthereum = await EthereumHelper.GetPaymentEthereum(orderId.ToString());

				if(paymentEthereum == null)
				{
					return new BaseResponse<Payment>()
					{
						Data = payment,
						Description = "Платеж не прошел.",
						StatusCode = StatusCode.InternalServerError
					};
				}

				BigInteger amountWei = BigInteger.Parse(payment.AmountWei);

				if(!MathHelper.IsApproximatelyEqual(amountWei, paymentEthereum.Amount, (BigInteger)1000) || payment.Amount != payment.Order.PriceTotal)
				{
					return new BaseResponse<Payment>()
					{
						Data = payment,
						Description = "Неверная сумма оплаты.",
						StatusCode = StatusCode.IncorrectAmount
					};
				}

				payment.TimePayment = DateTime.Now;
				await _paymentRepository.Update(payment);

				return new BaseResponse<Payment>()
				{
					Data = payment,
					Description = "Оплата прошла успешна.",
					StatusCode = StatusCode.OK
				};
			}
			catch(Exception ex )
			{
				return new BaseResponse<Payment>()
				{
					Data = null,
					Description = $"[CheckPaymentEthereum]: {ex.Message}",
					StatusCode = StatusCode.InternalServerError
				};
			}
		}

		public async Task<BaseResponse<Payment>> CreatePaymentCard(string returnUrl, decimal amount, Guid orderId)
		{
			try
			{
				if(amount <= 0)
				{
					return new BaseResponse<Payment>()
					{
						Data = null,
						Description = "Неккоректная сумма",
						StatusCode = StatusCode.InternalServerError
					};
				}

				var payment = new Payment()
				{
					PaymentType = PaymentType.Card,
					Amount = amount,
					ReturnUrl = returnUrl,
					TimeCreation = DateTime.Now,
					OrderId = orderId
				};

                await _paymentRepository.Create(payment);

                return new BaseResponse<Payment>()
                {
                    Data = payment,
                    Description = "Платеж создан.",
                    StatusCode = StatusCode.OK
                };
            }
			catch(Exception ex)
			{
                return new BaseResponse<Payment>()
                {
                    Data = null,
                    Description = $"[CreatePaymentCard]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}

		public async Task<BaseResponse<Payment>> CheckPaymentCard(Guid orderId)
		{
			try
			{
                var payment = await _paymentRepository.Get()
                    .OrderByDescending(p => p.TimeCreation)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment == null)
                {
                    return new BaseResponse<Payment>()
                    {
                        Data = null,
                        Description = "Платеж не найден",
                        StatusCode = StatusCode.EntityNotFound,
                    };
                }

                if (payment.TimePayment != null)
                {
                    return new BaseResponse<Payment>()
                    {
                        Data = null,
                        Description = "Платеж уже подтвержден.",
                        StatusCode = StatusCode.EntityExists
                    };
                }

                payment.TimePayment = DateTime.Now;
                await _paymentRepository.Update(payment);

                return new BaseResponse<Payment>()
                {
                    Data = payment,
                    Description = "Оплата прошла успешна.",
                    StatusCode = StatusCode.OK
                };
            }
			catch(Exception ex)
			{
                return new BaseResponse<Payment>()
                {
                    Data = null,
                    Description = $"[CheckPaymentCard]: {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
		}

    }
}
