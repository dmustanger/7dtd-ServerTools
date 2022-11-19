using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Vault
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static string Command_vault = "vault";

        public static Dictionary<int, string> VaultUser = new Dictionary<int, string>();

        public static TileEntityLootContainer Exec(int _entityIdThatOpenedIt, TileEntityLootContainer _container)
        {
            ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo != null)
            {
                if (Inside_Claim && !InsideClaim(cInfo))
                {
                    _container.SetContainerSize(new Vector2i(0, 0), true);
                    Phrases.Dict.TryGetValue("Vault1", out string phrase);
                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return _container;
                }
                if (!VaultUser.ContainsKey(_container.entityId))
                {
                    VaultUser.Add(_container.entityId, cInfo.CrossplatformId.CombinedString);
                }
                else
                {
                    VaultUser[_container.entityId] = cInfo.CrossplatformId.CombinedString;
                }
                int vaultSize = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                if (vaultSize < 4)
                {
                    _container.SetContainerSize(new Vector2i(4, 1), true);
                }
                else if (vaultSize == 48)
                {
                    _container.SetContainerSize(new Vector2i(8, 6), true);
                }
                else
                {
                    _container.SetContainerSize(new Vector2i(vaultSize, 1), true);
                }
                if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault != null)
                {
                    ItemStack[] items = _container.items;
                    ItemDataSerializable[] vault = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault;
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (vault[i] != null && vault[i].name != "")
                        {
                            ItemValue itemValue = ItemClass.GetItem(vault[i].name, false);
                            if (itemValue != null)
                            {
                                if (itemValue.ItemClass.HasQuality)
                                {
                                    itemValue.Quality = vault[i].quality;
                                }
                                itemValue.UseTimes = vault[i].useTimes;
                                itemValue.Seed = vault[i].seed;
                                if (vault[i].modSlots > 0)
                                {
                                    itemValue.Modifications = new ItemValue[vault[i].modSlots];
                                }
                                if (vault[i].cosmeticSlots > 0)
                                {
                                    itemValue.CosmeticMods = new ItemValue[vault[i].cosmeticSlots];
                                }
                                _container.items[i].itemValue = itemValue;
                                _container.items[i].count = vault[i].count;
                            }
                        }
                    }
                    _container.SetModified();
                }
            }
            return _container;
        }

        public static void UpdateData(TileEntityLootContainer _container)
        {
            if (VaultUser.ContainsKey(_container.entityId))
            {
                VaultUser.TryGetValue(_container.entityId, out string user);
                ItemDataSerializable[] vault = PersistentContainer.Instance.Players[user].Vault;
                if (vault == null)
                {
                    vault = new ItemDataSerializable[48];
                }
                ItemStack[] items = _container.GetItems();
                if (items != null && items.Length > 0)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] != null && !items[i].IsEmpty())
                        {
                            ItemDataSerializable itemSerialized = new ItemDataSerializable();
                            itemSerialized.name = items[i].itemValue.ItemClass.GetItemName();
                            itemSerialized.quality = items[i].itemValue.Quality;
                            itemSerialized.useTimes = items[i].itemValue.UseTimes;
                            itemSerialized.seed = items[i].itemValue.Seed;
                            itemSerialized.count = items[i].count;
                            if (items[i].itemValue.Modifications.Length > 0)
                            {
                                itemSerialized.modSlots = items[i].itemValue.Modifications.Length;
                            }
                            else
                            {
                                itemSerialized.modSlots = 0;
                            }
                            if (items[i].itemValue.CosmeticMods.Length > 0)
                            {
                                itemSerialized.cosmeticSlots = items[i].itemValue.CosmeticMods.Length;
                            }
                            else
                            {
                                itemSerialized.cosmeticSlots = 0;
                            }
                            vault[i] = itemSerialized;
                        }
                        else if (items[i].IsEmpty())
                        {
                            ItemDataSerializable itemSerialized = new ItemDataSerializable();
                            itemSerialized.name = "";
                            itemSerialized.quality = 0;
                            itemSerialized.useTimes = 0;
                            itemSerialized.seed = 0;
                            itemSerialized.modSlots = 0;
                            itemSerialized.cosmeticSlots = 0;
                            itemSerialized.count = 0;
                            vault[i] = itemSerialized;
                        }
                    }
                    PersistentContainer.Instance.Players[user].Vault = vault;
                    PersistentContainer.DataChange = true;
                }
            }
        }

        public static bool InsideClaim(ClientInfo _cInfo)
        {
            EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                Vector3 position = player.position;
                int x = (int)position.x;
                int y = (int)position.y;
                int z = (int)position.z;
                Vector3i vec3i = new Vector3i(x, y, z);
                EnumLandClaimOwner claimOwner = GeneralFunction.ClaimedByWho(_cInfo.CrossplatformId, vec3i);
                if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
