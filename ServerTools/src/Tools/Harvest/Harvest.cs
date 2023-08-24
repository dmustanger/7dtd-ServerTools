using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class Harvest
    {
        public static bool IsEnabled = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 20;
        public static string Command_harvest = "harvest";

        public static void Exec(ClientInfo _cInfo)
        {
            ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player == null)
                {
                    return;
                }
                World world = GameManager.Instance.World;
                Vector3i position = new Vector3i(player.serverPos.ToVector3() / 32f);
                position.x -= 2;
                position.y -= 1;
                position.z -= 2;
                List<ItemStack> harvest = new List<ItemStack>();
                BlockValue blockValue;
                List<Block.SItemDropProb> list = null;
                ItemStack itemStack;
                for (int i = position.x; i < position.x + 5; i++)
                {
                    for (int j = position.z; j < position.z + 5; j++)
                    {
                        EnumLandClaimOwner owner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, position);
                        if (owner != EnumLandClaimOwner.Self && owner != EnumLandClaimOwner.Ally)
                        {
                            continue;
                        }
                        blockValue = world.GetBlock(position);
                        if (!blockValue.Block.IsPlant() || !blockValue.Block.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out list) || list == null || list.Count == 0)
                        {
                            continue;

                        }
                        itemStack = new ItemStack(ItemClass.GetItem(list[0].name, false), 1);
                        if (itemStack != null && !itemStack.IsEmpty())
                        {
                            for (int k = 0; k < harvest.Count; k++)
                            {
                                if (harvest[k].itemValue.Equals(itemStack.itemValue))
                                {
                                    harvest[k].count += 1;
                                }
                            }
                            world.SetBlockRPC(0, position, BlockValue.Air);
                        }
                    }
                }
                if (harvest.Count > 0)
                {
                    ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                    {
                        player.PlayOneShot("item_plant_pickup", false);
                        ThreadManager.StartCoroutine(SpawnAndCollect(_cInfo, player, harvest, world));
                        Phrases.Dict.TryGetValue("Harvest1", out string phrase);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(phrase, string.Empty));
                    }, null);
                }
            });
        }

        public static IEnumerator SpawnAndCollect(ClientInfo _cInfo, EntityPlayer _player, List<ItemStack> _harvest, World _world)
        {
            try
            {
                if (_world != null && _player.IsSpawned() && !_player.IsDead())
                {
                    EntityItem entityItem;
                    for (int i = 0; i < _harvest.Count; i++)
                    {
                        entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_harvest[i].itemValue, _harvest[i].count),
                            pos = _world.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        _world.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                        _world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Harvest.SpawnItem: {0}", e.StackTrace));
            }
            yield break;
        }
    }
}