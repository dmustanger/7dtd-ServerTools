using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class Backpack
    {
        public static bool IsEnabled = false, Open = true, Observer = false;
        public static int Delay_Between_Uses = 120;
        public static Dictionary<int, Vector3i> LastBagPos = new Dictionary<int, Vector3i>();
        private static List<ClientInfo> que = new List<ClientInfo>();
        private static ChunkManager.ChunkObserver _chunkObserver = null;
        private static ItemStack[] _stacks = null;
        private static Entity _entityDel = null;

        public static void BackpackDelay(ClientInfo _cInfo, string _playerName)
        {
            if (Delay_Between_Uses < 1)
            {
                Check(_cInfo);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastBackpack == null)
                {
                    Check(_cInfo);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastBackpack;
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
                                int _newTime = _timepassed * 2;
                                _timepassed = _newTime;
                            }
                        }
                    }
                    if (_timepassed >= Delay_Between_Uses)
                    {
                        Check(_cInfo);
                    }
                    else
                    {
                        int _timeleft = Delay_Between_Uses - _timepassed;
                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    int _newTime = _timeleft / 2;
                                    _timeleft = _newTime;
                                    int _newDelay = Delay_Between_Uses / 2;
                                    Delay_Between_Uses = _newDelay;
                                }
                            }
                        }
                        string _phrase771;
                        if (!Phrases.Dict.TryGetValue(771, out _phrase771))
                        {
                            _phrase771 = "{PlayerName} you can only use /bag once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase771 = _phrase771.Replace("{PlayerName}", _playerName);
                        _phrase771 = _phrase771.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                        _phrase771 = _phrase771.Replace("{TimeRemaining}", _timeleft.ToString());
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase771), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
        }

        public static void Check(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3i _vec3i = _player.GetDroppedBackpackPosition();
            if (_vec3i != null)
            {
                if (!LastBagPos.ContainsKey(_cInfo.entityId))
                {
                    LastBagPos[_cInfo.entityId] = _vec3i;
                    if (Open)
                    {
                        Open = false;
                        Execute(_cInfo, _vec3i);
                    }
                    else
                    {
                        que.Add(_cInfo);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Bag return in use. You were added to the que.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Bag return in use and you are already in the que.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                Open = true;
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have no recorded bag position.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Execute(ClientInfo _cInfo, Vector3i _vec3i)
        {
            World world = GameManager.Instance.World;
            Vector3 _vec3 = _vec3i.ToVector3();
            Chunk chunk = (Chunk)world.GetChunkFromWorldPos(World.worldToBlockPos(_vec3));
            if (chunk != null)
            {
                bool _found = false, _alert = false;
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                int _counter = 0;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        string _name = EntityClass.list[_entity.entityClass].entityClassName;
                        if (_name == "Backpack")
                        {
                            int x = (int)_entity.position.x;
                            int y = (int)_entity.position.y;
                            int z = (int)_entity.position.z;
                            if (x >= _vec3i.x - 10 && x <= _vec3i.x + 10 && y >= _vec3i.y - 10 && y <= _vec3i.y + 10 && z >= _vec3i.z - 10 && z <= _vec3i.z + 10)
                            {
                                _counter++;
                                if (_counter <= 1)
                                {
                                    _found = true;
                                    _entityDel = _entity;
                                    _stacks = _entity.lootContainer.GetItems();
                                }
                                if (_counter > 1)
                                {
                                    _found = false;
                                    _alert = true;
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (_found)
                {
                    _entityDel.lootContainer.SetEmpty();
                    if (Observer == true)
                    {
                        world.GetGameManager().RemoveChunkObserver(_chunkObserver);
                        Observer = false;
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Bag was found. Attempting to send it to you.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    ReturnItem(_cInfo);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastBackpack = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (!_found && !_alert)
                {
                    if (Observer == true)
                    {
                        world.GetGameManager().RemoveChunkObserver(_chunkObserver);
                        Observer = false;
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}No bag was found. It may have been looted. If you are sure you have one, relog and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    LastBagPos.Remove(_cInfo.entityId);
                    Que();
                }
                if (_alert)
                {
                    if (Observer == true)
                    {
                        world.GetGameManager().RemoveChunkObserver(_chunkObserver);
                        Observer = false;
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Too many bags were found in your dropped bag location. Can not return your items.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    LastBagPos.Remove(_cInfo.entityId);
                    Que();
                }
            }
            else
            {
                Observer = true;
                _chunkObserver = world.GetGameManager().AddChunkObserver(_vec3, false, 3, -1);
                que.Insert(0, _cInfo);
            }
        }

        public static void ReturnItem(ClientInfo _cInfo)
        {
            foreach (ItemStack _item in _stacks)
            {
                ItemValue _itemValue = _item.itemValue;
                if (_itemValue != null && !_itemValue.Equals(ItemValue.None))
                {
                    int _quality = 1;
                    if (_itemValue.HasQuality)
                    {
                        _quality = _itemValue.Quality;
                    }
                    string _itemName = ItemClass.list[_itemValue.type].GetItemName();
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, true);
                    World world = GameManager.Instance.World;
                    if (world.Players.dict[_cInfo.entityId].IsSpawned())
                    {
                        var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(itemValue, _item.count),
                            pos = world.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        world.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                        world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                    }
                }
            }
            string _phrase770;
            if (!Phrases.Dict.TryGetValue(770, out _phrase770))
            {
                _phrase770 = "The items from your dropped bag were sent to your inventory. If your bag is full, check the ground.";
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase770), Config.Server_Response_Name, false, "ServerTools", false));
            LastBagPos.Remove(_cInfo.entityId);
            Que();
        }

        public static void Que()
        {
            if (que.Count > 0)
            {
                ClientInfo _cInfo = que.First();
                Vector3i _vec3i;
                if (LastBagPos.TryGetValue(_cInfo.entityId, out _vec3i))
                {
                    que.RemoveAt(0);
                    Execute(_cInfo, _vec3i);
                }
                else
                {
                    que.RemoveAt(0);
                    Que();
                }
            }
            else
            {
                Open = true;
            }
        }
    }
}