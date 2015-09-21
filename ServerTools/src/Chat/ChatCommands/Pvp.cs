using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class Pvp
    {
        public static bool IsEnabled = false;
        private static string _datafile = "PvpData.xml";
        private static string _datafilepath = string.Format("{0}/{1}", Config._datapath, _datafile);
        private static SortedDictionary<string, string> Locations = new SortedDictionary<string, string>();

        public static void Init()
        {
            if (IsEnabled)
            {
                LoadLocations();
            }
        }

        private static void LoadLocations()
        {

        }
    }
}