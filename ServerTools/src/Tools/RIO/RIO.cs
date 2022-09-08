using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    class RIO
    {
        public static bool IsEnabled = false;
        public static string Directory = "", Command_rio = "rio";
        public static int Bet = 25;

        public static Dictionary<string, int> Access = new Dictionary<string, int>();
        public static Dictionary<string, int[]> Tables = new Dictionary<string, int[]>();
        public static Dictionary<string, Dictionary<int, string>> Events = new Dictionary<string, Dictionary<int, string>>();
        public static Dictionary<string, Dictionary<string, int>> Claims = new Dictionary<string, Dictionary<string, int>>();

        private static readonly string AlphaSet = "JKQRLWBYZMPSNODHEFXTACGIUV", NumSet = "1928374650", DieSet = "136425";

        public static void Exec(ClientInfo _cInfo)
        {
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
            {
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Overlay is set off. Steam browser is disabled" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            if (WebAPI.IsEnabled && WebAPI.Connected)
            {
                string securityId = "";
                for (int i = 0; i < 10; i++)
                {
                    string pass = CreatePassword(4);
                    if (!Access.ContainsKey(pass))
                    {
                        securityId = pass;
                        if (!Access.ContainsValue(_cInfo.entityId))
                        {
                            Access.Add(securityId, _cInfo.entityId);
                            WebAPI.AuthorizedTime.Add(securityId, DateTime.Now.AddMinutes(5));
                        }
                        else
                        {
                            if (Access.Count > 0)
                            {
                                var accessList = Access.ToArray();
                                for (int j = 0; j < accessList.Length; j++)
                                {
                                    if (accessList[j].Value == _cInfo.entityId)
                                    {
                                        Access.TryGetValue(accessList[j].Key, out int id);
                                        var tables = Tables.ToArray();
                                        for (int k = 0; k < tables.Length; k++)
                                        {
                                            int[] players = tables[k].Value;
                                            for (int l = 0; l < players.Length; l++)
                                            {
                                                if (players[l] == id)
                                                {
                                                    players[i] = -1;
                                                    Tables[tables[k].Key] = players;
                                                    if (Events.TryGetValue(tables[k].Key, out Dictionary<int, string> events))
                                                    {
                                                        events.Add(events.Count, "Left╚" + (i + 1));
                                                    }
                                                }
                                            }
                                        }
                                        Access.Remove(accessList[j].Key);
                                        var authorizedList = WebAPI.Authorized.ToArray();
                                        for (int k = 0; k < authorizedList.Length; k++)
                                        {
                                            if (authorizedList[k].Key == accessList[j].Key)
                                            {
                                                WebAPI.Authorized.Remove(authorizedList[k].Key);
                                                WebAPI.AuthorizedTime.Remove(authorizedList[k].Key);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            Access.Add(securityId, _cInfo.entityId);
                            WebAPI.AuthorizedTime.Add(securityId, DateTime.Now.AddMinutes(5));
                        }
                        break;
                    }
                }
                Phrases.Dict.TryGetValue("Rio1", out string phrase);
                phrase = phrase.Replace("{Value}", securityId);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserRio", true));
            }
        }

        public static void SetLink()
        {
            try
            {
                if (File.Exists(PersistentOperations.XPathDir + "XUi/windows.xml"))
                {
                    string link = string.Format("http://{0}:{1}/rio.html", WebAPI.BaseAddress, WebAPI.Port);
                    List<string> lines = File.ReadAllLines(PersistentOperations.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserRio"))
                        {
                            if (!lines[i + 7].Contains(link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link);
                                File.WriteAllLines(PersistentOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                            }
                            return;
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("/append"))
                        {
                            lines.RemoveRange(i, 3);
                            lines.Add("  <window name=\"browserRio\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Roll It Out\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"coinIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_coin\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                            lines.Add("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                            lines.Add("      </panel>");
                            lines.Add("  </window>");
                            lines.Add("");
                            lines.Add("</append>");
                            lines.Add("");
                            lines.Add("</configs>");
                            File.WriteAllLines(PersistentOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                            return;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", PersistentOperations.XPathDir + "XUi/windows.xml", e.Message));
            }
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += AlphaSet.ElementAt(rnd.Next(0, 26));
            }
            return pass;
        }

        public static string CreateTable()
        {
            string tableNumber = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < 4; i++)
            {
                tableNumber += NumSet.ElementAt(rnd.Next(0, 10));
            }
            return tableNumber;
        }

        public static string GetRoll()
        {
            string dice = string.Empty;
            Random rnd = new System.Random();
            for (int i = 0; i < 5; i++)
            {
                dice += DieSet.ElementAt(rnd.Next(0, 6)) + ",";
            }
            dice = dice.Remove(dice.Length - 1);
            return dice;
        }

        public static void RemovePlayer(ClientInfo _cInfo)
        {
            var tables = Tables.ToArray();
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i].Value.Contains(_cInfo.entityId))
                {

                    break;
                }
            }
        }
    }
}
