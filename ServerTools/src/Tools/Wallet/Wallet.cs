using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Bank_Transfers = false, PVP = false;
        public static string Currency_Name = "coin", Item_Name = "casinoCoin";
        public static int Zombie_Kill = 10, Player_Kill = 25, Session_Bonus = 5;

        public static Dictionary<int, int> UpdateRequired = new Dictionary<int, int>();

        public static void SetItem(string _item)
        {
            try
            {
                if (File.Exists(API.GamePath + "/Mods/ServerTools/Config/items.xml"))
                {
                    string[] arrLines = File.ReadAllLines(API.GamePath + "/Mods/ServerTools/Config/items.xml");
                    int lineNumber = 0;
                    for (int i = 0; i < arrLines.Length; i++)
                    {
                        if (arrLines[i].Contains("set xpath"))
                        {
                            if (arrLines[i].Contains(_item))
                            {
                                return;
                            }
                            else
                            {
                                arrLines[lineNumber] = string.Format("<set xpath=\"/items/item[@name='{0}']/property[@name='Tags']/@value\">dukes,currency</set>", _item);
                                File.WriteAllLines(API.GamePath + "/Mods/ServerTools/Config/items.xml", arrLines);
                                break;
                            }
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", API.GamePath + "/Mods/ServerTools/Config/items.xml", e.Message));
                return;
            }
        }

        public static int GetCurrency(string _id)
        {
            int value = 0;
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                ItemStack[] stacks = cInfo.latestPlayerData.bag;
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == PersistentOperations.Currency_Item)
                    {
                        value += stacks[i].count;
                    }
                }
            }
            else
            {
                PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromId(_id);
                if (pdf != null)
                {
                    ItemStack[] stacks = pdf.bag;
                    for (int i = 0; i < stacks.Length; i++)
                    {
                        if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == PersistentOperations.Currency_Item)
                        {
                            value += stacks[i].count;
                        }
                    }
                }
            }
            return value;
        }

        public static void AddCurrency(string _id, int _amount)
        {
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                if (player != null)
                {
                    if (player.IsSpawned())
                    {
                        ItemValue itemValue = ItemClass.GetItem(PersistentOperations.Currency_Item, false);
                        if (itemValue != null)
                        {
                            List<int> stackList = new List<int>();
                            int maxStack = itemValue.ItemClass.Stacknumber.Value;
                            if (_amount > maxStack)
                            {
                                for (int i = 0; i < 100; i++)
                                {
                                    if (_amount > maxStack)
                                    {
                                        _amount = _amount - maxStack;
                                        stackList.Add(maxStack);
                                    }
                                    else
                                    {
                                        stackList.Add(_amount);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                stackList.Add(_amount);
                            }
                            for (int i = 0; i < stackList.Count; i++)
                            {
                                World world = GameManager.Instance.World;
                                EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, stackList[i]),
                                    pos = world.Players.dict[cInfo.entityId].position,
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = cInfo.entityId
                                });
                                world.SpawnEntityInWorld(entityItem);
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                            }
                        }
                    }
                    else
                    {
                        Timers.Wallet_Add_SingleUseTimer(cInfo.CrossplatformId.CombinedString, _amount);
                    }
                }
            }
            else
            {
                PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].PlayerWallet += _amount;
                PersistentContainer.DataChange = true;
            }
        }

        public static void RemoveCurrency(string _steamid, int _amount, bool _bankPayment)
        {
            int count = 0;
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_steamid);
            if (cInfo != null)
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.entityId);
                if (player != null)
                {
                    if (player.IsSpawned())
                    {
                        count = GetCurrency(cInfo.CrossplatformId.CombinedString);
                        GameEventManager.Current.HandleAction("action_currency", null, player, false, "");
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.playerName, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                        if (count > _amount)
                        {
                            count -= _amount;
                            ItemStack stack = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), count);
                            if (stack != null)
                            {
                                UpdateRequired.Add(cInfo.entityId, count);
                            }
                        }
                        else if (count < _amount && _bankPayment)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Bank -= _amount;
                            PersistentContainer.DataChange = true;
                        }
                    }
                    else
                    {
                        Timers.Wallet_Remove_SingleUseTimer(cInfo.CrossplatformId.CombinedString, count, _bankPayment);
                    }
                }
            }
        }
    }
}
