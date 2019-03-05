using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class PlayerLogsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Player Logs.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. PlayerLogs off\n" +
                   "  2. PlayerLogs on\n" +
                   "1. Turn off player logs\n" +
                   "2. Turn on player logs\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-PlayerLogs", "playerlogs" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    PlayerLogs.IsEnabled = false;
                    XmlDocument doc = new XmlDocument();
                    doc.Load("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    XmlNodeList aNodes = doc.SelectNodes("/ServerTools/Tools");
                    foreach (XmlNode aNode in aNodes)
                    {
                        XmlAttribute _attribute1 = aNode.Attributes["Name"];
                        XmlAttribute _attribute2 = aNode.Attributes["Enable"];
                        if (_attribute1 != null && _attribute1.Value == "Player_Logs" && _attribute2 != null)
                        {
                            _attribute2.Value = "False";
                        }
                    }
                    doc.Save("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    SdtdConsole.Instance.Output(string.Format("Player logs has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    PlayerLogs.IsEnabled = true;
                    XmlDocument doc = new XmlDocument();
                    doc.Load("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    XmlNodeList aNodes = doc.SelectNodes("/ServerTools/Tools");
                    foreach (XmlNode aNode in aNodes)
                    {
                        XmlAttribute _attribute1 = aNode.Attributes["Name"];
                        XmlAttribute _attribute2 = aNode.Attributes["Enable"];
                        if (_attribute1 != null && _attribute1.Value == "Player_Logs" && _attribute2 != null)
                        {
                            _attribute2.Value = "True";
                        }
                    }
                    doc.Save("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    SdtdConsole.Instance.Output(string.Format("Player logs has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerLogsConsole.Run: {0}.", e));
            }
        }
    }
}