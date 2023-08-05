using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ShanClothing.Domain.Entity
{
	public class OrderCloth
	{
        public Guid Id { get; set; }

		public Guid OrderId { get; set; }

		public Order Order { get; set; }

		public int ClothId { get; set; }

		public Cloth Cloth { get; set; }

		public char Size { get; set; }

		public int Number { get; set; }
	}
}
