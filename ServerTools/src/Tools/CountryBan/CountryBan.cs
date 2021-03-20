using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ServerTools
{
    class CountryBan
    {
        public static bool IsEnabled = false;
        public static string Countries_Not_Allowed = "CN,IL";
        public static Dictionary<long[], string> BannedCountries = new Dictionary<long[], string>();

        public static void FileCheck()
        {
            try
            {
                if (Directory.Exists(API.GamePath + "/Mods/ServerTools/"))
                {
                    string[] _txtFiles = Directory.GetFiles(API.GamePath + "/Mods/ServerTools/", "*.txt", SearchOption.AllDirectories);
                    if (_txtFiles != null && _txtFiles.Length > 0)
                    {
                        for (int i = 0; i < _txtFiles.Length; i++)
                        {
                            FileInfo _fileInfo = new FileInfo(_txtFiles[i]);
                            if (_fileInfo != null && _fileInfo.Name.Contains("IP2Location"))
                            {
                                using (FileStream fs = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                                    {
                                        string _line;
                                        string[] _split;
                                        long _0, _1;
                                        while ((_line = sr.ReadLine()) != null)
                                        {
                                            _split = _line.Split(',');
                                            long.TryParse(_split[0], out _0);
                                            long.TryParse(_split[1], out _1);
                                            BannedCountries.Add(new long[] { _0, _1 }, _split[2]);
                                        }
                                    }
                                }
                                return;
                            }
                        }
                    }
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
                if (BannedCountries != null && BannedCountries.Count > 0 && _cInfo != null && !string.IsNullOrEmpty(_cInfo.ip))
                {
                    long _ipInteger = ConvertIPToLong(_cInfo.ip);
                    foreach (var ipRange in BannedCountries)
                    {
                        if (_ipInteger >= ipRange.Key[0] && _ipInteger <= ipRange.Key[1] && Countries_Not_Allowed.Contains(ipRange.Value))
                        {
                            return true;
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

        public static long ConvertIPToLong(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress _ip))
                {
                    byte[] _bytes = _ip.MapToIPv4().GetAddressBytes();
                    return 16777216L * _bytes[0] +
                        65536 * _bytes[1] +
                        256 * _bytes[2] +
                        _bytes[3]
                        ;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CountryBan.ConvertIPToLong: {0}", e.Message));
            }
            return 0;
        }
    }
}
