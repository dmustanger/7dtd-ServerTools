using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerTools
{
    class BanPlayerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Bans A Player But Also Removes Their Active Claim Blocks.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. BanPlayer <EntityId/Name/SteamId> <Time> <Hours/Days/Years>\n" +
                   "  2. BanPlayer <EntityId/Name/SteamId> <Time> <Hours/Days/Years> <Reason>\n" +
                   "1. Ban a player for the specified time and remove all of their existing claim blocks\n" +
                   "2. Ban a player for the specified time and remove all of their existing claim blocks while giving a reason for the ban\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-BanPlayer", "banplayer", "bp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 3 || _params.Count > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 3 or 4, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 3)
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo != null)
                    {
                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_cInfo.entityId);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2}", _cInfo.playerId, _params[1], _params[2]), (ClientInfo)null);
                            SdtdConsole.Instance.Output(string.Format("Id {0} has been banned. Their claims have been removed.", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}. The player data for id can not be found.", _params[0]));
                            return;
                        }
                    }
                    else if (_params[0].Length == 17)
                    {
                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(_params[0]);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2}", _cInfo.playerId, _params[1], _params[2]), (ClientInfo)null);
                            SdtdConsole.Instance.Output(string.Format("Id {0} has been banned. Their claims have been removed.", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}.  The player data for id can not be found.", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}. If the player is offline, use their steam id to ban them", _params[0]));
                        return;
                    }
                }
                if (_params.Count == 4)
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo != null)
                    {
                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_cInfo.entityId);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2} {3}", _cInfo.playerId, _params[1], _params[2], _params[3]), (ClientInfo)null);
                            SdtdConsole.Instance.Output(string.Format("Id {0} has been banned. Their claims have been removed.", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}. The player data for id can not be found.", _params[0]));
                            return;
                        }
                    }
                    else if (_params[0].Length == 17)
                    {
                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(_params[0]);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} {1} {2} {3}", _cInfo.playerId, _params[1], _params[2], _params[3]), (ClientInfo)null);
                            SdtdConsole.Instance.Output(string.Format("Id {0} has been banned. Their claims have been removed.", _params[0]));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}. The player data for id can not be found.", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Unable to ban id {0}. If the player is offline, use their steam id to ban them", _params[0]));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BanPlayer.Execute: {0}", e));
            }
        }
    }
}
