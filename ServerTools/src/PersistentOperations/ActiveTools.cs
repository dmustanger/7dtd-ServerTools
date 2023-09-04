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
            if (DamageDetector.IsEnabled)
            {
                if (!Dict.Contains("Damage detector"))
                {
                    Dict.Add("Damage detector");
                    Log.Out("[SERVERTOOLS] Damage detector enabled");
                }
            }
            else if (Dict.Contains("Damage detector") && !_initiating)
            {
                Dict.Remove("Damage detector");
                Log.Out("[SERVERTOOLS] Damage detector disabled");
            }
            if (DupeLog.IsEnabled)
            {
                if (!Dict.Contains("Dupe log"))
                {
                    Dict.Add("Dupe log");
                    Log.Out("[SERVERTOOLS] Dupe log enabled");
                }
            }
            else if (Dict.Contains("Dupe log") && !_initiating)
            {
                Dict.Remove("Dupe log");
                Log.Out("[SERVERTOOLS] Dupe log disabled");
            }
            if (FlyingDetector.IsEnabled)
            {
                if (!Dict.Contains("Flying detector"))
                {
                    Dict.Add("Flying detector");
                    Log.Out("[SERVERTOOLS] Flying detector enabled");
                }
            }
            else if (Dict.Contains("Flying detector") && !_initiating)
            {
                Dict.Remove("Flying detector");
                Log.Out("[SERVERTOOLS] Flying detector disabled");
            }
            if (GodMode.IsEnabled)
            {
                if (!Dict.Contains("God mode"))
                {
                    Dict.Add("God mode");
                    Log.Out("[SERVERTOOLS] God mode enabled");
                }
            }
            else if (Dict.Contains("God mode") && !_initiating)
            {
                Dict.Remove("God mode");
                Log.Out("[SERVERTOOLS] God mode disabled");
            }
            if (InvalidItems.IsEnabled)
            {
                if (!Dict.Contains("Invalid items"))
                {
                    Dict.Add("Invalid items");
                    Log.Out("[SERVERTOOLS] Invalid items enabled");
                }
            }
            else if (Dict.Contains("Invalid items") && !_initiating)
            {
                Dict.Remove("Invalid items");
                Log.Out("[SERVERTOOLS] Invalid items disabled");
            }
            if (InfiniteAmmo.IsEnabled)
            {
                if (!Dict.Contains("Infinite ammo"))
                {
                    Dict.Add("Infinite ammo");
                    Log.Out("[SERVERTOOLS] Infinite ammo enabled");
                }
            }
            else if (Dict.Contains("Infinite ammo") && !_initiating)
            {
                Dict.Remove("Infinite ammo");
                Log.Out("[SERVERTOOLS] Infinite ammo disabled");
            }
            if (Jail.IsEnabled)
            {
                if (!Dict.Contains("Jail"))
                {
                    Dict.Add("Jail");
                    Log.Out("[SERVERTOOLS] Jail enabled");
                }
            }
            else if (Dict.Contains("Jail") && !_initiating)
            {
                Dict.Remove("Jail");
                Log.Out("[SERVERTOOLS] Jail disabled");
            }
            if (MagicBullet.IsEnabled)
            {
                if (!Dict.Contains("Magic bullet"))
                {
                    Dict.Add("Magic bullet");
                    Log.Out("[SERVERTOOLS] Magic bullet enabled");
                }
            }
            else if (Dict.Contains("Magic bullet") && !_initiating)
            {
                Dict.Remove("Magic bullet");
                Log.Out("[SERVERTOOLS] Magic bullet disabled");
            }
            if (PlayerStats.IsEnabled)
            {
                if (!Dict.Contains("Player stats"))
                {
                    Dict.Add("Player stats");
                    Log.Out("[SERVERTOOLS] Player stats enabled");
                }
            }
            else if (Dict.Contains("Player stats") && !_initiating)
            {
                Dict.Remove("Player stats");
                Log.Out("[SERVERTOOLS] Player stats disabled");
            }
            if (PlayerLogs.IsEnabled)
            {
                if (!Dict.Contains("Player logs"))
                {
                    Dict.Add("Player logs");
                    Log.Out("[SERVERTOOLS] Player logs enabled");
                }
            }
            else if (Dict.Contains("Player logs") && !_initiating)
            {
                Dict.Remove("Player logs");
                Log.Out("[SERVERTOOLS] Player logs disabled");
            }
            if (ProtectedZones.IsEnabled)
            {
                if (!Dict.Contains("Protected zones"))
                {
                    Dict.Add("Protected zones");
                    Log.Out("[SERVERTOOLS] Protected zones enabled");
                }
            }
            else if (Dict.Contains("Protected zones") && !_initiating)
            {
                Dict.Remove("Protected zones");
                Log.Out("[SERVERTOOLS] Protected zones disabled");
            }
            if (PlayerChecks.SpectatorEnabled)
            {
                if (!Dict.Contains("Spectator detector"))
                {
                    Dict.Add("Spectator detector");
                    Log.Out("[SERVERTOOLS] Spectator detector enabled");
                }
            }
            else if (Dict.Contains("Spectator detector") && !_initiating)
            {
                Dict.Remove("Spectator detector");
                Log.Out("[SERVERTOOLS] Spectator detector disabled");
            }
            if (SpeedDetector.IsEnabled)
            {
                if (!Dict.Contains("Speed detector"))
                {
                    Dict.Add("Speed detector");
                    Log.Out("[SERVERTOOLS] Speed detector enabled");
                }
            }
            else if (Dict.Contains("Speed detector") && !_initiating)
            {
                Dict.Remove("Speed detector");
                Log.Out("[SERVERTOOLS] Speed detector disabled");
            }
            if (WatchList.IsEnabled)
            {
                if (!Dict.Contains("Watch list"))
                {
                    Dict.Add("Watch list");
                    Log.Out("[SERVERTOOLS] Watch list enabled");
                }
            }
            else if (Dict.Contains("Watch list") && !_initiating)
            {
                Dict.Remove("Watch list");
                Log.Out("[SERVERTOOLS] Watch list disabled");
            }
            if (WorldRadius.IsEnabled)
            {
                if (!Dict.Contains("World radius"))
                {
                    Dict.Add("World radius");
                    Log.Out("[SERVERTOOLS] World radius enabled");
                }
            }
            else if (Dict.Contains("World radius") && !_initiating)
            {
                Dict.Remove("World radius");
                Log.Out("[SERVERTOOLS] World radius disabled");
            }
            if (XRayDetector.IsEnabled)
            {
                if (!Dict.Contains("XRay"))
                {
                    Dict.Add("XRay");
                    Log.Out("[SERVERTOOLS] XRay detector enabled");
                }
            }
            else if (Dict.Contains("XRay") && !_initiating)
            {
                Dict.Remove("XRay");
                Log.Out("[SERVERTOOLS] XRay detector disabled");
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
                    Log.Out("[SERVERTOOLS] Chat color enabled");
                }
            }
            else if (Dict.Contains("Chat color") && !_initiating)
            {
                Dict.Remove("Chat color");
                Log.Out("[SERVERTOOLS] Chat color disabled");
            }
            if (ChatHook.Normal_Player_Color_Prefix)
            {
                if (!Dict.Contains("Normal player color prefix"))
                {
                    Dict.Add("Normal player color prefix");
                    Log.Out("[SERVERTOOLS] Normal player color prefix enabled");
                }
            }
            else if (Dict.Contains("Normal player color prefix") && !_initiating)
            {
                Dict.Remove("Normal player color prefix");
                Log.Out("[SERVERTOOLS] Normal player color prefix disabled");
            }
            if (ChatHook.Message_Color_Enabled)
            {
                if (!Dict.Contains("Message color"))
                {
                    Dict.Add("Message color");
                    Log.Out("[SERVERTOOLS] Message color enabled");
                }
            }
            else if (Dict.Contains("Message color") && !_initiating)
            {
                Dict.Remove("Message color");
                Log.Out("[SERVERTOOLS] Message color disabled");
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
                    Log.Out("[SERVERTOOLS] Admin chat commands enabled");
                }
            }
            else if (Dict.Contains("Admin chat commands") && !_initiating)
            {
                Dict.Remove("Admin chat commands");
                Log.Out("[SERVERTOOLS] Admin chat commands disabled");
            }
            if (AdminList.IsEnabled)
            {
                if (!Dict.Contains("Admin list"))
                {
                    Dict.Add("Admin list");
                    Log.Out("[SERVERTOOLS] Admin list enabled");
                }
            }
            else if (Dict.Contains("Admin list") && !_initiating)
            {
                Dict.Remove("Admin list");
                Log.Out("[SERVERTOOLS] Admin list disabled");
            }
            if (AnimalTracking.IsEnabled)
            {
                if (!Dict.Contains("Animal tracking"))
                {
                    Dict.Add("Animal tracking");
                    Log.Out("[SERVERTOOLS] Animal tracking enabled");
                }
            }
            else if (Dict.Contains("Animal tracking") && !_initiating)
            {
                Dict.Remove("Animal tracking");
                Log.Out("[SERVERTOOLS] Animal tracking disabled");
            }
            if (Auction.IsEnabled)
            {
                if (!Dict.Contains("Auction"))
                {
                    Dict.Add("Auction");
                    Log.Out("[SERVERTOOLS] Auction enabled");
                }
            }
            else if (Dict.Contains("Auction") && !_initiating)
            {
                Dict.Remove("Auction");
                Log.Out("[SERVERTOOLS] Auction disabled");
            }
            if (AutoBackup.IsEnabled)
            {
                if (!Dict.Contains("Auto backup"))
                {
                    Dict.Add("Auto backup");
                    Log.Out("[SERVERTOOLS] Auto backup enabled");
                }
            }
            else if (Dict.Contains("Auto backup") && !_initiating)
            {
                Dict.Remove("Auto backup");
                Log.Out("[SERVERTOOLS] Auto backup disabled");
            }
            if (AutoPartyInvite.IsEnabled)
            {
                if (!Dict.Contains("Auto party invite"))
                {
                    Dict.Add("Auto party invite");
                    Log.Out("[SERVERTOOLS] Auto party invite enabled");
                }
            }
            else if (Dict.Contains("Auto party invite") && !_initiating)
            {
                Dict.Remove("Auto party invite");
                Log.Out("[SERVERTOOLS] Auto party invite disabled");
            }
            if (AutoRestart.IsEnabled)
            {
                if (!Dict.Contains("Auto restart"))
                {
                    Dict.Add("Auto restart");
                    Log.Out("[SERVERTOOLS] Auto restart enabled");
                }
            }
            else if (Dict.Contains("Auto restart") && !_initiating)
            {
                Dict.Remove("Auto restart");
                Log.Out("[SERVERTOOLS] Auto restart disabled");
            }
            if (AutoSaveWorld.IsEnabled)
            {
                if (!Dict.Contains("Auto save world"))
                {
                    Dict.Add("Auto save world");
                    Log.Out("[SERVERTOOLS] Auto save world enabled");
                }
            }
            else if (Dict.Contains("Auto save world") && !_initiating)
            {
                Dict.Remove("Auto save world");
                Log.Out("[SERVERTOOLS] Auto save world disabled");
            }
            if (Badwords.IsEnabled)
            {
                if (!Dict.Contains("Badword filter"))
                {
                    Dict.Add("Badword filter");
                    Log.Out("[SERVERTOOLS] Badword filter enabled");
                }
            }
            else if (Dict.Contains("Badword filter") && !_initiating)
            {
                Dict.Remove("Badword filter");
                Log.Out("[SERVERTOOLS] Badword filter disabled");
            }
            if (Bank.IsEnabled)
            {
                if (!Dict.Contains("Bank"))
                {
                    Dict.Add("Bank");
                    Log.Out("[SERVERTOOLS] Bank enabled");
                }
            }
            else if (Dict.Contains("Bank") && !_initiating)
            {
                Dict.Remove("Bank");
                Log.Out("[SERVERTOOLS] Bank disabled");
            }
            if (ExitCommand.IsEnabled)
            {
                if (!Dict.Contains("Battle logger"))
                {
                    Dict.Add("Battle logger");
                    Log.Out("[SERVERTOOLS] Battle logger enabled");
                }
            }
            else if (Dict.Contains("Battle logger") && !_initiating)
            {
                Dict.Remove("Battle logger");
                Log.Out("[SERVERTOOLS] Battle logger disabled");
            }
            if (Bed.IsEnabled)
            {
                if (!Dict.Contains("Bed"))
                {
                    Dict.Add("Bed");
                    Log.Out("[SERVERTOOLS] Bed enabled");
                }
            }
            else if (Dict.Contains("Bed") && !_initiating)
            {
                Dict.Remove("Bed");
                Log.Out("[SERVERTOOLS] Bed disabled");
            }
            if (BigHead.IsEnabled)
            {
                if (!Dict.Contains("Big head"))
                {
                    Dict.Add("Big head");
                    Log.Out("[SERVERTOOLS] Big head enabled");
                }
            }
            else if (Dict.Contains("Big head") && !_initiating)
            {
                Dict.Remove("Big head");
                Log.Out("[SERVERTOOLS] Big head disabled");
            }
            if (BlockLogger.IsEnabled)
            {
                if (!Dict.Contains("Block logger"))
                {
                    Dict.Add("Block logger");
                    Log.Out("[SERVERTOOLS] Block logger enabled");
                }
            }
            else if (Dict.Contains("Block logger") && !_initiating)
            {
                Dict.Remove("Block logger");
                Log.Out("[SERVERTOOLS] Block logger disabled");
            }
            if (BlockPickup.IsEnabled)
            {
                if (!Dict.Contains("Block pickup"))
                {
                    Dict.Add("Block pickup");
                    Log.Out("[SERVERTOOLS] Block pickup enabled");
                }
            }
            else if (Dict.Contains("Block pickup") && !_initiating)
            {
                Dict.Remove("Block pickup");
                Log.Out("[SERVERTOOLS] Block pickup disabled");
            }
            if (BloodMoans.IsEnabled)
            {
                if (!Dict.Contains("Blood moans"))
                {
                    Dict.Add("Blood moans");
                    Log.Out("[SERVERTOOLS] Blood moans enabled");
                }
            }
            else if (Dict.Contains("Blood moans") && !_initiating)
            {
                Dict.Remove("Blood moans");
                Log.Out("[SERVERTOOLS] Blood moans disabled");
            }
            if (Bloodmoon.IsEnabled)
            {
                if (!Dict.Contains("Bloodmoon"))
                {
                    Dict.Add("Bloodmoon");
                    Log.Out("[SERVERTOOLS] Bloodmoon enabled");
                }
            }
            else if (Dict.Contains("Bloodmoon") && !_initiating)
            {
                Dict.Remove("Bloodmoon");
                Log.Out("[SERVERTOOLS] Bloodmoon disabled");
            }
            if (BloodmoonWarrior.IsEnabled)
            {
                if (!Dict.Contains("Bloodmoon warrior"))
                {
                    Dict.Add("Bloodmoon warrior");
                    Log.Out("[SERVERTOOLS] Bloodmoon warrior enabled");
                }
            }
            else if (Dict.Contains("Bloodmoon warrior") && !_initiating)
            {
                Dict.Remove("Bloodmoon warrior");
                Log.Out("[SERVERTOOLS] Bloodmoon warrior disabled");
            }
            if (BotResponse.IsEnabled)
            {
                if (!Dict.Contains("Bot response"))
                {
                    Dict.Add("Bot response");
                    Log.Out("[SERVERTOOLS] Bot response enabled");
                }
            }
            else if (Dict.Contains("Bot response") && !_initiating)
            {
                Dict.Remove("Bot response");
                Log.Out("[SERVERTOOLS] Bot response disabled");
            }
            if (Bounties.IsEnabled)
            {
                if (!Dict.Contains("Bounties"))
                {
                    Dict.Add("Bounties");
                    Log.Out("[SERVERTOOLS] Bounties enabled");
                }
            }
            else if (Dict.Contains("Bounties") && !_initiating)
            {
                Dict.Remove("Bounties");
                Log.Out("[SERVERTOOLS] Bounties disabled");
            }
            if (BreakReminder.IsEnabled)
            {
                if (!Dict.Contains("Break reminder"))
                {
                    Dict.Add("Break reminder");
                    Log.Out("[SERVERTOOLS] Break reminder enabled");
                }
            }
            else if (Dict.Contains("Break reminder") && !_initiating)
            {
                Dict.Remove("Break reminder");
                Log.Out("[SERVERTOOLS] Break reminder disabled");
            }
            if (ChatCommandLog.IsEnabled)
            {
                if (!Dict.Contains("Chat command log"))
                {
                    Dict.Add("Chat command log");
                    Log.Out("[SERVERTOOLS] Chat command log enabled");
                }
            }
            else if (Dict.Contains("Chat command log") && !_initiating)
            {
                Dict.Remove("Chat command log");
                Log.Out("[SERVERTOOLS] Chat command log disabled");
            }
            if (ChatHook.ChatFlood)
            {
                if (!Dict.Contains("Chat flood protection"))
                {
                    Dict.Add("Chat flood protection");
                    Log.Out("[SERVERTOOLS] Chat flood protection enabled");
                }
            }
            else if (Dict.Contains("Chat flood protection") && !_initiating)
            {
                Dict.Remove("Chat flood protection");
                Log.Out("[SERVERTOOLS] Chat flood protection disabled");
            }
            if (ChatLog.IsEnabled)
            {
                if (!Dict.Contains("Chat log"))
                {
                    Dict.Add("Chat log");
                    Log.Out("[SERVERTOOLS] Chat log enabled");
                }
            }
            else if (Dict.Contains("Chat log") && !_initiating)
            {
                Dict.Remove("Chat log");
                Log.Out("[SERVERTOOLS] Chat log disabled");
            }
            if (ChunkReset.IsEnabled)
            {
                if (!Dict.Contains("Chunk reset"))
                {
                    Dict.Add("Chunk reset");
                    Log.Out("[SERVERTOOLS] Chunk reset enabled");
                }
            }
            else if (Dict.Contains("Chunk reset") && !_initiating)
            {
                Dict.Remove("Chunk reset");
                Log.Out("[SERVERTOOLS] Chunk reset disabled");
            }
            if (ClanManager.IsEnabled)
            {
                if (!Dict.Contains("Clan manager"))
                {
                    Dict.Add("Clan manager");
                    Log.Out("[SERVERTOOLS] Clan manager enabled");
                }
            }
            else if (Dict.Contains("Clan manager") && !_initiating)
            {
                Dict.Remove("Clan manager");
                Log.Out("[SERVERTOOLS] Clan manager disabled");
            }
            if (CleanBin.IsEnabled)
            {
                if (!Dict.Contains("Clean bin"))
                {
                    Dict.Add("Clean bin");
                    Log.Out("[SERVERTOOLS] Clean bin enabled");
                }
            }
            else if (Dict.Contains("Clean bin") && !_initiating)
            {
                Dict.Remove("Clean bin");
                Log.Out("[SERVERTOOLS] Clean bin disabled");
            }
            if (Confetti.IsEnabled)
            {
                if (!Dict.Contains("Confetti"))
                {
                    Dict.Add("Confetti");
                    Log.Out("[SERVERTOOLS] Confetti enabled");
                }
            }
            else if (Dict.Contains("Confetti") && !_initiating)
            {
                Dict.Remove("Confetti");
                Log.Out("[SERVERTOOLS] Confetti disabled");
            }
            if (ConsoleCommandLog.IsEnabled)
            {
                if (!Dict.Contains("Console command log"))
                {
                    Dict.Add("Console command log");
                    Log.Out("[SERVERTOOLS] Console command log enabled");
                }
            }
            else if (Dict.Contains("Console command log") && !_initiating)
            {
                Dict.Remove("Console command log");
                Log.Out("[SERVERTOOLS] Console command log disabled");
            }
            if (CustomCommands.IsEnabled)
            {
                if (!Dict.Contains("Custom commands"))
                {
                    Dict.Add("Custom commands");
                    Log.Out("[SERVERTOOLS] Custom commands enabled");
                }
            }
            else if (Dict.Contains("Custom commands") && !_initiating)
            {
                Dict.Remove("Custom commands");
                Log.Out("[SERVERTOOLS] Custom commands disabled");
            }
            if (Day7.IsEnabled)
            {
                if (!Dict.Contains("Day 7"))
                {
                    Dict.Add("Day 7");
                    Log.Out("[SERVERTOOLS] Day 7 enabled");
                }
            }
            else if (Dict.Contains("Day 7") && !_initiating)
            {
                Dict.Remove("Day 7");
                Log.Out("[SERVERTOOLS] Day 7 disabled");
            }
            if (Died.IsEnabled)
            {
                if (!Dict.Contains("Died"))
                {
                    Dict.Add("Died");
                    Log.Out("[SERVERTOOLS] Died enabled");
                }
            }
            else if (Dict.Contains("Died") && !_initiating)
            {
                Dict.Remove("Died");
                Log.Out("[SERVERTOOLS] Died disabled");
            }
            if (DiscordBot.IsEnabled)
            {
                if (!Dict.Contains("Discord bot"))
                {
                    Dict.Add("Discord bot");
                    Log.Out("[SERVERTOOLS] Discord bot enabled");
                }
            }
            else if (Dict.Contains("Discord bot") && !_initiating)
            {
                Dict.Remove("Discord bot");
                Log.Out("[SERVERTOOLS] Discord bot disabled");
            }
            if (DiscordLink.IsEnabled)
            {
                if (!Dict.Contains("Discord link"))
                {
                    Dict.Add("Discord link");
                    Log.Out("[SERVERTOOLS] Discord link enabled");
                }
            }
            else if (Dict.Contains("Discord link") && !_initiating)
            {
                Dict.Remove("Discord link");
                Log.Out("[SERVERTOOLS] Discord link disabled");
            }
            if (DonationLink.IsEnabled)
            {
                if (!Dict.Contains("Donation link"))
                {
                    Dict.Add("Donation link");
                    Log.Out("[SERVERTOOLS] Donation link enabled");
                }
            }
            else if (Dict.Contains("Donation link") && !_initiating)
            {
                Dict.Remove("Donation link");
                Log.Out("[SERVERTOOLS] Donation link disabled");
            }
            if (DroppedBagProtection.IsEnabled)
            {
                if (!Dict.Contains("Dropped bag protection"))
                {
                    Dict.Add("Dropped bag protection");
                    Log.Out("[SERVERTOOLS] Dropped bag protection enabled");
                }
            }
            else if (Dict.Contains("Dropped bag protection") && !_initiating)
            {
                Dict.Remove("Dropped bag protection");
                Log.Out("[SERVERTOOLS] Dropped bag protection disabled");
            }
            if (EntityCleanup.IsEnabled)
            {
                if (!Dict.Contains("Entity cleanup"))
                {
                    Dict.Add("Entity cleanup");
                    Log.Out("[SERVERTOOLS] Entity cleanup enabled");
                }
            }
            else if (Dict.Contains("Entity cleanup") && !_initiating)
            {
                Dict.Remove("Entity cleanup");
                Log.Out("[SERVERTOOLS] Entity cleanup disabled");
            }
            if (EntityCleanup.Underground)
            {
                if (!Dict.Contains("Entity cleanup underground"))
                {
                    Dict.Add("Entity cleanup underground");
                    Log.Out("[SERVERTOOLS] Entity cleanup underground enabled");
                }
            }
            else if (Dict.Contains("Entity cleanup underground") && !_initiating)
            {
                Dict.Remove("Entity cleanup underground");
                Log.Out("[SERVERTOOLS] Entity cleanup underground disabled");
            }
            if (EntityCleanup.FallingTreeEnabled)
            {
                if (!Dict.Contains("Entity falling tree cleanup"))
                {
                    Dict.Add("Entity falling tree cleanup");
                    Log.Out("[SERVERTOOLS] Entity falling tree cleanup enabled");
                }
            }
            else if (Dict.Contains("Entity falling tree cleanup") && !_initiating)
            {
                Dict.Remove("Entity falling tree cleanup");
                Log.Out("[SERVERTOOLS] Entity falling tree cleanup disabled");
            }
            if (FallingBlocks.IsEnabled)
            {
                if (!Dict.Contains("Falling blocks remover"))
                {
                    Dict.Add("Falling blocks remover");
                    Log.Out("[SERVERTOOLS] Falling blocks remover enabled");
                }
            }
            else if (Dict.Contains("Falling blocks remover") && !_initiating)
            {
                Dict.Remove("Falling blocks remover");
                Log.Out("[SERVERTOOLS] Falling blocks remover disabled");
            }
            if (FirstClaimBlock.IsEnabled)
            {
                if (!Dict.Contains("First claim block"))
                {
                    Dict.Add("First claim block");
                    Log.Out("[SERVERTOOLS] First claim block enabled");
                }
            }
            else if (Dict.Contains("First claim block") && !_initiating)
            {
                Dict.Remove("First claim block");
                Log.Out("[SERVERTOOLS] First claim block disabled");
            }
            if (Fps.IsEnabled)
            {
                if (!Dict.Contains("FPS"))
                {
                    Dict.Add("FPS");
                    Log.Out("[SERVERTOOLS] FPS enabled");
                }
            }
            else if (Dict.Contains("FPS") && !_initiating)
            {
                Dict.Remove("FPS");
                Log.Out("[SERVERTOOLS] FPS disabled");
            }
            if (FriendTeleport.IsEnabled)
            {
                if (!Dict.Contains("Friend teleport"))
                {
                    Dict.Add("Friend teleport");
                    Log.Out("[SERVERTOOLS] Friend teleport enabled");
                }
            }
            else if (Dict.Contains("Friend teleport") && !_initiating)
            {
                Dict.Remove("Friend teleport");
                Log.Out("[SERVERTOOLS] Friend teleport disabled");
            }
            if (Gamble.IsEnabled)
            {
                if (!Dict.Contains("Gamble"))
                {
                    Dict.Add("Gamble");
                    Log.Out("[SERVERTOOLS] Gamble enabled");
                }
            }
            else if (Dict.Contains("Gamble") && !_initiating)
            {
                Dict.Remove("Gamble");
                Log.Out("[SERVERTOOLS] Gamble disabled");
            }
            if (Gimme.IsEnabled)
            {
                if (!Dict.Contains("Gimme"))
                {
                    Dict.Add("Gimme");
                    Log.Out("[SERVERTOOLS] Gimme enabled");
                }
            }
            else if (Dict.Contains("Gimme") && !_initiating)
            {
                Dict.Remove("Gimme");
                Log.Out("[SERVERTOOLS] Gimme disabled");
            }
            if (HighPingKicker.IsEnabled)
            {
                if (!Dict.Contains("High ping kicker"))
                {
                    Dict.Add("High ping kicker");
                    Log.Out("[SERVERTOOLS] High ping kicker enabled");
                }
            }
            else if (Dict.Contains("High ping kicker") && !_initiating)
            {
                Dict.Remove("High ping kicker");
                Log.Out("[SERVERTOOLS] High ping kicker disabled");
            }
            if (Hordes.IsEnabled)
            {
                if (!Dict.Contains("Hordes"))
                {
                    Dict.Add("Hordes");
                    Log.Out("[SERVERTOOLS] Hordes enabled");
                }
            }
            else if (Dict.Contains("Hordes") && !_initiating)
            {
                Dict.Remove("Hordes");
                Log.Out("[SERVERTOOLS] Hordes disabled");
            }
            if (InfoTicker.IsEnabled)
            {
                if (!Dict.Contains("Info ticker"))
                {
                    Dict.Add("Info ticker");
                    Log.Out("[SERVERTOOLS] Info ticker enabled");
                }
            }
            else if (Dict.Contains("Info ticker") && !_initiating)
            {
                Dict.Remove("Info ticker");
                Log.Out("[SERVERTOOLS] Info ticker disabled");
            }
            if (KickVote.IsEnabled)
            {
                if (!Dict.Contains("Kick vote"))
                {
                    Dict.Add("Kick vote");
                    Log.Out("[SERVERTOOLS] Kick vote enabled");
                }
            }
            else if (Dict.Contains("Kick vote") && !_initiating)
            {
                Dict.Remove("Kick vote");
                Log.Out("[SERVERTOOLS] Kick vote disabled");
            }
            if (KillNotice.IsEnabled)
            {
                if (!Dict.Contains("Kill notice"))
                {
                    Dict.Add("Kill notice");
                    Log.Out("[SERVERTOOLS] Kill notice enabled");
                }
            }
            else if (Dict.Contains("Kill notice") && !_initiating)
            {
                Dict.Remove("Kill notice");
                Log.Out("[SERVERTOOLS] Kill notice disabled");
            }
            if (LandClaimCount.IsEnabled)
            {
                if (!Dict.Contains("Land claim count"))
                {
                    Dict.Add("Land claim count");
                    Log.Out("[SERVERTOOLS] Land claim count enabled");
                }
            }
            else if (Dict.Contains("Land claim count") && !_initiating)
            {
                Dict.Remove("Land claim count");
                Log.Out("[SERVERTOOLS] Land claim count disabled");
            }
            if (LevelUp.IsEnabled)
            {
                if (!Dict.Contains("Level up"))
                {
                    Dict.Add("Level up");
                    Log.Out("[SERVERTOOLS] Level up enabled");
                }
            }
            else if (Dict.Contains("Level up") && !_initiating)
            {
                Dict.Remove("Level up");
                Log.Out("[SERVERTOOLS] Level up disabled");
            }
            if (Lobby.IsEnabled)
            {
                if (!Dict.Contains("Lobby"))
                {
                    Dict.Add("Lobby");
                    Log.Out("[SERVERTOOLS] Lobby enabled");
                }
            }
            else if (Dict.Contains("Lobby") && !_initiating)
            {
                Dict.Remove("Lobby");
                Log.Out("[SERVERTOOLS] Lobby disabled");
            }
            if (Loc.IsEnabled)
            {
                if (!Dict.Contains("Location"))
                {
                    Dict.Add("Location");
                    Log.Out("[SERVERTOOLS] Location enabled");
                }
            }
            else if (Dict.Contains("Location") && !_initiating)
            {
                Dict.Remove("Location");
                Log.Out("[SERVERTOOLS] Location disabled");
            }
            if (LoginNotice.IsEnabled)
            {
                if (!Dict.Contains("Login notice"))
                {
                    Dict.Add("Login notice");
                    Log.Out("[SERVERTOOLS] Login notice enabled");
                }
            }
            else if (Dict.Contains("Login notice") && !_initiating)
            {
                Dict.Remove("Login notice");
                Log.Out("[SERVERTOOLS] Login notice disabled");
            }
            if (Lottery.IsEnabled)
            {
                if (!Dict.Contains("Lottery"))
                {
                    Dict.Add("Lottery");
                    Log.Out("[SERVERTOOLS] Lottery enabled");
                }
            }
            else if (Dict.Contains("Lottery") && !_initiating)
            {
                Dict.Remove("Lottery");
                Log.Out("[SERVERTOOLS] Lottery disabled");
            }
            if (Market.IsEnabled)
            {
                if (!Dict.Contains("Market"))
                {
                    Dict.Add("Market");
                    Log.Out("[SERVERTOOLS] Market enabled");
                }
            }
            else if (Dict.Contains("Market") && !_initiating)
            {
                Dict.Remove("Market");
                Log.Out("[SERVERTOOLS] Market disabled");
            }
            if (Motd.IsEnabled)
            {
                if (!Dict.Contains("Motd"))
                {
                    Dict.Add("Motd");
                    Log.Out("[SERVERTOOLS] Motd enabled");
                }
            }
            else if (Dict.Contains("Motd") && !_initiating)
            {
                Dict.Remove("Motd");
                Log.Out("[SERVERTOOLS] Motd disabled");
            }
            if (Mute.IsEnabled)
            {
                if (!Dict.Contains("Mute"))
                {
                    Dict.Add("Mute");
                    Log.Out("[SERVERTOOLS] Mute enabled");
                }
            }
            else if (Dict.Contains("Mute") && !_initiating)
            {
                Dict.Remove("Mute");
                Log.Out("[SERVERTOOLS] Mute disabled");
            }
            if (MuteVote.IsEnabled)
            {
                if (!Dict.Contains("Mute vote"))
                {
                    Dict.Add("Mute vote");
                    Log.Out("[SERVERTOOLS] Mute vote enabled");
                }
            }
            else if (Dict.Contains("Mute vote") && !_initiating)
            {
                Dict.Remove("Mute vote");
                Log.Out("[SERVERTOOLS] Mute vote disabled");
            }
            if (NewPlayer.IsEnabled)
            {
                if (!Dict.Contains("New player"))
                {
                    Dict.Add("New player");
                    Log.Out("[SERVERTOOLS] New player enabled");
                }
            }
            else if (Dict.Contains("New player") && !_initiating)
            {
                Dict.Remove("New player");
                Log.Out("[SERVERTOOLS] New player disabled");
            }
            if (NewPlayerProtection.IsEnabled)
            {
                if (!Dict.Contains("New player protection"))
                {
                    Dict.Add("New player protection");
                    Log.Out("[SERVERTOOLS] New player protection enabled");
                }
            }
            else if (Dict.Contains("New player protection") && !_initiating)
            {
                Dict.Remove("New player protection");
                Log.Out("[SERVERTOOLS] New player protection disabled");
            }
            if (NewSpawnTele.IsEnabled)
            {
                if (!Dict.Contains("New player teleport"))
                {
                    Dict.Add("New player teleport");
                    Log.Out("[SERVERTOOLS] New player teleport enabled");
                }
            }
            else if (Dict.Contains("New player teleport") && !_initiating)
            {
                Dict.Remove("New player teleport");
                Log.Out("[SERVERTOOLS] New player teleport disabled");
            }
            if (NightAlert.IsEnabled)
            {
                if (!Dict.Contains("Night alert"))
                {
                    Dict.Add("Night alert");
                    Log.Out("[SERVERTOOLS] Night alert enabled");
                }
            }
            else if (Dict.Contains("Night alert") && !_initiating)
            {
                Dict.Remove("Night alert");
                Log.Out("[SERVERTOOLS] Night alert disabled");
            }
            if (GeneralOperations.No_Vehicle_Pickup)
            {
                if (!Dict.Contains("No vehicle pickup"))
                {
                    Dict.Add("No vehicle pickup");
                    Log.Out("[SERVERTOOLS] No vehicle pickup enabled");
                }
            }
            else if (Dict.Contains("No vehicle pickup") && !_initiating)
            {
                Dict.Remove("No vehicle pickup");
                Log.Out("[SERVERTOOLS] No vehicle pickup disabled");
            }
            if (OutputLogBlocker.IsEnabled)
            {
                if (!Dict.Contains("Output log blocker"))
                {
                    Dict.Add("Output log blocker");
                    Log.Out("[SERVERTOOLS] Output log blocker enabled");
                }
            }
            else if (Dict.Contains("Output log blocker") && !_initiating)
            {
                Dict.Remove("Output log blocker");
                Log.Out("[SERVERTOOLS] Output log blocker disabled");
            }
            if (OversizedTraps.IsEnabled)
            {
                if (!Dict.Contains("Oversized traps"))
                {
                    Dict.Add("Oversized traps");
                    Log.Out("[SERVERTOOLS] Oversized traps enabled");
                }
            }
            else if (Dict.Contains("Oversized traps") && !_initiating)
            {
                Dict.Remove("Oversized traps");
                Log.Out("[SERVERTOOLS] Oversized traps disabled");
            }
            if (PlayerList.IsEnabled)
            {
                if (!Dict.Contains("Player list"))
                {
                    Dict.Add("Player list");
                    Log.Out("[SERVERTOOLS] Player list enabled");
                }
            }
            else if (Dict.Contains("Player list") && !_initiating)
            {
                Dict.Remove("Player list");
                Log.Out("[SERVERTOOLS] Player list disabled");
            }
            if (POIProtection.IsEnabled)
            {
                if (!Dict.Contains("POI protection"))
                {
                    Dict.Add("POI protection");
                    Log.Out("[SERVERTOOLS] POI protection enabled");
                }
            }
            else if (Dict.Contains("POI protection") && !_initiating)
            {
                Dict.Remove("POI protection");
                Log.Out("[SERVERTOOLS] POI protection disabled");
            }
            if (Prayer.IsEnabled)
            {
                if (!Dict.Contains("Prayer"))
                {
                    Dict.Add("Prayer");
                    Log.Out("[SERVERTOOLS] Prayer enabled");
                }
            }
            else if (Dict.Contains("Prayer") && !_initiating)
            {
                Dict.Remove("Prayer");
                Log.Out("[SERVERTOOLS] Prayer disabled");
            }
            if (Whisper.IsEnabled)
            {
                if (!Dict.Contains("Private message"))
                {
                    Dict.Add("Private message");
                    Log.Out("[SERVERTOOLS] Private message enabled");
                }
            }
            else if (Dict.Contains("Private message") && !_initiating)
            {
                Dict.Remove("Private message");
                Log.Out("[SERVERTOOLS] Private message disabled");
            }
            if (Waypoints.Public_Waypoints)
            {
                if (!Dict.Contains("Public waypoints"))
                {
                    Dict.Add("Public waypoints");
                    Log.Out("[SERVERTOOLS] Public waypoints enabled");
                }
            }
            else if (Dict.Contains("Public waypoints") && !_initiating)
            {
                Dict.Remove("Public waypoints");
                Log.Out("[SERVERTOOLS] Public waypoints disabled");
            }
            if (RealWorldTime.IsEnabled)
            {
                if (!Dict.Contains("Real world time"))
                {
                    Dict.Add("Real world time");
                    Log.Out("[SERVERTOOLS] Real world time enabled");
                }
            }
            else if (Dict.Contains("Real world time") && !_initiating)
            {
                Dict.Remove("Real world time");
                Log.Out("[SERVERTOOLS] Real world time disabled");
            }
            if (RegionReset.IsEnabled)
            {
                if (!Dict.Contains("Region reset"))
                {
                    Dict.Add("Region reset");
                    Log.Out("[SERVERTOOLS] Region reset enabled");
                }
            }
            else if (Dict.Contains("Region reset") && !_initiating)
            {
                Dict.Remove("Region reset");
                Log.Out("[SERVERTOOLS] Region reset disabled");
            }
            if (ReservedSlots.IsEnabled)
            {
                if (!Dict.Contains("Reserved slots"))
                {
                    Dict.Add("Reserved slots");
                    Log.Out("[SERVERTOOLS] Reserved slots enabled");
                }
            }
            else if (Dict.Contains("Reserved slots") && !_initiating)
            {
                Dict.Remove("Reserved slots");
                Log.Out("[SERVERTOOLS] Reserved slots disabled");
            }
            if (RIO.IsEnabled)
            {
                if (!Dict.Contains("Roll it up"))
                {
                    Dict.Add("Roll it up");
                    Log.Out("[SERVERTOOLS] Roll it up enabled");
                }
            }
            else if (Dict.Contains("Roll it up") && !_initiating)
            {
                Dict.Remove("Roll it up");
                Log.Out("[SERVERTOOLS] Roll it up disabled");
            }
            if (ScoutPlayer.IsEnabled)
            {
                if (!Dict.Contains("Scout player"))
                {
                    Dict.Add("Scout player");
                    Log.Out("[SERVERTOOLS] Scout player enabled");
                }
            }
            else if (Dict.Contains("Scout player") && !_initiating)
            {
                Dict.Remove("Scout player");
                Log.Out("[SERVERTOOLS] Scout player disabled");
            }
            if (Shutdown.IsEnabled)
            {
                if (!Dict.Contains("Shutdown"))
                {
                    Dict.Add("Shutdown");
                    Log.Out("[SERVERTOOLS] Shutdown enabled");
                }
            }
            else if (Dict.Contains("Shutdown") && !_initiating)
            {
                Dict.Remove("Shutdown");
                Log.Out("[SERVERTOOLS] Shutdown disabled");
            }
            if (Homes.IsEnabled)
            {
                if (!Dict.Contains("Homes"))
                {
                    Dict.Add("Homes");
                    Log.Out("[SERVERTOOLS] Homes enabled");
                }
            }
            else if (Dict.Contains("Homes") && !_initiating)
            {
                Dict.Remove("Homes");
                Log.Out("[SERVERTOOLS] Homes disabled");
            }
            if (Shop.IsEnabled)
            {
                if (!Dict.Contains("Shop"))
                {
                    Dict.Add("Shop");
                    Log.Out("[SERVERTOOLS] Shop enabled");
                }
            }
            else if (Dict.Contains("Shop") && !_initiating)
            {
                Dict.Remove("Shop");
                Log.Out("[SERVERTOOLS] Shop disabled");
            }
            if (SleeperRespawn.IsEnabled)
            {
                if (!Dict.Contains("Sleeper respawn"))
                {
                    Dict.Add("Sleeper respawn");
                    Log.Out("[SERVERTOOLS] Sleeper respawn enabled");
                }
            }
            else if (Dict.Contains("Sleeper respawn") && !_initiating)
            {
                Dict.Remove("Sleeper respawn");
                Log.Out("[SERVERTOOLS] Sleeper respawn disabled");
            }
            if (StartingItems.IsEnabled)
            {
                if (!Dict.Contains("Starting items"))
                {
                    Dict.Add("Starting items");
                    Log.Out("[SERVERTOOLS] Starting items enabled");
                }
            }
            else if (Dict.Contains("Starting items") && !_initiating)
            {
                Dict.Remove("Starting items");
                Log.Out("[SERVERTOOLS] Starting items disabled");
            }
            if (Suicide.IsEnabled)
            {
                if (!Dict.Contains("Suicide"))
                {
                    Dict.Add("Suicide");
                    Log.Out("[SERVERTOOLS] Suicide enabled");
                }
            }
            else if (Dict.Contains("Suicide") && !_initiating)
            {
                Dict.Remove("Suicide");
                Log.Out("[SERVERTOOLS] Suicide disabled");
            }
            if (Travel.IsEnabled)
            {
                if (!Dict.Contains("Travel"))
                {
                    Dict.Add("Travel");
                    Log.Out("[SERVERTOOLS] Travel enabled");
                }
            }
            else if (Dict.Contains("Travel") && !_initiating)
            {
                Dict.Remove("Travel");
                Log.Out("[SERVERTOOLS] Travel disabled");
            }
            if (Vault.IsEnabled)
            {
                if (!Dict.Contains("Vault"))
                {
                    Dict.Add("Vault");
                    Log.Out("[SERVERTOOLS] Vault enabled");
                }
            }
            else if (Dict.Contains("Vault") && !_initiating)
            {
                Dict.Remove("Vault");
                Log.Out("[SERVERTOOLS] Vault disabled");
            }
            if (VehicleRecall.IsEnabled)
            {
                if (!Dict.Contains("Vehicle recall"))
                {
                    Dict.Add("Vehicle recall");
                    Log.Out("[SERVERTOOLS] Vehicle recall enabled");
                }
            }
            else if (Dict.Contains("Vehicle recall") && !_initiating)
            {
                Dict.Remove("Vehicle recall");
                Log.Out("[SERVERTOOLS] Vehicle recall disabled");
            }
            if (Voting.IsEnabled)
            {
                if (!Dict.Contains("Voting"))
                {
                    Dict.Add("Voting");
                    Log.Out("[SERVERTOOLS] Voting enabled");
                }
            }
            else if (Dict.Contains("Voting") && !_initiating)
            {
                Dict.Remove("Voting");
                Log.Out("[SERVERTOOLS] Voting disabled");
            }
            if (Wall.IsEnabled)
            {
                if (!Dict.Contains("Wall"))
                {
                    Dict.Add("Wall");
                    Log.Out("[SERVERTOOLS] Wall enabled");
                }
            }
            else if (Dict.Contains("Wall") && !_initiating)
            {
                Dict.Remove("Wall");
                Log.Out("[SERVERTOOLS] Wall disabled");
            }
            if (Wallet.IsEnabled)
            {
                if (!Dict.Contains("Wallet"))
                {
                    Dict.Add("Wallet");
                    Log.Out("[SERVERTOOLS] Wallet enabled");
                }
            }
            else if (Dict.Contains("Wallet") && !_initiating)
            {
                Dict.Remove("Wallet");
                Log.Out("[SERVERTOOLS] Wallet disabled");
            }
            if (Waypoints.IsEnabled)
            {
                if (!Dict.Contains("Waypoints"))
                {
                    Dict.Add("Waypoints");
                    Log.Out("[SERVERTOOLS] Waypoints enabled");
                }
            }
            else if (Dict.Contains("Waypoints") && !_initiating)
            {
                Dict.Remove("Waypoints");
                Log.Out("[SERVERTOOLS] Waypoints disabled");
            }
            if (WebAPI.IsEnabled)
            {
                if (!Dict.Contains("Web API"))
                {
                    Dict.Add("Web API");
                    Log.Out("[SERVERTOOLS] Web API enabled");
                }
            }
            else if (Dict.Contains("Web API") && !_initiating)
            {
                Dict.Remove("Web API");
                Log.Out("[SERVERTOOLS] Web API disabled");
            }
            if (WebPanel.IsEnabled)
            {
                if (!Dict.Contains("Web panel"))
                {
                    Dict.Add("Web panel");
                    Log.Out("[SERVERTOOLS] Web panel enabled");
                }
            }
            else if (Dict.Contains("Web panel") && !_initiating)
            {
                Dict.Remove("Web panel");
                Log.Out("[SERVERTOOLS] Web panel disabled");
            }
            if (WorkstationLock.IsEnabled)
            {
                if (!Dict.Contains("Workstation lock"))
                {
                    Dict.Add("Workstation lock");
                    Log.Out("[SERVERTOOLS] Workstation lock enabled");
                }
            }
            else if (Dict.Contains("Workstation lock") && !_initiating)
            {
                Dict.Remove("Workstation lock");
                Log.Out("[SERVERTOOLS] Workstation lock disabled");
            }
            if (Zones.IsEnabled)
            {
                if (!Dict.Contains("Zones"))
                {
                    Dict.Add("Zones");
                    Log.Out("[SERVERTOOLS] Zones enabled");
                }
            }
            else if (Dict.Contains("Zones") && !_initiating)
            {
                Dict.Remove("Zones");
                Log.Out("[SERVERTOOLS] Zones disabled");
            }
            if (_initiating)
            {
                Log.Out("--------------------------------");
                Log.Out("[SERVERTOOLS] Tool load complete");
                Log.Out("--------------------------------");
            }
            else
            {
                Log.Out("[SERVERTOOLS] Config updated");
            }
        }
    }
}
