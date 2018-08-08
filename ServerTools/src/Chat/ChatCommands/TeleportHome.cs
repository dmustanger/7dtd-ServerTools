using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false, Set_Home2_Enabled = false, Set_Home2_Donor_Only = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();
        public static Dictionary<int, int[]> HomeRequest = new Dictionary<int, int[]>();
        public static Dictionary<int, DateTime> HomeRequestTime = new Dictionary<int, DateTime>();
        public static Dictionary<int, Vector3> HomeRequestPos = new Dictionary<int, Vector3>();

        public static void SetHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i (_player.position.x, _player.position.y, _player.position.z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetLandProtectionBlockOwner(_vec3i);
                if (_persistentPlayerData != null)
                {
                    int _id = _persistentPlayerData.EntityId;
                    if (_cInfo.entityId == _id)
                    {
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
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase10), Config.Server_Response_Name, false, "ServerTools", true);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase10), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                        if (_cInfo2 != null)
                        {
                            if (!FriendTeleport.Dict.ContainsKey(_cInfo2.entityId))
                            {
                                if (!HomeRequest.ContainsKey(_cInfo2.entityId))
                                {
                                    int[] _idAndHome = { _cInfo.entityId, 1 };
                                    HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                    HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                    HomeRequestPos.Add(_cInfo2.entityId, _position);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    DateTime _time;
                                    HomeRequestTime.TryGetValue(_cInfo2.entityId, out _time);
                                    TimeSpan varTime = DateTime.Now - _time;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed <= 5)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the claim owner is handling a request already. Try again in a few minutes.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        if (HomeRequest.ContainsKey(_cInfo2.entityId))
                                        {
                                            HomeRequest.Remove(_cInfo2.entityId);
                                            HomeRequestTime.Remove(_cInfo2.entityId);
                                            HomeRequestPos.Remove(_cInfo2.entityId);
                                        }
                                        int[] _idAndHome = { _cInfo.entityId, 1 };
                                        HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                        HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                        HomeRequestPos.Add(_cInfo2.entityId, _position);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                int _dictValue;
                                FriendTeleport.Dict.TryGetValue(_cInfo2.entityId, out _dictValue);
                                DateTime _dict1Value;
                                FriendTeleport.Dict1.TryGetValue(_cInfo2.entityId, out _dict1Value);
                                TimeSpan varTime = DateTime.Now - _dict1Value;
                                double fractionalSeconds = varTime.TotalSeconds;
                                int _timepassed = (int)fractionalSeconds;
                                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                                {
                                    if (ReservedSlots.Dict.ContainsKey(_cInfo2.playerId))
                                    {
                                        DateTime _dt;
                                        ReservedSlots.Dict.TryGetValue(_cInfo2.playerId, out _dt);
                                        if (DateTime.Now < _dt)
                                        {
                                            int _newTime = _timepassed / 2;
                                            _timepassed = _newTime;
                                        }
                                    }
                                }
                                if (_timepassed <= 30)
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the claim owner is handling a request already. Try again in a few minutes.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                    if (HomeRequest.ContainsKey(_cInfo2.entityId))
                                    {
                                        HomeRequest.Remove(_cInfo2.entityId);
                                        HomeRequestTime.Remove(_cInfo2.entityId);
                                        HomeRequestPos.Remove(_cInfo2.entityId);
                                    }
                                    int[] _idAndHome = { _cInfo.entityId, 1 };
                                    HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                    HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                    HomeRequestPos.Add(_cInfo2.entityId, _position);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this claim owner is offline, you can not save your home here.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
                else
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = "{PlayerName} you are not inside your own or another player's claimed space. You can not save this as your home.";
                    }
                    _phrase817 = _phrase817.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or inside an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Check(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.HomePosition == null)
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "{PlayerName} you do not have a home saved.";
                    }
                    _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost(_cInfo, p.HomePosition, _announce);
                        }
                        else
                        {
                            Home(_cInfo, p.HomePosition, _announce);
                        }
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, p.HomePosition, _announce);
                            }
                            else
                            {
                                Home(_cInfo, p.HomePosition, _announce);
                            }
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                CommandCost(_cInfo, p.HomePosition, _announce);
                                            }
                                            else
                                            {
                                                Home(_cInfo, p.HomePosition, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, p.HomePosition, _announce);
                                    }
                                    else
                                    {
                                        Home(_cInfo, p.HomePosition, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or in an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void CommandCost(ClientInfo _cInfo, string _pos, bool _announce)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (currentCoins >= Command_Cost)
                {
                    Home(_cInfo, _pos, _announce);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void Home(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (TeleportDelay.PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 50 * 50)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "{PlayerName} you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    _phrase819 = _phrase819.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase819), Config.Server_Response_Name, false, "ServerTools", false));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (TeleportDelay.Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 20 * 20)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "{PlayerName} you are too close to a zombie. Command unavailable.";
                                }
                                _phrase820 = _phrase820.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase820), Config.Server_Response_Name, false, "ServerTools", false));
                                return;
                            }
                        }
                    }
                }
            }
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            }
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition != null)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} deleted home.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have no home to delete.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void SetHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                Vector3i _vec3i = new Vector3i(_player.position.x, _player.position.y, _player.position.z);
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetLandProtectionBlockOwner(_vec3i);
                if (_persistentPlayerData != null)
                {
                    int _id = _persistentPlayerData.EntityId;
                    if (_cInfo.entityId == _id)
                    {
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
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase607), Config.Server_Response_Name, false, "ServerTools", true);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase607), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_id);
                        if (_cInfo2 != null)
                        {
                            if (!FriendTeleport.Dict.ContainsKey(_cInfo2.entityId))
                            {
                                if (!HomeRequest.ContainsKey(_cInfo2.entityId))
                                {
                                    int[] _idAndHome = { _cInfo.entityId, 2 };
                                    HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                    HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                    HomeRequestPos.Add(_cInfo2.entityId, _position);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    DateTime _time;
                                    HomeRequestTime.TryGetValue(_cInfo2.entityId, out _time);
                                    TimeSpan varTime = DateTime.Now - _time;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed <= 5)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the claim owner is handling a request already. Try again in a few minutes.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        if (HomeRequest.ContainsKey(_cInfo2.entityId))
                                        {
                                            HomeRequest.Remove(_cInfo2.entityId);
                                            HomeRequestTime.Remove(_cInfo2.entityId);
                                            HomeRequestPos.Remove(_cInfo2.entityId);
                                        }
                                        int[] _idAndHome = { _cInfo.entityId, 2 };
                                        HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                        HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                        HomeRequestPos.Add(_cInfo2.entityId, _position);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                        _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                int _dictValue;
                                FriendTeleport.Dict.TryGetValue(_cInfo2.entityId, out _dictValue);
                                DateTime _dict1Value;
                                FriendTeleport.Dict1.TryGetValue(_cInfo2.entityId, out _dict1Value);
                                TimeSpan varTime = DateTime.Now - _dict1Value;
                                double fractionalSeconds = varTime.TotalSeconds;
                                int _timepassed = (int)fractionalSeconds;
                                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                                {
                                    if (ReservedSlots.Dict.ContainsKey(_cInfo2.playerId))
                                    {
                                        DateTime _dt;
                                        ReservedSlots.Dict.TryGetValue(_cInfo2.playerId, out _dt);
                                        if (DateTime.Now < _dt)
                                        {
                                            int _newTime = _timepassed / 2;
                                            _timepassed = _newTime;
                                        }
                                    }
                                }
                                if (_timepassed <= 30)
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} the claim owner is handling a request already. Try again in 1 minute.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                    if (HomeRequest.ContainsKey(_cInfo2.entityId))
                                    {
                                        HomeRequest.Remove(_cInfo2.entityId);
                                        HomeRequestTime.Remove(_cInfo2.entityId);
                                        HomeRequestPos.Remove(_cInfo2.entityId);
                                    }
                                    int[] _idAndHome = { _cInfo.entityId, 2 };
                                    HomeRequest.Add(_cInfo2.entityId, _idAndHome);
                                    HomeRequestTime.Add(_cInfo2.entityId, DateTime.Now);
                                    HomeRequestPos.Add(_cInfo2.entityId, _position);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you need permission from the claim owner to set your home here. They have been sent a request.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, {2} is requesting to set their home in your claimed space located at {3} {4} {5}. Type /accept or /decline in the next 5 minutes.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} this claim owner is offline, you can not save your home here.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
                else
                {
                    string _phrase817;
                    if (!Phrases.Dict.TryGetValue(817, out _phrase817))
                    {
                        _phrase817 = "{PlayerName} you are not inside your own or another player's claimed space. You can not save this as your home.";
                    }
                    _phrase817 = _phrase817.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase817), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or inside an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Check2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.HomePosition2 == null)
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "{PlayerName} you do not have a home saved.";
                    }
                    _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            CommandCost2(_cInfo, p.HomePosition2, _announce);
                        }
                        else
                        {
                            Home2(_cInfo, p.HomePosition2, _announce);
                        }
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost2(_cInfo, p.HomePosition2, _announce);
                            }
                            else
                            {
                                Home2(_cInfo, p.HomePosition2, _announce);
                            }
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                CommandCost2(_cInfo, p.HomePosition2, _announce);
                                            }
                                            else
                                            {
                                                Home2(_cInfo, p.HomePosition2, _announce);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost2(_cInfo, p.HomePosition2, _announce);
                                    }
                                    else
                                    {
                                        Home2(_cInfo, p.HomePosition2, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase13;
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or in an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void CommandCost2(ClientInfo _cInfo, string _pos, bool _announce)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (currentCoins >= Command_Cost)
                {
                    Home2(_cInfo, _pos, _announce);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void Home2(ClientInfo _cInfo, string _pos, bool _announce)
        {
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (TeleportDelay.PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 50 * 50)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "{PlayerName} you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    _phrase819 = _phrase819.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase819), Config.Server_Response_Name, false, "ServerTools", false));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (TeleportDelay.Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 20 * 20)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "{PlayerName} you are too close to a zombie. Command unavailable.";
                                }
                                _phrase820 = _phrase820.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase820), Config.Server_Response_Name, false, "ServerTools", false));
                                return;
                            }
                        }
                    }
                }
            }
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
        }

        public static void DelHome2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            if (p.HomePosition2 != null)
            {
                if (_announce)
                {
                    string _phrase609;
                    if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                    {
                        _phrase609 = "{PlayerName} your home2 has been removed.";
                    }
                    _phrase609 = _phrase609.Replace("{PlayerName}", _cInfo.playerName);
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase609), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    string _phrase609;
                    if (!Phrases.Dict.TryGetValue(609, out _phrase609))
                    {
                        _phrase609 = "{PlayerName} your home2 has been removed.";
                    }
                    _phrase609 = _phrase609.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase609), Config.Server_Response_Name, false, "ServerTools", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].HomePosition2 = null;
                PersistentContainer.Instance.Save();
            }
            else
            {
                if (_announce)
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "{PlayerName} you do not have a home2 saved.";
                    }
                    _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} you have no home2 to delete.[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", true);
                }
                else
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "{PlayerName} you do not have a home2 saved.";
                    }
                    _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void FCheck(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.HomePosition == null)
                {
                    string _phrase11;
                    if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                    {
                        _phrase11 = "{PlayerName} you do not have a home saved.";
                    }
                    _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase11), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost(_cInfo, p.HomePosition, _announce);
                        }
                        else
                        {
                            FHome(_cInfo, p.HomePosition, _announce, _player);
                        }
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                FCommandCost(_cInfo, p.HomePosition, _announce);
                            }
                            else
                            {
                                FHome(_cInfo, p.HomePosition, _announce, _player);
                            }
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                FCommandCost(_cInfo, p.HomePosition, _announce);
                                            }
                                            else
                                            {
                                                FHome(_cInfo, p.HomePosition, _announce, _player);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase13;
                                            if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                            {
                                                _phrase13 = "{PlayerName} you can only use /home or /home2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase13 = _phrase13.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase13), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        FCommandCost(_cInfo, p.HomePosition, _announce);
                                    }
                                    else
                                    {
                                        FHome(_cInfo, p.HomePosition, _announce, _player);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or inside an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void FCommandCost(ClientInfo _cInfo, string _pos, bool _announce)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (currentCoins >= Command_Cost)
                {
                    FHome(_cInfo, _pos, _announce, _player);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void FHome(ClientInfo _cInfo, string _pos, bool _announce, EntityPlayer _player)
        {
            FriendInvite(_cInfo, _player.position, _pos);
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            if (TeleportDelay.PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 50 * 50)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "{PlayerName} you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    _phrase819 = _phrase819.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase819), Config.Server_Response_Name, false, "ServerTools", false));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (TeleportDelay.Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 20 * 20)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "{PlayerName} you are too close to a zombie. Command unavailable.";
                                }
                                _phrase820 = _phrase820.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase820), Config.Server_Response_Name, false, "ServerTools", false));
                                return;
                            }
                        }
                    }
                }
            }
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = "{PlayerName} you are traveling home.";
            }
            _phrase818 = _phrase818.Replace("{PlayerName}", _cInfo.playerName);
            _phrase818 = _phrase818.Replace("{WalletCoinName}", Wallet.Coin_Name);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase818), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void FCheck2(ClientInfo _cInfo, string _playerName, bool _announce)
        {
            if (!Event.Players.Contains(_cInfo.entityId))
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.HomePosition2 == null)
                {
                    string _phrase608;
                    if (!Phrases.Dict.TryGetValue(608, out _phrase608))
                    {
                        _phrase608 = "{PlayerName} you do not have a home2 saved.";
                    }
                    _phrase608 = _phrase608.Replace("{PlayerName}", _cInfo.playerName);
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", true);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase608), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    bool _donator = false;
                    if (Delay_Between_Uses < 1)
                    {
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            FCommandCost2(_cInfo, p.HomePosition2, _announce);
                        }
                        else
                        {
                            FHome2(_cInfo, p.HomePosition2, _announce, _player);
                        }
                    }
                    else
                    {
                        if (p.LastSetHome == null)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                FCommandCost2(_cInfo, p.HomePosition2, _announce);
                            }
                            else
                            {
                                FHome2(_cInfo, p.HomePosition2, _announce, _player);
                            }
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.LastSetHome;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = Delay_Between_Uses / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                FCommandCost2(_cInfo, p.HomePosition2, _announce);
                                            }
                                            else
                                            {
                                                FHome2(_cInfo, p.HomePosition2, _announce, _player);
                                            }
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            string _phrase815;
                                            if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                            {
                                                _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                            }
                                            _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                            _phrase815 = _phrase815.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                            _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                }
                            }
                            if (!_donator)
                            {
                                if (_timepassed >= Delay_Between_Uses)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        FCommandCost2(_cInfo, p.HomePosition2, _announce);
                                    }
                                    else
                                    {
                                        FHome2(_cInfo, p.HomePosition2, _announce, _player);
                                    }
                                }
                                else
                                {
                                    int _timeleft = Delay_Between_Uses - _timepassed;
                                    string _phrase815;
                                    if (!Phrases.Dict.TryGetValue(815, out _phrase815))
                                    {
                                        _phrase815 = "{PlayerName} you can only use /fhome or /fhome2 once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase815 = _phrase815.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase815 = _phrase815.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                                    _phrase815 = _phrase815.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase815), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, you can not use home commands while signed up for or inside an event.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void FCommandCost2(ClientInfo _cInfo, string _pos, bool _announce)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            int currentCoins = 0;
            if (p != null)
            {
                int spentCoins = p.PlayerSpentCoins;
                int gameMode = world.GetGameMode();
                if (gameMode == 7)
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                else
                {
                    currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                }
                if (currentCoins >= Command_Cost)
                {
                    FHome2(_cInfo, _pos, _announce, _player);
                }
                else
                {
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "{PlayerName} you do not have enough {WalletCoinName} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase814), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void FHome2(ClientInfo _cInfo, string _pos, bool _announce, EntityPlayer _player)
        {
            FriendInvite(_cInfo, _player.position, _pos);
            int x, y, z;
            string[] _cords = _pos.Split(',');
            int.TryParse(_cords[0], out x);
            int.TryParse(_cords[1], out y);
            int.TryParse(_cords[2], out z);
            if (TeleportDelay.PvP_Check)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    if (_cInfo2 != null)
                    {
                        EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                        if (_player2 != null)
                        {
                            Vector3 _pos2 = _player2.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 50 * 50)
                            {
                                if (!_player.IsFriendsWith(_player2))
                                {
                                    string _phrase819;
                                    if (!Phrases.Dict.TryGetValue(819, out _phrase819))
                                    {
                                        _phrase819 = "{PlayerName} you are too close to a player that is not a friend. Command unavailable.";
                                    }
                                    _phrase819 = _phrase819.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase819), Config.Server_Response_Name, false, "ServerTools", false));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (TeleportDelay.Zombie_Check)
            {
                World world = GameManager.Instance.World;
                List<Entity> Entities = world.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        EntityType _type = _entity.entityType;
                        if (_type == EntityType.Zombie)
                        {
                            Vector3 _pos2 = _entity.GetPosition();
                            if ((x - (int)_pos2.x) * (x - (int)_pos2.x) + (z - (int)_pos2.z) * (z - (int)_pos2.z) <= 20 * 20)
                            {
                                string _phrase820;
                                if (!Phrases.Dict.TryGetValue(820, out _phrase820))
                                {
                                    _phrase820 = "{PlayerName} you are too close to a zombie. Command unavailable.";
                                }
                                _phrase820 = _phrase820.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase820), Config.Server_Response_Name, false, "ServerTools", false));
                                return;
                            }
                        }
                    }
                }
            }
            Players.NoFlight.Add(_cInfo.entityId);
            TeleportDelay.TeleportQue(_cInfo, x, y, z);
            int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase818;
            if (!Phrases.Dict.TryGetValue(818, out _phrase818))
            {
                _phrase818 = "{PlayerName} you are traveling home.";
            }
            _phrase818 = _phrase818.Replace("{PlayerName}", _cInfo.playerName);
            _phrase818 = _phrase818.Replace("{WalletCoinName}", Wallet.Coin_Name);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase818), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            int x = (int)_position.x;
            int y = (int)_position.y;
            int z = (int)_position.z;
            World world = GameManager.Instance.World;
            EntityPlayer _player = world.Players.dict[_cInfo.entityId];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                EntityPlayer _player2 = world.Players.dict[_cInfo2.entityId];
                if (_player2 != null)
                {
                    if (_player.IsFriendsWith(_player2))
                    {
                        if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                        {
                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your friend {2} has invited you to their saved home. Type /go to accept the request.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Invited your friend {1} to your saved home.[-]", Config.Chat_Response_Color, _cInfo2.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            if (Invite.ContainsKey(_cInfo2.entityId))
                            {
                                Invite.Remove(_cInfo2.entityId);
                                FriendPosition.Remove(_cInfo2.entityId);
                            }
                            Invite.Add(_cInfo2.entityId, DateTime.Now);
                            FriendPosition.Add(_cInfo2.entityId, _destination);
                        }
                    }
                }
            }
        }

        public static void FriendHome(ClientInfo _cInfo)
        {
            DateTime _dt;
            Invite.TryGetValue(_cInfo.entityId, out _dt);
            {
                TimeSpan varTime = DateTime.Now - _dt;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (_timepassed <= 2)
                {
                    string _pos;
                    FriendPosition.TryGetValue(_cInfo.entityId, out _pos);
                    {
                        int x, y, z;
                        string[] _cords = _pos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        Players.NoFlight.Add(_cInfo.entityId);
                        TeleportDelay.TeleportQue(_cInfo, x, y, z);
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} sending you to your friend's home.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    Invite.Remove(_cInfo.entityId);
                    FriendPosition.Remove(_cInfo.entityId);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} you have run out of time to accept your friend's invitation.[-]", Config.Chat_Response_Color, _cInfo.playerName), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void AcceptRequest(ClientInfo _cInfo, int[] _idAndHome, Vector3 _position)
        {
            ClientInfo _cInfo2 = ConnectionManager.Instance.GetClientInfoForEntityId(_idAndHome[0]);
            if (_cInfo2 != null)
            {
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _sposition = x + "," + y + "," + z;
                if (_idAndHome[1] == 1)
                {
                    PersistentContainer.Instance.Players[_cInfo2.playerId, true].HomePosition = _sposition;
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo2.playerId, true].HomePosition2 = _sposition;
                }
                PersistentContainer.Instance.Save();
                string _phrase10;
                if (!Phrases.Dict.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = "{PlayerName} your home has been saved.";
                }
                _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase10), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}The player requesting to set their home on your claim has gone offline.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}