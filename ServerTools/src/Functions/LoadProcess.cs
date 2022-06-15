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
            string assemblyVersionNoDecimal = Config.Version.Replace(".", "");
            int.TryParse(assemblyVersionNoDecimal, out int assemblyVersion);
            for (int i = 0; i < ModManager.GetLoadedMods().Count; i++)
            {
                Mod mod = ModManager.GetLoadedMods()[i];
                if (mod.ModInfo.Name.Value.Contains("ServerTools") && mod.ModInfo.Version != null)
                {
                    string versionNoDecimal = mod.ModInfo.Version.Value.Replace(".", "");
                    int.TryParse(versionNoDecimal, out int version);
                    if (assemblyVersion < version)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Disabled version '{0}' of ServerTools. Version '{1}' was detected operating simultaneously", Config.Version, version));
                        return;
                    }
                }
            }

            string configPath = API.ConfigPath;
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
                Log.Out(string.Format("[SERVERTOOLS] Created new directory '{0}'", configPath));
            }
            else
            {
                Log.Out(string.Format("[SERVERTOOLS] Located directory '{0}'", configPath));
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
            if (!Directory.Exists(configPath + "/Logs/WebPanelLogs"))
            {
                Directory.CreateDirectory(configPath + "/Logs/WebPanelLogs");
            }
            if (!Directory.Exists(configPath + "/Logs/OutputLogs"))
            {
                Directory.CreateDirectory(configPath + "/Logs/OutputLogs");
            }
            if (!Directory.Exists(configPath + "/Mods/ServerTools/Config"))
            {
                Directory.CreateDirectory(configPath + "/Mods/ServerTools/Config");
            }
            if (!Directory.Exists(configPath + "/Mods/ServerTools/Config/XUi"))
            {
                Directory.CreateDirectory(configPath + "/Mods/ServerTools/Config/XUi");
            }
            string gamePath = API.GamePath;
            if (Directory.Exists(gamePath + "/Mods/ServerTools"))
            {
                if (Directory.Exists(gamePath + "/Mods/ServerTools/WebAPI"))
                {
                    WebAPI.Directory = gamePath + "/Mods/ServerTools/WebAPI/";
                }
                if (Directory.Exists(gamePath + "/Mods/ServerTools/Config"))
                {
                    PersistentOperations.XPathDir = gamePath + "/Mods/ServerTools/Config/";
                }
            }

            StateManager.Awake();

            RunTimePatch.PatchAll();

            Config.Load();
            Log.Out("[SERVERTOOLS] Running ServerTools Config v.{0}", Config.Version);

            CommandList.BuildList();
            CommandList.Load();

            PersistentOperations.CreateCustomXUi();
            PersistentOperations.GetCurrencyName();
            PersistentOperations.GetMeleeHandPlayer();
            PersistentOperations.EntityIdList();
            PersistentOperations.Player_Killing_Mode = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);

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

            Timers.Thirty_Second_Delay();
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
