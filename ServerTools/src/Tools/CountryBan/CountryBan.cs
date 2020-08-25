using System;
using System.Collections.Generic;
using System.IO;
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
                                        string _line, _cleanLine;
                                        string[] _split;
                                        long _0, _1;
                                        while ((_line = sr.ReadLine()) != null)
                                        {
                                            _cleanLine = _line.Replace("\"", "");
                                            _split = _cleanLine.Split(',');
                                            long.TryParse(_split[0], out _0);
                                            long.TryParse(_split[1], out _1);
                                            BannedCountries.Add(new long[] { _0, _1 }, _split[2]);
                                        }
                                    }
                                }
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
                if (BannedCountries != null && BannedCountries.Count > 0)
                {
                    if (_cInfo != null && !string.IsNullOrEmpty(_cInfo.ip))
                    {
                        string[] _split = _cInfo.ip.Split('.');
                        long.TryParse(_split[0], out long _block1);
                        long.TryParse(_split[1], out long _block2);
                        long.TryParse(_split[2], out long _block3);
                        long.TryParse(_split[3], out long _block4);
                        _block1 = _block1 * 16777216;
                        _block2 = _block2 * 65536;
                        _block3 = _block3 * 256;
                        long _ipv4 = _block1 + _block2 + _block3 + _block4;
                        foreach (var i in BannedCountries)
                        {
                            if (_ipv4 >= i.Key[0] && _ipv4 <= i.Key[1])
                            {
                                if (Countries_Not_Allowed.Contains(i.Value))
                                {
                                    return true;
                                }
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
