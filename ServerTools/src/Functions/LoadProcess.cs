using System;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoadProcess
    {
        public static int Days_Before_Log_Delete = 5;

        public static void Load(bool _initiating)
        {
            try
            {
                Log.Out(string.Format("[SERVERTOOLS] Checking for save directory {0}", API.ConfigPath));
                if (!Directory.Exists(API.ConfigPath))
                {
                    Directory.CreateDirectory(API.ConfigPath);
                    Log.Out(string.Format("[SERVERTOOLS] Created directory {0}", API.ConfigPath));
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
            }
            catch (XmlException e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in creation of directory @ {0}. Error = {1}", API.ConfigPath, e.Message));
            }

            try
            {
                StateManager.Awake();
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the persistent database bin file. Restart the server and check for errors. Error = {0}", e.Message);
            }

            try
            {
                Config.Load();
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the configuration file. Check for errors in the file. Error = {0}", e.Message);
            }

            try
            {
                Mods.Load(true);
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the tools. Restart the server and check for errors. Error = {0}", e.Message);
            }

            try
            {
                CommandList.LoadXml();
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the CommandList.xml. Check for errors in the file. Error = {0}", e.Message);
            }

            try
            {
                Phrases.Load();
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the Phrases.xml. Check for errors in the file. Error = {0}", e.Message);
            }

            try
            {
                HowToSetup.Load();
            }
            catch (XmlException e)
            {
                Log.Out("[SERVERTOOLS] Failed to load the HowToSetup.xml. Error = {0}", e.Message);
            }

            if (Fps.IsEnabled)
            {
                try
                {
                    Fps.SetTarget();
                }
                catch (XmlException e)
                {
                    Log.Out("[SERVERTOOLS] Failed to set the target fps. Error = {0}", e.Message);
                }
            }

            if (SleeperRespawn.IsEnabled)
            {
                try
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("sleeperreset"), null);
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
                Log.Out(string.Format("[SERVERTOOLS] Log clean up completed"));
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
                if (!CleanBin.IsEnabled)
                {
                    Log.Out("[SERVERTOOLS] Detected a new world. You have old ServerTools data saved from the last map. Run the Clean_Bin tool to remove the data of your choice");
                }
            }

            if (CleanBin.IsEnabled)
            {
                CleanBin.Exec();
                Log.Out("[SERVERTOOLS] ServerTools.bin has been cleaned. The tool will now disable automatically");
                CleanBin.IsEnabled = false;
                Config.WriteXml();
                Config.LoadXml();
            }

            PersistentOperations.EntityIdList();
            PersistentOperations.Player_Killing_Mode = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
            CountryBan.BuildList();
            DroppedBagProtection.BuildList();
            CommandList.BuildList();
            CommandList.Load();
            Track.Cleanup();
            ActiveTools.Exec(true);
            RestartVote.Cycle = true;
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
