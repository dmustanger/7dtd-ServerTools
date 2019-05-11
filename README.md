# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>
<br>
# Installation
<br>
Go to the releases tab and check for the latest files https://github.com/dmustanger/7dtd-ServerTools/releases <br>
<br>
Download and extract the files.<br>
<br>
Copy the Mods and 7DaysToDieServer_Data folder to the root installation directory of your server.<br>
<br>
Start the server.<br>
<br>
The mod will auto create the config file in the game installation directory.<br>
Enable each part of the mod you want via ..\your server installation directory\ServerTools\ServerToolsConfig.xml<br>
Once a module is enabled, if it has a config it will auto create them in the ServerTools folder.<br>
<br>
<br>
<br>
# Current-Tools-and-Features
<br>
<br>
Anti Cheat: Multiple tools available to deal with cheaters. Automatically detects and deals with them. Customizable<br>
<br>
Other Tools<br>
<br>
Admin_List: Displays the current online admins and moderators<br>
<br>
Animal_Tracking: Use chat command /track to spawn an animal in game near by for players to hunt<br>
<br>
Auction: Place items up for auction for other players to buy<br>
<br>
Auto_Backup: Back up your world save files automatically to a zip file. Set the destination or leave default<br>
<br>
Auto_Save_World: Starts a world save in game<br>
<br>
Auto_Shutdown: Set the time your server will run before it shuts down automatically. Does not auto restart<br>
<br>
Bad_Word_Filter: Filters bad words and symbols of your choice from chat and player names<br>
<br>
Bank: Store your in game coins in the bank for safe keeping. Transfer between accounts or convert to wallet coins<br>
<br>
Bloodmoon: Remind players of the upcoming bloodmoon periodically and when they join the server<br>
<br>
Bounties: Set bounties on each other for PvP play. The killers are awarded wallet coins and can get on kill streaks where they gain a bounty against them<br>
<br>
Break_Reminder: Periodically remind all players to take a break with a custom message of your choice<br>
<br>
Chat_Color_Prefix: Control the in game chat colors for admins, moderators, reserved players, special players, clans and anyone else you want to customize. Allows for a special prefix and color for each group and limitless groups and combinations possible<br>
<br>
Chat_Flood_Protection: Protects the chat from spam by limiting the length of a message and how many can be sent in a single minute<br>
<br>
Chat_Logger: Log all chat from players and commands used for review<br>
<br>
Clan_Manager: Let players create clans which add tags to their names in chat and to talk to each other as a group<br>
<br>
Country_Ban: Control what countries a player may reside in. Checks their ip against known ISO country codes. Kicks banned countries<br>
<br>
Credentials: Checks the i.p. address and steam id supplied by the player upon joining as valid or not<br>
<br>
Custom_Commands: Create your own custom commands that trigger unlimited console commands with custom delays and costs to use<br>
<br>
Day7: Lets players check for the upcoming bloodmoon, current server fps, current zombie and animal numbers<br>
<br>
Death_Spot: If a player dies, they will be offered a return command upon respawn to teleport them back to their death spot<br>
<br>
Dupe_Log: Monitors player inventory and reports duplicates in a log. This does not mean they duped the items but gives a report for review<br>
<br>
Entity_Cleanup: Allows removal of specific objects such as falling trees and blocks or minibikes<br>
<br>
First_Claim_Block: Allows players to spawn a claim block through a chat command but only once<br>
<br>
FPS: Set the target fps for your server. They are default locked for console operations. Recommend setting at least 60<br>
<br>
Friend_Teleport: Players can list online friends and their entityId so they can send a request to teleport to them. The friend must accept it first<br>
<br>
Gimme: Make a list of items players can periodically spawn an item from. Set a delay so players get a gift for continued play<br>
<br>
God_And_Flight_Detector: Detects users using god mode and no clip flight mode. Automatically bans them within .5 seconds of enabling it<br>
<br>
Hardcore: Players have limited lives available and if they lose them all, their character is kicked and deleted. Roguelike game play<br>
<br>
Hatch_Elevator_Detector: Detects players that use hatch elevators. Automatically breaks their legs and stuns them when detected<br>
<br>
High_Ping_Kicker: Detects users with an overly high ping and kicks them from the server<br>
<br>
Hordes: Automatically checks how many active zombies are alive and spawns a horde randomly if too few zombies alive<br>
<br>
Info_Ticker: Create a list of rotating messages displayed to players periodically. Set to display chronologically or randomly<br>
<br>
Invalid_Item_Kicker: Monitors players inventory for items listed as invalid. Create a custom list of forbidden items. Optionally detects items marked dev or none for creativemode in the items.xml. Optionally checks secureloot for invalid items and removes them<br>
<br>
Jail: Send players to jail where they are stuck in a specific location. Traveling too far from it teleports them back and optionally shocks them for attempting to leave<br>
<br>
Kick_Vote: Players can open a vote to kick a player from the server. Set the players needed online and to vote yes<br>
<br>
Kill_Notice: Display an alert in chat of a player on player kill showing the weapon used by the killer<br>
<br>
Lobby: Players can teleport to the lobby area and optionally return<br>
<br>
Location: Displays the current player location via chat command<br>
<br>
Login_Notice: Set custom messages for players entering the server<br>
<br>
Logs: Logs of cheaters, detection, events and chat are all kept for a limited time and automatically removed based on your setting<br>
<br>
Lottery: Players can start a gambling lottery that randomly picks a winner on draw<br>
<br>
Market: Players can teleport to the market area and optionally return<br>
<br>
Motd: Message of the day will display when players first join the server. Optionally on respawn<br>
<br>
Mute_Vote: Players can open a vote to mute player from the server. Set the players needed online and to vote yes<br>
<br>
New_Player: Set a custom message displayed when a new player joins the server<br>
<br>
New_Spawn_Tele: Set a location new players will teleport to upon first joining the server<br>
<br>
Night_Alert: Displays an alert to how long before night<br>
<br>
Night_Vote: Players can open a vote to skip the night on the server. Set the players needed online and to vote yes<br>
<br>
Normal_Player_Name_Coloring: A Players chat, where they are not on the chat color prefix list, will have their chat color set to this<br>
<br>
Player_List: Shows the current online players and entity id in chat<br>
<br>
Player_Logs: Monitors and logs players inventory. Optionally their position and if dead or alive<br>
<br>
Player_Stat_Check: Monitors player stats for improper values for height, speed, jump strength, health and stamina<br>
<br>
Private_Message: Allows players to send private messages to each other<br>
<br>
Real_World_Time: Periodically displays the real world time in chat<br>
<br>
Report: Allows players to send a report to all online admins and to a report log for later review<br>
<br>
Reserved_Slots: Set players for reserved slots to help them get in your server. If the server is full, if reserved it will allow you to stay and kick one person if not. Optionally controlled by session time to help rotate new players. None reserved must reach the session time before kick due to a full server<br>
<br>
Restart_Vote: Players can open a vote to restart the server. Set the players needed online and to vote yes. Alerts an admin if they are online instead of opening the vote<br>
<br>
Session: Logs the length of player play time. Responds in chat with the totals<br>
<br>
Set_Home: Set locations as home points inside claimed space for teleport. Second location optional. Reserved only optional<br>
<br>
Shop: Setup a list of categories and items for each that players can buy<br>
<br>
Starting_Items: Spawns a list of items for a new player when they first join the server<br>
<br>
Stopserver: Stop the server by alerting the players and automatically saving the world to prevent data loss<br>
<br>
Stuck: Lets players that are stuck, teleport to the surface. Checks if truly stuck. Not full proof<br>
<br>
Suicide: Players can kill their character at will<br>
<br>
Tracking: Logs player locations and the item they were holding at the time. Check who has traveled through an area based on time and range from your location or a specified one<br>
<br>
Travel: Setup locations players can stand and type /travel to teleport to a specified location<br>
<br>
Vehicle_Teleport: Players can set one of each vehicle type to them self and teleport them near by if they are found close enough<br>
<br>
Voting: Players can vote for their favorite server, yours of course and then receive a reward in game you setup as a list of items or as an entity spawn such as a loot crate<br>
<br>
Wallet: Enables a digital wallet that players receive coin in for killing zombies and players or lose value for dying. Set the values received for each action. Set if players lose it all on death. Use wallet coins for optional command costs<br>
<br>
Watchlist: Set a list of suspicious players you want admins to receive an alert to when online<br>
<br>
Waypoints: Players can save, name and delete custom waypoints for teleport but it must not be inside a claimed space<br>
<br>
Weather_Vote: Players can open a vote to change the weather on the server. Set the players needed online and to vote yes<br>
<br>
World_Radius: Controls the amount of blocks from center of the map a player can travel before being teleport back. Optional reserved player distance can be set larger for special area only for donors<br>
<br>
Zone: Create zones that give messages, apply console commands upon entering, remove zombies that enter it or create PvE spaces that do not allow PvP without automatic detection and punishment to the killer. Victims are offered teleport back to their death spot<br>
<br>
<br> 
<br>
