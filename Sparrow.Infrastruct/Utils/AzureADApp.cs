using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SparrowPlatform.Infrastruct.Utils
{
    public class AzureADApp
    {
        public static bool isExpiredToken(string ClaimType = "exp")
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = Permissions.AAD_TOKEN;
            if (!string.IsNullOrEmpty(token) && jwtHandler.CanReadToken(token))
            {
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);

                var expLong = long.Parse((from item in jwtToken.Claims
                                          where item.Type == ClaimType
                                          select item.Value).FirstOrDefault());

                var expDate = DateTimeOffset.FromUnixTimeSeconds(expLong).LocalDateTime;

                return expDate <= DateTime.Now;
            }

            return true;
        }

        public static string GetToken()
        {
            if (!isExpiredToken())
            {
                return Permissions.AAD_TOKEN;
            }

            var client = new RestClient($"{AzureADAppSetup.loginDomain}/{AzureADAppSetup.application}/oauth2/v2.0/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", AzureADAppTokenSetup.grantType);
            request.AddParameter("client_id", AzureADAppTokenSetup.clientId);
            request.AddParameter("client_secret", AzureADAppTokenSetup.clientSecret);
            request.AddParameter("scope", AzureADAppTokenSetup.scope);
            IRestResponse response = client.Execute(request);

            Console.WriteLine(AzureADAppTokenSetup.scope);

            var content = response.Content;

            var j = JObject.Parse(content);

            var token = j.Value<string>("access_token");
            if (!string.IsNullOrEmpty(token))
            {
                Permissions.AAD_TOKEN = token;
                return token;
            }

            return "";
        }

        public static AADResponse AddUserByToken(ExpandoObject azureUserDto)
        {
            AADResponse aadResponse = new AADResponse();
            var client = new RestClient($"{AzureADAppSetup.domain}/v1.0/users");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {GetToken()}");
            request.AddParameter("application/json", JsonConvert.SerializeObject(azureUserDto), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            var content = response.Content;

            var j = JObject.Parse(content);
            Console.WriteLine(content);
            if (!string.IsNullOrEmpty(j.Value<string>("id")))
            {
                aadResponse.suc = true;
                aadResponse.id = j.Value<string>("id");
            }
            else
            {
                aadResponse.msg = AAD_SET.Debug ? (j["error"]["message"])?.ToString() : "error";
            }
            return aadResponse;
        }

        public static AADResponse UpdateUserByToken(string azureUserDtoJson, string aadId = "")
        {
            AADResponse aadResponse = new AADResponse();
            if (string.IsNullOrEmpty(aadId))
            {
                return aadResponse;
            }

            var client = new RestClient($"{AzureADAppSetup.domain}/v1.0/users/{aadId}");
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {GetToken()}");
            request.AddParameter("application/json", azureUserDtoJson, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            var content = response.Content;
            Console.WriteLine("update suc:" + content);

            if (!string.IsNullOrEmpty(content ?? ""))
            {
                var j = JObject.Parse(content);
                aadResponse.msg = AAD_SET.Debug ? (j["error"]["message"])?.ToString() : "error";
            }
            else
            {
                aadResponse.suc = true;
            }
            return aadResponse;
        }

        public static bool DeleteUserByToken(string aadId = "")
        {
            if (string.IsNullOrEmpty(aadId))
            {
                return false;
            }

            var client = new RestClient($"{AzureADAppSetup.domain}/v1.0/users/{aadId}");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {GetToken()}");

            IRestResponse response = client.Execute(request);

            var content = response.Content;
            Console.WriteLine("delete suc:" + content);

            return string.IsNullOrEmpty(content ?? "");
        }
    }

    public class AADResponse
    {
        public bool suc { get; set; } = false;
        public string id { get; set; } = "";
        public string msg { get; set; }
    }

    public class AzureUserDto
    {
        public bool accountEnabled { get; set; }
        public string displayName { get; set; }
        public string mailNickname { get; set; }
        public string userPrincipalName { get; set; }
        public Passwordprofile passwordProfile { get; set; }
    }

    public class Passwordprofile
    {
        public bool forceChangePasswordNextSignIn { get; set; }
        public string password { get; set; }
    }

    public class AzureUserDtoEmail
    {
        public string displayName { get; set; }
        public List<Identity> identities { get; set; }
        public Passwordprofile passwordProfile { get; set; }
        public string passwordPolicies { get; set; }
    }

    public class Identity
    {
        public string signInType { get; set; }
        public string issuer { get; set; }
        public string issuerAssignedId { get; set; }
    }

}
