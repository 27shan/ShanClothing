using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Entity
{
    public class TypeCloth
    {
        public TypeCloth()
        {
            Clothes = new List<Cloth>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Cloth> Clothes { get; set; }
    }
}
