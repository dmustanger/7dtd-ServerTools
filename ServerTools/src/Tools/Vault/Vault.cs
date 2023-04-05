using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Vault
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Slots = 4, Lines = 1;

        public static Dictionary<int, string> VaultUser = new Dictionary<int, string>();

        public static TileEntityLootContainer Exec(int _entityIdThatOpenedIt, TileEntityLootContainer _container)
        {
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo == null)
            {
                return _container;
            }
            if (Inside_Claim && !InsideClaim(cInfo))
            {
                _container.SetContainerSize(new Vector2i(0, 0), true);
                _container.SetModified();
                Phrases.Dict.TryGetValue("Vault1", out string phrase);
                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return _container;
            }
            int[] vaultSize = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
            if (vaultSize != null && vaultSize.Length == 2 && vaultSize[0] > 0 && vaultSize[1] > 0)
            {
                _container.SetContainerSize(new Vector2i(vaultSize[0], vaultSize[1]), true);
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
                return _container;
            }
            else if (Slots > 0 && Lines > 0)
            {
                _container.SetContainerSize(new Vector2i(Slots, Lines), true);
                _container.SetModified();
                return _container;
            }
            else
            {
                _container.SetContainerSize(new Vector2i(0, 0), true);
                _container.SetModified();
                return _container;
            }
        }

        public static void UpdateData(ClientInfo _cInfo, TileEntityLootContainer _container)
        {
            ItemDataSerializable[] vault = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vault;
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
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vault = vault;
                PersistentContainer.DataChange = true;
            }
        }

        public static bool InsideClaim(ClientInfo _cInfo)
        {
            EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (player == null)
            {
                return false;
            }
            Vector3 position = player.position;
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;
            Vector3i vec3i = new Vector3i(x, y, z);
            EnumLandClaimOwner claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, vec3i);
            if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
            {
                return true;
            }
            return false;
        }
    }
}
