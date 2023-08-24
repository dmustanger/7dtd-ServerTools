
namespace ServerTools
{
    public class ModularLoader
    {
        public static bool Startup = false;

        public static void Load()
        {
            DiscordBot.BuildToken();
            if (!Timers.CoreIsRunning)
            {
                Timers.CoreTimerStart();
            }
            if (XRayDetector.IsEnabled && !Timers.HalfSecondIsRunning)
            {
                Timers.HalfSecondTimerStart();
            }
            if (!XRayDetector.IsEnabled && Timers.HalfSecondIsRunning)
            {
                Timers.HalfSecondTimerStop();
            }
            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen)
            {
                Poll.CheckTime();
            }
            if (!ClanManager.IsEnabled)
            {
                ClanManager.Clans.Clear();
                ClanManager.ClanMember.Clear();
            }
            if (!InfoTicker.IsEnabled && InfoTicker.IsRunning)
            {
                InfoTicker.Unload();
            }
            if (InfoTicker.IsEnabled && !InfoTicker.IsRunning)
            {
                InfoTicker.Load();
            }
            if (Gimme.IsRunning && !Gimme.IsEnabled)
            {
                Gimme.Unload();
            }
            if (!Gimme.IsRunning && Gimme.IsEnabled)
            {
                Gimme.Load();
            }
            if (Badwords.IsRunning && !Badwords.IsEnabled)
            {
                Badwords.Unload();
            }
            if (!Badwords.IsRunning && Badwords.IsEnabled)
            {
                Badwords.Load();
            }
            if (!LoginNotice.IsRunning && LoginNotice.IsEnabled)
            {
                LoginNotice.Load();
            }
            if (LoginNotice.IsRunning && !LoginNotice.IsEnabled)
            {
                LoginNotice.Unload();
            }
            if (!Zones.IsRunning && Zones.IsEnabled)
            {
                Zones.Load();
            }
            if (Zones.IsRunning && !Zones.IsEnabled)
            {
                Zones.Unload();
            }
            if (!Voting.IsRunning && Voting.IsEnabled)
            {
                Voting.Load();
            }
            if (Voting.IsRunning && !Voting.IsEnabled)
            {
                Voting.Unload();
            }
            if (!WatchList.IsRunning && WatchList.IsEnabled)
            {
                WatchList.Load();
            }
            if (WatchList.IsRunning && !WatchList.IsEnabled)
            {
                WatchList.Unload();
            }
            if (!ReservedSlots.IsRunning && ReservedSlots.IsEnabled)
            {
                ReservedSlots.Load();
            }
            if (ReservedSlots.IsRunning && !ReservedSlots.IsEnabled)
            {
                ReservedSlots.Unload();
            }
            if (!StartingItems.IsRunning && StartingItems.IsEnabled)
            {
                StartingItems.Load();
            }
            if (StartingItems.IsRunning && !StartingItems.IsEnabled)
            {
                StartingItems.Unload();
            }
            if (!Travel.IsRunning && Travel.IsEnabled)
            {
                Travel.Load();
            }
            if (Travel.IsRunning && !Travel.IsEnabled)
            {
                Travel.Unload();
            }
            if (!Shop.IsRunning && Shop.IsEnabled)
            {
                Shop.Load();
            }
            if (Shop.IsRunning && !Shop.IsEnabled)
            {
                Shop.Unload();
            }
            if (!Motd.IsRunning && Motd.IsEnabled)
            {
                Motd.Load();
            }
            if (Motd.IsRunning && !Motd.IsEnabled)
            {
                Motd.Unload();
            }
            if (InvalidItems.IsRunning && !InvalidItems.IsEnabled)
            {
                InvalidItems.Unload();
            }
            if (!InvalidItems.IsRunning && InvalidItems.IsEnabled)
            {
                InvalidItems.Load();
            }
            if (HighPingKicker.IsRunning && !HighPingKicker.IsEnabled)
            {
                HighPingKicker.Unload();
            }
            if (!HighPingKicker.IsRunning && HighPingKicker.IsEnabled)
            {
                HighPingKicker.Load();
            }
            if (CustomCommands.IsRunning && !CustomCommands.IsEnabled)
            {
                CustomCommands.Unload();
            }
            if (!CustomCommands.IsRunning && CustomCommands.IsEnabled)
            {
                CustomCommands.Load();
            }
            if (DupeLog.IsRunning && !DupeLog.IsEnabled)
            {
                DupeLog.Unload();
            }
            if (!DupeLog.IsRunning && DupeLog.IsEnabled)
            {
                DupeLog.Load();
            }
            if (ChatColor.IsRunning && !ChatColor.IsEnabled)
            {
                ColorList.Unload();
                ChatColor.Unload();
            }
            if (!ChatColor.IsRunning && ChatColor.IsEnabled)
            {
                ColorList.Load();
                ChatColor.Load();
            }
            if (KillNotice.IsRunning && !KillNotice.IsEnabled)
            {
                KillNotice.Unload();
            }
            if (!KillNotice.IsRunning && KillNotice.IsEnabled)
            {
                KillNotice.Load();
            }
            if (Prayer.IsRunning && !Prayer.IsEnabled)
            {
                Prayer.Unload();
            }
            if (!Prayer.IsRunning && Prayer.IsEnabled)
            {
                Prayer.Load();
            }
            if (BloodmoonWarrior.IsRunning && !BloodmoonWarrior.IsEnabled)
            {
                BloodmoonWarrior.Unload();
            }
            else if (!BloodmoonWarrior.IsRunning && BloodmoonWarrior.IsEnabled)
            {
                BloodmoonWarrior.Load();
            }
            if (Waypoints.IsRunning && !Waypoints.Public_Waypoints)
            {
                Waypoints.Unload();
            }
            else if (!Waypoints.IsRunning && Waypoints.Public_Waypoints)
            {
                Waypoints.Load();
            }
            if (LevelUp.IsRunning && !LevelUp.IsEnabled)
            {
                LevelUp.Unload();
            }
            else if (!LevelUp.IsRunning && LevelUp.IsEnabled)
            {
                LevelUp.Load();
            }
            if (BotResponse.IsRunning && !BotResponse.IsEnabled)
            {
                BotResponse.Unload();
            }
            else if (!BotResponse.IsRunning && BotResponse.IsEnabled)
            {
                BotResponse.Load();
            }
            if (LandClaimCount.IsRunning && !LandClaimCount.IsEnabled)
            {
                LandClaimCount.Unload();
            }
            else if (!LandClaimCount.IsRunning && LandClaimCount.IsEnabled)
            {
                LandClaimCount.Load();
            }
            if (ProtectedZones.IsRunning && !ProtectedZones.IsEnabled)
            {
                ProtectedZones.Unload();
            }
            else if (!ProtectedZones.IsRunning && ProtectedZones.IsEnabled)
            {
                ProtectedZones.Load();
            }
            if (InteractiveMap.IsRunning && !InteractiveMap.IsEnabled)
            {
                InteractiveMap.Unload();
            }
            else if (!InteractiveMap.IsRunning && InteractiveMap.IsEnabled)
            {
                InteractiveMap.Load();
            }
            if (BigHead.IsEnabled && !BigHead.IsRunning)
            {
                BigHead.Enable();
            }
            else if (!BigHead.IsEnabled && BigHead.IsRunning)
            {
                BigHead.Disable();
            }
            if (InvalidBuffs.IsRunning && !InvalidBuffs.IsEnabled)
            {
                InvalidBuffs.Unload();
            }
            else if (!InvalidBuffs.IsRunning && InvalidBuffs.IsEnabled)
            {
                InvalidBuffs.Load();
            }
            if (OutputLogBlocker.IsRunning && !OutputLogBlocker.IsEnabled)
            {
                OutputLogBlocker.Unload();
            }
            else if (!OutputLogBlocker.IsRunning && OutputLogBlocker.IsEnabled)
            {
                OutputLogBlocker.Load();
            }
            if (ChunkReset.IsRunning && !ChunkReset.IsEnabled)
            {
                ChunkReset.Unload();
            }
            else if (!ChunkReset.IsRunning && ChunkReset.IsEnabled)
            {
                ChunkReset.Load();
            }
            if (RegionReset.IsRunning && !RegionReset.IsEnabled)
            {
                RegionReset.Unload();
            }
            else if (!RegionReset.IsRunning && RegionReset.IsEnabled)
            {
                RegionReset.Load();
            }
            if (ClanManager.IsEnabled)
            {
                ClanManager.ClanList();
            }
            if (Auction.IsEnabled)
            {
                Auction.AuctionList();
            }
            if (Mute.IsEnabled)
            {
                Mute.ClientMuteList();
                Mute.MuteList();
            }
            if (Jail.IsEnabled)
            {
                Jail.JailList();
            }
            if (OversizedTraps.IsEnabled && !OversizedTraps.IsRunning)
            {
                OversizedTraps.CreateXPath();
            }
            else if(OversizedTraps.IsRunning && !OversizedTraps.IsEnabled)
            {
                OversizedTraps.RemoveXPath();
            }
            if (WebAPI.IsEnabled && !WebAPI.IsRunning)
            {
                WebAPI.Load();
            }
            else if (WebAPI.IsRunning && !WebAPI.IsEnabled)
            {
                WebAPI.Unload();
                Log.Out("[SERVERTOOLS] The various panels offered by ServerTools are now unavailable due to the Web_API being disabled");
            }
            if (WebPanel.IsEnabled)
            {
                if (!WebPanel.Alert)
                {
                    if (WebAPI.Panel_Address != "")
                    {
                        WebPanel.Alert = true;
                        Log.Out("[SERVERTOOLS] ServerTools web panel link @ '{0}'", WebAPI.Panel_Address);
                    }
                    else
                    {
                        Timers.WebPanelAlertTimer();
                    }
                }
            }
            else if (WebPanel.Alert)
            {
                WebPanel.Alert = false;
            }
        }
    }
}