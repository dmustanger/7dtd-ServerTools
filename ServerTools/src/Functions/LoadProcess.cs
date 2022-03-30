using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoadProcess
    {
        public static int Days_Before_Log_Delete = 5;

        public static void Load()
        {
            if (!Directory.Exists(API.ConfigPath))
            {
                Directory.CreateDirectory(API.ConfigPath);
                Log.Out(string.Format("[SERVERTOOLS] Created new directory '{0}'", API.ConfigPath));
            }
            else
            {
                Log.Out(string.Format("[SERVERTOOLS] Located directory '{0}'", API.ConfigPath));
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/ChatLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/ChatLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/DetectionLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/DetectionLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/BountyLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/BountyLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/AuctionLog"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/AuctionLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/BankLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/BankLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/DupeLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/DupeLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/PlayerReports"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/PlayerReports");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/PollLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/PollLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/ChatCommandLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/ChatCommandLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/DamageLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/DamageLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/BlockLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/BlockLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/ConsoleCommandLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/ConsoleCommandLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/WebPanelLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/WebPanelLogs");
            }
            if (!Directory.Exists(API.ConfigPath + "/Logs/OutputLogs"))
            {
                Directory.CreateDirectory(API.ConfigPath + "/Logs/OutputLogs");
            }
            if (!Directory.Exists(API.GamePath + "/Mods/ServerTools/Config"))
            {
                Directory.CreateDirectory(API.GamePath + "/Mods/ServerTools/Config");
            }
            if (!Directory.Exists(API.GamePath + "/Mods/ServerTools/Config/XUi"))
            {
                Directory.CreateDirectory(API.GamePath + "/Mods/ServerTools/Config/XUi");
            }

            StateManager.Awake();

            RunTimePatch.PatchAll();

            Config.Load();
            Log.Out("[SERVERTOOLS] Running ServerTools v.{0}", Config.Version);

            CommandList.BuildList();
            CommandList.Load();

            PersistentOperations.SetFolders();
            PersistentOperations.CreateCustomXUi();
            PersistentOperations.GetCurrencyName();
            PersistentOperations.EntityIdList();
            PersistentOperations.Player_Killing_Mode = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
            PersistentOperations.ThirtySeconds = true;

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
                DeleteFiles("DetectionLogs");
                DeleteFiles("BountyLogs");
                DeleteFiles("AuctionLogs");
                DeleteFiles("BankLogs");
                DeleteFiles("DupeLogs");
                DeleteFiles("PlayerLogs");
                DeleteFiles("PlayerReports");
                DeleteFiles("PollLogs");
                DeleteFiles("ChatLogs");
                DeleteFiles("ChatCommandLogs");
                DeleteFiles("DamageLogs");
                DeleteFiles("BlockLogs");
                DeleteFiles("ConsoleCommandLogs");
                DeleteFiles("WebPanelLogs");
                DeleteFiles("OutputLogs");
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

            EventSchedule.Add("ThirtyMinutes", DateTime.Now.AddMinutes(30));
            Timers.PersistentDataSave();
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
