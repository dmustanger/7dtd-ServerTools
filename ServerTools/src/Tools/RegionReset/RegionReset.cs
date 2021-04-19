using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class RegionReset
    {
        public static List<string> Regions = new List<string>();

        public static void Exec()
        {
            try
            {
                if (Regions.Count > 0)
                {
                    string _regionDir = GameUtils.GetSaveGameRegionDir();
                    if (Directory.Exists(_regionDir))
                    {
                        string[] _files = Directory.GetFiles(_regionDir, "*.7rg", SearchOption.AllDirectories);
                        if (_files != null && _files.Length > 0)
                        {
                            foreach (var _file in _files)
                            {
                                string _fileName = _file.Remove(0, _file.IndexOf("Region") + 9).Replace(".7rg", "");
                                if (Regions.Contains(_fileName))
                                {
                                    FileInfo _fInfo = new FileInfo(_file);
                                    if (_fInfo != null)
                                    {
                                        _fInfo.Delete();
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset: r.{0}.7rg", _fileName));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Region directory not found. Unable to delete regions from the reset list");
                        return;
                    }

                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionReset.Exec: {0}", e.Message));
            }
        }
    }
}
