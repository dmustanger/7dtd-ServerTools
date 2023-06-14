using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TestConsole : ConsoleCmdAbstract
    {

        protected override string getDescription()
        {
            return "[ServerTools] - Test a command";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-test\n" +
                "  2. st-test off\n" +
                "1. Runs the test\n" +
                "2. Disables the test\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Test", "test" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (GameManager.Instance.World == null)
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] The world is not loaded. Unable to run command");
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
