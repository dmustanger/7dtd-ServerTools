using HarmonyLib;
using System.Collections.Generic;

namespace ServerTools
{
    class Sorter
    {
        public static bool IsEnabled = false;
        public static string Command_sort = "sort";
        public static int Delay_Between_Uses = 5, Command_Cost = 10;

        private static AccessTools.FieldRef<GameManager, Dictionary<TileEntity, int>> lockedTileEntities = AccessTools.FieldRefAccess<GameManager, Dictionary<TileEntity, int>>("lockedTileEntities");

        public static void Exec(ClientInfo _cInfo)
        {
            EntityPlayer entityPlayer = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            if (entityPlayer == null)
            {
                return;
            }
            Vector3i position = new Vector3i(entityPlayer.serverPos.ToVector3() / 32f);
            TileEntitySecureLootContainerSigned signedContainer;
            List<Chunk> surroundingChunks = GeneralOperations.GetSurroundingChunks(position);
            if (surroundingChunks == null || surroundingChunks.Count == 0)
            {
                return;
            }
            Dictionary<TileEntity, int> lockedTiles = lockedTileEntities(GameManager.Instance);
            DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
            for (int i = 0; i < surroundingChunks.Count; i++)
            {
                tiles = surroundingChunks[i].GetTileEntities();
                foreach (var tile in tiles.dict)
                {
                    if (!(tile.Value is TileEntitySecureLootContainerSigned))
                    {
                        continue;
                    }
                    signedContainer = tile.Value as TileEntitySecureLootContainerSigned;
                    if (signedContainer != null && signedContainer.GetText().ToLower() == "sort" && !lockedTiles.ContainsKey(signedContainer))
                    {
                        EnumLandClaimOwner claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, signedContainer.ToWorldPos());
                        if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
                        {
                            SecureLootContainer(_cInfo, signedContainer, surroundingChunks, lockedTiles);
                            Phrases.Dict.TryGetValue("Sorter1", out string phrase);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                            return;
                        }
                    }
                }
            }
        }

        public static void SecureLootContainer(ClientInfo _cInfo, TileEntitySecureLootContainerSigned _secureLoot, List<Chunk> _surroundingChunks, Dictionary<TileEntity, int> _lockedTiles)
        {
            if (_secureLoot.IsEmpty() || _lockedTiles.ContainsKey(_secureLoot))
            {
                return;
            }
            ItemStack[] mainContainer = _secureLoot.items;
            EnumLandClaimOwner claimOwner;
            TileEntityLootContainer lootContainer;
            ItemStack[] secondaryContainer;
            Dictionary<Vector3i, TileEntity> tiles = new Dictionary<Vector3i, TileEntity>();
            int newStackCount = 0, stackMax = 0;
            for (int i = 0; i < _surroundingChunks.Count; i++)
            {
                if (_secureLoot.IsEmpty())
                {
                    break;
                }
                tiles = _surroundingChunks[i].GetTileEntities().dict;
                foreach (var tile in tiles)
                {
                    if (_lockedTiles.ContainsKey(tile.Value) || _secureLoot.ToWorldPos() == tile.Value.ToWorldPos())
                    {
                        continue;
                    }
                    lootContainer = tile.Value as TileEntityLootContainer;
                    if (lootContainer == null)
                    {
                        continue;
                    }
                    claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, lootContainer.ToWorldPos());
                    if (claimOwner == EnumLandClaimOwner.Self || claimOwner == EnumLandClaimOwner.Ally)
                    {
                        secondaryContainer = lootContainer.items;
                        for (int j = 0; j < mainContainer.Length; j++)
                        {
                            if (mainContainer[j].IsEmpty() || !mainContainer[j].itemValue.ItemClass.CanStack() || !lootContainer.HasItem(mainContainer[j].itemValue))
                            {
                                continue;
                            }
                            else
                            {
                                for (int k = 0; k < secondaryContainer.Length; k++)
                                {
                                    if (!secondaryContainer[k].IsEmpty() && mainContainer[j].itemValue.GetItemOrBlockId() == secondaryContainer[k].itemValue.GetItemOrBlockId())
                                    {
                                        newStackCount = mainContainer[j].count + secondaryContainer[k].count;
                                        stackMax = mainContainer[j].itemValue.ItemClass.Stacknumber.Value;
                                        if (newStackCount <= stackMax)
                                        {
                                            _secureLoot.RemoveItem(mainContainer[j].itemValue);
                                            secondaryContainer[k].count = newStackCount;
                                            lootContainer.items[k].count = newStackCount;
                                            lootContainer.SetModified();
                                            break;
                                        }
                                        else if (secondaryContainer[k].count < stackMax)
                                        {
                                            newStackCount = secondaryContainer[k].count + mainContainer[j].count;
                                            if (newStackCount <= stackMax)
                                            {
                                                _secureLoot.RemoveItem(mainContainer[j].itemValue);
                                                lootContainer.items[k].count = newStackCount;
                                                lootContainer.SetModified();
                                                break;
                                            }
                                            else
                                            {
                                                newStackCount = stackMax - secondaryContainer[k].count;
                                                _secureLoot.items[j].count = newStackCount;
                                                lootContainer.items[k].count = stackMax;
                                                lootContainer.SetModified();
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!mainContainer[j].IsEmpty() && lootContainer.AddItem(mainContainer[j]))
                                {
                                    _secureLoot.RemoveItem(mainContainer[j].itemValue);
                                    lootContainer.SetModified();
                                    continue;
                                }
                            }
                        }
                        _secureLoot.SetModified();
                    }
                }
            }
        }
    }
}
