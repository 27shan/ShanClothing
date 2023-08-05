using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.ViewModels
{
    public class EditClothViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Discount { get; set; }

        public string Description { get; set; }

        public int NumberS { get; set; }

        public int NumberM { get; set; }

        public int NumberL { get; set; }

        public int TypeId { get; set; }

        public bool IsVisible { get; set; }

        public int[] ImagesClothId { get; set; }

        public IFormFileCollection? Files { get; set; }
    }
}
