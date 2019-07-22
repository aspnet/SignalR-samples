using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationSample.Models
{
    public class JWTModel
    {
        public string UserName { get; set; }
        public DateTime Expiration { get; set; }
        public string AccessToken { get; set; }
    }
}
