using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Entity
{
    public class ImageCloth
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string RelativePath { get; set; }

        public int ClothId { get; set; }

        public Cloth Cloth { get; set; }
    }
}
