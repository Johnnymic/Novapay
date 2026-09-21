using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Dto.Response
{

    public class LoginResponse
    {
        public string AccessToken { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }

        
    }
}
