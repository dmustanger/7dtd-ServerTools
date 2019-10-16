using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerTools
{
    class ViewDistances
    {
        public static bool IsEnabled = false;
        public static int Tree_Distance = 3, View_Distance = 10, Field_Of_View = 70;

        public static void MaxTreeDistance(ClientInfo _cInfo)
        {
            if (Tree_Distance < 3)
            {
                Tree_Distance = 3;
            }
            if (Tree_Distance > 7)
            {
                Tree_Distance = 7;
            }
            int _treeDistance = PersistentContainer.Instance.Players[_cInfo.playerId].TreeDistance;
            if (_treeDistance < 3)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].TreeDistance = 3;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsTreeDistance {0}", 3), true));
            }
            else if (_treeDistance > 7)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].TreeDistance = 7;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsTreeDistance {0}", 7), true));
            }
            else
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsTreeDistance {0}", _treeDistance), true));
            }
        }

        public static void MaxViewDistance(ClientInfo _cInfo)
        {
            if (View_Distance < 3)
            {
                View_Distance = 3;
            }
            if (View_Distance > 10)
            {
                View_Distance = 10;
            }
            int _viewDistance = PersistentContainer.Instance.Players[_cInfo.playerId].ViewDistance;
            if (_viewDistance < 3)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].ViewDistance = 3;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsViewDistance {0}", 3), true));
            }
            else if (_viewDistance > 10)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].ViewDistance = 10;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsViewDistance {0}", 10), true));
            }
            else
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg OptionsViewDistance {0}", _viewDistance), true));
            }
        }

        public static void FieldOfView(ClientInfo _cInfo)
        {
            if (Field_Of_View < 70)
            {
                Field_Of_View = 70;
            }
            if (Field_Of_View > 100)
            {
                Field_Of_View = 100;
            }
            int _fieldOfView = PersistentContainer.Instance.Players[_cInfo.playerId].FieldOfView;
            if (_fieldOfView < 70)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].FieldOfView = 70;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg FieldOfView {0}", 70), true));
            }
            else if (_fieldOfView > 100)
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].FieldOfView = 100;
                PersistentContainer.Instance.Save();
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg FieldOfView {0}", 100), true));
            }
            else
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format("sg FieldOfView {0}", _fieldOfView), true));
            }
        }
    }
}
