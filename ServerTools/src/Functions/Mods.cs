
namespace ServerTools
{
    public class Mods
    {
        public static bool Startup = false;

        public static void Load()
        {
            PersistentOperations.SetInstallFolder();
            if (!DiscordBot.TokenLoaded)
            {
                DiscordBot.BuildToken();
            }
            if (!RunTimePatch.Applied)
            {
                RunTimePatch.PatchAll();
            }
            if (!Timers.IsRunning)
            {
                Timers.TimerStart();
            }
            if (!CommandList.IsRunning)
            {
                CommandList.Load();
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
            if (!VoteReward.IsRunning && VoteReward.IsEnabled)
            {
                VoteReward.Load();
            }
            if (VoteReward.IsRunning && !VoteReward.IsEnabled)
            {
                VoteReward.Unload();
            }
            if (!Watchlist.IsRunning && Watchlist.IsEnabled)
            {
                Watchlist.Load();
            }
            if (Watchlist.IsRunning && !Watchlist.IsEnabled)
            {
                Watchlist.Unload();
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
            if (CredentialCheck.IsRunning && !CredentialCheck.IsEnabled)
            {
                CredentialCheck.Unload();
            }
            if (!CredentialCheck.IsRunning && CredentialCheck.IsEnabled)
            {
                CredentialCheck.Load();
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
            if (ProtectedSpaces.IsRunning && !ProtectedSpaces.IsEnabled)
            {
                ProtectedSpaces.Unload();
            }
            else if (!ProtectedSpaces.IsRunning && ProtectedSpaces.IsEnabled)
            {
                ProtectedSpaces.Load();
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
            if (PrefabReset.IsEnabled) {
                PrefabReset.Load();
            }


            if (WebAPI.IsEnabled && !WebAPI.IsRunning)
            {
                WebAPI.Load();
            }
            else if (WebAPI.IsRunning && !WebAPI.IsEnabled)
            {
                WebAPI.Unload();
            }
        }
    }
}