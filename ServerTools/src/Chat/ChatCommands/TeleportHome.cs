using System;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false;
        public static int DelayBetweenUses = 60;

        public static void SetHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null & !p.IsJailed)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = _sposition;
                PersistentContainer.Instance.Save();
                string _phrase10;
                if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = "{PlayerName} your home has been saved.";
                }
                _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, Config.ChatColor), "Server", false, "", false));
                }
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = _sposition;
                PersistentContainer.Instance.Save();
                string _phrase10;
                if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = "{PlayerName} your home has been saved.";
                }
                _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, Config.ChatColor), "Server", false, "", false));
                }
            }
        }

        public static void TeleHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition == null & !p.IsJailed)
            {
                string _phrase11;
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = "{PlayerName} you do not have a home saved.";
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase11, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase11, Config.ChatColor), "Server", false, "", false));
                }
            }
            else
            {
                if (DelayBetweenUses < 1)
                {
                    Home(_cInfo, p.HomePosition, _announce);
                }
                else
                {
                    if (p.LastSetHome == null)
                    {
                        Home(_cInfo, p.HomePosition, _announce);
                    }
                    else
                    {
                        TimeSpan varTime = DateTime.Now - p.LastSetHome;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed > DelayBetweenUses)
                        {
                            Home(_cInfo, p.HomePosition, _announce);
                        }
                        else
                        {
                            int _timeleft = DelayBetweenUses - _timepassed;
                            string _phrase13;
                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                            {
                                _phrase13 = "{PlayerName} you can only use /home once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase13, Config.ChatColor), _playerName, false, "ServerTools", true);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase13, Config.ChatColor), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }       


        private static void Home(ClientInfo _cInfo, string _home, bool _announce)
        {
            float xf;
            float yf;
            float zf;
            string[] _cords = _home.Split(',');
            float.TryParse(_cords[0], out xf);
            float.TryParse(_cords[1], out yf);
            float.TryParse(_cords[2], out zf);
            int x = (int)xf;
            int y = (int)yf;
            int z = (int)zf;
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), _cInfo);
            PersistentContainer.Instance.Players[_cInfo.playerId, false].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition != null)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.ChatColor, _playerName), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.ChatColor, _playerName), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, false].HomePosition = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.ChatColor, _playerName), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.ChatColor, _playerName), "Server", false, "", false));
                }
            }
        }

        public static void SetHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p != null & !p.IsJailed)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = _sposition;
                PersistentContainer.Instance.Save();
                string _phrase607;
                if (!Phrases.Dict.TryGetValue(607, out _phrase607))
                {
                    _phrase607 = "{PlayerName} your home2 has been saved.";
                }
                _phrase607 = _phrase607.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase607, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase607, Config.ChatColor), "Server", false, "", false));
                }
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = _sposition;
                PersistentContainer.Instance.Save();
                string _phrase607;
                if (!Phrases.Dict.TryGetValue(607, out _phrase607))
                {
                    _phrase607 = "{PlayerName} your home2 has been saved.";
                }
                _phrase607 = _phrase607.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase607, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase607, Config.ChatColor), "Server", false, "", false));
                }
            }
        }

        public static void TeleHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p == null || p.HomePosition2 == null & !p.IsJailed)
            {
                string _phrase608;
                if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                {
                    _phrase608 = "{PlayerName} you do not have a home2 saved.";
                }
                _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase608, Config.ChatColor), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase608, Config.ChatColor), "Server", false, "", false));
                }
            }
            else
            {
                if (DelayBetweenUses < 1)
                {
                    Home2(_cInfo, p.HomePosition2, _playerName, _announce);
                }
                else
                {
                    if (p.LastSetHome2 == null)
                    {
                        Home2(_cInfo, p.HomePosition2, _playerName, _announce);
                    }
                    else
                    {
                        TimeSpan varTime = DateTime.Now - p.LastSetHome2;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed > DelayBetweenUses)
                        {
                            Home2(_cInfo, p.HomePosition2, _playerName, _announce);
                        }
                        else
                        {
                            int _timeleft = DelayBetweenUses - _timepassed;
                            string _phrase609;
                            if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                            {
                                _phrase609 = "{PlayerName} you can only use /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase609 = _phrase609.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase609 = _phrase609.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                            _phrase609 = _phrase609.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase609, Config.ChatColor), _playerName, false, "ServerTools", true);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase609, Config.ChatColor), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }


        private static void Home2(ClientInfo _cInfo, string _home2, string _playerName, bool _announce)
        {
            float xf;
            float yf;
            float zf;
            string[] _cords = _home2.Split(',');
            float.TryParse(_cords[0], out xf);
            float.TryParse(_cords[1], out yf);
            float.TryParse(_cords[2], out zf);
            int x = (int)xf;
            int y = (int)yf;
            int z = (int)zf;
            SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, x, y, z), _cInfo);
            PersistentContainer.Instance.Players[_cInfo.playerId, false].LastSetHome2 = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition2 != null)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} deleted home2.[-]", Config.ChatColor, _playerName), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deleted home2.[-]", Config.ChatColor, _playerName), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, false].HomePosition2 = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1} you have no home2 to delete.[-]", Config.ChatColor, _playerName), _playerName, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no home2 to delete.[-]", Config.ChatColor, _playerName), "Server", false, "", false));
                }
            }
        }
    }
}