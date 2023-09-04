using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false, IsRunning = false, Permissions = false;

        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        public static List<int> TeleportCheckProtection = new List<int>();
        
        private const string file = "CustomCommands.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Trigger") && line.HasAttribute("Command") && line.HasAttribute("DelayBetweenUses") && line.HasAttribute("Hidden") &&
                            line.HasAttribute("Reserved") && line.HasAttribute("Cost") && line.HasAttribute("Bloodmoon"))
                        {
                            string trigger = line.GetAttribute("Trigger").ToLower();
                            if (trigger == "")
                            {
                                continue;
                            }
                            string command = line.GetAttribute("Command");
                            string delay = line.GetAttribute("DelayBetweenUses");
                            string hidden = line.GetAttribute("Hidden");
                            if (!bool.TryParse(line.GetAttribute("Reserved").ToLower(), out bool reserved))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (true/false) value for 'Reserved' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("Cost"), out int cost))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (non-numeric) value for 'Cost' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!bool.TryParse(line.GetAttribute("Bloodmoon").ToLower(), out bool bloodmoon))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring CustomCommands.xml entry. Invalid (true/false) value for 'Bloodmoon' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            string[] c = { command, delay, hidden, reserved.ToString(), cost.ToString(), bloodmoon.ToString() };
                            if (!Dict.ContainsKey(trigger))
                            {
                                Dict.Add(trigger, c);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeCustomCommandsXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                    Log.Out(string.Format("[SERVERTOOLS] CustomCommands.xml has been created for version {0}", Config.Version));
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<CustomCommands>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName}, {Delay}, {RandomId}, {SetTeleport}, {RemoveTeleport}, {Teleport}, whisper, global -->");
                    sw.WriteLine("    <!-- <Custom Trigger=\"Example\" Command=\"whisper Server Info... ^ global You have triggered the example\" DelayBetweenUses=\"0\" Hidden=\"false\" Reserved=\"false\" Cost=\"0\" Bloodmoon=\"true\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Custom Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Reserved=\"{4}\" Cost=\"{5}\" Bloodmoon=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], kvp.Value[5]));
                        }
                    }
                    sw.WriteLine("</CustomCommands>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }


        public static void CustomCommandList(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    string commands = "";
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        bool.TryParse(kvp.Value[2], out bool hidden);
                        bool.TryParse(kvp.Value[3], out bool reserved);
                        if (!hidden)
                        {
                            if (reserved && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || 
                                ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                            {
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        string c = kvp.Key;
                                        if (GameManager.Instance.adminTools.Commands.GetCommands().ContainsKey(c))
                                        {
                                            string[] commandPermission = { c };
                                            int tier = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId);
                                            if (tier == 0 || tier == GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(commandPermission))
                                            {
                                                continue;
                                            }
                                        }
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, c);
                                        if (commands.Length >= 100)
                                        {
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            commands = "";
                                        }
                                    }
                                }
                                else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        string c = kvp.Key;
                                        if (GameManager.Instance.adminTools.Commands.GetCommands().ContainsKey(c))
                                        {
                                            string[] commandPermission = { c };
                                            int tier = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId);
                                            if (tier == 0 || tier == GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(commandPermission))
                                            {
                                                continue;
                                            }
                                        }
                                        commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, c);
                                        if (commands.Length >= 100)
                                        {
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            commands = "";
                                        }
                                    }
                                }
                                continue;
                            }
                            string d = kvp.Key;
                            if (GameManager.Instance.adminTools.Commands.GetCommands().ContainsKey(d))
                            {
                                string[] commandPermission = { d };
                                int tier = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId);
                                if (tier == 0 || tier == GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(commandPermission))
                                {
                                    continue;
                                }
                            }
                            commands = string.Format("{0} {1}{2}", commands, ChatHook.Chat_Command_Prefix1, d);
                            if (commands.Length >= 100)
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                commands = "";
                            }
                        }
                    }
                    if (commands.Length > 0)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + commands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in CustomCommands.CustomCommandList: {0}", e.Message);
            }
        }

        public static void InitiateCommand(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (Dict.TryGetValue(_command, out string[] c))
                {
                    int.TryParse(c[1], out int delay);
                    int.TryParse(c[4], out int cost);
                    bool.TryParse(c[3], out bool reserved);
                    if (reserved && !ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) && !ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        return;
                    }
                    if (GameManager.Instance.adminTools.Commands.GetCommands().ContainsKey(_command))
                    {
                        string[] commands = { _command };
                        if (!GameManager.Instance.adminTools.CommandAllowedFor(commands, _cInfo))
                        {
                            Phrases.Dict.TryGetValue("CustomCommands2", out string phrase);
                            phrase = phrase.Replace("{Command}", _command);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    bool.TryParse(c[5], out bool bloodmoon);
                    if (!bloodmoon && Bloodmoon(_cInfo, _command))
                    {
                        return;
                    }
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays.ContainsKey(_command))
                    {
                        DateTime lastUse = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays[_command];
                        Delay(_cInfo, _command, delay, cost, lastUse);
                    }
                    else
                    {
                        Delay(_cInfo, _command, delay, cost, DateTime.MinValue);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.ProcessCommand: {0}", e.Message));
            }
        }

        private static bool Bloodmoon(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (GeneralOperations.IsBloodmoon())
                {
                    Phrases.Dict.TryGetValue("CustomCommands5", out string phrase);
                    phrase = phrase.Replace("{Command}", _command);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.Bloodmoon: {0}", e.Message));
            }
            return false;
        }

        private static void Delay(ClientInfo _cInfo, string _command, int _delay, int _cost, DateTime _lastUse)
        {
            TimeSpan varTime = DateTime.Now - _lastUse;
            double fractionalMinutes = varTime.TotalMinutes;
            int timePassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            int newDelay = _delay / 2;
                            TimePass(_cInfo, _command, timePassed, newDelay, _cost);
                            return;
                        }
                    }
                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            int newDelay = _delay / 2;
                            TimePass(_cInfo, _command, timePassed, newDelay, _cost);
                            return;
                        }
                    }
                }
            }
            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
            {
                int newDelay = _delay / 2;
                TimePass(_cInfo, _command, timePassed, newDelay, _cost);
                return;
            }
            TimePass(_cInfo, _command, timePassed, _delay, _cost);
        }

        private static void TimePass(ClientInfo _cInfo, string _command, int _timePassed, int _delay, int _cost)
        {
            if (_timePassed >= _delay)
            {
                CommandCost(_cInfo, _command, _cost);
            }
            else
            {
                int _timeleft = _delay - _timePassed;
                Delayed(_cInfo, _command, _timeleft, _delay);
            }
        }

        private static void Delayed(ClientInfo _cInfo, string _command, int _timeLeft, int _delay)
        {
            try
            {
                Phrases.Dict.TryGetValue("CustomCommands1", out string phrase);
                phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                phrase = phrase.Replace("{CommandCustom}", _command);
                phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                phrase = phrase.Replace("{TimeRemaining}", _timeLeft.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.Delayed: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo, string _command, int _cost)
        {
            try
            {
                if (_cost > 0 && Wallet.IsEnabled || (Bank.IsEnabled && Bank.Direct_Payment))
                {
                    int currency = 0, bankCurrency = 0, cost = _cost;
                    if (Wallet.IsEnabled)
                    {
                        currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                    }
                    if (Bank.IsEnabled && Bank.Direct_Payment)
                    {
                        bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                    }
                    if (currency + bankCurrency >= cost)
                    {
                        if (currency > 0)
                        {
                            if (currency < cost)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                                cost -= currency;
                                Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                            }
                            else
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                            }
                        }
                        else
                        {
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        ProcessCommand(_cInfo, _command, _cost);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("CustomCommands3", out string phrase);
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    ProcessCommand(_cInfo, _command, 0);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandCost: {0}", e.Message));
            }
        }

        private static void ProcessCommand(ClientInfo _cInfo, string _command, int _cost)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays.ContainsKey(_command))
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays[_command] = DateTime.Now;
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays.Add(_command, DateTime.Now);
                    }
                }
                else
                {
                    Dictionary<string, DateTime> delays = new Dictionary<string, DateTime>
                    {
                        { _command, DateTime.Now }
                    };
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomCommandDelays = delays;
                }
                PersistentContainer.DataChange = true;
                if (Dict.TryGetValue(_command, out string[] commandData))
                {
                    if (commandData[0].Contains("^"))
                    {
                        List<string> commands = commandData[0].Split('^').ToList();
                        for (int i = 0; i < commands.Count; i++)
                        {
                            string commandTrimmed = commands[i].Trim();
                            if (commandTrimmed.StartsWith("{Delay}"))
                            {
                                string[] commandSplit = commandTrimmed.Split(' ');
                                if (int.TryParse(commandSplit[1], out int time))
                                {
                                    commands.RemoveRange(0, i + 1);
                                    Timers.Custom_SingleUseTimer(time, _cInfo.CrossplatformId.CombinedString, commands, _command);
                                    return;
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with invalid integer '{0}'", _command));
                                }
                            }
                            else
                            {
                                CommandExec(_cInfo, commandTrimmed, _command);
                            }
                        }
                    }
                    else
                    {
                        CommandExec(_cInfo, commandData[0], _command);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandDelay: {0}", e.Message));
            }
        }

        public static void CustomCommandDelayed(string _playerId, List<string> commands, string _trigger)
        {
            try
            {
                ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_playerId);
                if (cInfo != null)
                {
                    for (int i = 0; i < commands.Count; i++)
                    {
                        string commandTrimmed = commands[i].Trim();
                        if (commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] commandSplit = commandTrimmed.Split(' ');
                            if (int.TryParse(commandSplit[1], out int _time))
                            {
                                commands.RemoveRange(0, i + 1);
                                Timers.Custom_SingleUseTimer(_time, cInfo.CrossplatformId.CombinedString, commands, _trigger);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", commands));
                            }
                        }
                        else
                        {
                            CommandExec(cInfo, commandTrimmed, _trigger);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CustomCommandDelayed: {0}", e.Message));
            }
        }

        private static void CommandExec(ClientInfo _cInfo, string _command, string _trigger)
        {
            try
            {
                if (_cInfo != null)
                {
                    if (_command.Contains("{EntityId}"))
                    {
                        _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                    }
                    if (_command.Contains("{Id}"))
                    {
                        _command = _command.Replace("{Id}", _cInfo.PlatformId.CombinedString);
                    }
                    if (_command.Contains("{EOS}"))
                    {
                        _command = _command.Replace("{EOS}", _cInfo.CrossplatformId.CombinedString);
                    }
                    if (_command.Contains("{PlayerName}"))
                    {
                        _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                    }
                    if (_command.Contains("{RandomId}"))
                    {
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            ClientInfo cInfo2 = clientList.ElementAt(Random.Next(clientList.Count));
                            if (cInfo2 != null)
                            {
                                _command = _command.Replace("{RandomId}", cInfo2.CrossplatformId.CombinedString);
                            }
                        }
                    }
                    if (_command.Contains("{SetTeleport}"))
                    {
                        _command = _command.Substring(_command.IndexOf('}') + 1);
                        _command.Trim(' ');
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                        string position = player.position.x + "," + player.position.y + "," + player.position.z;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions != null)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.ContainsKey(_command))
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions[_command] = position;
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.Add(_command, position);
                            }
                        }
                        else
                        {
                            Dictionary<string, string> positions = new Dictionary<string, string>();
                            positions.Add(_command, position);
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions = positions;
                        }
                        PersistentContainer.DataChange = true;
                    }
                    else if (_command.Contains("{RemoveTeleport}"))
                    {
                        _command = _command.Substring(_command.IndexOf('}') + 1);
                        _command.Trim(' ');
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions != null)
                        {
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.ContainsKey(_command))
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.Remove(_command);
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                    else if (_command.Contains("{Teleport}"))
                    {
                        _command = _command.Substring(_command.IndexOf('}') + 1);
                        _command.Trim(' ');
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.ContainsKey(_command))
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].CustomReturnPositions.TryGetValue(_command, out string position);
                            string[] cords = position.Split(',');
                            float.TryParse(cords[0], out float x);
                            float.TryParse(cords[1], out float y);
                            float.TryParse(cords[2], out float z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                        }
                    }
                    else
                    {
                        if (_command.ToLower().StartsWith("global"))
                        {
                            _command = _command.Replace("global", "");
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else if (_command.ToLower().StartsWith("whisper"))
                        {
                            _command = _command.Replace("whisper", "");
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else if (_command.ToLower().StartsWith("tp"))
                        {
                            _command = _command.Replace("tp", "teleportplayer");
                            SdtdConsole.Instance.ExecuteSync(_command, null);
                        }
                        else if (_command.ToLower().StartsWith("tele"))
                        {
                            _command = _command.Replace("tele", "teleportplayer");
                            SdtdConsole.Instance.ExecuteSync(_command, null);
                        }
                        else
                        {
                            SdtdConsole.Instance.ExecuteSync(_command, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommand.CommandExec: {0}", e.Message));
            }
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<CustomCommands>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName}, {Delay}, {RandomId}, {SetTeleport}, {RemoveTeleport}, {Teleport}, whisper, global -->");
                    sw.WriteLine("    <!-- <Custom Trigger=\"Example\" Command=\"whisper Server Info... ^ global You have triggered the example\" DelayBetweenUses=\"0\" Hidden=\"false\" Reserved=\"false\" Cost=\"0\" Bloodmoon=\"true\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- Possible variables") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Custom Trigger=\"Example\"") && !nodeList[i].OuterXml.Contains("<Custom Trigger=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && (line.Name == "Custom" || line.Name == "Command"))
                            {
                                string trigger = "", command = "", delay = "0", hidden = "false", reserved = "false", cost = "0", bloodmoon = "false";
                                if (line.HasAttribute("Trigger"))
                                {
                                    trigger = line.GetAttribute("Trigger");
                                }
                                if (line.HasAttribute("Command"))
                                {
                                    command = line.GetAttribute("Command");
                                }
                                if (line.HasAttribute("DelayBetweenUses"))
                                {
                                    delay = line.GetAttribute("DelayBetweenUses");
                                }
                                if (line.HasAttribute("Hidden"))
                                {
                                    hidden = line.GetAttribute("Hidden");
                                }
                                if (line.HasAttribute("Reserved"))
                                {
                                    reserved = line.GetAttribute("Reserved");
                                }
                                if (line.HasAttribute("Cost"))
                                {
                                    cost = line.GetAttribute("Cost");
                                }
                                if (line.HasAttribute("Bloodmoon"))
                                {
                                    bloodmoon = line.GetAttribute("Bloodmoon");
                                }
                                sw.WriteLine(string.Format("    <Custom Trigger=\"{0}\" Command=\"{1}\" DelayBetweenUses=\"{2}\" Hidden=\"{3}\" Reserved=\"{4}\" Cost=\"{5}\" Bloodmoon=\"{6}\" />", trigger, command, delay, hidden, reserved, cost, bloodmoon));
                            }
                        }
                    }
                    sw.WriteLine("</CustomCommands>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommands.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}