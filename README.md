# 7dtd-ServerTools

Server tools for 7 Days to Die dedicated Windows & Linux servers

## Installation

1. Go to the releases tab and check for the latest release: <https://github.com/dmustanger/7dtd-ServerTools/releases>
2. Download and extract the zip file.
3. Copy the **Mods** & **7DaysToDieServer_Data** folders to the installation directory of your 7Days to Die dedicated server. Copy the entire two folders, not their contents.
4. Start your 7 Days to Die dedicated server.

  After your dedicated server starts, the modlet will automatically create a ServerTools\ServerToolsConfig.xml file within the 'current directory' of your 7 Days to Die dedicated server. Edit this ServerToolsConfig.xml file and enable any module of the mod that you wish. Any changes to this file are live and do not require a restart. Once a module is enabled, if it has a configuration file it will automatically create it within the ServerTools folder.

## Current-Tools-and-Features

**Anti Cheat**

Automatically detects and deals with cheaters. Highly customizable.

- **Detects Anomalies:** flying above or below ground, hatch elevator, teleport, invalid items, invalid stack sizes, family share accounts, player stats such as height, speed, jump strength, max stamina, max health, and available skill points.
- **Logs all players' inventory** to a file for later review. Set the time between loggings. Default is once-per-minute.
- **Records players' IP addresses** and location to the inventory log.
- **Records identical items and stacks** found in a players inventory. These are not necessarily duped but this gives a record.
- **Create logs of specific violators** which are automatically created and deleted based on your settings.

**Player Features**

- **New Player Items:** New players to receive all items you specify in StartingItems.xml
- **Player List:** Shows all online player names and entity id so players can use commands against them.
- **Location:** Players can check their own world location, response is their x, y, z cordinates.
- **Clan Manager:** Create clan tags, add players, hire officers and rule the server as a elite crew.
- **Wallet:** Tracks player kills, zombie kills, deaths, and spent coins. Provides coins for use in the shop, auction, and bounties.
- **Shop & Wallet:** Create a custom shop list for players to buy from. Wallet is calculated automatically. Can only shop in trader zone.
- **Bank:** Players can deposit in-game coins or wallet coins to a bank account. Exchange coin types through the bank.
- **Death Return:** Players can return to where they died.
- **Bike:** Save a bike id while inside a claimed space. Return a saved bike if close enough to it.
- **Weather Vote:** Vote to change the weather.
- **Teleport to Friend:** Request to teleport to a friend. They must accept the request within a time limit.
- **Gimme System:** Spawn a free item periodically with a custom delay.
- **Lottery:** Player-created lotter for any players to buy in. Lottery draws after 10 players join or 1 hour has gone by.
- **Auction:** Place an item inside a secure loot and use commands to remove it and sell it in the auction.
- **Vote Rewards:** Vote rewards for voting on your server at: <https://7daystodie-servers.com>
- **Kick Vote:** Vote to kick a player. Requires at least 10 players online and 8 must vote yes.
- **Restart Vote:** Vote to restart the server. Requires at least 10 players online and 8 must vote yes.
- **Travel:** Travel to specific locations when in the right location. Can teleport a bike. Custom delay.
- **First Claim Block:** Type /claim to receive a single claim block. Helps to prevent griefing.
- **Admin List:** Players can type trigger an in-game list of currently online admins and moderators.
- **Animal Spawning:** Spawn a random animal from a list.
- **Check Next Shutdown:** Check when the next scheduled shutdown will happen.
- **Reservation Check:** Check the expiration time for their reserved status.
- **Killme Command:** Commit suicide with a custom delay.
- **Bounties:** Players can put bounties on each other. If a player kills another player with a bounty on them, the value is added to their wallet.
- **Kill Streaks:** While bounties are active, a kill streak can add to a player's bounty automatically.
- **Set Home:** Save a home location so players can teleport to it with a custom delay.
- **Set Home 2:** Save a second home location that can be set for donators only.
- **Friend Teleports:** Players can teleport to saved home points and send invites to near by friends to teleport as well.
- **Lobby:** Teleports to a defined location. Allows players to return when finished but they must be with in 50 blocks to the lobby location.
- **Real World Time:** Displays the real world time based on the servers local time.

**Admin Features**

- **Message Of The Day:** Displays a custom message when a player enters the server.
- **Infoticker:** Periodically displays custom server messages. Can be set to display in a random order.
- **Custom Chat Command Triggers:** Commands are prefixed with / and ! but can be set to any symbol.
- **Command Costs:** Set a price required to use certain commands. Dependent on in-game wallet and earning coins by killing zombies. In-game coins such as casino coin can be transferred through the bank.
- **Chat Colors and Prefixes:** administrators, moderators, donators/reserved players, special players can all get custom chat colors and prefix.
- **Kill Notice:** Alerts when a player kills another player and what weapon was used.
- **Bad Word Filter:** Replace any words used in chat that matches ones from a list. Can also change bad player names to Invalid Name-No commands.
- **Custom Phrases:** Phrases listed in the phrase file can be set to a custom response.
- **Admin Chat:** Admins can send all other online admins a direct private message.
- **Stealth Admins:** While chat color and prefix is active, admins can disable their chat color to stay hidden.
- **Custom Commands:** Make your own commands and set optional delays for the first ten entries.
- **Command Delays:** Multiple commands have delay times. Players can not use the command until the delay has expired since last use.
- **Command Delay Reset:** Reset any player command delays via the console.
- ****Day7 Alerts**: Receive in-game alerts to the upcoming BloodMoon. Can set a custom horde night value.
- **Zones:** Set an area as a protected space. PvP inside is dealt with automatically. Alert upon entry/exit.
- **Give Item:** Give item into a player's inventory or all online players. Drops to the ground if inventory is full.
- **Give Block:** Give block into a player's inventory or all online players. Drops to the ground if inventory is full.
- **Remove Entity:** Remove a live entity from the game using its entity id via console.
- **Admin Alerts:** Detections that require manual inspection is sent to currently online admins.
- **Hardcore:** Limited life count. Kicked and profile deleted if out of lives. Stats recorded.
- **FPS:** Set the server FPS goal.

**Server Management**

- **Auto World Save:** Automatically saves the world based on custom delay time.
- **Auto Shutdown:** Server will auto shutdown after a scheduled time frame. Initiates shutdown system with alerts, world save, countdown.
- **Stopserver Command:** Shutdown your server with an alert system and countdown so players are aware.
- **Temporary Ban:** Console command /tempban to allow a limited ban time for moderators or low tier admins.
- **Stuck Entity:** Automatically sends bikes, bags, and zombies to the world surface to prevent errors and server lag.
- **Entity Cleanup:** Removes falling blocks, stuck falling trees.
- **Reserved Players:** Based on session time. Final slot can only be filled by admin or reserved status player. Boots players passed max session time to make space for others. Reserved and admins are never kicked.
- **High Ping Kicker:** kicks players that have a ping too high.
- **Chat Logger:** saves all in game chat to a file for later review.
- **Watchlist:** players listed here will be able to join but send online admins an alert when they are in game.
- **Reset Player Profile:** Console command will kick and delete the saved data of a player.
