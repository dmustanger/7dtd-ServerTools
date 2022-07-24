using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoadProcess
    {
        public static int Days_Before_Log_Delete = 5;
        private static bool Loaded = false;

        public static void Load()
        {
            if (!Loaded)
            {
                Loaded = true;
                string configPath = API.ConfigPath;
                if (!Directory.Exists(configPath.Replace("ServerTools", "Mods") + "/ServerTools"))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate installation files at '{0}'. ServerTools has been disabled", configPath.Replace("ServerTools", "Mods") + "/ServerTools"));
                    return;
                }
                else if (!File.Exists(configPath.Replace("ServerTools", "Mods") + "/ServerTools/ServerTools.dll"))
                {
                    Log.Out(string.Format("[SERVERTOOLS] Unable to locate installation files at '{0}'. ServerTools has been disabled", configPath.Replace("ServerTools", "Mods") + "/ServerTools"));
                    return;
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Located ServerTools installation files at '{0}'", configPath.Replace("ServerTools", "Mods") + "/ServerTools"));
                }
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                    Log.Out(string.Format("[SERVERTOOLS] Created new directory at '{0}'", configPath));
                    Log.Out("[SERVERTOOLS] XML and log files for ServerTools will be placed in this folder");
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Located xml and log directory at '{0}'", configPath));
                }
                if (!Directory.Exists(configPath + "/Logs/ChatLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/ChatLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/DetectionLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/DetectionLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/BountyLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/BountyLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/AuctionLog"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/AuctionLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/BankLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/BankLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/DupeLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/DupeLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/PlayerLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/PlayerLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/PlayerReports"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/PlayerReports");
                }
                if (!Directory.Exists(configPath + "/Logs/PollLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/PollLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/ChatCommandLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/ChatCommandLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/DamageLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/DamageLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/BlockLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/BlockLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/ConsoleCommandLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/ConsoleCommandLogs");
                }
                if (!Directory.Exists(configPath + "/Logs/WebAPILogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/WebAPILogs");
                }
                if (!Directory.Exists(configPath + "/Logs/OutputLogs"))
                {
                    Directory.CreateDirectory(configPath + "/Logs/OutputLogs");
                }

                string gamePath = API.GamePath;
                if (!Directory.Exists(gamePath + "/Mods/ServerTools/Config"))
                {
                    Directory.CreateDirectory(gamePath + "/Mods/ServerTools/Config");
                }
                if (!Directory.Exists(gamePath + "/Mods/ServerTools/Config/XUi"))
                {
                    Directory.CreateDirectory(gamePath + "/Mods/ServerTools/Config/XUi");
                }
                if (Directory.Exists(gamePath + "/Mods/ServerTools/WebAPI"))
                {
                    WebAPI.Directory = gamePath + "/Mods/ServerTools/WebAPI/";
                }
                if (Directory.Exists(gamePath + "/Mods/ServerTools/Config"))
                {
                    PersistentOperations.XPathDir = gamePath + "/Mods/ServerTools/Config/";
                }

                StateManager.Awake();

                RunTimePatch.PatchAll();

                PersistentOperations.CreateCustomXUi();
                PersistentOperations.GetCurrencyName();
                PersistentOperations.GetMeleeHandPlayer();
                PersistentOperations.EntityIdList();
                PersistentOperations.Player_Killing_Mode = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);

                Config.Load();
                Log.Out("[SERVERTOOLS] Running ServerTools Config v.{0}", Config.Version);

                CommandList.BuildList();
                CommandList.Load();

                Mods.Load();

                Phrases.Load();

                HowToSetup.Load();

                if (Fps.IsEnabled)
                {
                    Fps.SetTarget();
                }

                if (SleeperRespawn.IsEnabled)
                {
                    try
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("sleeperreset"), null);
                    }
                    catch (XmlException e)
                    {
                        Log.Out("[SERVERTOOLS] Failed to reset sleeper spawn points. Error = {0}", e.Message);
                    }
                }
                try
                {
                    DeleteFiles("AuctionLogs");
                    DeleteFiles("BankLogs");
                    DeleteFiles("BlockLogs");
                    DeleteFiles("BountyLogs");
                    DeleteFiles("ChatCommandLogs");
                    DeleteFiles("ChatLogs");
                    DeleteFiles("ConsoleCommandLogs");
                    DeleteFiles("DamageLogs");
                    DeleteFiles("DetectionLogs");
                    DeleteFiles("DupeLogs");
                    DeleteFiles("OutputLogs");
                    DeleteFiles("PlayerLogs");
                    DeleteFiles("PlayerReports");
                    DeleteFiles("PollLogs");
                    DeleteFiles("WebAPILogs");

                    Log.Out(string.Format("[SERVERTOOLS] Xml log clean up complete"));
                }
                catch (XmlException e)
                {
                    Log.Out("[SERVERTOOLS] Failed to delete old logs. Error = {0}", e.Message);
                }

                if (PersistentContainer.Instance.WorldSeed == 0)
                {
                    PersistentContainer.Instance.WorldSeed = GameManager.Instance.World.Seed;
                    PersistentContainer.DataChange = true;
                }
                else if (PersistentContainer.Instance.WorldSeed != GameManager.Instance.World.Seed)
                {
                    PersistentContainer.Instance.WorldSeed = GameManager.Instance.World.Seed;
                    PersistentContainer.DataChange = true;
                    if (!CleanBin.IsEnabled && PersistentContainer.Instance.Players.IDs.Count > 0)
                    {
                        Log.Out("[SERVERTOOLS] Detected a new world. You have old ServerTools data saved from the last map. Run the Clean_Bin tool to remove the data of your choice");
                    }
                }

                if (PersistentContainer.Instance.Connections == null)
                {
                    Dictionary<string, byte[]> connections = new Dictionary<string, byte[]>();
                    PersistentContainer.Instance.Connections = connections;
                    Dictionary<string, DateTime> timeOuts = new Dictionary<string, DateTime>();
                    PersistentContainer.Instance.ConnectionTimeOut = timeOuts;
                    PersistentContainer.DataChange = true;
                }

                if (CleanBin.IsEnabled)
                {
                    CleanBin.Exec();
                    Log.Out("[SERVERTOOLS] ServerTools.bin has been cleaned. The tool will now disable automatically");
                    CleanBin.IsEnabled = false;
                    Config.WriteXml();
                    Config.LoadXml();
                }

                Track.Cleanup();

                DroppedBagProtection.BuildList();
                BlackJack.BuildDeck();

                ActiveTools.Exec(true);

                Timers.Thirty_Second_Delay();
                Timers.PersistentDataSave();
            }
        }

        private static void DeleteFiles(string name)
        {
            string[] files = Directory.GetFiles(API.ConfigPath + "/Logs/" + name);
            if (files != null && files.Length > 0)
            {
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Now.AddDays(Days_Before_Log_Delete * -1))
                    {
                        fi.Delete();
                    }
                }
            }
        }
    }
}
