using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class AdminChatCommandConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Admin Chat Commands.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AdminChatCommand off\n" +
                   "  2. AdminChatCommand on\n" +
                   "1. Turn off server admin chat commands\n" +
                   "2. Turn on server admin chat commands\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AdminChatCommand", "adminchatcommand" };
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
                    AdminChat.IsEnabled = false;
                    XmlDocument doc = new XmlDocument();
                    doc.Load("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    XmlNodeList aNodes = doc.SelectNodes("/ServerTools/Tools");
                    foreach (XmlNode aNode in aNodes)
                    {
                        XmlAttribute _attribute1 = aNode.Attributes["Name"];
                        XmlAttribute _attribute2 = aNode.Attributes["Enable"];
                        if (_attribute1 != null && _attribute1.Value == "Admin_Chat_Commands" && _attribute2 != null)
                        {
                            _attribute2.Value = "False";
                        }
                    }
                    doc.Save("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    SdtdConsole.Instance.Output(string.Format("Admin chat command has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    AdminChat.IsEnabled = true;
                    XmlDocument doc = new XmlDocument();
                    doc.Load("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    XmlNodeList aNodes = doc.SelectNodes("/ServerTools/Tools");
                    foreach (XmlNode aNode in aNodes)
                    {
                        XmlAttribute _attribute1 = aNode.Attributes["Name"];
                        XmlAttribute _attribute2 = aNode.Attributes["Enable"];
                        if (_attribute1 != null && _attribute1.Value == "Admin_Chat_Commands" && _attribute2 != null)
                        {
                            _attribute2.Value = "True";
                        }
                    }
                    doc.Save("@" + API.ConfigPath + "/ServerToolsConfig.xml");
                    SdtdConsole.Instance.Output(string.Format("Admin chat command has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminChatCommandConsole.Run: {0}.", e));
            }
        }
    }
}
