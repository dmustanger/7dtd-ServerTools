using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class CountryBan
    {
        public static bool IsEnabled = false;
        public static string Countries_Not_Allowed = "CN,IL", FileLocation = "";
        public static Dictionary<long[], string> BannedCountries = new Dictionary<long[], string>();

        public static void BuildList()
        {
            try
            {
                if (FileLocation != "")
                {
                    using (FileStream fs = new FileStream(FileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            string _line;
                            string[] _split;
                            while ((_line = sr.ReadLine()) != null)
                            {
                                _split = _line.Split(',');
                                long.TryParse(_split[0], out long _0);
                                long.TryParse(_split[1], out long _1);
                                BannedCountries.Add(new long[] { _0, _1 }, _split[2]);
                            }
                        }
                    }
                    return;
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Directory not found. Unable to load country ban list"));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBan.FileCheck: {0}", e.Message));
            }
        }

        public static bool IsCountryBanned(ClientInfo _cInfo)
        {
            try
            {
                if (_cInfo.playerId != null && _cInfo.ip != null)
                {
                    if (PersistentContainer.Instance.Players.Players.ContainsKey(_cInfo.playerId) && PersistentContainer.Instance.Players[_cInfo.playerId].CountryBanImmune)
                    {
                        return false;
                    }
                    if (BannedCountries != null && BannedCountries.Count > 0 && _cInfo != null && !string.IsNullOrEmpty(_cInfo.ip))
                    {
                        long _ipInteger = PersistentOperations.ConvertIPToLong(_cInfo.ip);
                        foreach (var ipRange in BannedCountries)
                        {
                            if (_ipInteger >= ipRange.Key[0] && _ipInteger <= ipRange.Key[1] && Countries_Not_Allowed.Contains(ipRange.Value))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBan.IsCountryBanned: {0}", e.Message));
            }
            return false;
        }
    }
}
