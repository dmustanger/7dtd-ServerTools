using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Vault
    {
        public static bool IsEnabled = false, Inside_Claim = false;
        public static int Slots = 4, Lines = 1;
        public static Dictionary<Vector3i, string> VaultUser = new Dictionary<Vector3i, string>();

        public static bool Exec(int _entityIdThatOpenedIt, ref TileEntityLootContainer _container)
        {
            Vector2i containerSize = _container.GetContainerSize();
            if (!IsEnabled)
            {
                if (containerSize.x != 0 || containerSize.y != 0)
                {
                    _container.SetContainerSize(new Vector2i(0, 0), true);
                    _container.SetModified();
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser.Remove(_container.ToWorldPos());
                    }
                    return true;
                }
            }
            ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(_entityIdThatOpenedIt);
            if (cInfo == null)
            {
                if (containerSize.x != 0 || containerSize.y != 0)
                {
                    _container.SetContainerSize(new Vector2i(0, 0), true);
                    _container.SetModified();
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser.Remove(_container.ToWorldPos());
                    }
                    return true;
                }
            }
            else
            {
                if (Inside_Claim && !InsideClaim(cInfo))
                {
                    if (containerSize.x != 0 || containerSize.y != 0)
                    {
                        _container.SetContainerSize(new Vector2i(0, 0), true);
                        _container.SetModified();
                    }
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser.Remove(_container.ToWorldPos());
                    }
                    Phrases.Dict.TryGetValue("Vault1", out string phrase);
                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return true;
                }
                if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize != null)
                {
                    int[] vaultSize = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].VaultSize;
                    if (vaultSize != null && vaultSize.Length == 2 && vaultSize[0] > 0 && vaultSize[1] > 0)
                    {
                        if (containerSize.x != vaultSize[0] || containerSize.y != vaultSize[1])
                        {
                            _container.SetContainerSize(new Vector2i(vaultSize[0], vaultSize[1]), true);
                        }
                    }
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser[_container.ToWorldPos()] = cInfo.CrossplatformId.CombinedString;
                    }
                    else
                    {
                        VaultUser.Add(_container.ToWorldPos(), cInfo.CrossplatformId.CombinedString);
                    }
                    FillContainer(cInfo, ref _container);
                }
                else if (Slots > 0 && Lines > 0)
                {
                    if (containerSize.x != Slots || containerSize.y != Lines)
                    {
                        _container.SetContainerSize(new Vector2i(Slots, Lines), true);
                    }
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser[_container.ToWorldPos()] = cInfo.CrossplatformId.CombinedString;
                    }
                    else
                    {
                        VaultUser.Add(_container.ToWorldPos(), cInfo.CrossplatformId.CombinedString);
                    }
                    FillContainer(cInfo, ref _container);
                }
                else
                {
                    _container.SetContainerSize(new Vector2i(0, 0), true);
                    _container.SetModified();
                    if (VaultUser.ContainsKey(_container.ToWorldPos()))
                    {
                        VaultUser.Remove(_container.ToWorldPos());
                    }
                }
            }
            return true;
        }

        public static void FillContainer(ClientInfo _cInfo, ref TileEntityLootContainer _container)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vault != null &&
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vault.Length > 0)
            {
                ItemStack[] items = _container.items;
                ItemDataSerializable[] vault = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Vault;
                ItemValue itemValue;
                for (int i = 0; i < items.Length; i++)
                {
                    if (vault[i] != null)
                    {
                        if (vault[i].name != "")
                        {
                            itemValue = ItemClass.GetItem(vault[i].name, false);
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
                        else
                        {
                            _container.items[i] = ItemStack.Empty.Clone();
                        }
                    }
                }
                _container.SetModified();
            }
        }

        public static void UpdateData(TileEntityLootContainer _container)
        {
            if (VaultUser.ContainsKey(_container.ToWorldPos()))
            {
                VaultUser.TryGetValue(_container.ToWorldPos(), out string id);
                ItemStack[] items = _container.GetItems();
                if (items != null)
                {
                    if (PersistentContainer.Instance.Players[id].Vault == null ||
                    PersistentContainer.Instance.Players[id].Vault.Length != 48)
                    {
                        PersistentContainer.Instance.Players[id].Vault = new ItemDataSerializable[48];
                    }
                    ItemDataSerializable[] vault = PersistentContainer.Instance.Players[id].Vault;
                    ItemValue itemValue;
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] != null)
                        {
                            if (!items[i].IsEmpty())
                            {
                                itemValue = items[i].itemValue;
                                vault[i].name = itemValue.ItemClass.GetItemName();
                                vault[i].quality = itemValue.Quality;
                                vault[i].useTimes = itemValue.UseTimes;
                                vault[i].seed = itemValue.Seed;
                                vault[i].count = items[i].count;
                                if (itemValue.Modifications.Length > 0)
                                {
                                    vault[i].modSlots = itemValue.Modifications.Length;
                                }
                                else
                                {
                                    vault[i].modSlots = 0;
                                }
                                if (itemValue.CosmeticMods.Length > 0)
                                {
                                    vault[i].cosmeticSlots = itemValue.CosmeticMods.Length;
                                }
                                else
                                {
                                    vault[i].cosmeticSlots = 0;
                                }
                            }
                            else
                            {
                                vault[i].name = "";
                            }
                        }
                    }
                    PersistentContainer.Instance.Players[id].Vault = vault;
                    PersistentContainer.DataChange = true;
                }
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
