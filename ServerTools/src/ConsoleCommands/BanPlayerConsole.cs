using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BanPlayerConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Bans a player but also removes their active claim blocks.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-bp <Id/EOS/EntityId/PlayerName> <Time> <Hours/Days/Years>\n" +
                   "  2. st-bp <Id/EOS/EntityId/PlayerName> <Time> <Hours/Days/Years> <Reason>\n" +
                   "1. Ban a player for the specified time and remove all of their existing claim blocks\n" +
                   "2. Ban a player for the specified time and remove all of their existing claim blocks while giving a reason for the ban\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-BanPlayer", "bp", "st-bp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 3 || _params.Count > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 4, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 3)
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                    if (cInfo != null)
                    {
                        PersistentPlayerData persistentPlayerData = GeneralOperations.GetPersistentPlayerDataFromEntityId(cInfo.entityId);
                        if (persistentPlayerData != null)
                        {
                            List<Vector3i> claimBlockList = persistentPlayerData.GetLandProtectionBlocks();
                            if (claimBlockList != null)
                            {
                                for (int i = 0; i < claimBlockList.Count; i++)
                                {
                                    Vector3i claimLocation = claimBlockList[i];
                                    persistentPlayerData.RemoveLandProtectionBlock(claimLocation);
                                }
                            }
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2}", cInfo.CrossplatformId.CombinedString, _params[1], _params[2]), null);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' '{1}' has been banned. Their claims have been removed", cInfo.PlatformId.CombinedString, cInfo.CrossplatformId.CombinedString, _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to ban '{0}'. The player data can not be found", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        PersistentPlayerData persistentPlayerData = GeneralOperations.GetPersistentPlayerDataFromId(_params[0]);
                        if (persistentPlayerData != null)
                        {
                            List<Vector3i> _claimBlockList = persistentPlayerData.GetLandProtectionBlocks();
                            if (_claimBlockList != null)
                            {
                                for (int i = 0; i < _claimBlockList.Count; i++)
                                {
                                    Vector3i _claimLocation = _claimBlockList[i];
                                    persistentPlayerData.RemoveLandProtectionBlock(_claimLocation);
                                }
                            }
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2}", persistentPlayerData.UserIdentifier.CombinedString, _params[1], _params[2]), null);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' has been banned. Their claims have been removed", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to ban '{0}'. The player data can not be found", _params[0]));
                            return;
                        }
                    }
                }
                if (_params.Count == 4)
                {
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (cInfo != null)
                    {
                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(cInfo.entityId);
                        if (_persistentPlayerData != null)
                        {
                            List<Vector3i> _claimBlockList = _persistentPlayerData.GetLandProtectionBlocks();
                            if (_claimBlockList != null)
                            {
                                for (int i = 0; i < _claimBlockList.Count; i++)
                                {
                                    Vector3i _claimLocation = _claimBlockList[i];
                                    _persistentPlayerData.RemoveLandProtectionBlock(_claimLocation);
                                }
                            }
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2} {3}", cInfo.CrossplatformId.CombinedString, _params[1], _params[2], _params[3]), null);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} has been banned. Their claims have been removed", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to ban id {0}. The player data for id can not be found", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        PersistentPlayerData persistentPlayerData = GeneralOperations.GetPersistentPlayerDataFromId(_params[0]);
                        if (persistentPlayerData != null)
                        {
                            List<Vector3i> claimBlockList = persistentPlayerData.GetLandProtectionBlocks();
                            if (claimBlockList != null)
                            {
                                for (int i = 0; i < claimBlockList.Count; i++)
                                {
                                    Vector3i claimLocation = claimBlockList[i];
                                    persistentPlayerData.RemoveLandProtectionBlock(claimLocation);
                                }
                            }
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2} {3}", persistentPlayerData.UserIdentifier.CombinedString, _params[1], _params[2], _params[3]), null);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' has been banned. Their claims have been removed", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to ban '{0}'. The player data can not be found", _params[0]));
                            return;
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BanPlayer.Execute: {0}", e.Message));
            }
        }
    }
}
