using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NormalPlayerColoringConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable normal player coloring.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-npc off\n" +
                   "  2. st-npc on\n" +
                   "1. Turn off normal player coloring\n" +
                   "2. Turn on normal player coloring\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-NormalPlayerColoring", "npc", "st-npc" };
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
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatHook.Normal_Player_Color_Prefix)
                    {
                        ChatHook.Normal_Player_Color_Prefix = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Normal player coloring has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Normal player coloring is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (ChatHook.Normal_Player_Color_Prefix)
                    {
                        ChatHook.Normal_Player_Color_Prefix = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Normal player coloring has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Normal player coloring is already on"));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in NormalPlayerColoringConsole.Execute: {0}", e.Message));
            }
        }
    }
}