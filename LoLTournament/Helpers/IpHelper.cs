using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace LoLTournament.Helpers
{
    public class IpHelper
    {
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
            var whitelist = new List<string> {"131.174.*.*", "127.0.0.1", "::1"}; // IPv4 /24 subnet reserved by C&CZ for the CognAC League of Legends Tournament

            return whitelist.Any(x => Match(x, ip));
        }

        private static bool Match(string pattern, string ipAddr)
        {
            byte[] subnetMask = IPAddress.Parse(string.Join(".", pattern.Split('.').Select(s => s == "*" ? "0" : "255"))).GetAddressBytes();
            byte[] patternIp = IPAddress.Parse(pattern.Replace('*', '0')).GetAddressBytes();
            byte[] ip = IPAddress.Parse(ipAddr).GetAddressBytes();

            return subnetMask.Where((t, i) => (t & patternIp[i]) != (t & ip[i])).Any() == false;
        }
    }
}
