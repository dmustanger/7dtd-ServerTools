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
                    string regionDir = GameIO.GetSaveGameRegionDir();
                    if (Directory.Exists(regionDir))
                    {
                        string[] files = Directory.GetFiles(regionDir, "*.7rg", SearchOption.AllDirectories);
                        if (files != null && files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                string fileName = file.Remove(0, file.IndexOf("Region") + 9).Replace(".7rg", "");
                                if (Regions.Contains(fileName))
                                {
                                    FileInfo fInfo = new FileInfo(file);
                                    if (fInfo != null)
                                    {
                                        fInfo.Delete();
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset: r.{0}.7rg", fileName));
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
