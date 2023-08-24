using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace ServerTools
{
    public class GiveStartingItemsConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Gives the starting items from the xml list";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-st-gsi <EOS/EntityId/PlayerName>\n" +
                "1. Gives a player the item(s) from the StatingItems.xml to their inventory unless full. Drops to the ground when full\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-GiveStartingItems", "gsi", "st-gsi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (StartingItems.IsEnabled)
            {
                try
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count);
                        return;
                    }
                    else
                    {
                        ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                        if (cInfo != null)
                        {
                            ThreadManager.AddSingleTaskMainThread("Coroutine", delegate
                            {
                                ThreadManager.StartCoroutine(SpawnItems(cInfo, SdtdConsole.Instance));
                            }, null);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Player with id '{0}' is not online", _params[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out("[SERVERTOOLS] Error in GiveStartingItemsConsole.Execute: {0}", e.Message);
                }
            }
        }

        public static IEnumerator SpawnItems(ClientInfo _cInfo, SdtdConsole _sdtd)
        {
            try
            {
                if (StartingItems.Dict.Count > 0)
                {
                    World world = GameManager.Instance.World;
                    if (world.Players.dict.ContainsKey(_cInfo.entityId))
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                        if (player != null && player.IsSpawned() && !player.IsDead())
                        {
                            List<string> itemList = StartingItems.Dict.Keys.ToList();
                            StartingItems.Exec(_cInfo, itemList);
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].StartingItems = true;
                            PersistentContainer.DataChange = true;
                            _sdtd.Output("[SERVERTOOLS] '{0}' with id '{1}' is receiving their starting items", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString);
                        }
                        else
                        {
                            _sdtd.Output("[SERVERTOOLS] Player with id '{0}' has not spawned. Unable to give starting items", _cInfo.CrossplatformId.CombinedString);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in GiveStartingItemsConsole.SpawnItems: {0}", e.Message);
            }
            yield break;
        }
    }
}
