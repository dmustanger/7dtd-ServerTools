using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Wallet
    {
        public static bool IsEnabled = false, Bank_Transfers = false, PVP = false;
        public static string Currency_Name = "coin", Command_wallet = "wallet";
        public static int Zombie_Kill = 10, Player_Kill = 25, Session_Bonus = 5;

        public static Dictionary<int, int> UpdateRequired = new Dictionary<int, int>();

        public static int GetCurrency(string _steamId)
        {
            int value = 0;
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromSteamId(_steamId);
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
                PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_steamId);
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

        public static void AddCurrency(string _steamid, int _amount)
        {
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromSteamId(_steamid);
            if (cInfo != null)
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_steamid);
                if (player != null)
                {
                    if (player.IsSpawned())
                    {
                        ItemStack stack = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), _amount);
                        if (stack != null)
                        {
                            World world = GameManager.Instance.World;
                            EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = stack,
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
                    else
                    {
                        Timers.Wallet_Add_SingleUseTimer(cInfo.playerId, _amount);
                    }
                }
            }
            else
            {
                PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_steamid);
                if (pdf != null)
                {
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(PersistentOperations.Currency_Item).type);
                    if (itemValue != null)
                    {
                        int remaining = _amount;
                        ItemStack[] stacks = pdf.bag;
                        bool update = false;
                        for (int i = 0; i < stacks.Length; i++)
                        {
                            if (stacks[i].IsEmpty())
                            {
                                if (remaining > stacks[i].itemValue.ItemClass.Stacknumber.Value)
                                {
                                    stacks[i] = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), itemValue.ItemClass.Stacknumber.Value);
                                    remaining -= itemValue.ItemClass.Stacknumber.Value;
                                    update = true;
                                    continue;
                                }
                                else
                                {
                                    stacks[i] = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), remaining);
                                    update = true;
                                    break;
                                }
                            }
                            else if (stacks[i].itemValue.ItemClass.Name == PersistentOperations.Currency_Item && stacks[i].count < itemValue.ItemClass.Stacknumber.Value)
                            {
                                int maxAllowed = itemValue.ItemClass.Stacknumber.Value - stacks[i].count;
                                if (remaining > maxAllowed)
                                {
                                    stacks[i] = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), stacks[i].count + maxAllowed);
                                    update = true;
                                    continue;
                                }
                                else
                                {
                                    stacks[i] = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), stacks[i].count + remaining);
                                    update = true;
                                    break;
                                }
                            }
                        }
                        if (update)
                        {
                            pdf.bag = stacks;
                            pdf.Save(GameUtils.GetPlayerDataDir(), _steamid);
                        }
                    }
                }
            }
        }

        public static void RemoveCurrency(string _steamid, int _amount)
        {
            int count = 0;
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromSteamId(_steamid);
            if (cInfo != null)
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                if (player != null)
                {
                    if (player.IsSpawned())
                    {
                        count = GetCurrency(cInfo.playerId);
                        if (count > _amount)
                        {
                            count -= _amount;
                            ItemStack stack = new ItemStack(ItemClass.GetItem(PersistentOperations.Currency_Item, false), count);
                            if (stack != null)
                            {
                                UpdateRequired.Add(cInfo.entityId, count);
                                GameEventManager.Current.HandleAction("action_currency", null, player, false, "");
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.playerName, "", NetPackageGameEventResponse.ResponseTypes.Approved));
                            }
                        }
                        else if (count == _amount)
                        {
                            GameEventManager.Current.HandleAction("action_currency", null, player, false, "");
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.playerName, "", NetPackageGameEventResponse.ResponseTypes.Approved));
                        }
                    }
                    else
                    {
                        Timers.Wallet_Remove_SingleUseTimer(cInfo.playerId, count);
                    }
                }
            }
        }

        public static void ClearBag(string _steamId)
        {
            ClientInfo cInfo = PersistentOperations.GetClientInfoFromSteamId(_steamId);
            if (cInfo != null)
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(cInfo.playerId);
                if (player != null)
                {
                    if (player.IsSpawned())
                    {
                        GameEventManager.Current.HandleAction("action_currency", null, player, false, "");
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup("action_currency", cInfo.playerName, "", NetPackageGameEventResponse.ResponseTypes.Approved));
                    }
                }
            }
            else
            {
                PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_steamId);
                if (pdf != null)
                {
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(PersistentOperations.Currency_Item).type);
                    if (itemValue != null)
                    {
                        ItemStack[] stacks = pdf.bag;
                        if (stacks != null)
                        {
                            bool update = false;
                            for (int i = 0; i < stacks.Length; i++)
                            {
                                if (!stacks[i].IsEmpty() && stacks[i].itemValue.ItemClass.Name == PersistentOperations.Currency_Item)
                                {
                                    stacks[i] = ItemStack.Empty.Clone();
                                    update = true;
                                }
                            }
                            if (update)
                            {
                                pdf.bag = stacks;
                                pdf.Save(GameUtils.GetPlayerDataDir(), _steamId);
                            }
                        }
                    }
                }
            }
        }
    }
}
