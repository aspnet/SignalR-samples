using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationSample.Models
{
    public class LoginModel
    {
        [Required, MinLength(1), MaxLength(15)]
        public string UserName { get; set; }
        [Required, MinLength(6), MaxLength(15)]
        public string Password { get; set; }
    }
}
