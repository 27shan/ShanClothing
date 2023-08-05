using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShanClothing.Domain.ViewModels
{
    public class ResetPasswordTokenViewModel
    {
        public string Email { get; set; }

        public string Token { get; set; }
    }
}
