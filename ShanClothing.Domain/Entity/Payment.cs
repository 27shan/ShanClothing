using ShanClothing.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Entity
{
	public class Payment
	{
        public Guid Id { get; set; }

		public PaymentType PaymentType { get; set; }

		public decimal Amount { get; set; }

		public string? AmountWei { get; set; }

		public string ReturnUrl { get; set; }

		public DateTime TimeCreation { get; set; }

		public DateTime? TimePayment { get; set;}
		
		public Guid OrderId { get; set; }

		public Order Order { get; set; }

		public string? Message { get; set; }
	}
}
