# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>
<br>
# Installation
<br>
Go to the releases tab and check for the latest files https://github.com/dmustanger/7dtd-ServerTools/releases <br>
<br>
Download and extract the Mods folder found inside the .zip file.<br>
<br>
Copy the Mods folder to the installation directory of your dedicated server. Example: C:\7DaysToDieServers\Dedicated<br>
<br>
Start the server.<br>
<br>
It will create another folder called ServerTools. It will be in the same folder your Mods folder is located.<br>
The mod will auto create the main config file inside of this folder.<br>
Enable each tool from the mod you want via your ServerToolsConfig.xml<br>
Once a tool is enabled, if it has a xml file then it will auto create them in the ServerTools folder.<br>
<br>
<br>
<br>
# Current-Tools-and-Features
<br>
<br>
Anti Cheat<br>
<br>
<br>
Automatically detects and deals with cheaters. Customizable<br>
<br>
Detects: flying above or below ground, god mode, specatator mode, invalid items, invalid stack sizes, family share accounts. <br>
<br>
Player stats: height, speed, jump strength, max stamina, max health.<br>
<br>
Maximum damage to players, entities and blocks.<br>
<br>
Logs all player's inventory to a file for later review. Set the time between loggings. Default is once per minute.<br>
<br>
Records player's IP address and location in the world.<br>
<br>
Records identical items and stacks found in a players inventory. These are not necessarily duped but this gives a record.<br>
<br>
Logs of violators are auto created and deleted based on your settings.<br>
<br>
<br>
<br>
Other Tools<br>
<br>
<br>
Day7 alerts: automatically receive in game alerts to the upcoming bloodmoon. Can set a custom horde night value<br>
<br>
Zones: set an area as a protected space. PvP inside is dealt with automatically. Alert upon entry/exit<br>
<br>
Weather vote: vote to change the weather<br>
<br>
Teleport to friend: request to teleport to a friend. They must accept the request in a time limit<br>
<br>
Shop and wallet: make a custom shop list for players to buy from. Wallet is calculated automatically<br>
<br>
Vote rewards: players receive rewards from a custom list you setup after they vote at https://7daystodie-servers.com<br>
<br>
Travel: players can travel to specific locations when in the right location<br>
<br>
Starting items: new players receive all the items from the StartingItems.xml<br>
<br>
First claim block: players can type /claim to receive a single claim block. Prevents griefing<br>
<br>
Give item: give item directly into a players inventory or all online players. Drops to the ground if their inventory is full<br>
<br>
Admin List: players can see an ingame list of currently online admins and moderators<br>
<br>
Command delays: multiple commands have delay times. Players can not use the command until the delay has expired since last use<br>
<br>
Temporary ban: console command tempban to allow a limited ban time for moderators or low tier admins<br>
<br>
Custom chat command triggers: commands use / and ! to initialize commands but they can be set to any symbol<br>
<br>
Falling entities: automatically deletes zombies, trees and items that fall below the world<br>
<br>
Chat colors and prefixes: administrators, moderators, donators/reserved players, special players can all get custom chat colors and prefix<br>
<br>
Auto shutdown: the server will auto shutdown after a scheduled time frame. Initiates shutdown system with alerts, world save, countdown<br>
<br>
Animal spawning: made to be like an animal tracking system. Players spawn a random animal from a list of entity id you can choose<br>
<br>
Stealth admins: while chat color and prefix is active, admins can disable their chat color to stay hidden<br>
<br>
Reserved players: reserve players on a list which will allow them to join the server if full and a non reserved player can be kicked<br>
This also helps with admins joining a full server. It will kick a reserved player if it must but attempt a non reserved first<br>
<br>
Custom commands: make your own commands and set optional delays, command costs, permission requirement or hide it from /commands response<br>
<br>
Check next shutdown: can check when the next scheduled shutdown period is<br>
<br>
Reservation check: players can check the expiration time for their reserved status<br>
<br>
Gimme system: players can spawn a free item periodically<br>
<br>
Suicide commands: players can commit suicide with a few different suicide commands. /Hang, /wrist, /suicide<br>
<br>
Set home: saves a home location so that players can teleport to it. Can teleport with friends<br>
<br>
Set home 2: a second home location that optionally can be set for donators only<br>
<br>
High ping kicker: kicks players that have too high of a ping<br>
<br>
Chat logger: saves all in game chat to a file for later review<br>
<br>
Bad word filter: replaces any words used in chat that matches ones from a list. Can also filter bad player names<br>
<br>
Message of the day: shows players a custom message upon entering the server<br>
<br>
Infoticker: automatically displays custom server messages. Can be set to display in a random order or sequential<br>
<br>
Auto world save: automatically saves the world based on custom delay time<br>
<br>
Watchlist: players listed here will be able to join but send online admins an alert when they are in game<br>
<br>
Custom phrases: any phrases listed in the phrase file can be set to a custom value<br>
<br>
Custom command triggers: any command trigger listed in the trigger file can be set to a custom value<br>
<br>
Admin chat: sends all online admins a direct private message<br>
<br>
Clan manager: create clan tags, add players, hire officers and rule the server as a elite crew<br>
<br>
Remove entity: remove a live entity from the game using its entity id via console<br>
<br>
Reset player profile: will kick the player and delete their saved data via console<br>
<br>
Stopserver command: easily shutdown your server with an alert system and countdown so players are aware<br>
<br>
Lottery: players can open a lottery for any players to buy in. Lottery draws after 10 players join, 1 hour has passed or server is shutting down<br>
<br>
Hardcore: limited life count. Kicked and profile deleted if out of lives. Stats recorded<br>
<br>
Friend teleports: players can teleport to saved home points and send invites to near by friends to teleport as well<br>
<br>
Kill notice: alerts when a player kills another player and what weapon was used<br>
<br>
Bounties: players can put bounties on each other. If a player kills another player with a bounty on them, the value is added to their wallet<br>
<br>
Kill streaks: while bounties are active, a kill streak can add to a player's bounty automatically<br>
<br>
Kick vote: vote to kick a player. Requires at least 10 players online and 8 must vote yes<br>
<br>
Restart vote: vote to restart the server. Requires at least 10 players online and 8 must vote yes<br>
<br>
Player list: shows all online player names and entity id so players can use commands against them<br>
<br>
Lobby: teleports to a defined location. Allows players to return when finished but they must stay inside the lobby<br>
<br>
Market: teleports to a defined location. Allows players to return when finished but they must stay inside the market<br>
<br>
Real world time: displays the real world time based on the servers local time<br>
<br>
Fps: allows you to set the target server fps on server load up<br>
<br>
Location: players can check their world location, response is their x, y, z cordinates<br>
<br>
Auction: place an item inside a secure loot and use commands for it to be removed and listed for sale in the auction<br>
<br>
Wallet: takes note of player kills, zombie kills, deaths and spent coins. Calculates a total for use in the shop, auction, bounties<br>
<br>
Bank: allows players to deposit in game coins or wallet coins to a bank account. Exchange coin types through the bank<br>
<br>
Vehicle teleport: save a vehicle while inside a claimed space. Teleport the vehicle if close enough to it<br>
<br>
Death spot: players can return to where they died<br>
<br>
Command costs: set a price required to use certain commands. Dependant on ingame wallet and earning coins by killing zombies/players. In game coins such as casino coin can be transferred through the bank<br>
<br>
Country ban: blocks players attempting to join with an ip matching specific country ranges. Uses ISO country codes<br>
<br>
Bloodmoon warrior: players that are chosen by Hades on the bloodmoon must survive the night and kill enough zombies for a reward<br>
<br>
Falling blocks removal: all falling blocks are removed immediately to prevent server lag from block collapse<br>
<br>
Break reminder: reminds players to stretch and take a break if they have been playing too long<br>
<br>
Login notice: shows a custom notice when specific players join<br>
<br>
Jail: players can be jailed manually or from PvE violations that will trap them within a specific range of a set postion. Optional electric fence for attempted escapes<br>
<br>
Kill notice: shows the PvP combat with more information upon kills. Can show final damage, player levels and weapons used<br>
<br>
Message color: change the default chat message color for everyone<br>
<br>
Mute: players can mute each other or be muted from all chat function by an admin<br>
<br>
New player teleport: teleports new players to a designated position<br>
<br>
Night alert: will give periodic alerts to how many hours remain before night<br>
<br>
Poll: setup polls for players to vote on<br>
<br>
Prayer: gives players a buff from a custom list. Recommend making new buffs with longer durations<br>
<br>
Private messages: players can send private messages to each other<br>
<br>
Scout players: players can check for tracks of nearby players within a limited range<br>
<br>
Tracking: logs all player movement for admins to review via console<br>
<br>
Waypoints: players can setup teleport waypoints and can teleport with friends<br>
<br>
<br> 
<br>
