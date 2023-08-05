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
    public class Order
    {
		public Order()
		{
			OrderClothes = new List<OrderCloth>();
			Payments = new List<Payment>();
		}

        public Guid Id { get; set; }

        public DateTime TimeCreation { get; set; }

        public StatusOrder Status { get; set; }

        public ICollection<OrderCloth> OrderClothes { get; set; }

        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public decimal PriceTotal { get; set; }

		[ForeignKey("AppUserId")]
		public Guid AppUserId { get; set; }

		public AppUser AppUser { get; set; }

		public DeliveryType DeliveryType { get; set; }

		public string? Tracker { get; set; }

        public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
		public string? Postcode { get; set; }

		public ICollection<Payment> Payments { get; set; }
	}
}
