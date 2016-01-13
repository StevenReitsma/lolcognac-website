using System.Net;
using System.Web;
using System.Web.Configuration;

namespace LoLTournament.Helpers
{
    public class IpHelper
    {
        private static readonly IPNetwork EsportsNetwork = IPNetwork.Parse(WebConfigurationManager.AppSettings["EsportsNetwork"]);
        private static readonly IPNetwork EduroamNetwork = IPNetwork.Parse(WebConfigurationManager.AppSettings["EduroamNetwork"]);
        private static readonly IPNetwork LocalhostNetwork = IPNetwork.Parse("127.0.0.1");
        private static readonly IPNetwork LocalhostNetworkIPv6 = IPNetwork.Parse("::1");

        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress)) return context.Request.ServerVariables["REMOTE_ADDR"];

            var addresses = ipAddress.Split(',');
            if (addresses.Length != 0)
            {
                return addresses[0];
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static bool IsAllowed(HttpContext context)
        {
            var ip = GetIpAddress(context);

            var userIsOnEsportsNetwork = IPNetwork.Contains(EsportsNetwork, IPAddress.Parse(ip));
            var userIsOnEduroamNetwork = IPNetwork.Contains(EduroamNetwork, IPAddress.Parse(ip));
            var userIsLocalhost = IPNetwork.Contains(LocalhostNetwork, IPAddress.Parse(ip));
            var userIsLocalhostIPv6 = IPNetwork.Contains(LocalhostNetworkIPv6, IPAddress.Parse(ip));

            return userIsOnEsportsNetwork || userIsOnEduroamNetwork || userIsLocalhost || userIsLocalhostIPv6;
        }

        public static bool IsOnEduroam(HttpContext context)
        {
            var ip = GetIpAddress(context);

            return IPNetwork.Contains(EduroamNetwork, IPAddress.Parse(ip));
        }
    }
}
