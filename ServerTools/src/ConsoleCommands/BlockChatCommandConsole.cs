using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BlockChatCommandConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Blocks chat commands for a specific player";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-bcc <Id/EntityId/PlayerName>\n" +
                "  2. st-bcc list\n" +
                "1. Activates or disables the block on chat commands for the specific player\n" +
                "2. Shows a list of the current players with blocked chat commands\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-BlockChatCommands", "bcc", "st-bcc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower() == "list")
                {
                    if (PersistentOperations.BlockChatCommands.Count > 0)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Blocked chat command list:");
                        for (int i = 0; i < PersistentOperations.BlockChatCommands.Count; i++)
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Id '{0}' named '{1}'", PersistentOperations.BlockChatCommands[i].CrossplatformId.CombinedString, PersistentOperations.BlockChatCommands[i].playerName));
                        }
                    }
                    return;
                }
                ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[0]);
                if (cInfo != null)
                {
                    if (PersistentOperations.BlockChatCommands.Contains(cInfo))
                    {
                        PersistentOperations.BlockChatCommands.Remove(cInfo);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed player '{0}' named '{1}' from the blocked chat command list", cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                    }
                    else
                    {
                        PersistentOperations.BlockChatCommands.Add(cInfo);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added player '{0}' named '{1}' to the blocked chat command list", cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                    }
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}'", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BlockCommandConsole.Execute: {0}", e.Message));
            }
        }
    }
}
