using ShanClothing.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.ViewModels
{
    public class AdminClothesViewModel
    {
        public List<TypeCloth> TypesClothes { get; set; }

        public List<Cloth> Clothes { get; set; }
    }
}
