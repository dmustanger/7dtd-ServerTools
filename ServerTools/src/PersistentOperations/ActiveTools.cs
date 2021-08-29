using System;
using System.Collections.Generic;


namespace ServerTools
{
    class ActiveTools
    {
        public static List<string> Dict = new List<string>();

        public static void Exec(bool _initiating)
        {
            if (_initiating)
            {
                Log.Out("-------------------------------");
                Log.Out("[SERVERTOOLS] Anti-Cheat tools:");
                Log.Out("-------------------------------");
            }
            if (CredentialCheck.IsEnabled)
            {
                if (!Dict.Contains("Credentials"))
                {
                    Dict.Add("Credentials");
                    Log.Out("Credentials enabled");
                }
            }
            else if (Dict.Contains("Credentials") && !_initiating)
            {
                Dict.Remove("Credentials");
                Log.Out("Credentials disabled");
            }
            if (DamageDetector.IsEnabled)
            {
                if (!Dict.Contains("Damage detector"))
                {
                    Dict.Add("Damage detector");
                    Log.Out("Damage detector enabled");
                }
            }
            else if (Dict.Contains("Damage detector") && !_initiating)
            {
                Dict.Remove("Damage detector");
                Log.Out("Damage detector disabled");
            }
            if (DupeLog.IsEnabled)
            {
                if (!Dict.Contains("Dupe log"))
                {
                    Dict.Add("Dupe log");
                    Log.Out("Dupe log enabled");
                }
            }
            else if (Dict.Contains("Dupe log") && !_initiating)
            {
                Dict.Remove("Dupe log");
                Log.Out("Dupe log disabled");
            }
            if (PlayerChecks.FlyEnabled)
            {
                if (!Dict.Contains("Flying detector"))
                {
                    Dict.Add("Flying detector");
                    Log.Out("Flying detector enabled");
                }
            }
            else if (Dict.Contains("Flying detector") && !_initiating)
            {
                Dict.Remove("Flying detector");
                Log.Out("Flying detector disabled");
            }
            if (PlayerChecks.GodEnabled)
            {
                if (!Dict.Contains("God mode"))
                {
                    Dict.Add("God mode");
                    Log.Out("God mode enabled");
                }
            }
            else if (Dict.Contains("God mode") && !_initiating)
            {
                Dict.Remove("God mode");
                Log.Out("God mode disabled");
            }
            if (InvalidItems.IsEnabled)
            {
                if (!Dict.Contains("Invalid items"))
                {
                    Dict.Add("Invalid items");
                    Log.Out("Invalid items enabled");
                }
            }
            else if (Dict.Contains("Invalid items") && !_initiating)
            {
                Dict.Remove("Invalid items");
                Log.Out("Invalid items disabled");
            }
            if (Jail.IsEnabled)
            {
                if (!Dict.Contains("Jail"))
                {
                    Dict.Add("Jail");
                    Log.Out("Jail enabled");
                }
            }
            else if (Dict.Contains("Jail") && !_initiating)
            {
                Dict.Remove("Jail");
                Log.Out("Jail disabled");
            }
            if (PlayerStats.IsEnabled)
            {
                if (!Dict.Contains("Player stats"))
                {
                    Dict.Add("Player stats");
                    Log.Out("Player stats enabled");
                }
            }
            else if (Dict.Contains("Player stats") && !_initiating)
            {
                Dict.Remove("Player stats");
                Log.Out("Player stats disabled");
            }
            if (PlayerLogs.IsEnabled)
            {
                if (!Dict.Contains("Player logs"))
                {
                    Dict.Add("Player logs");
                    Log.Out("Player logs enabled");
                }
            }
            else if (Dict.Contains("Player logs") && !_initiating)
            {
                Dict.Remove("Player logs");
                Log.Out("Player logs disabled");
            }
            if (ProtectedSpaces.IsEnabled)
            {
                if (!Dict.Contains("Protected spaces"))
                {
                    Dict.Add("Protected spaces");
                    Log.Out("Protected spaces enabled");
                }
            }
            else if (Dict.Contains("Protected spaces") && !_initiating)
            {
                Dict.Remove("Protected spaces");
                Log.Out("Protected spaces disabled");
            }
            if (PlayerChecks.SpectatorEnabled)
            {
                if (!Dict.Contains("Spectator detector"))
                {
                    Dict.Add("Spectator detector");
                    Log.Out("Spectator detector enabled");
                }
            }
            else if (Dict.Contains("Spectator detector") && !_initiating)
            {
                Dict.Remove("Spectator detector");
                Log.Out("Spectator detector disabled");
            }
            if (Watchlist.IsEnabled)
            {
                if (!Dict.Contains("Watch list"))
                {
                    Dict.Add("Watch list");
                    Log.Out("Watch list enabled");
                }
            }
            else if (Dict.Contains("Watch list") && !_initiating)
            {
                Dict.Remove("Watch list");
                Log.Out("Watch list disabled");
            }
            if (WorldRadius.IsEnabled)
            {
                if (!Dict.Contains("World radius"))
                {
                    Dict.Add("World radius");
                    Log.Out("World radius enabled");
                }
            }
            else if (Dict.Contains("World radius") && !_initiating)
            {
                Dict.Remove("World radius");
                Log.Out("World radius disabled");
            }
            if (_initiating)
            {
                Log.Out("--------------------------------------");
                Log.Out("[SERVERTOOLS] Chat color-prefix tools:");
                Log.Out("--------------------------------------");
            }
            if (ChatColor.IsEnabled)
            {
                if (!Dict.Contains("Chat color"))
                {
                    Dict.Add("Chat color");
                    Log.Out("Chat color enabled");
                }
            }
            else if (Dict.Contains("Chat color") && !_initiating)
            {
                Dict.Remove("Chat color");
                Log.Out("Chat color disabled");
            }
            if (ChatHook.Normal_Player_Color_Prefix)
            {
                if (!Dict.Contains("Normal player color prefix"))
                {
                    Dict.Add("Normal player color prefix");
                    Log.Out("Normal player color prefix enabled");
                }
            }
            else if (Dict.Contains("Normal player color prefix") && !_initiating)
            {
                Dict.Remove("Normal player color prefix");
                Log.Out("Normal player color prefix disabled");
            }
            if (ChatHook.Message_Color_Enabled)
            {
                if (!Dict.Contains("Message color"))
                {
                    Dict.Add("Message color");
                    Log.Out("Message color enabled");
                }
            }
            else if (Dict.Contains("Message color") && !_initiating)
            {
                Dict.Remove("Message color");
                Log.Out("Message color disabled");
            }
            if (_initiating)
            {
                Log.Out("--------------------------");
                Log.Out("[SERVERTOOLS] Other tools:");
                Log.Out("--------------------------");
            }
            if (AdminChat.IsEnabled)
            {
                if (!Dict.Contains("Admin chat commands"))
                {
                    Dict.Add("Admin chat commands");
                    Log.Out("Admin chat commands enabled");
                }
            }
            else if (Dict.Contains("Admin chat commands") && !_initiating)
            {
                Dict.Remove("Admin chat commands");
                Log.Out("Admin chat commands disabled");
            }
            if (AdminList.IsEnabled)
            {
                if (!Dict.Contains("Admin list"))
                {
                    Dict.Add("Admin list");
                    Log.Out("Admin list enabled");
                }
            }
            else if (Dict.Contains("Admin list") && !_initiating)
            {
                Dict.Remove("Admin list");
                Log.Out("Admin list disabled");
            }
            if (Animals.IsEnabled)
            {
                if (!Dict.Contains("Animal tracking"))
                {
                    Dict.Add("Animal tracking");
                    Log.Out("Animal tracking enabled");
                }
            }
            else if (Dict.Contains("Animal tracking") && !_initiating)
            {
                Dict.Remove("Animal tracking");
                Log.Out("Animal tracking disabled");
            }
            if (Auction.IsEnabled)
            {
                if (!Dict.Contains("Auction"))
                {
                    Dict.Add("Auction");
                    Log.Out("Auction enabled");
                }
            }
            else if (Dict.Contains("Auction") && !_initiating)
            {
                Dict.Remove("Auction");
                Log.Out("Auction disabled");
            }
            if (AutoBackup.IsEnabled)
            {
                if (!Dict.Contains("Auto backup"))
                {
                    Dict.Add("Auto backup");
                    Log.Out("Auto backup enabled");
                }
            }
            else if (Dict.Contains("Auto backup") && !_initiating)
            {
                Dict.Remove("Auto backup");
                Log.Out("Auto backup disabled");
            }
            if (AutoSaveWorld.IsEnabled)
            {
                if (!Dict.Contains("Auto save world"))
                {
                    Dict.Add("Auto save world");
                    Log.Out("Auto save world enabled");
                }
            }
            else if (Dict.Contains("Auto save world") && !_initiating)
            {
                Dict.Remove("Auto save world");
                Log.Out("Auto save world disabled");
            }
            if (Badwords.IsEnabled)
            {
                if (!Dict.Contains("Badword filter"))
                {
                    Dict.Add("Badword filter");
                    Log.Out("Badword filter enabled");
                }
            }
            else if (Dict.Contains("Badword filter") && !_initiating)
            {
                Dict.Remove("Badword filter");
                Log.Out("Badword filter disabled");
            }
            if (Bank.IsEnabled)
            {
                if (!Dict.Contains("Bank"))
                {
                    Dict.Add("Bank");
                    Log.Out("Bank enabled");
                }
            }
            else if (Dict.Contains("Bank") && !_initiating)
            {
                Dict.Remove("Bank");
                Log.Out("Bank disabled");
            }
            if (ExitCommand.IsEnabled)
            {
                if (!Dict.Contains("Battle logger"))
                {
                    Dict.Add("Battle logger");
                    Log.Out("Battle logger enabled");
                }
            }
            else if (Dict.Contains("Battle logger") && !_initiating)
            {
                Dict.Remove("Battle logger");
                Log.Out("Battle logger disabled");
            }
            if (EntityCleanup.BlockIsEnabled)
            {
                if (!Dict.Contains("Block cleanup"))
                {
                    Dict.Add("Block cleanup");
                    Log.Out("Block cleanup enabled");
                }
            }
            else if (Dict.Contains("Block cleanup") && !_initiating)
            {
                Dict.Remove("Block cleanup");
                Log.Out("Block cleanup disabled");
            }
            if (Bloodmoon.IsEnabled)
            {
                if (!Dict.Contains("Bloodmoon"))
                {
                    Dict.Add("Bloodmoon");
                    Log.Out("Bloodmoon enabled");
                }
            }
            else if (Dict.Contains("Bloodmoon") && !_initiating)
            {
                Dict.Remove("Bloodmoon");
                Log.Out("Bloodmoon disabled");
            }
            if (BloodmoonWarrior.IsEnabled)
            {
                if (!Dict.Contains("Bloodmoon warrior"))
                {
                    Dict.Add("Bloodmoon warrior");
                    Log.Out("Bloodmoon warrior enabled");
                }
            }
            else if (Dict.Contains("Bloodmoon warrior") && !_initiating)
            {
                Dict.Remove("Bloodmoon warrior");
                Log.Out("Bloodmoon warrior disabled");
            }
            if (Bounties.IsEnabled)
            {
                if (!Dict.Contains("Bounties"))
                {
                    Dict.Add("Bounties");
                    Log.Out("Bounties enabled");
                }
            }
            else if (Dict.Contains("Bounties") && !_initiating)
            {
                Dict.Remove("Bounties");
                Log.Out("Bounties disabled");
            }
            if (BreakTime.IsEnabled)
            {
                if (!Dict.Contains("Break time"))
                {
                    Dict.Add("Break time");
                    Log.Out("Break time enabled");
                }
            }
            else if (Dict.Contains("Break time") && !_initiating)
            {
                Dict.Remove("Break time");
                Log.Out("Break time disabled");
            }
            if (ChatCommandLog.IsEnabled)
            {
                if (!Dict.Contains("Chat command log"))
                {
                    Dict.Add("Chat command log");
                    Log.Out("Chat command log enabled");
                }
            }
            else if (Dict.Contains("Chat command log") && !_initiating)
            {
                Dict.Remove("Chat command log");
                Log.Out("Chat command log disabled");
            }
            if (ChatHook.ChatFlood)
            {
                if (!Dict.Contains("Chat flood protection"))
                {
                    Dict.Add("Chat flood protection");
                    Log.Out("Chat flood protection enabled");
                }
            }
            else if (Dict.Contains("Chat flood protection") && !_initiating)
            {
                Dict.Remove("Chat flood protection");
                Log.Out("Chat flood protection disabled");
            }
            if (ChatLog.IsEnabled)
            {
                if (!Dict.Contains("Chat log"))
                {
                    Dict.Add("Chat log");
                    Log.Out("Chat log enabled");
                }
            }
            else if (Dict.Contains("Chat log") && !_initiating)
            {
                Dict.Remove("Chat log");
                Log.Out("Chat log disabled");
            }
            if (ClanManager.IsEnabled)
            {
                if (!Dict.Contains("Clan manager"))
                {
                    Dict.Add("Clan manager");
                    Log.Out("Clan manager enabled");
                }
            }
            else if (Dict.Contains("Clan manager") && !_initiating)
            {
                Dict.Remove("Clan manager");
                Log.Out("Clan manager disabled");
            }
            if (ConsoleCommandLog.IsEnabled)
            {
                if (!Dict.Contains("Console command log"))
                {
                    Dict.Add("Console command log");
                    Log.Out("Console command log enabled");
                }
            }
            else if (Dict.Contains("Console command log") && !_initiating)
            {
                Dict.Remove("Console command log");
                Log.Out("Console command log disabled");
            }
            if (CountryBan.IsEnabled)
            {
                if (!Dict.Contains("Country ban"))
                {
                    Dict.Add("Country ban");
                    Log.Out("Country ban enabled");
                }
            }
            else if (Dict.Contains("Country ban") && !_initiating)
            {
                Dict.Remove("Country ban");
                Log.Out("Country ban disabled");
            }
            if (CustomCommands.IsEnabled)
            {
                if (!Dict.Contains("Custom commands"))
                {
                    Dict.Add("Custom commands");
                    Log.Out("Custom commands enabled");
                }
            }
            else if (Dict.Contains("Custom commands") && !_initiating)
            {
                Dict.Remove("Custom commands");
                Log.Out("Custom commands disabled");
            }
            if (Day7.IsEnabled)
            {
                if (!Dict.Contains("Day 7"))
                {
                    Dict.Add("Day 7");
                    Log.Out("Day 7 enabled");
                }
            }
            else if (Dict.Contains("Day 7") && !_initiating)
            {
                Dict.Remove("Day 7");
                Log.Out("Day 7 disabled");
            }
            if (Died.IsEnabled)
            {
                if (!Dict.Contains("Died"))
                {
                    Dict.Add("Died");
                    Log.Out("Died enabled");
                }
            }
            else if (Dict.Contains("Died") && !_initiating)
            {
                Dict.Remove("Died");
                Log.Out("Died disabled");
            }
            if (DiscordBot.IsEnabled)
            {
                if (!Dict.Contains("Discord bot"))
                {
                    Dict.Add("Discord bot");
                    Log.Out("Discord bot enabled");
                }
            }
            else if (Dict.Contains("Discord bot") && !_initiating)
            {
                Dict.Remove("Discord bot");
                Log.Out("Discord bot disabled");
            }
            if (EntityCleanup.IsEnabled)
            {
                if (!Dict.Contains("Entity cleanup"))
                {
                    Dict.Add("Entity cleanup");
                    Log.Out("Entity cleanup enabled");
                }
            }
            else if (Dict.Contains("Entity cleanup") && !_initiating)
            {
                Dict.Remove("Entity cleanup");
                Log.Out("Entity cleanup disabled");
            }
            if (EntityCleanup.Underground)
            {
                if (!Dict.Contains("Entity cleanup underground"))
                {
                    Dict.Add("Entity cleanup underground");
                    Log.Out("Entity cleanup underground enabled");
                }
            }
            else if (Dict.Contains("Entity cleanup underground") && !_initiating)
            {
                Dict.Remove("Entity cleanup underground");
                Log.Out("Entity cleanup underground disabled");
            }
            if (EntityCleanup.FallingTreeEnabled)
            {
                if (!Dict.Contains("Entity falling tree cleanup"))
                {
                    Dict.Add("Entity falling tree cleanup");
                    Log.Out("Entity falling tree cleanup enabled");
                }
                Log.Out("Entity falling tree cleanup enabled");
            }
            else if (Dict.Contains("Entity falling tree cleanup") && !_initiating)
            {
                Dict.Remove("Entity falling tree cleanup");
                Log.Out("Entity falling tree cleanup disabled");
            }
            if (FallingBlocks.IsEnabled)
            {
                if (!Dict.Contains("Falling blocks remover"))
                {
                    Dict.Add("Falling blocks remover");
                    Log.Out("Falling blocks remover enabled");
                }
            }
            else if (Dict.Contains("Falling blocks remover") && !_initiating)
            {
                Dict.Remove("Falling blocks remover");
                Log.Out("Falling blocks remover disabled");
            }
            if (FirstClaimBlock.IsEnabled)
            {
                if (!Dict.Contains("First claim block"))
                {
                    Dict.Add("First claim block");
                    Log.Out("First claim block enabled");
                }
            }
            else if (Dict.Contains("First claim block") && !_initiating)
            {
                Dict.Remove("First claim block");
                Log.Out("First claim block disabled");
            }
            if (Fps.IsEnabled)
            {
                if (!Dict.Contains("FPS"))
                {
                    Dict.Add("FPS");
                    Log.Out("FPS enabled");
                }
            }
            else if (Dict.Contains("FPS") && !_initiating)
            {
                Dict.Remove("FPS");
                Log.Out("FPS disabled");
            }
            if (FriendTeleport.IsEnabled)
            {
                if (!Dict.Contains("Friend teleport"))
                {
                    Dict.Add("Friend teleport");
                    Log.Out("Friend teleport enabled");
                }
            }
            else if (Dict.Contains("Friend teleport") && !_initiating)
            {
                Dict.Remove("Friend teleport");
                Log.Out("Friend teleport disabled");
            }
            if (Gimme.IsEnabled)
            {
                if (!Dict.Contains("Gimme"))
                {
                    Dict.Add("Gimme");
                    Log.Out("Gimme enabled");
                }
            }
            else if (Dict.Contains("Gimme") && !_initiating)
            {
                Dict.Remove("Gimme");
                Log.Out("Gimme disabled");
            }
            if (HighPingKicker.IsEnabled)
            {
                if (!Dict.Contains("High ping kicker"))
                {
                    Dict.Add("High ping kicker");
                    Log.Out("High ping kicker enabled");
                }
            }
            else if (Dict.Contains("High ping kicker") && !_initiating)
            {
                Dict.Remove("High ping kicker");
                Log.Out("High ping kicker disabled");
            }
            if (Hordes.IsEnabled)
            {
                if (!Dict.Contains("Hordes"))
                {
                    Dict.Add("Hordes");
                    Log.Out("Hordes enabled");
                }
            }
            else if (Dict.Contains("Hordes") && !_initiating)
            {
                Dict.Remove("Hordes");
                Log.Out("Hordes disabled");
            }
            if (InfoTicker.IsEnabled)
            {
                if (!Dict.Contains("Info ticker"))
                {
                    Dict.Add("Info ticker");
                    Log.Out("Info ticker enabled");
                }
            }
            else if (Dict.Contains("Info ticker") && !_initiating)
            {
                Dict.Remove("Info ticker");
                Log.Out("Info ticker disabled");
            }
            if (KickVote.IsEnabled)
            {
                if (!Dict.Contains("Kick vote"))
                {
                    Dict.Add("Kick vote");
                    Log.Out("Kick vote enabled");
                }
            }
            else if (Dict.Contains("Kick vote") && !_initiating)
            {
                Dict.Remove("Kick vote");
                Log.Out("Kick vote disabled");
            }
            if (KillNotice.IsEnabled)
            {
                if (!Dict.Contains("Kill notice"))
                {
                    Dict.Add("Kill notice");
                    Log.Out("Kill notice enabled");
                }
            }
            else if (Dict.Contains("Kill notice") && !_initiating)
            {
                Dict.Remove("Kill notice");
                Log.Out("Kill notice disabled");
            }
            if (LevelUp.IsEnabled)
            {
                if (!Dict.Contains("Level up"))
                {
                    Dict.Add("Level up");
                    Log.Out("Level up enabled");
                }
            }
            else if (Dict.Contains("Level up") && !_initiating)
            {
                Dict.Remove("Level up");
                Log.Out("Level up disabled");
            }
            if (Lobby.IsEnabled)
            {
                if (!Dict.Contains("Lobby"))
                {
                    Dict.Add("Lobby");
                    Log.Out("Lobby enabled");
                }
            }
            else if (Dict.Contains("Lobby") && !_initiating)
            {
                Dict.Remove("Lobby");
                Log.Out("Lobby disabled");
            }
            if (Loc.IsEnabled)
            {
                if (!Dict.Contains("Location"))
                {
                    Dict.Add("Location");
                    Log.Out("Location enabled");
                }
            }
            else if (Dict.Contains("Location") && !_initiating)
            {
                Dict.Remove("Location");
                Log.Out("Location disabled");
            }
            if (LoginNotice.IsEnabled)
            {
                if (!Dict.Contains("Login notice"))
                {
                    Dict.Add("Login notice");
                    Log.Out("Login notice enabled");
                }
            }
            else if (Dict.Contains("Login notice") && !_initiating)
            {
                Dict.Remove("Login notice");
                Log.Out("Login notice disabled");
            }
            if (Lottery.IsEnabled)
            {
                if (!Dict.Contains("Lottery"))
                {
                    Dict.Add("Lottery");
                    Log.Out("Lottery enabled");
                }
            }
            else if (Dict.Contains("Lottery") && !_initiating)
            {
                Dict.Remove("Lottery");
                Log.Out("Lottery disabled");
            }
            if (Market.IsEnabled)
            {
                if (!Dict.Contains("Market"))
                {
                    Dict.Add("Market");
                    Log.Out("Market enabled");
                }
            }
            else if (Dict.Contains("Market") && !_initiating)
            {
                Dict.Remove("Market");
                Log.Out("Market disabled");
            }
            if (Motd.IsEnabled)
            {
                if (!Dict.Contains("Motd"))
                {
                    Dict.Add("Motd");
                    Log.Out("Motd enabled");
                }
            }
            else if (Dict.Contains("Motd") && !_initiating)
            {
                Dict.Remove("Motd");
                Log.Out("Motd disabled");
            }
            if (Mute.IsEnabled)
            {
                if (!Dict.Contains("Mute"))
                {
                    Dict.Add("Mute");
                    Log.Out("Mute enabled");
                }
            }
            else if (Dict.Contains("Mute") && !_initiating)
            {
                Dict.Remove("Mute");
                Log.Out("Mute disabled");
            }
            if (MuteVote.IsEnabled)
            {
                if (!Dict.Contains("Mute vote"))
                {
                    Dict.Add("Mute vote");
                    Log.Out("Mute vote enabled");
                }
            }
            else if (Dict.Contains("Mute vote") && !_initiating)
            {
                Dict.Remove("Mute vote");
                Log.Out("Mute vote disabled");
            }
            if (NewPlayer.IsEnabled)
            {
                if (!Dict.Contains("New player"))
                {
                    Dict.Add("New player");
                    Log.Out("New player enabled");
                }
            }
            else if (Dict.Contains("New player") && !_initiating)
            {
                Dict.Remove("New player");
                Log.Out("New player disabled");
            }
            if (NewPlayerProtection.IsEnabled)
            {
                if (!Dict.Contains("New player protection"))
                {
                    Dict.Add("New player protection");
                    Log.Out("New player protection enabled");
                }
            }
            else if (Dict.Contains("New player protection") && !_initiating)
            {
                Dict.Remove("New player protection");
                Log.Out("New player protection disabled");
            }
            if (NewSpawnTele.IsEnabled)
            {
                if (!Dict.Contains("New player teleport"))
                {
                    Dict.Add("New player teleport");
                    Log.Out("New player teleport enabled");
                }
            }
            else if (Dict.Contains("New player teleport") && !_initiating)
            {
                Dict.Remove("New player teleport");
                Log.Out("New player teleport disabled");
            }
            if (NightAlert.IsEnabled)
            {
                if (!Dict.Contains("Night alert"))
                {
                    Dict.Add("Night alert");
                    Log.Out("Night alert enabled");
                }
            }
            else if (Dict.Contains("Night alert") && !_initiating)
            {
                Dict.Remove("Night alert");
                Log.Out("Night alert disabled");
            }
            if (POIProtection.IsEnabled)
            {
                if (!Dict.Contains("POI protection"))
                {
                    Dict.Add("POI protection");
                    Log.Out("POI protection enabled");
                }
            }
            else if (Dict.Contains("POI protection") && !_initiating)
            {
                Dict.Remove("POI protection");
                Log.Out("POI protection disabled");
            }
            if (Prayer.IsEnabled)
            {
                if (!Dict.Contains("Prayer"))
                {
                    Dict.Add("Prayer");
                    Log.Out("Prayer enabled");
                }
            }
            else if (Dict.Contains("Prayer") && !_initiating)
            {
                Dict.Remove("Prayer");
                Log.Out("Prayer disabled");
            }
            if (Whisper.IsEnabled)
            {
                if (!Dict.Contains("Private message"))
                {
                    Dict.Add("Private message");
                    Log.Out("Private message enabled");
                }
            }
            else if (Dict.Contains("Private message") && !_initiating)
            {
                Dict.Remove("Private message");
                Log.Out("Private message disabled");
            }
            if (RealWorldTime.IsEnabled)
            {
                if (!Dict.Contains("Real world time"))
                {
                    Dict.Add("Real world time");
                    Log.Out("Real world time enabled");
                }
            }
            else if (Dict.Contains("Real world time") && !_initiating)
            {
                Dict.Remove("Real world time");
                Log.Out("Real world time disabled");
            }
            if (ReservedSlots.IsEnabled)
            {
                if (!Dict.Contains("Reserved slots"))
                {
                    Dict.Add("Reserved slots");
                    Log.Out("Reserved slots enabled");
                }
            }
            else if (Dict.Contains("Reserved slots") && !_initiating)
            {
                Dict.Remove("Reserved slots");
                Log.Out("Reserved slots disabled");
            }
            if (ScoutPlayer.IsEnabled)
            {
                if (!Dict.Contains("Scout player"))
                {
                    Dict.Add("Scout player");
                    Log.Out("Scout player enabled");
                }
            }
            else if (Dict.Contains("Scout player") && !_initiating)
            {
                Dict.Remove("Scout player");
                Log.Out("Scout player disabled");
            }
            if (Shutdown.IsEnabled)
            {
                if (!Dict.Contains("Shutdown"))
                {
                    Dict.Add("Shutdown");
                    Log.Out("Shutdown enabled");
                }
            }
            else if (Dict.Contains("Shutdown") && !_initiating)
            {
                Dict.Remove("Shutdown");
                Log.Out("Shutdown disabled");
            }
            if (Homes.IsEnabled)
            {
                if (!Dict.Contains("Homes"))
                {
                    Dict.Add("Homes");
                    Log.Out("Homes enabled");
                }
            }
            else if (Dict.Contains("Homes") && !_initiating)
            {
                Dict.Remove("Homes");
                Log.Out("Homes disabled");
            }
            if (Shop.IsEnabled)
            {
                if (!Dict.Contains("Shop"))
                {
                    Dict.Add("Shop");
                    Log.Out("Shop enabled");
                }
            }
            else if (Dict.Contains("Shop") && !_initiating)
            {
                Dict.Remove("Shop");
                Log.Out("Shop disabled");
            }
            if (SleeperRespawn.IsEnabled)
            {
                if (!Dict.Contains("Sleeper respawn"))
                {
                    Dict.Add("Sleeper respawn");
                    Log.Out("Sleeper respawn enabled");
                }
            }
            else if (Dict.Contains("Sleeper respawn") && !_initiating)
            {
                Dict.Remove("Sleeper respawn");
                Log.Out("Sleeper respawn disabled");
            }
            if (StartingItems.IsEnabled)
            {
                if (!Dict.Contains("Starting items"))
                {
                    Dict.Add("Starting items");
                    Log.Out("Starting items enabled");
                }
            }
            else if (Dict.Contains("Starting items") && !_initiating)
            {
                Dict.Remove("Starting items");
                Log.Out("Starting items disabled");
            }
            if (Suicide.IsEnabled)
            {
                if (!Dict.Contains("Suicide"))
                {
                    Dict.Add("Suicide");
                    Log.Out("Suicide enabled");
                }
            }
            else if (Dict.Contains("Suicide") && !_initiating)
            {
                Dict.Remove("Suicide");
                Log.Out("Suicide disabled");
            }
            if (Track.IsEnabled)
            {
                if (!Dict.Contains("Tracking"))
                {
                    Dict.Add("Tracking");
                    Log.Out("Tracking enabled");
                }
            }
            else if (Dict.Contains("Tracking") && !_initiating)
            {
                Dict.Remove("Tracking");
                Log.Out("Tracking disabled");
            }
            if (Travel.IsEnabled)
            {
                if (!Dict.Contains("Travel"))
                {
                    Dict.Add("Travel");
                    Log.Out("Travel enabled");
                }
            }
            else if (Dict.Contains("Travel") && !_initiating)
            {
                Dict.Remove("Travel");
                Log.Out("Travel disabled");
            }
            if (VehicleRecall.IsEnabled)
            {
                if (!Dict.Contains("Vehicle recall"))
                {
                    Dict.Add("Vehicle recall");
                    Log.Out("Vehicle recall enabled");
                }
            }
            else if (Dict.Contains("Vehicle recall") && !_initiating)
            {
                Dict.Remove("Vehicle recall");
                Log.Out("Vehicle recall disabled");
            }
            if (VoteReward.IsEnabled)
            {
                if (!Dict.Contains("Vote reward"))
                {
                    Dict.Add("Vote reward");
                    Log.Out("Vote reward enabled");
                }
            }
            else if (Dict.Contains("Vote reward") && !_initiating)
            {
                Dict.Remove("Vote reward");
                Log.Out("Vote reward disabled");
            }
            if (Wallet.IsEnabled)
            {
                if (!Dict.Contains("Wallet"))
                {
                    Dict.Add("Wallet");
                    Log.Out("Wallet enabled");
                }
            }
            else if (Dict.Contains("Wallet") && !_initiating)
            {
                Dict.Remove("Wallet");
                Log.Out("Wallet disabled");
            }
            if (Waypoints.IsEnabled)
            {
                if (!Dict.Contains("Waypoints"))
                {
                    Dict.Add("Waypoints");
                    Log.Out("Waypoints enabled");
                }
            }
            else if (Dict.Contains("Waypoints") && !_initiating)
            {
                Dict.Remove("Waypoints");
                Log.Out("Waypoints disabled");
            }
            if (WebAPI.IsEnabled)
            {
                if (!Dict.Contains("Web API"))
                {
                    Dict.Add("Web API");
                    Log.Out("Web API enabled");
                }
            }
            else if (Dict.Contains("Web API") && !_initiating)
            {
                Dict.Remove("Web API");
                Log.Out("Web API disabled");
            }
            if (WebPanel.IsEnabled)
            {
                if (!Dict.Contains("Web panel"))
                {
                    Dict.Add("Web panel");
                    Log.Out("Web panel enabled");
                }
            }
            else if (Dict.Contains("Web panel") && !_initiating)
            {
                Dict.Remove("Web panel");
                Log.Out("Web panel disabled");
            }
            if (Zones.IsEnabled)
            {
                if (!Dict.Contains("Zones"))
                {
                    Dict.Add("Zones");
                    Log.Out("Zones enabled");
                }
            }
            else if (Dict.Contains("Zones") && !_initiating)
            {
                Dict.Remove("Zones");
                Log.Out("Zones disabled");
            }
            if (_initiating)
            {
                Log.Out("--------------------------------");
                Log.Out("[SERVERTOOLS] Tool load complete");
                Log.Out("--------------------------------");
            }
            else
            {
                Log.Out("--------------------------------");
            }
        }
    }
}
