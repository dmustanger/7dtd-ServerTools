using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class TestConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Test a command";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-test\n" +
                "  2. st-test off\n" +
                "1. Runs the test\n" +
                "2. Disables the test\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Test", "test" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (GameManager.Instance.World == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] The world is not loaded. Unable to run command");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TestConsole.Execute: '{0}'", e.Message));
            }
        }
    }
}
