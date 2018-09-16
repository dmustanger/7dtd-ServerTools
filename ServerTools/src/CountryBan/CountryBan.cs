using Geotargeting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ServerTools
{
    class CountryBan
    {
        public static bool IsEnabled = false;
        private static string _assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string _dbPath = string.Format("{0}/GeoLiteCity.dat", _assemblyFolder);
        public static List<string> BannedCountries = new List<string>();
        private static LookupService reader;

        public static void Load ()
        {
            if (File.Exists(_dbPath))
            {
                reader = new LookupService(_dbPath, LookupService.GEOIP_STANDARD);
            }
            else
            {
                IsEnabled = false;
            }
        }

        public static bool IsCountryBanned (ClientInfo _cInfo)
        {
            Location _location = reader.getLocation(_cInfo.ip);
            if (BannedCountries.Contains(_location.countryCode))
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Players from {1} are not allowed\"", _cInfo.playerId, _location.countryCode), (ClientInfo)null);
                return true;
            }
            return false;
        }
    }
}
