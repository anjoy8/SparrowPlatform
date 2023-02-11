using NetTools;
using System.Net;

namespace SparrowPlatform.Gateway.Utils
{
    public class IPHelper
    {
        public static bool IsContainsInRange(string ipRange, string targetIp)
        {
            var rangeA = IPAddressRange.Parse(ipRange);
            return rangeA.Contains(IPAddress.Parse(targetIp));
        }
    }
}
