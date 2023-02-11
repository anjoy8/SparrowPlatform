using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowPlatform.Infrastruct.Utils
{
    public class AzureAdB2CSetup
    {
        public static string Instance { get; set; }
        public static string ClientId { get; set; }
        public static string Domain { get; set; }
        public static string SignUpSignInPolicyId { get; set; }
    }
    public class AzureADAppSetup
    {
        public static string loginDomain { get; set; }
        public static string domain { get; set; }
        public static string application { get; set; }
        public static string connectionString { get; set; }
        public static string blobFileDownloadConnectionString { get; set; }
        public static string b2cExtensionsApplicationClientID { get; set; }
        public static string blobAccountName { get; set; }

    }
    public class AzureADAppTokenSetup
    {
        public static string grantType { get; set; }
        public static string clientId { get; set; }
        public static string clientSecret { get; set; }
        public static string scope { get; set; }

    }
}
