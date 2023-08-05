using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.ViewModels
{
    public class UpdatePasswordViewModel
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set;}

        public string ConfirmPassword { get; set;}
    }
}
