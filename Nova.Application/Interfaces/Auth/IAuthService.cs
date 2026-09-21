using Nova.Application.Dto.Request;
using Nova.Application.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<ResponseModel< string>> RegisterAsync(
            RegisterRequest request);

        Task<ResponseModel< LoginResponse>> LoginAsync(
            LoginRequest request);
    }


}
