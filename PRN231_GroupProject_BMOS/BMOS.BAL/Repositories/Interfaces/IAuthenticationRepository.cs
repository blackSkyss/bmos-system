using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.DTOs.Authentications;
using BMOS.BAL.DTOs.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<PostLoginResponse> LoginAsync(PostAccountRequest account, JwtAuth jwtAuth);
        Task<PostRecreateTokenResponse> ReCreateTokenAsync(PostRecreateTokenRequest token, JwtAuth jwtAuth);
    }
}
