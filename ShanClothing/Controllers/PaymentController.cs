using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShanClothing.Domain.Enum;
using ShanClothing.Service.Interfaces;
using ShanClothing.Service.Implementations;
using ShanClothing.Domain.Entity;

namespace ShanClothing.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IAppUserService _appUserService;
        private readonly IOrderService _orderService;
		private readonly IPaymentService _paymentService;

        public PaymentController(IAppUserService appUserService, IOrderService orderService, IPaymentService paymentService)
        {
            _appUserService = appUserService;
            _orderService = orderService;
			_paymentService = paymentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Payment(DeliveryType deliveryType, PaymentType paymentType)
        {
			var responseUser = await _appUserService.GetByName(User.Identity.Name);

			if (responseUser.StatusCode == Domain.Enum.StatusCode.OK)
			{
				var responseOrder = await _orderService.SaveAddress(responseUser.Data, deliveryType);

				if (responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
				{
					switch (paymentType)
					{
						case PaymentType.Card:
							var responsePaymentCard = await _paymentService.CreatePaymentCard("https://localhost:7218/Payment/ConfirmPaymentCard",
								responseOrder.Data.PriceTotal, responseOrder.Data.Id);

                            if (responsePaymentCard.StatusCode == Domain.Enum.StatusCode.OK)
                            {
                                return View("PaymentCard", responsePaymentCard.Data);
                            }
                            return View("Error", $"{responsePaymentCard.Description}");
						break;

						case PaymentType.Ethereum:
							var responsePaymentEth = await _paymentService.CreatePaymentEthereum("https://localhost:7218/Payment/ConfirmPaymentEthereum",
								responseOrder.Data.PriceTotal, responseOrder.Data.Id);

							if(responsePaymentEth.StatusCode == Domain.Enum.StatusCode.OK)
							{
								return View("PaymentEthereum", responsePaymentEth.Data);
							}
							return View("Error", $"{responsePaymentEth.Description}");
						break;
					}
				}
				return View("Error", $"{responseOrder.Description}");
			}
			return View("Error", $"{responseUser.Description}");
        }

		[Authorize]
		public async Task<IActionResult> ConfirmPaymentEthereum()
		{
			var responseUser = await _appUserService.GetByName(User.Identity.Name);

			if(responseUser.StatusCode == Domain.Enum.StatusCode.OK)
			{
				var responseOrder = await _orderService.GetOnlyBasket(responseUser.Data.Id);

				if(responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
				{
					var responsePayment = await _paymentService.CheckPaymentEthereum(responseOrder.Data.Id);

					if(responsePayment.StatusCode == Domain.Enum.StatusCode.OK)
					{
						var responseOrderConfirm = await _orderService.Confirm(responseOrder.Data.Id, responsePayment.Data.Amount);

						if(responseOrderConfirm.StatusCode == Domain.Enum.StatusCode.OK)
						{
							var responseOrderCreate = await _orderService.CreateBasket(responseUser.Data.Id);

							if (responseOrderCreate.StatusCode == Domain.Enum.StatusCode.OK)
							{
								return RedirectToAction("Orders", "Account");
							}
							return View("Error", $"{responseOrder.Description}");
						}
					}
					return View("Error", $"{responsePayment.Description}");
				}
				return View("Error", $"{responseOrder.Description}");
			}
			return View("Error", $"{responseUser.Description}");
		}

		[Authorize]
		public async Task<IActionResult> ConfirmPaymentCard()
		{
            var responseUser = await _appUserService.GetByName(User.Identity.Name);

            if (responseUser.StatusCode == Domain.Enum.StatusCode.OK)
            {
                var responseOrder = await _orderService.GetOnlyBasket(responseUser.Data.Id);

                if (responseOrder.StatusCode == Domain.Enum.StatusCode.OK)
                {
                    var responsePayment = await _paymentService.CheckPaymentCard(responseOrder.Data.Id);

                    if (responsePayment.StatusCode == Domain.Enum.StatusCode.OK)
                    {
                        var responseOrderConfirm = await _orderService.Confirm(responseOrder.Data.Id, responsePayment.Data.Amount);

                        if (responseOrderConfirm.StatusCode == Domain.Enum.StatusCode.OK)
                        {
                            var responseOrderCreate = await _orderService.CreateBasket(responseUser.Data.Id);

                            if (responseOrderCreate.StatusCode == Domain.Enum.StatusCode.OK)
                            {
                                return RedirectToAction("Orders", "Account");
                            }
                            return View("Error", $"{responseOrder.Description}");
                        }
                    }
                    return View("Error", $"{responsePayment.Description}");
                }
                return View("Error", $"{responseOrder.Description}");
            }
            return View("Error", $"{responseUser.Description}");
        }
    }
}
