using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Entity
{
    public class Cloth
    {
        public Cloth()
        {
            ImagesCloth = new List<ImageCloth>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Discount { get; set; }

        public string Description { get; set; }

        public int NumberS { get; set; }

        public int NumberM { get; set; }

        public int NumberL { get; set; }

        public DateTime TimeCreation { get; set; }

        public int TypeClothId { get; set; }

        public TypeCloth TypeCloth { get; set; }

        public ICollection<ImageCloth> ImagesCloth { get; set; }

        public bool IsVisible { get; set; }

        public int NumberSold { get; set; }

        public decimal PriceSold { get; set; }
    }
}
