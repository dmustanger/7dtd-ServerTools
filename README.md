# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>
<br>
Donations to the project can be made here. Donors and their information is private<br>
https://www.paypal.me/ObsessiveCoder <br>
<br>
# Installation
<br>
Go to the releases tab and check for the latest ServerTools files for download. https://github.com/dmustanger/7dtd-ServerTools/releases <br>
<br>
*Optionally you can find Country ban and Proxy ban tools at the links provided below. They are modules that work along side ServerTools and get installed the same as any other mod for your server. They both require ServerTools to function. <br>
https://github.com/ObsComp/ServerTools-CountryBan/releases <br>
https://github.com/ObsComp/ServerTools-ProxyBan/releases <br>
<br>
They have been separated to reduce the size of ServerTools since not all servers need it. It also allows us to update the database for the IP lists of countries and proxies without needing to update ServerTools.*<br>
<br>
Download and extract the files.<br>
<br>
Copy the Mods folder from the latest release to the user data directory or to the root directory of your server if you prefer.<br>
It is highly recommended that you use the user data folder. The root folder can be used but the developers will eventually drop support for mods located there.<br>
<br>
Start the server.<br>
<br>
The mod will auto create a new folder named ServerTools. Inside this folder is the ServerToolsConfig.xml that controls most of mod.<br>
Enable each part of the mod you want via ServerToolsConfig.xml<br>
Once a module/tool is enabled, if it has an xml it will be generated and placed in the same ServerTools folder.<br>
<br>

# AntiCheat

- [AntiCheat](#anticheat)
  - [Damage_Detector](#damage_detector)
  - [Dupe_Log](#dupe_log)
  - [Flying_Detector](#flying_detector)
  - [Godmode_Detector](#godmode_detector)
  - [Infinite_Ammo](#infinite_ammo)
  - [Invalid_Buffs](#invalid_buffs)
  - [Invalid_Items](#invalid_items)
  - [Invalid_Item_Stack](#invalid_item_stack)
  - [Jail](#jail)
  - [Player_Logs](#player_logs)
  - [Player_Stats & Player_Stats_Extended](#player_stats--player_stats_extended)
  - [Protected_Zones](#protected_zones)
  - [PvE_Violations](#pve_violations)
  - [Spectator_Detector](#spectator_detector)
  - [Speed_Detector](#speed_detector)
  - [XRay_Detector](#xray_detector)

# Tools

- [Tools](#tools)
  - [AdminChatCommands](#adminchatcommands)
  - [Admin_List](#admin_list)
  - [Allocs_Map](#allocs_map)
  - [Animal_Tracking](#animal_tracking)
  - [Auction](#auction)
  - [Auto_Backup](#auto_backup)
  - [Auto_Party_Invite](#auto_party_invite)
  - [Auto_Restart](#auto_restart)
  - [Auto_Save_World](#auto_save_world)
  - [Bad_Word_Filter](#bad_word_filter)
  - [Bank](#bank)
  - [Bed](#bed)
  - [Big_Head](#big_head)
  - [Block_Logger](#block_logger)
  - [Block_Pickup](#block_pickup)
  - [Blood_Moans](#blood_moans)
  - [Bloodmoon](#bloodmoon)
  - [Bloodmoon_Warrior](#bloodmoon_warrior)
  - [Bot_Response](#bot_response)
  - [Bounties](#bounties)
  - [Break_Reminder](#break_reminder)
  - [Chat_Color](#chat_color)
  - [Chat_Command_Log](#chat_command_log)
  - [Chat_Command_Response](#chat_command_response)
  - [Chat_Command_Response_Extended](#chat_command_response_extended)
  - [Chat_Flood_Protection](#chat_flood_protection)
  - [Chat_Logger](#chat_logger)
  - [Chunk_Reset](#chunk_reset)
  - [Clan_Manager](#clan_manager)
  - [Clean_Bin](#clean_bin)
  - [Confetti](#confetti)
  - [Console_Command_Log](#console_command_log)
  - [Custom_Commands](#custom_commands)
  - [Day7](#day7)
  - [Died](#died)
  - [Discord_Bot & Discord_Bot_Extended](#discord_bot--discord_bot_extended)
  - [Dropped_Bag_Protection](#dropped_bag_protection)
  - [Entity_Cleanup & Entity_Cleanup_Extended](#entity_cleanup--entity_cleanup_extended)
  - [Exit_Command](#exit_command)
  - [Falling_Blocks_Remover](#falling_blocks_remover)
  - [First_Claim_Block](#first_claim_block)
  - [FPS](#fps)
  - [Friend_Teleport](#friend_teleport)
  - [Gamble](#gamble)
  - [Gimme](#gimme)
  - [Hardcore](#hardcore)
  - [Harvest](#harvest)
  - [High_Ping_Kicker](#high_ping_kicker)
  - [Homes & Homes_Extended](#homes--homes_extended)
  - [Hordes](#hordes)
  - [Info_Ticker](#info_ticker)
  - [Kick_Vote](#kick_vote)
  - [Kill_Notice](#kill_notice)
  - [Land_Claim_Count](#land_claim_count)
  - [Level_Up](#level_up)
  - [Lobby & Lobby_Extended](#lobby--lobby_extended)
  - [Location](#location)
  - [Login_Notice](#login_notice)
  - [Logs](#logs)
  - [Lottery](#lottery)
  - [Market & Market_Extended](#market--market_extended)
  - [Message_Color](#message_color)
  - [Motd](#motd)
  - [Mute](#mute)
  - [Mute_Vote](#mute_vote)
  - [New_Player](#new_player)
  - [New_Player_Protection](#new_player_protection)
  - [New_Spawn_Tele](#new_spawn_tele)
  - [Night_Alert](#night_alert)
  - [No_Vehicle_Pickup](#no_vehicle_pickup)
  - [Normal_Player_Color_Prefix](#normal_player_color_prefix)
  - [Oversized_Traps](#oversized_traps)
  - [Player_List](#player_list)
  - [POI_Protection](#poi_protection)
  - [Poll](#poll)
  - [Prayer](#prayer)
  - [Private_Message](#private_message)
  - [Public_Waypoints](#public_waypoints)
  - [Real_World_Time](#real_world_time)
  - [Region_Reset](#region_reset)
  - [Report](#report)
  - [Reserved_Slots](#reserved_slots)
  - [Restart_Vote](#restart_vote)
  - [Roll_It_Out](#roll_it_out)
  - [Scout_Player](#scout_player)
  - [Shop](#shop)
  - [Shutdown & Shutdown_Extended](#shutdown--shutdown_extended)
  - [Sleeper_Respawn](#sleeper_respawn)
  - [Sorter](#sorter)
  - [Starting_Items](#starting_items)
  - [Stuck](#stuck)
  - [Suicide](#suicide)
  - [Travel](#travel)
  - [Vault](#vault)
  - [Vehicle_Recall & Vehicle_Rcall_Extended](#vehicle_recall--vehicle_rcall_extended)
  - [Voting & Voting_Extended](#voting--voting_extended)
  - [Wall](#wall)
  - [Wallet](#wallet--wallet_extended)
  - [Watch_List](#watch_list)
  - [Waypoints & Waypoints_Extended](#waypoints--waypoints_extended)
  - [Web_API](#web_api)
  - [Web_Panel](#web_panel)
  - [Workstation_Lock](#workstation_lock)
  - [World_Radius](#world_radius)
  - [Zones](#zones)

## Damage_Detector
```xml
<Tool Name="Damage_Detector" Enable="False" Entity_Damage_Limit="1000" Block_Damage_Limit="2000" Player_Damage_Limit="2000" Admin_Level="0" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Entity_Damage_Limit__  
Set a numeric value for Entity_Damage_Limit
* __Block_Damage_Limit__  
Set a numeric value for Block_Damage_Limit
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Watches for damage done to players and blocks.

If the damage is too high, they will be banned. Set the max damage a person can commit.

Admins will be skipped if they are the right permission level.

## Dupe_Log
```xml
<Tool Name="Dupe_Log" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
If a player adds a duplicate item or stack into their inventory, it will be logged for review.

This does not guarantee they have duped anything as the tool is not streaming data from the player live.

Example: Player buys 5 stacks of concrete blocks from a trader or takes them out of their chest.

This will show up in the log because they all appeared together.

Creates a file named DuplicateItems.xml. Items in this list will not be logged when found in player's inventory.

## Family_Share_Prevention
```xml
<Tool Name="Family_Share_Prevention" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Once enabled, clients using a family share account will be banned from the server upon joining.

Clients can be added to the family share list through console command. This will allow them to join.

## Flying_Detector
```xml
<Tool Name="Flying_Detector" Enable="True" Admin_Level="0" Flags="3" />
```
### Attributes
* __Enable__  
Set True or False for Flying_Detector
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Flags__  
Set a numeric value for Flags

### Description
Automatically detects players flying in the air or using no collision underground.
Players are automatically banned and given the reason detected for flying.
Detected players are added to the detection log.
Admins are skipped if the right level.
Flags controls how many times a player can be flagged as flying in a row before considered detected.

## Godmode_Detector
```xml
<Tool Name="Godmode_Detector" Enable="True" Admin_Level="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Starts an automatic check for players using god mode.

Set the admin level required to use god mode.

## Infinite_Ammo
```xml
<Tool Name="Infinite_Ammo" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Detects players using a hack to never run out of ammo in their guns and bans them.

## Invalid_Buffs
```xml
<Tool Name="Invalid_Buffs" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create a InvalidBuffs.xml in your main xml folder for ServerTools

Each entry in InvalidBuffs.xml is checked against each player.

If any buffs on a player match the invalid buff list, a message will show in the server alerting everyone.

The player is banned upon detection.

## Invalid_Items
```xml
<Tool Name="Invalid_Items" Enable="False" Ban="False" Admin_Level="0" Check_Storage="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Ban__  
Set True or False for Ban
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Check_Storage__  
Set True or False for Check_Storage

### Description
Enabling will create a InvalidItems.xml in your main installation folder in a ServerTools folder

Each entry in InvalidItems.xml is checked against each players inventory.

If any items in a player's inventory match the invalid items list, a message will show in the server alerting everyone. The player is also warned.

If Ban is set to true it will ban the player instead of kicking them for their illegal items.

Setting a Admin_Level will ignore all admins and mods based on the ServerAdmin.xml level they are set to.

Setting Admin_Level to 5 will ignore all admins and mods level 0-5 from the serveradmin.xml permission list.

Check_Storage will check inside secure chests for invalid items and remove them every 5 minutes.

## Invalid_Item_Stack
```xml
<Tool Name="Invalid_Item_Stack" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Automatically checks players inventory for invalid stack sizes. The player will receive a message telling them they have an invalid stack and a log will be made to the output_log.

## Jail
```xml
<Tool Name="Jail" Enable="False" Jail_Size="8" Jail_Position="0,0,0" Jail_Shock="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Jail_Size__  
Set a numeric value for Jail_Size
* __Jail_Position__  
Set a numeric value set of x, y, z position for Jail_Position
* __Jail_Shock__  
Set True or False for Jail_Shock

### Description
Enables the jail system for naughty players. This must be enabled to utilize other tools that send a player to jail.

Jail_Size controls how far a player can stray from the jail before it teleports them back to the Jail_Position.

Jail_Position controls where a jailed player is sent to. This is the x, y, z position.

Jail shock will apply the shock buff to them if they try to leave the prison area.

## Player_Logs
```xml
<Tool Name="Player_Logs" Enable="False" Vehicle="False" Interval="120" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Interval__  
Set a numeric value for Interval

### Description
Starts a log that will keep track of multi stats and the player inventory.

Setting the Interval will control how often these checks and logs and written to the file.

## Player_Stats & Player_Stats_Extended
```xml
<Tool Name="Player_Stats" Enable="True" Health="255" Stamina="255" Jump_Strength="1.5" />
<Tool Name="Player_Stats_Extended" Height="1.8" Admin_Level="0" Kick_Enabled="False" Ban_Enabled="False" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Max_Speed__  
Set a numeric value for Max_Speed
* __Health__  
Set a numeric value for Health
* __Stamina__  
Set a numeric value for Stamina
* __Jump_Strength__  
Set a numeric value for Jump_Strength

#### Extended
* __Height__  
Set a numeric value for Height
* __Kick__  
Set True or False for Kick
* __Ban__  
Set True or False for Ban

### Description
Automatically checks if a player has an illegal value for their speed, health, stamina or jump strength.

Detected players who fail any of the checks will have a log created.

Setting Kick_Enabled to true will kick the detected player.

Setting Ban_Enabled to true will ban the detected player.

Automatically checks if a player has an illegal height value.

Setting Kick_Enabled to true will kick the detected player.

Setting Ban_Enabled to true will ban the detected player.

## Protected_Zones
```xml
<Tool Name="Protected_Zones" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create an xml file named ProtectedZones.xml.

Use the console commands while in game to add protection to an area or add zones via the xml.

Use the console commands while in game to remove protection to an area or remove zones via the xml.

You can set the area protection to false so that you do not have to remove it from the list.

Use two opposing corner points to designate the protected space. This will form a square or rectangle depending on the locations chosen.

Protected spaces do not allow for any damage to the blocks nor for anyone to build inside of it including admins.

You can list the protected spaces in console.

## PvE_Violations
```xml
<Tool Name="PvE_Violations" Jail="4" Kill="1" Kick="2" Ban="4" />
```
### Attributes
* __Enable__  
Set a numeric value for Jail
* __Kill__  
Set a numeric value for Kill
* __Kick__  
Set a numeric value for Kick
* __Ban__  
Set a numeric value for Ban

### Description
Players that violate a PvE Lobby or Market space will be hit with a strike. If they get too many of them, it will apply a penalty.

Set how many strikes a player can have before applying the penalties.

Setting 0 will not apply the penalty.

Jail will send the player to the designated jail space provided it is setup.

Kill will kill the player.

Kick will kick the player from the server.

Ban will ban the player from the server.

## Spectator_Detector
```xml
<Tool Name="Spectator_Detector" Enable="True" Admin_Level="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Automatically detects players using spectator mode without authorization.

Admins lower or equal to tier Admin_Level are immune to the spectator check.

## Speed_Detector
```xml
<Tool Name="Speed_Detector" Enable="False" Admin_Level="0" Flags="4" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Flags__  
Set a numeric value for Flags

### Description
Automatically detects players moving at speeds exceeding that of a flying admin.

Admins lower or equal to tier Admin_Level are immune to the speed detection.

Set the number of flags required to trigger the detection. This helps with lag and latency triggering false positives.

## Tracking
```xml
<Tool Name="Tracking" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enables the tracking logger to create a database log of player positions.

Use console commands to see the players and their positions from a specified location and time.

## XRay_Detector
```xml
<Tool Name="XRay_Detector" Enable="False" Admin_Level="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Enabling will automatically check players head location and sets their screen black until it stops colliding with blocks.
Admin_Level controls the permission required in your ServerAdmin.xml to skip specific players, which allows them to view inside blocks.
<br>
<br>
<br>

## AdminChatCommands 
```xml
<Tool Name="Admin_Chat_Commands" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Adminstrators can use chat to send administrators an ingame chat message. 

## Admin_List
```xml
<Tool Name="Admin_List" Enable="False" Admin_Level="0" Moderator_Level="1" />
```

### Attributes
* __Enable__  
Set True or False to enable
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Moderator_Level__  
Set a numeric value for Mod_Level

### Description
Type /admins or !admins in chat to get a response showing the currently online administrators and moderators based on the ServerAdmin.xml. 

Setting Admin_Level to 2 would show all online administrators
tier 0-2 as admin in the chat response.

## Allocs_Map
```xml
<Tool Name="Allocs_Map" Enable="False" Link="" />
```

### Attributes
* __Enable__  
Set True or False to enable
* __Link__  
Set a numeric value for Admin_Level

### Description
Type /map in game for a pop up window with a clickable link that takes the player to allocs map via steam browser/overlay. 

The Link provided is what players will be taken to when they click the link. 

## Animal_Tracking
```xml
<Tool Name="Animal_Tracking" Enable="False" Delay_Between_Uses="60" Minimum_Spawn_Radius="40" Maximum_Spawn_Radius="60" Animal_Ids="85,86,87,88" />
<Tool Name="Animal_Tracking_Extended" Command_Cost="0" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Minimum_Spawn_Radius__  
Set a numeric value for Minimum_Spawn_Radius
* __Maximum_Spawn_Radius__  
Set a numeric value for Maximum_Spawn_Radius
* __Animal_Ids__  
A list of Ids for the type of animals to spawn.  
The list should be comma separated.
* __Command_Cost__  
Set a numeric values for Command_Cost

### Commands
* /animaltracking  
Spawns an animal from the allowed IDs to within the radius set
* /track  
Spawns an animal from the allowed IDs to within the radius set

### Description

Set the animal id based on the entity id list found in game. Type spawnentity or se in console to view them. Choose any id you wish.

The delay is the time that must pass before the command can be used again. The delay is in minutes.

Players will receive an in game message saying they have tracked down an animal to with in the radius it spawned at.

Experiment with the radius to find one that suits your preference.

## Auction

```xml
<Tool Name="Auction" Enable="False" No_Admins="False" Admin_Level="0" Total_Items="1" Tax="0" />
```

### Attributes

* __Enable__   
Set True or False for Enable
* __No_Admins__  
Set True or False for No_Admins
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Total_Items__  
Set a numeric value for Total_Items
* __Tax__  
Set a numeric value for Tax

### Commands
* /auction sell #
* /auction buy #
* /auction cancel

### Description

Players can sell items by putting it into the first slot of a secure chest they own and typing /auction sell #. The number is how much the price is.

Typing '_/auction_' shows available items in the auction or it will open the panel if enabled, allowing players to view and exchange Auction items. 

Players can attempt to purchase the corresponding item # with '_/auction buy #_'.

Profits are sent to the sellers wallet.

___Total_Items___ controls how many items they may have in the auction.

## Auto_Backup
```xml
<Tool Name="Auto_Backup" Enable="False" Delay_Between_Saves="240" Compression_Level="0" Backup_Count="5" />
<Tool Name="Auto_Backup_Extended" Destination="" Save_Directory="" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Saves__  
Set a numeric value for Delay_Between_Saves in minutes
* __Compression_Level__  
Compression level:  
   0 - None  
   9 - maximum 
* __Backup_Count__  
How many total backups to keep before deleting old ones.
* __Destination__  
Set the desired location to save backups with Destination
Leave this blank for the default location
You should enter an absolute path, e.g. c:/MyFiles/7DaysBackups
* __Save_Directory__  
Set the desired save folder to backup with Save_Directory
Leave this blank for the default location
You should enter an absolute path, e.g. c:/Users/BananaMan/AppData/Roaming/7DaysToDie/Saves

### Description
An automatic backup of the world files will be created in a zip file.

Time_Between_Saves controls how many minutes will pass between backups.
You can allow the default location or specify a save directory you would like your backups to be located by setting the Destination.

You should enter an absolute path, e.g. Example C:/MyFiles/7DaysBackups/.

Set the compression level from 0-9. 0 is none, 9 is maximum. Increasing this value may impact performance of the server and will increase the time to run.

## Auto_Party_Invite

```xml
<Tool Name="Auto_Party_Invite" Enable="False" />
```

### Attributes
* __Enable__   
Set True or False for Enable

### Description
Allows a player to make a list of other players in game using entity id.

It is recommended that you enable Player_List tool so players can view a list of other players entity id.

If a player on this list joins the game while the list creator is the leader of a group that is not full or they are not in a group, it will auto invite the joining player.

## Auto_Save_World
```xml
<Tool Name="Auto_Save_World" Enable="False" Delay_Between_Saves="60" />
```
### Attributes  
* __Enable__  
Set True or False for Enable

### Description
Automatically begins a timer on world start that will start a world save every time the Delay_Between_Saves expires.

## Auto_Restart
```xml
<Tool Name="Auto_Restart" Enable="False" />
```
### Attributes  
* __Enable__  
Set True or False for Enable

### Description
Automatically restarts the server when it shuts down. This does not trigger a shutdown. It only restarts the server if it shuts down. This does not well for all users. Hosts typically have their own restart service.

## Bad_Word_Filter
```xml
<Tool Name="Bad_Word_Filter" Enable="False" Invalid_Name="False" />
```
### Attributes
* __Enable__   
Set True or False for Enable
* __Invalid_Name__  
Set True or False for Invalid_Name

### Description
Enabling will create a BadWords.xml in your main installation folder in a ServerTools folder.

Filters out bad words or symbols from chat matching the list in BadWords.xml.

It will not filter a phrase or multiple word salad, only single words.

## Bank
```xml
<Tool Name="Bank" Enable="False" Inside_Claim="False" Deposit_Fee_Percent="5" Player_Transfers="False" Direct_Deposit="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Inside_Claim__  
Set True or False for Inside_Claim
* __Deposit_Fee_Percent__  
Set a numeric value for Deposit_Fee_Percent
* __Player_Transfers__ 
Set True or False for Player_Transfers
* __Direct_Deposit__ 
Set True or False for Direct_Deposit

### Description
Inside_Claim controls whether players must be inside their own claim to use the bank commands.

The bank will use the same currency as the Wallet. It defaults to the casinoCoin but this can be changed via the items.xml provided in the Config folder with the latest release.

This file is provided with the installation files.

Deposit_Fee controls how much is withdrawn from a deposit to the bank. Withdraw has no fee. Set 0 for none.

Players can type /bank to see their bank value.

Transferring from the Wallet to the Bank will take currency from the players bag.

/deposit # takes from the players bag and adds it to their bank.

Direct_Deposit controls whether currency goes directly to a player's bank instead of their bag.

## Bed
```xml
<Tool Name="Bed" Enable="False" Delay_Between_Uses="60" Command_Cost="10" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric value for Command_Cost

### Description
Bed can be used to teleport to a players bed position.

The delay between using the command is controled with Delay_Between_Uses. This is in minutes.

Command_Cost controls the amount needed to run the command.

Players type /bed to activate it
 
## Big_Head
```xml
<Tool Name="Big_Head" Enable="False" />
```
### Attributes
* __Enable__   
Set True or False for Enable

### Description
Inflates regular zombie heads to over sized. Does not affect sleepers

## Block_Pickup
```xml
<Tool Name="Block_Pickup" Enable="False" />
```
### Attributes
* __Enable__   
Set True or False for Enable

### Description
Allows players to pick up blocks that are not terrain, full health and inside of their claimed space.

Players must type /pickup in chat to active it and then punch the block they desire to pickup.

## Blood_Moans
```xml
<Tool Name="Blood_Moans" Enable="False" />
```
### Attributes
* __Enable__   
Set True or False for Enable

### Description
During a bloodmoon, players will hear random sounds every 20 to 30 seconds.

Sounds include zombie moans and groans, opening doors, footsteps, vomit and more.

## Bloodmoans
```xml
<Tool Name="Blood_Moans" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Show_On_Respawn__  
Set True or False for Show_On_Respawn
* __Auto_Show__  
Set True or False for Auto_Show
* __Delay__  
Set a numeric value for Delay

### Description
Displays the amount of days before the next bloodmoon in chat.
Typing /bloodmoon in chat will respond with the remaining days until the next bloodmoon.

If the tool is on, it will always display upon entry to the server.
Setting Show_On_Respawn will display the remaining days to a player that has died and respawn.

Auto_Show will enable it to display periodically in chat while playing.

## Bloodmoon_Warrior
```xml
<Tool Name="Bloodmoon_Warrior" Enable="True" Zombie_Kills="50" Chance="100" Reduce_Death_Count="False" Reward_Count="1" />
```
### Attributes
* __Enable__  
Set True or False for Enable.
* __Zombie_Kills__  
Set a numeric value for Zombie_Kills.
* __Chance__  
Set a numeric value for Chance as a percentage.  
i.e. 50=50% chance to win something
* __Reduce_Death_Count__  
Set True or False for Reduce_Death_Count.  
Subtracts 1 from the winning players total death count.

### Description
Players online during the start of the bloodmoon may randomly be invited to an event.
They must survive the entire bloodmoon, can not die and must kill enough zombies.
Set home many zombies they must kill to succeed.

Created an xml file called BloodmoonWarrior.xml.

Set the potential items players can be rewarded by adding or removing them from BloodmoonWarrior.xml.

If Reduce_Death_Count is set to true, their death count will reduce by one if successful. They will always receive one item from the list.

## Bot_Response
```xml
<Tool Name="Bot_Response" Enable="True" />
```
### Attributes
* __Enable__   
Set True or False for Enable

### Description
Creates a BotResponse.xml to customize bot responses.

## Bounties
```xml
<Tool Name="Bounties" Enable="False" Minimum_Bounty="5" Kill_Streak="0" Bonus="25" />
```
### Attributes
* ___Enable__  
Set True or False for Enable
* __Minimum_Bounty__  
Set a numeric value for Minimum_Bounty
* __Kill_Streak__  
Set a numeric value for Kill_Streak
* __Bonus__  
Set a numeric value for Bonus

### Description
Players can see the online players and their id by using /bounty. It also shows the current bounty on each player.

The cost of the bounty is based on what the player inputs in chat and is available in their wallet.

Players are awarded the bounty value upon killing the target and a chat message is shown.

If a player goes over the Kill_Streak by killing enough players without dying, they automatically receive the bounty value to their value on each kill.

## Break_Reminder
```xml
<Tool Name="Break_Reminder" Enable="False" Break_Time="60" Message="It has been {Time} minutes since the last break reminder. Stretch and get some water." />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Break_Time__  
Set a numeric value for Break_Time
* __Message__  
Set your desired message sent to players

### Description
Players will be reminded every set amount of minutes to take a break.
Break_Time is in minutes.

## Chat_Color
```xml
<Tool Name="Chat_Color" Enable="True" Rotate="False" Custom_Color="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Rotate__  
Set True or False for Rotate
* ___Custom_Color___  
Set True or False for Custom_Color

### Description
Enabling will create a ChatColor.xml file. Use this define each players html color tags and prefix as desired.

The colors must be entered as an HTML color or from the ColorList.xml.   
Example: [FF0000] or [FF0000],[FFCC00] or Red

Expiration date controls when their prefix and color will stop working in chat.

Rotate can be used in combination with the ColorList.xml, so that players can rotate between colors from the list.

Custom_Color allows players to type an html color in the chat and sets their color to it

## Chat_Command_Log
```xml
<Tool Name="Chat_Command_Log" Enable="True" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description

## Chat_Command_Response

## Chat_Command_Response_Extended

```xml
<Tool Name="Chat_Command_Response" Server_Response_Name="[FFCC00]Tartarus" Main_Color="[00FF00]" Chat_Command_Prefix1="/" Chat_Command_Prefix2="!" />
<Tool Name="Chat_Command_Response_Extended" Friend_Chat_Color="[33CC33]" Party_Chat_Color="[FFCC00]" Passthrough="True" />
```

### Attributes

#### Normal
* __Server_Response_Name__  
Set a server response name to Chat_Command_Response
* __Main_Color__  
Set a color using a html value in brackets for Main_Color
* __Chat_Command_Prefix1__  
Set a symbol ServerTools will use for chat based commands
* __Chat_Command_Prefix2__  
Set a symbol ServerTools will use for chat based commands

#### Extended
* __Friend_Chat_Color__  
Set a color using a html value in brackets for Friend_Chat_Color
* __Party_Chat_Color__  
Set a color using a html value in brackets for Party_Chat_Color
* __Passthrough__  
Set True or False for Passthrough

### Description
*Note* Do not use @ or \\ for a command prefix.

Set a server response name to chat commands and server responses from ServerTools.

The color all general response messages will display as in chat.

__*Note:* Do not use @ or \ for a command prefix.__

Chat_Command_Prefix is the symbol used as a prefix to chat based commands.

Chat_Command_Prefix2 is the symbol used as a prefix to chat based commands.

## Chat_Flood_Protection
```xml
<Tool Name="Chat_Flood_Protection" Enable="False" Max_Length="250" Messages_Per_Min="8" Wait_Time="60" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Max_Length__  
Set a numeric value for Max_Length
* __Messages_Per_Min__  
Set a numeric value for Messages_Per_Min
* __Wait_Time__  
Set a numeric value for Wait_Time

### Description
Protect the chat from being flooded from a player's chat spam.
Set the maximum amount of characters that can be in a single chat message.

Set the maximum amount of message a player can send in a single minute.

## Chunk_Reset
```xml
<Tool Name="## Chunk_Reset" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Creates a ChunkReset.xml after enabling the tool.
Edit the xml file with chunk positions and the time expected to reset.
The designated chunks will reset automatically on schedule.
The tool will initiate when the server finishes loading.

## Chat_Logger
```xml
<Tool Name="Chat_Logger" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Logs all chat to a file for later review.

## Clan_Manager
```xml
<Tool Name="Clan_Manager" Enable="False" Max_Name_Length="6" Private_Chat_Color="[00FF00]" />
```
### Attributes
* __Enable__   
Set True or False for Enable
* __Max_Name_Length__  
```????Maximum length of clan name?```
* __Private_Chat_Color__  
Set a color using a html value in brackets for Private_Chat_Color

### Description
Enables the clan manager. 

Players can control it via chat.

Clan commands are shown in chat based on their availability by typing /clancommands.

Players can create and manage a clan with these various commands.

## Clean_Bin
```xml
<Tool Name="Clean_Bin" Enable="False" Auction="False" Bank="False" Bounties="False" Delays="False" />
<Tool Name="Clean_Bin_Extended1" Homes="False" Jail="False" Lobby="False" Market="False" New_Spawn_Tele="False" />
<Tool Name="Clean_Bin_Extended2" Poll="False" Protected_Zones="False" Vehicles="False" Waypoints="False" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Auction__  
Set True or False for Auction
* __Bank__  
Set True or False for Bank
* __Bounties__  
Set True or False for Bounties
* __Delays__  
Set True or False for Delays

#### Extended 1
* __Homes__  
Set True or False for Homes
* __Jail__  
Set True or False for Jail
* __Lobby__  
Set True or False for Lobby
* __Market__  
Set True or False for Market
* __New_Spawn_Tele__  
Set True or False for New_Spawn_Tele

#### Extended 2
* __Poll__  
Set True or False for Poll
* __Protected_Spaces__  
Set True or False for Protected_Spaces
* __Vehicles__  
Set True or False for Vehicles
* __Wallet__  
Set True or False for Wallet
* __Waypoints__  
Set True or False for Waypoints

### Description
Enabling will clean the ServerTools.bin file on the next server start and then disable the tool automatically.

Each option controls which data is removed from the ServerTools.bin file.

The options are labeled by the tool name the data corresponds to, e.g. Bank, Auction.

## Confetti
```xml
<Tool Name="Confetti" Enable="True" Player="True" Zombie="True" Sound="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Player__  
Set True or False for Player
* __Zombie__  
Set True or False for Zombie
* __Sound__  
Set True or False for Sound

### Description
Zombies and/or players will launch confetti when they are killed. Enabling the sound plays a fun noise when it triggers

## Console_Command_Log
```xml
<Tool Name="Console_Command_Log" Enable="True" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description
All console commands will be logged to its own file.

## Country_Ban

Set True or False for Enable
Add what countries you do not want to allow to join the server separated with a comma

## Custom_Commands
```xml
<Tool Name="Custom_Commands" Enable="True" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create a CustomCommands.xml in your main installation folder in a ServerTools folder.

Allows custom commands to be used via chat. Example: The player can type /tpmarket and it runs \"tele {EOS} 0 -1 5\" based on the response the admin has setup.

Separate command response with a ^. Example: tele {EntityId} 10 -1 50 ^ whisper You are now in the market.

A delay in the operation of the response can be done with {Delay} #. Example: tele {EntityId} 10 -1 50 ^ {Delay} 3 ^ whisper {PlayerName} you are now in the market.

Delays between command use are available for the first twenty entries in the list only.

Run a permission check on the player using the command by setting it via the xml file to true or false. Users must be the right permission level in the serveradmin.xml.

Responses in chat can include whisper or global with a message. 

Whisper will send it to the player that triggers the command, while global will send the message to the entire server.

{SetReturn} will save the users current location with the trigger name. {Return} 'trigger' will teleport the user to the saved location

## Day7
```xml
<Tool Name="Day7" Enable="True" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enables the chat commands /day and /day7. 

Using these will respond with the days left until a horde night as well as the current server FPS, count of mobs, animals, minibikes, and supply crates.

## Died
```xml
<Tool Name="Died" Enable="True" Time="2" Delay_Between_Uses="15" Command_Cost="0" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Time__ 
Set a numeric value for Time
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost

### Description
After a player dies, they have to type /died within the Time limit you have set to be brought back to their death position.

Delay_Between_Uses controls the time a player must wait before being able to use the command again.

## Discord_Bot & Discord_Bot_Extended
```xml
<Tool Name="Discord_Bot" Enable="False" Webhook="" />
<Tool Name="Discord_Bot_Extended" Prefix="[Discord]" Prefix_Color="[FFFFFF]" Name_Color="[FFFFFF]" Message_Color="[FFFFFF]" />
```

### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Webhook__  
Set a value for Webhook

#### Extended
* __Prefix__  
Set a value for Prefix
* __Prefix_Color__  
Set a value for Prefix_Color
* __Name_Color__  
Set a value for Name_Color
* __Message_Color__  
Set a value for Message_Color

### Description
Enables communications through Discordian discord bot.

Requires Web_API be enabled.

Webhook must match the Webhook token from Discord. Instructions are provided with the bot.

The bot is downloaded separately on Github.

Prefix controls the prefix shown in chat from Discord messages.

Prefix_Color controls the color of the prefix shown in chat.

## Dropped_Bag_Protection
```xml
<Tool Name="Dropped_Bag_Protection" Enable="False" Friend_Access="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Friend_Access__  
Set True or False for Friend_Acceess

### Description
Enables protection on dropped bags when a player dies. Other players can not open this bag until someone with permission does.

Friend_Acceess allows friends of the bag owner to access it.

The tool must be enabled to apply. Bags dropped before enabling will not be protected.

## Entity_Cleanup & Entity_Cleanup_Extended
```xml
<Tool Name="Entity_Cleanup" Enable="True" Falling_Tree="True" Underground="True" Delete_Bicycles="False" Delete_Drones="False" />
<Tool Name="Entity_Cleanup_Extended" Delete_MiniBikes="False" Delete_MotorBikes="False" Delete_Jeeps="False" Delete_Gyros="False" />
```

### Attributes

#### Normal
* __Enable__   
Set True or False for Enable
* __Blocks__  
Set True or False for Blocks
* __Falling_Tree__  
Set True or False for Falling_Tree
* __Entity_Underground__  
Set True or False for Entity_Underground
* __Delete_Bicycles__  
Set True or False for Delete_Bicycles

#### Extended
* __Delete_MiniBikes__  
Set True or False for Delete_MiniBikes
* __Delete_MotorBikes__  
Set True or False for Delete_MotorBikes
* __Delete_Jeeps__  
Set True or False for Delete_Jeeps
* __Delete_Gyros__  
Set True or False for Delete_Gyros

### Description
If any of these are active, they are triggered every 15 seconds.

Blocks will automatically clean up falling blocks.

Falling_Tree will automatically clean up a falling tree that gets stuck at 0 health and does not disappear.

Entity_Underground will automatically send entities to the surface except falling blocks are removed.

Delete_Bicycles will automatically remove a bicycle if found in the world.

Delete_MiniBikes will automatically remove a minibike if found in the world.

Delete_MotorBikes will automatically remove a motor bike if found in the world.

Delete_Jeeps will automatically remove a jeep if found in the world.

## Exit_Command
```xml
<Tool Name="Exit_Command" Enable="False" All="False" Belt="False" Bag="False" Equipment="False" />
<Tool Name="Exit_Command_Extended" Admin_Level="0" Exit_Time="15" />
```

### Attributes
#### Normal
* __Enable__  
Set True or False for Enable
* __All__  
Set True or False for All
* __Belt__  
Set True or False for Belt
* __Bag__  
Set True or False for Bag
* __Equipment__  
Set True or False for Equipment

#### Extended
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Exit_Time__  
Set a numeric value for Exit_Time

### Description
Enabling will require players to type /exit to leave the server or else drop things.

All will make players drop equipment, backpack and toolbelt.

Belt will make a player drop just their toolbelt.

Bag will make a player drop just their backpack.

Equipment will make a player drop just their equipment.

Admin_Level is the permission level where typing /exit will not count down and exit.

Admins do not require typing /exit to leave. No penalty is applied to them.

## Falling_Blocks_Remover
```xml
<Tool Name="Falling_Blocks_Remover" Enable="True" Log="False" Max_Blocks="25" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Log__  
Set True or False for Log
* __Max_Blocks__  
Set a numeric value for Max_Blocks

### Description
Attempts to remove all falling blocks in the game for performance improvements.

If the log is enabled, it will log the general location of the falling blocks when it goes over the Max_Blocks count. It will also attempt to log the closest player id and name.

Set the Max_Blocks to control how many blocks must be detected falling at one time for the log to engage. This can be used to detect large collapses.


## First_Claim_Block
```xml
<Tool Name="First_Claim_Block" Enable="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description
Players can type /claim to receive a claim block but only once.

This is used to help prevent claim block grief with noobs especially on PVP servers.

Recommend removing the claim block given to new players by the base game when in use to help prevent lost claims.

It will not interfere with the initial item spawn.

## FPS
```xml
<Tool Name="FPS" Enable="False" Set_Target="60" Low_FPS="5" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Players can type /fps to show the fps read out from /day7 command.

Set_Target controls the fps target the server will be set to on load up.

## Friend_Teleport
```xml
<Tool Name="Friend_Teleport" Enable="True" Delay_Between_Uses="10" Command_Cost="0" Player_Check="False" Zombie_Check="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check

### Description
Friends can type /friend to see a list of their current online friends and their Id #.

/friend # will send a request to teleport to the friend with that Id.

The other player will receive the request and can type /accept to verify the request.

A request will only last for one minute before a player must make a new request.

Command cost controls how much the command will take from the wallet to use the command.

Player_Check controls whether the player can be close to another player before using this command.

## Gamble
```xml
<Tool Name="Gamble" Enable="False" Delay_Between_Uses="10" Command_Cost="20" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost

### Description
Allows players to gamble Wallet currency using chat commands.

It will begin a 50/50 draw with 1 in 2 chance of winning. The player must spend the Command_Cost to enter. If they win, they can collect the winnings or bet again.

If the player wins in sequential rounds, the winnings increase but their chance of success decreases.

## Gimme
```xml
<Tool Name="Gimme" Enable="True" Delay_Between_Uses="60" Zombies="False" Zombie_Id="4,9,11" Command_Cost="0" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Zombies__  
Set True or False for Zombies
* __Zombie_Id__  
Set a numeric value for Zombie_Id
* __Command_Cost__  
Set a numeric values for Command_Cost

### Description
Enabling will create a Gimme.xml in your main installation folder in a ServerTools folder.

Players can type /gimme to initiate a free item from the gimme list.

The gimme.xml contains a list of all the items a player can receive from using /gimme.

A delay can be set with Delay_Between_Uses so players must wait that period of time before using /gimme again.

Zombies will control whether a zombie can be spawned instead of an item from the list. Chances are 1 in 8.

Zombie_Id controls which zombie can potentially be spawned instead of an item.

Command cost controls how much must be in a player's wallet to use the command.

## Hardcore
```xml
<Tool Name="Hardcore" Enable="False" Optional="True" Max_Deaths="9" Max_Extra_Lives="3" Life_Price="2000" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Optional__  
Set True or False for Optional
* __Max_Deaths__  
Set a numeric value for Max_Deaths
* __Max_Extra_Lives__  
Set a numeric value for Max_Extra_Lives
* __Life_Price__  
Set a numeric value for Life_Price

### Description
___Be careful if enabling this feature.___

Players have a limited amount of lives. If they run out, their stats are recorded and they are kicked, then their player profile is deleted.

Setting optional allows players to type /hardcore on to enable the mode at any point while playing but this can not be reversed.

When a player is kicked, they are given their stats and score.

When the player joins again, they can check their last stats/score or check the top 3 playtime and top 3 scores on the server.

Set Max_Extra_Lives above 0 to allow players to buy lives and how many.

## Harvest
```xml
<Tool Name="Harvest" Enable="False" Delay_Between_Uses="30" Command_Cost="50" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric value for Command_Cost

### Description
Players can harvest near by plants.

Typing /harvest will attempt to harvest plants in a 5 x 5 square surrounding the player's location.

Command_Cost controls how much they must pay in currency to use the command.


## High_Ping_Kicker
```xml
<Tool Name="High_Ping_Kicker" Enable="True" Max_Ping="250" Flags="2" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Max_Ping__  
Set a numeric value for Max_Ping
* __Flags__  
Set a numeric value for Flags

### Description
Players who have too high of a ping will be kicked from the server automatically.

Max_Ping is the limit a player can ping at before being flagged for kick.

## Homes & Homes_Extended
```xml
<Tool Name="Homes" Enable="True" Max_Homes="2" Reserved_Max_Homes="4" Command_Cost ="0" Delay_Between_Uses="0" />
<Tool Name="Homes_Extended" Player_Check ="False" Zombie_Check="False" Vehicle="False" />
```

### Attributes
#### Normal
* __Enable__   
Set True or False for Enable
* __Max_Homes__  
Set a numeric value for Max_Homes
* __Reserved_Max_Homes__  
Set a numeric value for Reserved_Max_Homes
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost

#### Extended
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check
* __Vehicle__  
Set True or False for Vehicle

### Description
While enabled, players can use chat commands /sethome 'name', /home save 'name', /home, /fhome 'name' and /home del 'name'.

/sethome 'name' and /home save 'name' will save the player's current location as the name they specify.

/home lists their saved homes.

/home 'name' will teleport the player to the specified location.

/fhome 'name' will teleport the player and send an invitation to nearby friends to teleport with them.

/home del 'name' will delete the specified home location.

Delay_Between_Uses controls the delay before a player can use /home and /fhome. This is set in minutes.

Command_Cost controls how much must be in a player's wallet to use the command.

Set Player_Check to true so they must be far enough from players to use this command.

## Hordes
```xml
<Tool Name="Hordes" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Automatically starts a horde every 20 minutes if player count is over 5 and zombie count is less than 30.

## Info_Ticker
```xml
<Tool Name="Info_Ticker" Enable="True" Delay="60" Random="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Messages__  
Set a numeric value for Delay_Between_Messages
* __Random__  
Set True or False for Random

### Description
Enabling will create a InfoTicker.xml in your main installation folder in a ServerTools folder.

Each entry in InfoTicker.xml is a message that will display in game.

Delay_Between_Messages controls the time before the next message is displayed.

Setting Random to true will show the messages in a random order until all have been displayed. The list will repeat again but in a new random order.

Leaving Random to false will display each message in order of the list until all have been displayed. The list will repeat.

## Kick_Vote

Set True or False for Enable
Set True or False for Players_Online
Set True or False for Votes_Needed

Allows players to start a vote to kick a player from the game.
Can only start a vote if 10 or more players and 8 must vote yes.
Players_Online is the amount needed to start a vote.

## Kill_Notice
```xml
<Tool Name="Kill_Notice" Enable="True" Player="True" Zombie="True" Animal="True" Show_Level="False" />
<Tool Name="Kill_Notice_Extended" Show_Damage="False" />
```
### Attributes
#### Normal
* __Enable__  
Set True or False for Enable
* __Player__  
Set True or False for Player
* __Zombie__  
Set True or False for Zombie
* __Animal__  
Set True or False for Zombie
* __Show_Level__  
Set True or False for Show_Level

#### Extended
* __Show_Damage__  
Set True or False for Show_Damage

### Description
If a player, zombie or animal kills a player, a chat notice will show it.
Player controls whether it shows players killing a player.
Zombie controls whether it shows zombies killing a player.
Animal controls whether it shows animals killing a player.
Show_Level controls if the player level is shown in the notice.

## Land_Claim_Count
```xml
<Tool Name="Land_Claim_Count" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create a LandClaimCount.xml. Controls how many land claims a player can have active on the map.

## Level_Up
```xml
<Tool Name="Level_Up" Enable="True" Xml_Only="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Xml_Only__  
Set True or False for Xml_Only

### Description
Enabling will create a LevelUp.xml.

If enabled, chat will display a message about a player reaching a new level.

You can set what will happen when a player reaches specific levels in the xml file.
Commands in LevelUp.xml will accept console commands.

## Lobby & Lobby_Extended
```xml
<Tool Name="Lobby" Enable="False" Return="False" Delay_Between_Uses="5" Lobby_Size="25" Lobby_Position="0,0,0" />
<Tool Name="Lobby_Extended" Reserved_Only="False" Command_Cost="0" Player_Check="False" Zombie_Check="False" PvE="False" />
```

### Attributes
#### Normal
* __Enable__  
Set True or False for Enable
* __Return__  
Set True or False for Return
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Lobby_Size__  
Set a numeric value for Lobby_Size
* __Lobby_Position__  
Set a numeric value set of x, y, z position for Lobby_Position

#### Extended
* __Reserved_Only__  
Set True or False for Reserved_Only
* __Command_Cost__  
Set a numeric values for Command_Cost
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check
* __PvE__  
Set True or False for PvE

### Description
Typing /lobby will send a player to the lobby if the position is set.

Delay_Between_Uses controls how long a player must wait before using /lobby again.

If return is enabled, players are alert upon using /lobby that they can type /return to be sent back to the location they came from.

They must be within range of the lobby position based on the Lobby_Size.

Lobby_Position controls where they will be teleport to when using /lobby.

Reserved only locks the lobby commands to only reserved players.

Command cost controls how much must be in a player's wallet to use the command.

Set Player_Check to true so they must be far enough from players to use this command.

Set Zombie_Check to true so they must be far enough from zombies to use this command.

## Location
```xml
<Tool Name="Location" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Players can type /loc to show their x, y, z coordinates in chat.

## Login_Notice
```xml
<Tool Name="Login_Notice" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create a file named LoginNotice.xml in your main installation folder in a ServerTools folder.

Add players Id to the id field and whatever message of your choice in the xml file.

## Logs
```xml
<Tool Name="Logs" Days_Before_Log_Delete="5" />
```

### Attributes
* __Days_Before_Log_Delete__  
Set a numeric value for Delay_Between_Uses

### Description
## Lottery
```xml
<Tool Name="Lottery" Enable="False" Bonus="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Bonus__  
Set a numeric value for Bonus

### Description
Players can start a new lottery by typing /lottery #. They can check the current lotto with /lottery.

To enter a lottery, players must match what the first player opened the lottery value as.

When an hour has passed or ten players have entered the lottery, it will draw the winner.

## Market & Market_Extended
```xml
<Tool Name="Market" Enable="False" Return="False" Delay_Between_Uses="5" Market_Size="25" Market_Position="0,0,0" />
```

### Attributes
#### Normal
* __Enable__  
Set True or False for Enable
* __Return__  
Set True or False for Return
* __Delay_Between_Uses__ 
Set a numeric value for Delay_Between_Uses
* __Market_Size__ 
Set a numeric value for Market_Size
* __Market_Position__ 
Set a numeric value set of x,y,z position for Market_Position

#### Extended
* __Reserved_Only__  
Set True or False for Reserved_Only
* __Command_Cost__  
Set a numeric values for Command_Cost
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check
* __PvE__  
Set True or False for PvE

### Description
Typing /market will send a player to the market if the position is set.

Delay_Between_Uses controls how long a player must wait before using /market again.

If return is enabled, players are alert upon using /market that they can type /return to be sent back to the location they came from.

They must be within range of the market position based on the Market_Size.

Market_Position controls where they will be teleport to when using /market.

Reserved only locks the market commands to only donor/reserved players

Command cost controls how much must be in a player's wallet to use the command.

Set Player_Check to true so they must be far enough from players to use this command.

Set Zombie_Check to true so they must be far enough from zombies to use this command.

Set PvE to true so they can not damage each other while inside the market.

## Message_Color
```xml
<Tool Name="Message_Color" Enable="True" Color="[bbbbbb]" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Color__  
Set a color using a html value in brackets for Color

### Description
General chat message color can be controlled with this.

The color must be entered as an HTML color. Example:[FF0000]

This does not effect their name or add a prefix to it. It only changes the color of the message.

## Motd
```xml
<Tool Name="Motd" Enable="True" Show_On_Respawn="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Show_On_Respawn__  
Set True or False for Show_On_Respawn

### Description
Enabling will create a Motd.xml in your main installation folder in a ServerTools folder.

Message of the day or Motd entries will show to the player upon joining the server.

## Mute
```xml
<Tool Name="Mute" Enable="False" Block_Commands="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description

## Mute_Vote
```xml
<Tool Name="Mute_Vote" Enable="False" Players_Online="5" Votes_Needed="3" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Players_Online__  
Set a numeric value for Players_Online
* __Votes_Needed__  
Set a numeric value for Votes_Needed

### Description
Allows players to start a vote to mute a player in game from chat for 60 minutes.

Players_Online is the amount needed to start a vote.

## New_Player
```xml
<Tool Name="New_Player" Enable="True" Entry_Message="[ffff00]{PlayerName} has entered the world for the first time.  Be sure to help a noob out.[bbbbbb]" />
<Tool Name="New_Player_Extended" Block_During_Bloodmoon="False" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Entry_Message__  
Create an entry message for Entry_Message

#### Extended
* __Block_During_Bloodmoon__  
Set True or False for Block_During_Bloodmoon

### Description

## New_Player_Protection
```xml
<Tool Name="New_Player_Protection" Enable="False" Level="5" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Level__  
Set a numeric value for Level

### Description
This will block all PvP damages until the player reaches the desired level.

Sets the level required for PvP damages to occur.

## New_Spawn_Tele
```xml
<Tool Name="New_Spawn_Tele" Enable="False" New_Spawn_Tele_Position="0,0,0" Return="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __New_Spawn_Tele_Position__  
Set a numeric value for New_Spawn_Tele_Position
* __Return__  
Set True or False for Return

### Description
Teleports a new player upon joining to the New_Spawn_Tele_Position.

Set New_Spawn_Tele_Position to control where a new player will spawn to.

New_Spawn_Tele_Position is an x, y, z position.

Enabling return will alert players upon teleport that they can type /ready to go back to their first spawn point.

## Night_Alert
```xml
<Tool Name="Night_Alert" Enable="True" Delay="60" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay__  
Set a numeric value for Delay

### Description
Enabling will automatically show the hours left until night time.

Use Delay to control how often it shows.

## No_Vehicle_Pickup
```xml
<Tool Name="No_Vehicle_Pickup" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
## Normal_Player_Color_Prefix
```xml
<Tool Name="Normal_Player_Color_Prefix" Enable="False" Prefix="NOOB" Name_Color="[00B3B3]" Prefix_Color="[FFFFFF]" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Prefix__  
Set a value of your choice in parenthesis for Prefix
* __Color__  
Set a color using a html value in brackets for Color

### Description
Enabling makes all standard players chat message show with a prefix and color.

Prefix controls the prefix for the message.

## Oversized_Traps
```xml
<Tool Name="Oversized_Traps" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Player placed traps will have a large default footprint when placing them down. The model remains the same size.

## Player_List
```xml
<Tool Name="Player_List" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling lets players type /list to see all the current online player names and entity id.

## POI_Protection
```xml
<Tool Name="POI_Protection" Enable="True" Bed="True" Claim="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Bed__  
Set True or False for Bed
* __Claim__  
Set True or False for Claim

### Description
Enable protection from placement of beds and claims on a POI(Place of interest).

## Poll
```xml
<Tool Name="Poll" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description

## Prayer
```xml
<Tool Name="Prayer" Enable="True" Delay_Between_Uses="30" Command_Cost="10" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric value for Command_Cost

### Description
Players can type /pray to receive a buff from a custom list called Prayer.xml.

Set the time delay before players can use this command again. Time is in minutes.

## Private_Message
```xml
<Tool Name="Private_Message" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Players can send a private message to each other by using their id or name in chat.

Example: /pm 171 Hey buddy, lets meet up.

In response, a player can return a message to the last private sender by using /rm.

Example: /rm Sounds good. Lets meet by the town in the morning.

## Public_Waypoints
```xml
<Tool Name="Public_Waypoints" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Allows you to setup a list of waypoints that all players can access.

A command cost can be set for each waypoint.

## Real_World_Time
```xml
<Tool Name="Real_World_Time" Enable="False" Delay="60" Time_Zone="UTC" Adjustment="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay__  
Set a numeric value for Delay
* __Time_Zone__  
Set a string value for Time_Zone
* __Adjustment__  
Set a numeric value for Adjustment

### Description
Enabling will show the real world time periodically based on the delay.

Recommend changing the Time_Zone to match the time zone of the server host location. 

This will not adjust for the difference.

## Region_Reset
```xml
<Tool Name="## Region_Reset" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Creates a RegionReset.xml after enabling the tool.
Edit the xml file with region file names and the time expected to reset.
The designated regions will reset automatically on schedule.
The tool will initiate when the server finishes loading.

## Report
```xml
<Tool Name="Report" Enable="False" Delay_Between_Uses="60" Length="150" Admin_Level="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Enabling will allow a player to type /report and their message.

The message will be logged in a file called Report.xml under the Reports folder.

Admins online at the time of sending, will be sent the message as well.

Delay_Between_Uses controls how often a report can be made by a player in minutes.

## Reserved_Slots
```xml
<Tool Name="Reserved_Slots" Enable="False" Session_Time="30" Admin_Level="0" Reduced_Delay="False" Bonus_Exp="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Session_Time__  
Set a numeric value for Session_Time
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Admin_Slots__  
Set a numeric value for Admin_Slots
* __Reduced_Delay__  
Set True or False for Reduced_Delay

### Description
Enabling will create a ReservedSlots.xml in your main installation folder in the ServerTools folder.

Turns on an automatic reservation system. When the server is full, it will kick 1 player that is not listed in the ReservedSlots.xml or an admin.

The auto kick chooses based on the player's overall play session time. Setting zero runs it immediately.

If a player is not reserved but also has not played longer than the Session_Time, they will not be kicked, except when set to zero.

Admins lower or equal to tier Admin_Level are immune to the kick system. Default is 0. Setting 2 covers 0-2.

Admins_Slots keeps one extra slot open for admins to rotate in to the server. A 30 player server will start kicking at 28 players.

If Reduced_Delay is set to true, valid players on the list will have a reduced delay after using a relevant chat commands.

## Restart_Vote
```xml
<Tool Name="Restart_Vote" Enable="False" Players_Online="5" Votes_Needed="3" Admin_Level="0" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Players_Online__  
Set True or False for Players_Online
* __Votes_Needed__  
Set True or False for Votes_Needed
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Allows players to vote for a server restart. Initiates stopserver 1 if successful.

Players_Online is the amount needed to start a vote.

Votes_Needed is the amount of players that must vote yes.

## Roll_It_Out
```xml
<Tool Name="Roll_It_Out" Enable="False" Bet="25" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Bet__  
Set a numeric value for Bet

### Description
Allows players to enter a dice rolling game against each other or AI.

The game is played through the steam browser and requires Web_API to be enabled and running.

Bet is the amount of currency required to play the game. The winner of the game will receive the total from the bets.

## Scout_Player
```xml
<Tool Name="Scout_Player" Enable="False" Delay_Between_Uses="60" Command_Cost="10" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
## Shop
```xml
<Tool Name="Shop" Enable="True" Inside_Market="False" Inside_Traders="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Inside_Market__  
Set True or False for Inside_Market
* __Inside_Traders__  
Set True or False for Inside_Traders

### Description
While enabled, players can use chat command /shop while anywhere on the map.

If you set Inside_Market or Inside_Traders to true, they must be inside one of these to use /shop and /buy.

Players can buy an item multiple times from the shop with /buy # #. The second number controls how many times they want to buy it.

## Shutdown & Shutdown_Extended
```xml
<Tool Name="Shutdown" Enable="True" Countdown="2" Time="240" Alert_On_Login="True" Alert_Count="1" />
<Tool Name="Shutdown_Extended" UI_Lock="True" Interrupt_Bloodmoon="False" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Countdown__  
Set a numeric value for Countdown
* __Time__  
Set a numeric value for Time
* __Alert_On_Login__  
Set True or False for Alert_On_Login
* __Alert_Count__  
Set a numeric value for Alert_Count

#### Extended
* __UI_Lock__    
Set True or False for UI_Lock

### Description
Automatically begins the shutdown process for the server after the Time runs out or has been met.

Begins a warning of the coming shutdown with a timer based on the Countdown after the Time has run out or been met.

Saves the world at one minute remaining of the count down and gives the players a warning not to exchange items or build during the final minute.

If the server is set as a service on the host hardware, it will automatically restart. ServerTools can not automatically restart a server.

Alert on login announces the time remaining before the next scheduled shutdown when a player joins the server.

Alert_Count controls how many times the alert messages post in chat on each event to help players see it.

Bloodmoon nights and events are automatically detected. The shutdown will be extended. When the bloodmoon or event ends, the shutdown process will commence.

UI_Lock Closes and locks access to all loot, storage, vendor, trader and workstations for the last 45 seconds of the shutdown process.

## Sleeper_Respawn
```xml
<Tool Name="Sleeper_Respawn" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Sleeper spawn points will be reset on server restart.

## Sorter
```xml
<Tool Name="## Sorter" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Players can place a writable box and mark it 'sort'. Stackable items inside the box will be sent to surrounding storage based on identical items.
Typing /sort initiates the tool.
All storage must be inside of a claimed space.

## Starting_Items
```xml
<Tool Name="Starting_Items" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will create a StartingItems.xml in your main installation folder inside the ServerTools folder.

All items listed in StartingItems.xml will be given to a new player when they first join the server.

## Stuck
```xml<Tool Name="Stuck" Enable="True" Delay_Between_Uses="60" />

```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
### Description
Players can type /stuck to send themselves to the world surface.

Can not be used inside other player's claim space.

Set the Delay_Between_Uses to control how long a player must wait before stuck will work for them again.

## Suicide
```xml
<Tool Name="Suicide" Enable="True" Delay_Between_Uses="60" Player_Check="False" Zombie_Check="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check

### Description
Enables the chat commands /killme, /suicide, /wrist and /hang so players can kill themselves.

Set the Delay_Between_Uses to control how long a player must wait before suicide will work for them again.

Set Player_Check to true so they must be far enough from players to use this command.

Set Zombie_Check to true so they must be far enough from zombies to use this command.

## Travel
```xml
<Tool Name="Travel" Enable="True" Delay_Between_Uses="60" Command_Cost="0" Player_Check="False" Zombie_Check="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check

### Description
Enabling will create a TravelLocations.xml in your main installation folder in the ServerTools folder.

All locations listed in the TravelLocations.xml will allow a player to type /travel while inside one and teleport to the corresponding destination.

Inside the TravelLocations.xml, destination is where the player will teleport to.

The name will be the name of the travel zone, Corner1 is first corner of the zone, Corner2 is the opposite corner of the zone.

Corner1, corner2 and destination are represented by x, y, z coordinates.

Command cost controls how much must be in a player's wallet to use the command.

Set Player_Check to true so they must be far enough from players to use this command.

## Vault
```xml
<Tool Name="Vault" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable
* __Inside_Claim__  
Set True or False for Enable
* __Slots__  
Set a numeric values for Slots
* __Lines__  
Set a numeric values for Lines

### Description
Enabling allows the Vault to be accessed for each player. It will have a particular amount of slots and lines.

3 slots and 2 lines would equal six slots total.

Disabling the tool will not remove the recipe but does disable the available slots. Items already stored in the Vault will be safe regardless of the tool state.

Items can also be removed from the Vault across maps.

This is a very powerful tool and could be abused in the wrong hands. Be careful with how many slots are offered.

## Vehicle_Recall & Vehicle_Rcall_Extended
```xml
<Tool Name="Vehicle_Recall" Enable="False" Inside_Claim="False" Distance="50" Delay_Between_Uses="120" Command_Cost="0" />
<Tool Name="Vehicle_Recall_Extended" Normal_Max="2" Reserved_Max="4" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Inside_Claim__  
Set True or False for Inside_Claim
* __Distance__  
Set a numeric value for Distance
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses
* __Command_Cost__  
Set a numeric values for Command_Cost

#### Extended
* __Normal_Max__  
Set a numeric value for Normal_Max
* __Reserved_Max__  
Set a numeric value for Reserved_Max

### Description
Inside_Claim controls whether they must be inside their claim space to save the vehicle.

Distance controls how far a player can be from their vehicle to use the command. Distance is in blocks.

Delay_Between_Uses controls how often they can use the command. Delay is in minutes.

Command cost controls how much must be in a player's wallet to use the command.

## Voting & Voting_Extended
```xml
<Tool Name="Voting" Enable="False" Your_Voting_Site="https://7daystodie-servers.com/server/12345" API_Key="xxxxxxxx" Delay_Between_Uses="24" />
<Tool Name="Voting_Extended" Reward_Count="1" Reward_Entity="False" Entity_Id="73" Weekly_Votes="5" />
```
### Attributes
#### Normal
* __Enable__   
Set True or False for Enable
* __Your_Voting_Site__  
Set your server https address value for Your_Voting_Site. This will match your registered server at https://7daystodie-servers.com
* __API_Key__  
Set your server API_Key value found at https://7daystodie-servers.com for API_Key
* __Delay_Between_Rewards__  
Set a numeric value for Delay_Between_Rewards
* __Reward_Count__  
Set a numeric value for Reward_Count
* __Reward_Entity__  
Set True or False for Reward_Entity
* __Entity_Id__  
Set a numeric value for Entity_Id

#### Extended
* __Reward_Entity__  
Set True or False for Reward_Entity
* __Entity_Id__  
Set a numeric value for Entity_Id
* __Weekly_Votes__  
Set a numeric value for Weekly_Votes

### Description
Enabling allows players to use chat command /reward after voting at https://7daystodie-servers.com for your server.

If a player has not voted, they will be told to vote at YourVotingSite value.
The APIKey is attached to your registered server listed in your server details at https://7daystodie-servers.com.

DelayBetweenRewards controls how many hours a player must wait before being able to vote and receive a reward for voting.

Reward count controls how many items a player will receive from the VoteRewards.xml each time they use /reward.

Reward count does not control how many of a particular item you will receive, it controls how many entries from the VoteRewards.xml a player will receive.

Setting Reward_Entity to true will not use the VoteReward.xml. Instead it will spawn a single entity based on the Entity_Id.

Set Reward_Entity to true so that players receive an entity spawned for their reward.

## Wall
```xml
<Tool Name="Wall" Enable="True" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
Wall helps players create walls faster.

The player can enable and disable wall mode using command /wall.

While enabled, players can place two blocks in the corner points of an intended wall. It will automatically check nearby chests for matching blocks and fill the space between the corners with them.

Players must be inside of their own claimed space.

## Wallet
```xml
<Tool Name="Wallet" Enable="True" PVP="False" Zombie_Kill="10" Player_Kill="25" Bank_Transfers="False" />
<Tool Name="Wallet_Extended" Session_Bonus="5" Currency_Name="coin" />
```
### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __PvP__  
Set True or False for PvP
* __Zombie_Kill__  
Set a numeric value for Zombie_Kill
* __Player_Kill__  
Set a numeric value for Player_Kill
* __Bank_Transfers__  
Set True or False for Bank_Transfers

#### Extended
* __Session_Bonus__  
Set a numeric value for Session_Bonus
* __Coin_Name__  
Set a value of your choice for Coin_Name

### Description
The Wallet will use the casinoCoin by default but this can be changed via the items.xml provided in the Config folder with the latest release.

This file is provided with the installation files.

Each kill will reward currency to the player. Set what they are worth in the ServerTools config.

Set any values you want for zombie and player kills. If PvP is not set to true, player kills will earn nothing.

Bank_Transfers let a player send and receive wallet funds with the bank.

## Watch_List
```xml
<Tool Name="Watch_List" Enable="False" Admin_Level="0" Delay="5" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Admin_Level__  
Set a numeric value for Admin_Level
* __Alert_Delay__  
Set a numeric value for Alert_Delay

### Description
Enabling will create a Watchlist.xml in your main installation folder in a ServerTools folder.

All user Id that matches those listed in the watchlist will trigger an ingame alert to all online admins.

## Waypoints & Waypoints_Extended
```xml
<Tool Name="Waypoints" Enable="True" Max_Waypoints="4" Reserved_Max_Waypoints="8" Command_Cost ="0" Delay_Between_Uses="0" />
<Tool Name="Waypoints_Extended" Player_Check ="False" Zombie_Check="False" Vehicle="False" No_POI="False" />
```

### Attributes

#### Normal
* __Enable__  
Set True or False for Enable
* __Max_Waypoints__  
Set a numeric value for Max_Waypoints
* __Reserved_Max_Waypoints__  
Set a numeric value for Reserved_Max_Waypoints
* __Command_Cost__  
Set a numeric value for Command_Cost
* __Delay_Between_Uses__  
Set a numeric value for Delay_Between_Uses

#### Extended
* __Player_Check__  
Set True or False for Player_Check
* __Zombie_Check__  
Set True or False for Zombie_Check
* __Vehicle__  
Set True or False for Vehicle

### Description
Allows players to save a waypoint for teleport.

Players can list their own waypoints, add and delete them freely.

Command_Cost is how much it costs to use a waypoint out of their wallet.

Set the delay for players to use this command again. Time is in minutes.

Set Player_Check to true so they must be far enough from players to use this command.

Set Zombie_Check to true so they must be far enough and not targeted from zombies to use this command.

Players type /way to list existing. /way 'name' to use that waypoint. /way save 'name' to save where they are standing with that name. /waydel 'name' to remove the existing point.

Command options are as follows. Waypoint 'name', way 'name', wp 'name', fwaypoint 'name', fway 'name', fwp 'name', waypoint save 'name', way save 'name', ws 'name', waypoint del 'name', way del 'name', wd 'name'

## Web_API
```xml
<Tool Name="Web_API" Enable="False" Port="8084" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Port__  
Set a value of your choice for Port

### Description
Enabling will allow the Web Panel and Discord bot to communicate through the web api.
Requires port forwarding. The web api requires an open port to function.

## Web_Panel
```xml
<Tool Name="Web_Panel" Enable="False" />
```

### Attributes
* __Enable__  
Set True or False for Enable

### Description
Enabling will start the web panel.

The web panel requires Web_API be enabled.

Set the port you wish the panel to utilize under the Web_API tool. It will be reported in the output log.

Access via browser at http://IP:Port/st.html
Example: http://123.123.123.123:8084/st.html

Allow access to new clients by adding them via console command. A new password will be generated for them.

The console command is st-web add userName.

Clients can change their password upon successful sign in.

Clients can monitor a player list with kick, ban, mute, jail and reward options available.

Clients can alter the ServerToolsConfig.xml

## Workstation_Lock
```xml
<Tool Name="Workstation_Lock" Enable="False" />
```
### Attributes
* __Enable__  
Set True or False for Enable

### Description
While enabled, players can only access a workstation inside of a claimed space if they are the owner or allied with the claim owner.

## World_Radius
```xml
<Tool Name="World_Radius" Enable="False" Normal_Player="8000" Reserved="10000" Admin_Level="0" />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Normal_Player__  
Set a numeric value for Normal_Player
* __Reserved__  
Set a numeric value for Reserved
* __Admin_Level__  
Set a numeric value for Admin_Level

### Description
Enabling will automatically check player locations vs specified block ranges from center.

Normal_Player is the amount of blocks a non reserved player can travel from center before being teleport backwards away from the edge.

Reserved is the amount of blocks a reserved player can travel from center before being teleport backwards away from the edge.

Admin_Level controls the admin level required for the tool to skip them and allow travel outside of the block ranges.

## Zones
```xml
<Tool Name="Zones" Enable="False" Zone_Message="False" Reminder_Delay="20" Set_Home="False"  />
```

### Attributes
* __Enable__  
Set True or False for Enable
* __Zone_Message__  
Set True or False for Zone_Message
* __Reminder_Notice_Delay__  
Set a numeric value for Reminder_Notice_Delay
* __Set_Home__  
Set True or False for Set_Home

### Description
Enabling will create a Zones.xml in your main installation folder in a ServerTools folder.

Automatically detects players locations to see if they match those listed in Zones.xml.

Each zone can set the PvPvE mode which corresponds to the player killing mode.

PvPvE: 0 = No Killing, 1 = Kill Allies Only, 2 = Kill Strangers Only, 3 = Kill Everyone

If Zone_Message is false, players will not receive a message upon entering or exiting a protected zone.

EntryCommand and ExitCommand will run console commands upon entering and exiting the zone.

Multiple console commands can be run using ^ to separate them.

Reminder notice is the message the player receives if they stay in the same zone long enough.

Remove zombies from zones by setting No_Zombie to true.

Set_Home controls whether players can set a home inside of zones.

# Console Commands
 
Type help to get a list of available console commands.
Type help 'command name' to get more detailed information about a specific command.
Type help * st- for a list of ServerTools commands only.
 
# Chat commands
 
Type /commands in chat to get a list of available chat commands. It will only display commands relevent and enabled for the player.
