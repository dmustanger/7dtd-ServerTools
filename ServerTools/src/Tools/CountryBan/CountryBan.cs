using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace ServerTools
{
    class CountryBan
    {
        public static bool IsEnabled = false;
        private static string _assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string _dbPath = string.Format("{0}/GeoLite2-Country.mmdb", _assemblyFolder);
        public static List<string> BannedCountries = new List<string>();

        //public static bool IsCountryBanned (ClientInfo _cInfo)
        //{
        //    if (_cInfo != null)
        //    {
        //        if (!_cInfo.ip.StartsWith("192.168"))
        //        {
        //            if (BannedCountries.Contains(Get(_cInfo.ip)))
        //            {
        //                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Players from your country are not allowed\"", _cInfo.playerId), (ClientInfo)null);
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        
        //public static string Get(string _ip)
        //{
        //    if (File.Exists(_dbPath))
        //    {
        //        using (var _reader = new Reader(_dbPath))
        //        {
        //            var ip = IPAddress.Parse(_ip);
        //            var data = _reader.Find<Dictionary<string, object>>(ip);
        //            Log.Out("[SERVERTOOLS]" + data.Keys.ToString());
        //
        //            return "Unknown";
        //        }
        //    }
        //    return "Unknown";
        //}
    }
}
