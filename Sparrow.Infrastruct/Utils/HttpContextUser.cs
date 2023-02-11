using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SparrowPlatform.Infrastruct.Utils
{
    public class HttpContextUser : IUser
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<HttpContextUser> _logger;

        public HttpContextUser(IHttpContextAccessor accessor, ILogger<HttpContextUser> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        public string GetToken()
        {
            return _accessor.HttpContext?.Request?.Headers["Authorization"].ObjToString().Replace("Bearer ", "");
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public string GetName()
        {
            if (IsAuthenticated() && _accessor.HttpContext.User.Identity.Name.IsNotEmptyOrNull())
            {
                return _accessor.HttpContext.User.Identity.Name;
            }
            else
            {
                if (!string.IsNullOrEmpty(GetToken()))
                {
                    return GetUserInfoFromToken("name").FirstOrDefault().ObjToString();
                }
            }

            return "";
        }

        public string GetAADID()
        {
            if (!string.IsNullOrEmpty(GetToken()))
            {
                return GetUserInfoFromToken("sub").FirstOrDefault().ObjToString();
            }

            return "";
        }

        public List<string> GetUserInfoFromToken(string ClaimType)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = "";

            token = GetToken();
            if (token.IsNotEmptyOrNull() && jwtHandler.CanReadToken(token))
            {
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);

                return (from item in jwtToken.Claims
                        where item.Type == ClaimType
                        select item.Value).ToList();
            }

            return new List<string>() { };
        }
    }
}
