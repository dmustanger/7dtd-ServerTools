using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Bank_Transfers = false, PVP = false;
        public static string Currency_Name = "coin", Item_Name = "casinoCoin";
        public static int Zombie_Kill = 10, Player_Kill = 25, Session_Bonus = 5;

        public static Dictionary<int, int> UpdateMainCurrency = new Dictionary<int, int>();
        public static Dictionary<int, List<string[]>> UpdateAltCurrency = new Dictionary<int, List<string[]>>();

        public static void SetItem(string _item)
        {
            try
            {
                if (Item_Name != _item)
                {
                    Item_Name = _item;
                    if (File.Exists(GeneralOperations.XPathDir + "items.xml"))
                    {
                        File.Delete(GeneralOperations.XPathDir + "items.xml");
                    }
                    using (StreamWriter sw = new StreamWriter(GeneralOperations.XPathDir + "items.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("  <set xpath=\"/items/item[@name='{0}']/property[@name='Tags']/@value\">dukes,currency</set>", Item_Name);
                        sw.WriteLine("  <!-- ..... Wallet and Bank currency ^ ..... -->");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", GeneralOperations.XPathDir + "items.xml", e.Message));
            }
        }

        public static int GetCurrency(string _id)
        {
            int value = 0;
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                ItemStack[] stacks = cInfo.latestPlayerData.bag;
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == GeneralOperations.Currency_Item)
                    {
                        value += stacks[i].count;
                    }
                }
            }
            else
            {
                PlayerDataFile pdf = GeneralOperations.GetPlayerDataFileFromId(_id);
                if (pdf != null)
                {
                    ItemStack[] stacks = pdf.bag;
                    for (int i = 0; i < stacks.Length; i++)
                    {
                        if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == GeneralOperations.Currency_Item)
                        {
                            value += stacks[i].count;
                        }
                    }
                }
            }
            return value;
        }

        public static List<string[]> GetOtherCurrency(string _id, List<string[]> otherCurrency)
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                ItemStack[] stacks = cInfo.latestPlayerData.bag;
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.HasAnyTags(FastTags.Parse("currency")) && 
                        stacks[i].itemValue.ItemClass.Name != GeneralOperations.Currency_Item)
                    {
                        string[] entry = { i.ToString(), stacks[i].itemValue.ItemClass.Name, stacks[i].count.ToString() };
                        otherCurrency.Add(entry);
                    }
                }
            }
            else
            {
                PlayerDataFile pdf = GeneralOperations.GetPlayerDataFileFromId(_id);
                if (pdf != null)
                {
                    ItemStack[] stacks = pdf.bag;
                    for (int i = 0; i < stacks.Length; i++)
                    {
                        if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.HasAnyTags(FastTags.Parse("currency")) &&
                        stacks[i].itemValue.ItemClass.Name != GeneralOperations.Currency_Item)
                        {
                            string[] entry = { i.ToString(), stacks[i].itemValue.ItemClass.Name, stacks[i].count.ToString() };
                            otherCurrency.Add(entry);
                        }
                    }
                }
            }
            return otherCurrency;
        }

        public static void AddCurrency(string _id, int _amount, bool _directAllowed)
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo == null || _amount < 1)
            {
                return;
            }
            EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
            if (player == null)
            {
                return;
            }
            if (!player.IsSpawned())
            {
                Timers.Wallet_Add_SingleUseTimer(cInfo.CrossplatformId.CombinedString, _amount, _directAllowed);
            }
            else
            {
                if (Bank.IsEnabled && Bank.Direct_Deposit && _directAllowed)
                {
                    Bank.AddCurrencyToBank(cInfo.CrossplatformId.CombinedString, _amount);
                    if (Bank.Deposit_Message)
                    {
                        Phrases.Dict.TryGetValue("Bank17", out string phrase);
                        phrase = phrase.Replace("{Value}", _amount.ToString());
                        phrase = phrase.Replace("{CoinName}", Currency_Name);
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    return;
                }
                ItemValue itemValue = ItemClass.GetItem(GeneralOperations.Currency_Item, false);
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
        }

        public static void AddAltCurrency(string _id, List<string[]> altCurrency)
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_id);
            if (cInfo != null)
            {
                for (int i = 0; i < altCurrency.Count; i++)
                {
                    ItemValue itemValue = ItemClass.GetItem(altCurrency[i][1], false);
                    if (itemValue != null)
                    {
                        int count = int.Parse(altCurrency[i][2]);
                        World world = GameManager.Instance.World;
                        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(itemValue, count),
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
        }

        public static void RemoveCurrency(string _steamid, int _amount)
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_steamid);
            if (cInfo == null || _amount < 1 || !GameEventManager.GameEventSequences.ContainsKey("action_currency"))
            {
                return;
            }
            EntityPlayer player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
            if (player == null)
            {
                return;
            }
            List<string[]> otherCurrency = new List<string[]>();
            otherCurrency = GetOtherCurrency(cInfo.CrossplatformId.CombinedString, otherCurrency);
            int currency = GetCurrency(cInfo.CrossplatformId.CombinedString);
            if (!player.IsSpawned())
            {
                Timers.Wallet_Remove_SingleUseTimer(cInfo.CrossplatformId.CombinedString, _amount);
            }
            if (currency >= _amount)
            {
                int balance = currency - _amount;
                GameEventManager.Current.HandleAction("action_currency", null, player, false);
                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                if (!UpdateMainCurrency.ContainsKey(cInfo.entityId))
                {
                    UpdateMainCurrency.Add(cInfo.entityId, balance);
                    if (otherCurrency.Count > 0)
                    {
                        if (!UpdateAltCurrency.ContainsKey(cInfo.entityId))
                        {
                            UpdateAltCurrency.Add(cInfo.entityId, otherCurrency);
                        }
                        else
                        {
                            UpdateAltCurrency[cInfo.entityId] = otherCurrency;
                        }
                    }
                }
                else
                {
                    UpdateMainCurrency[cInfo.entityId] = balance;
                    if (otherCurrency.Count > 0)
                    {
                        if (!UpdateAltCurrency.ContainsKey(cInfo.entityId))
                        {
                            UpdateAltCurrency.Add(cInfo.entityId, otherCurrency);
                        }
                        else
                        {
                            UpdateAltCurrency[cInfo.entityId] = otherCurrency;
                        }
                    }
                }
            }
            else if (Bank.IsEnabled && Bank.Direct_Payment)
            {
                int remainder = _amount - currency;
                if (Bank.GetCurrency(cInfo.CrossplatformId.CombinedString) >= remainder)
                {
                    GameEventManager.Current.HandleAction("action_currency", null, player, false);
                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.entityId, "", "", NetPackageGameEventResponse.ResponseTypes.Approved));
                    Bank.SubtractCurrencyFromBank(cInfo.CrossplatformId.CombinedString, remainder);
                    if (otherCurrency.Count > 0)
                    {
                        if (!UpdateAltCurrency.ContainsKey(cInfo.entityId))
                        {
                            UpdateAltCurrency.Add(cInfo.entityId, otherCurrency);
                        }
                        else
                        {
                            UpdateAltCurrency[cInfo.entityId] = otherCurrency;
                        }
                    }
                }
            }
        }
    }
}
