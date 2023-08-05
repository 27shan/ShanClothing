using Microsoft.AspNetCore.Identity;
using ShanClothing.Domain.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public AppUser()
        {
            Orders = new List<Order>();
            IsModerator = false;
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }

        [InverseProperty("AppUser")]
        public ICollection<Order> Orders { get; set; }

        public bool IsModerator { get; set; }
    }
}
