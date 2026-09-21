using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Application.Common
{
    public static class CommonUtils
    {

        public static string GenerateHash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

       
        public static DateOnly GetNigeriaBusinessDate()
        {
            var timeZoneId = OperatingSystem.IsWindows()
                ? "W. Central Africa Standard Time"
                : "Africa/Lagos";

            var nigeriaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var nigeriaNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nigeriaTimeZone);

            return DateOnly.FromDateTime(nigeriaNow);
        }
    }
}
