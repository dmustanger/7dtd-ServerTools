using System;
using System.Collections.Generic;
using System.Data;

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
                string _sql = string.Format("SELECT lastBike FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out DateTime _lastBike);
                _result.Dispose();
                TimeSpan varTime = DateTime.Now - _lastBike;
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

        public static void CommandCost(ClientInfo _cInfo)
        {
            World world = GameManager.Instance.World;
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int currentCoins = 0;
            string _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _playerSpentCoins);
            _result.Dispose();
            int gameMode = world.GetGameMode();
            if (gameMode == 7)
            {
                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + _playerSpentCoins;
            }
            else
            {
                currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + _playerSpentCoins;
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
                        string _sql = string.Format("UPDATE Players SET bikeId = {0} WHERE steamid = '{1}'", _player.AttachedToEntity.entityId, _cInfo.playerId);
                        SQL.FastQuery(_sql);
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
                string _sql = string.Format("SELECT bikeId FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _bikeId);
                _result.Dispose();
                if (_bikeId != 0)
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
                                if (_entity.entityId == _bikeId)
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
                                            _sql = string.Format("SELECT playerSpentCoins FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                            DataTable _result1 = SQL.TQuery(_sql);
                                            int.TryParse(_result1.Rows[0].ItemArray.GetValue(0).ToString(), out int _playerSpentCoins);
                                            _result1.Dispose();
                                            _sql = string.Format("UPDATE Players SET playerSpentCoins = {0} WHERE steamid = '{1}'", _playerSpentCoins - Command_Cost, _cInfo.playerId);
                                            SQL.FastQuery(_sql);
                                        }
                                        _sql = string.Format("UPDATE Players SET lastBike = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                                        SQL.FastQuery(_sql);
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
