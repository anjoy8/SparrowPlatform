using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SparrowPlatform.Gateway.Controllers
{
    /// <summary>
    /// common manager
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommonController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CommonController(IConfiguration configuration )
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Get all role information.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public dynamic swgLogin([FromBody] SwaggerLoginRequest loginRequest)
        {
            var pwd = _configuration.GetValue<string>("GATEWAY-SWAGGER-PASSWORD");
            if (!string.IsNullOrEmpty(pwd) && loginRequest?.name == "admin" && loginRequest?.pwd == pwd)
            {
                HttpContext.Session.SetString("swagger-code", "success");
                return new { result = true };
            }

            return new { result = false };
        }

    }

    public class SwaggerLoginRequest {
        public string name { get; set; }
        public string pwd { get; set; }
    }
}
