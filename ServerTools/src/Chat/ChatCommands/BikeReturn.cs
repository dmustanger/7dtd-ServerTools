using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BikeReturn
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 120, Command_Cost = 0;

        public static void BikeDelay(ClientInfo _cInfo, string _playerName)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    Exec(_cInfo);
                }
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastBike == null)
                {
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        CommandCost(_cInfo);
                    }
                    else
                    {
                        Exec(_cInfo);
                    }
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastBike;
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
                                        CommandCost(_cInfo);
                                    }
                                    else
                                    {
                                        Exec(_cInfo);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase786;
                                    if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                                    {
                                        _phrase786 = "{PlayerName} you can only use /bike once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase786 = _phrase786.Replace("{PlayerName}", _playerName);
                                    _phrase786 = _phrase786.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase786), Config.Server_Response_Name, false, "ServerTools", false));
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
                                CommandCost(_cInfo);
                            }
                            else
                            {
                                Exec(_cInfo);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase786;
                            if (!Phrases.Dict.TryGetValue(786, out _phrase786))
                            {
                                _phrase786 = "{PlayerName} you can only use /bike once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase786 = _phrase786.Replace("{PlayerName}", _playerName);
                            _phrase786 = _phrase786.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase786 = _phrase786.Replace("{TimeRemaining}", _timeleft.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase786), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
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
                    Exec(_cInfo);
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

        public static void Exec(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.AttachedToEntity != null)
            {
                int _counter = 0;
                int _claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize) / 2;
                PersistentPlayerList _persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                PersistentPlayerData _persistentPlayerData = _persistentPlayerList.GetPlayerData(_cInfo.playerId);
                List<Vector3i> _blocks = _persistentPlayerData.LPBlocks;
                for (int i = 0; i < _blocks.Count; i++)
                {
                    Vector3i _vec3i = _blocks[i];
                    if ((_vec3i.x - _player.position.x) * (_vec3i.x - _player.position.x) + (_vec3i.z - _player.position.z) * (_vec3i.z - _player.position.z) <= _claimSize * _claimSize)
                    {
                        string _phrase781;
                        if (!Phrases.Dict.TryGetValue(781, out _phrase781))
                        {
                            _phrase781 = "{PlayerName} saved your current bike for retrieval.";
                        }
                        _phrase781 = _phrase781.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase781), Config.Server_Response_Name, false, "ServerTools", false));
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].BikeId = _player.AttachedToEntity.entityId;
                        PersistentContainer.Instance.Save();
                    }
                    else
                    {
                        _counter++;
                    }
                }
                if (_counter == _blocks.Count)
                {
                    string _phrase780;
                    if (!Phrases.Dict.TryGetValue(780, out _phrase780))
                    {
                        _phrase780 = "{PlayerName} you do not own this land. You can only save your bike inside your own claimed space.";
                    }
                    _phrase780 = _phrase780.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase780), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p != null)
                {
                    if (p.BikeId != 0)
                    {
                        List<Entity> Entities = GameManager.Instance.World.Entities.list;
                        int _counter = 0;
                        bool Other = false, Found = false;
                        for (int i = 0; i < Entities.Count; i++)
                        {
                            _counter++;
                            Entity _entity = Entities[i];
                            string _name = EntityClass.list[_entity.entityClass].entityClassName;
                            if (_name == "minibike")
                            {
                                if ((_player.position.x - _entity.position.x) * (_player.position.x - _entity.position.x) + (_player.position.z - _entity.position.z) * (_player.position.z - _entity.position.z) <= 50 * 50)
                                {
                                    if (_entity.entityId == p.BikeId)
                                    {
                                        if (_entity.AttachedToEntity == false)
                                        {
                                            _entity.SetPosition(_player.position);
                                            string _phrase782;
                                            if (!Phrases.Dict.TryGetValue(782, out _phrase782))
                                            {
                                                _phrase782 = "{PlayerName} found your bike and sent it to you.";
                                            }
                                            _phrase782 = _phrase782.Replace("{PlayerName}", _cInfo.playerName);
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase782), Config.Server_Response_Name, false, "ServerTools", false));
                                            if (Wallet.IsEnabled && Command_Cost >= 1)
                                            {
                                                int _oldCoins = PersistentContainer.Instance.Players[_cInfo.playerId, false].PlayerSpentCoins;
                                                PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = _oldCoins - Command_Cost;
                                            }
                                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBike = DateTime.Now;
                                            PersistentContainer.Instance.Save();
                                            Found = true;
                                        }
                                        else
                                        {
                                            string _phrase785;
                                            if (!Phrases.Dict.TryGetValue(785, out _phrase785))
                                            {
                                                _phrase785 = "{PlayerName} found your bike but someone else is on it.";
                                            }
                                            _phrase785 = _phrase785.Replace("{PlayerName}", _cInfo.playerName);
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase785), Config.Server_Response_Name, false, "ServerTools", false));
                                            Other = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (_counter == Entities.Count && !Other && !Found)
                        {
                            string _phrase784;
                            if (!Phrases.Dict.TryGetValue(784, out _phrase784))
                            {
                                _phrase784 = "{PlayerName} could not find your bike near by.";
                            }
                            _phrase784 = _phrase784.Replace("{PlayerName}", _cInfo.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase784), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        string _phrase783;
                        if (!Phrases.Dict.TryGetValue(783, out _phrase783))
                        {
                            _phrase783 = "{PlayerName} you do not have a bike saved.";
                        }
                        _phrase783 = _phrase783.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase783), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }

        }
    }
}
