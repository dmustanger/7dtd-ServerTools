using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoadProcess
    {
        public static int Days_Before_Log_Delete = 5;
        public static bool Loaded = false, ResettingChunks = false;

        public static void Load()
        {
            try
            {
                if (!Loaded)
                {
                    Loaded = true;
                    string gamePath = API.GamePath;
                    string configPath = API.ConfigPath;
                    string installPath = gamePath + "/Mods/ServerTools";
                    if (!Directory.Exists(installPath))
                    {
                        Log.Out("[SERVERTOOLS] Unable to locate ServerTools installation files in a mods folder located at '{0}'", installPath);
                        return;
                    }
                    else if (!File.Exists(installPath + "/ServerTools.dll"))
                    {
                        Log.Out("[SERVERTOOLS] Unable to locate ServerTools.dll file in the mods folder located at '{0}'", installPath);
                        return;
                    }
                    if (Directory.Exists(configPath))
                    {
                        Log.Out("[SERVERTOOLS] Located xml and log directory at '{0}'", API.ConfigPath);
                        Log.Out("[SERVERTOOLS] Tool XML and log files for ServerTools will be placed in this folder");
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
                    if (!Directory.Exists(configPath + "/Logs/ShopLogs"))
                    {
                        Directory.CreateDirectory(configPath + "/Logs/ShopLogs");
                    }

                    if (!Directory.Exists(installPath + "/Config"))
                    {
                        Directory.CreateDirectory(installPath + "/Config");
                    }
                    if (!Directory.Exists(installPath + "/Config/XUi"))
                    {
                        Directory.CreateDirectory(installPath + "/Config/XUi");
                    }
                    if (Directory.Exists(installPath + "/WebAPI"))
                    {
                        WebAPI.Directory = installPath + "/WebAPI/";
                    }
                    if (Directory.Exists(installPath + "/Config"))
                    {
                        GeneralOperations.XPathDir = installPath + "/Config/";
                    }
                    if (PersistentContainer.Instance.Load())
                    {
                        Log.Out("[SERVERTOOLS] Data loaded");
                    }
                    RunTimePatch.PatchAll();
                    Config.Load();
                    GeneralOperations.CreateCustomXUi();
                    DamageDetector.GetClaimProtectionLevel();
                    GeneralOperations.GetProtectionLevel();
                    GeneralOperations.Player_Killing_Mode = GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
                    GeneralOperations.StartTime = DateTime.Now;
                    CommandList.Load();
                    ModularLoader.Load();
                    Phrases.Load();
                    HowToSetup.Load();
                    if (SleeperRespawn.IsEnabled)
                    {
                        try
                        {
                            SdtdConsole.Instance.ExecuteSync("sleeper r", null);
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

                        Log.Out("[SERVERTOOLS] Xml log clean up complete");
                    }
                    catch (XmlException e)
                    {
                        Log.Out("[SERVERTOOLS] Failed to delete old logs. Error = {0}", e.Message);
                    }

                    if (Directory.Exists(GameIO.GetGamePath().Replace("..", "")))
                    {
                        DirectoryInfo parent = Directory.GetParent(GameIO.GetGamePath().Replace("..", "")).Parent;
                        if (Directory.Exists(parent.FullName + "/Data/ItemIcons"))
                        {
                            Log.Out("[SERVERTOOLS] Located folder containing game icons");
                            WebAPI.Icon_Folder = parent.FullName + "/Data/ItemIcons";
                        }
                        else
                        {
                            Log.Out("[SERVERTOOLS] Unable to locate game icons. Shop, Auction and Web panels will be affected by this");
                        }
                    }
                    if (PersistentContainer.Instance.WorldSeed == 0)
                    {
                        PersistentContainer.Instance.WorldSeed = GameManager.Instance.World.Seed;
                    }
                    else if (PersistentContainer.Instance.WorldSeed != GameManager.Instance.World.Seed)
                    {
                        PersistentContainer.Instance.WorldSeed = GameManager.Instance.World.Seed;
                        if (PersistentContainer.Instance.Players.IDs.Count > 0)
                        {
                            CleanBin.ClearFirstClaims();
                            Log.Out("[SERVERTOOLS] Detected a new world. Some old data has been cleaned up but the majority remains. Run the Clean_Bin tool to remove the data of your choice");
                        }
                    }
                    if (PersistentContainer.Instance.ConnectionTimeOut == null)
                    {
                        Dictionary<string, DateTime> timeOuts = new Dictionary<string, DateTime>();
                        PersistentContainer.Instance.ConnectionTimeOut = timeOuts;
                    }
                    if (PersistentContainer.Instance.ShopLog == null)
                    {
                        List<string[]> log = new List<string[]>();
                        PersistentContainer.Instance.ShopLog = log;
                    }
                    if (PersistentContainer.Instance.Track == null)
                    {
                        List<string[]> track = new List<string[]>();
                        PersistentContainer.Instance.Track = track;
                    }
                    PersistentContainer.DataChange = true;
                    if (RegionReset.IsEnabled || ChunkReset.IsEnabled)
                    {
                        ResettingChunks = true;
                        Timers.ChunkRegionResetTimer();
                        Log.Out("[SERVERTOOLS] Running Region and Chunk reset in 10 seconds");
                    }
                    if (CleanBin.IsEnabled)
                    {
                        CleanBin.Exec();
                        CleanBin.IsEnabled = false; 
                        CleanBin.Auction = false;
                        CleanBin.Bank = false;
                        CleanBin.Bounties = false;
                        CleanBin.Player_Delays = false;
                        CleanBin.Homes = false;
                        CleanBin.Jail = false;
                        CleanBin.Lobby = false;
                        CleanBin.Market = false;
                        CleanBin.New_Spawn_Tele = false;
                        CleanBin.Poll = false;
                        CleanBin.Protected_Zones = false;
                        CleanBin.Shop_Log = false;
                        CleanBin.Waypoints = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        Log.Out("[SERVERTOOLS] ServerTools.bin has been cleaned. The Clean_Bin tool and all of its options are now disabled");
                    }
                    Track.Cleanup();
                    Timers.Currency_Tag_Timer();
                    Timers.PersistentDataSave();
                    Log.Out("[SERVERTOOLS] Running ServerTools Config v.{0}", Config.Version);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoadProcess.Load: {0}", e.Message));
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

        public static void ChunkRegionReset()
        {
            if (RegionReset.IsEnabled)
            {
                RegionReset.Exec();
            }
            if (ChunkReset.IsEnabled)
            {
                ChunkReset.Exec();
            }
            ResettingChunks = false;
            Log.Out("[SERVERTOOLS] Region and Chunk reset has completed");
        }
    }
}
