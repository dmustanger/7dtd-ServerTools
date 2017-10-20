using System;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false;
        public static int DelayBetweenUses = 60;

        public static void SetHome(ClientInfo _cInfo, string _message)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            string x = _position.x.ToString();
            string y = _position.y.ToString();
            string z = _position.z.ToString();
            string _sposition = x + "," + y + "," + z;
			string homeName = GetHomeName(_message);
            PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePositions[homeName] = _sposition;
            PersistentContainer.Instance.Save();
            string _phrase10;
            if (!Phrases.Dict.TryGetValue(10, out _phrase10))
            {
                _phrase10 = "{PlayerName} your home {HomeName} has been saved.";
            }
            _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
			_phrase10 = _phrase10.Replace("{HomeName}", homeName);
			_cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, CustomCommands.ChatColor), "Server", false, "", false));
        }

		public static void TeleHome(ClientInfo _cInfo, string _message)
		{
			Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
			string homeName = GetHomeName(_message);
			if (p == null || p.HomePositions[homeName] == null)
            {
                string _phrase11;
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = "{PlayerName} you do not have a {HomeName} home saved.";
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
				_phrase11 = _phrase11.Replace("{HomeName}", homeName);
				_cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase11, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                if (DelayBetweenUses < 1)
                {
                    Home(_cInfo, p.HomePositions[homeName]);
                }
                else
                {
                    if (p.LastSetHome == null)
                    {
                        Home(_cInfo, p.HomePositions[homeName]);
                    }
                    else
                    {
                        TimeSpan varTime = DateTime.Now - p.LastSetHome;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed > DelayBetweenUses)
                        {
                            Home(_cInfo, p.HomePositions[homeName]);
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase13, CustomCommands.ChatColor), "Server", false, "", false));

                        }
                    }
                }
            }
        }

		private static string GetHomeName(string _message)
		{
			if (_message.Contains(" "))
			{
				return _message.Split(' ')[1];
			}
			else
			{
				return "default";
			}

		}

        private static void Home(ClientInfo _cInfo, string _home)
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
    }
}