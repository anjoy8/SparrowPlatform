using Microsoft.AspNetCore.Authorization;

namespace SparrowPlatform.API.Extensions
{
    /// <summary>
    /// ClaimRequirement.
    /// </summary>
    public class ClaimRequirement : IAuthorizationRequirement
    {
        public ClaimRequirement(string claimName, string claimValue)
        {
            ClaimName = claimName;
            ClaimValue = claimValue;
        }

        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
    }
}
