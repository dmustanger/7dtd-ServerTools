using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class PhraseResetConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Reset the Phrases.xml file";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-pr\n" +
                   "1. Resets the current Phrases.xml to the default values\n" +
                   "*Note*  This will wipe out all custom entries to this file\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-PhraseReset", "pr", "st-pr" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (!File.Exists(Phrases.FilePath))
                {
                    Phrases.ResetXml();
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Phrase file has been reset to default");
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Phrase file was not found. Unable to reset");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PhraseResetConsole.Execute: {0}", e.Message));
            }
        }
    }
}
