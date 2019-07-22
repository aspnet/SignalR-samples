using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationSample.Models
{
    public class RegisterModel
    {
        [Required, MinLength(1), MaxLength(15)]
        public string UserName { get; set; }
        [Required, MinLength(6), MaxLength(35)]
        public string EmailAddress { get; set; }
        [Required, MinLength(6), MaxLength(15)]
        public string Password { get; set; }
        [Required, MinLength(6), MaxLength(15)]
        public string ConfirmPassword { get; set; }
    }
}
