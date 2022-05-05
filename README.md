# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>
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
Copy the Mods folder to the root directory of your server.<br>
<br>
Start the server.<br>
<br>
The mod will auto create a new folder named ServerTools. Inside this folder is the ServerToolsConfig.xml that controls most of mod.<br>
Enable each part of the mod you want via ServerToolsConfig.xml<br>
Once a module/tool is enabled, if it has an xml it will be generated and placed in the same ServerTools folder.<br>
<br>
<br>
<br>
# AntiCheat

- [AntiCheat](#anticheat)
  - [Damage_Detector](#damage_detector)
  - [Dupe_Log](#dupe_log)
  - [Family_Share_Prevention](#family_share_prevention)
  - [Flying_Detector](#flying_detector)
  - [Godmode_Detector](#godmode_detector)
  - [Infinite_Ammo](#infinite_ammo)
  - [Invalid_Items](#invalid_items)
  - [Invalid_Item_Stack](#invalid_item_stack)
  - [Jail](#jail)
  - [Net_Package_Detector](#net_package_detector)
  - [Player_Logs](#player_logs)
  - [Player_Stats & Player_Stats_Extended](#player_stats--player_stats_extended)
  - [Protected_Zones](#protected_zones)
  - [PvE_Violations](#pve_violations)
  - [Spectator_Detector](#spectator_detector)
  - [Speed_Detector](#speed_detector)
  - [Tracking](#tracking)
  - [XRay_Detector](#xray_detector)

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
```????No documentation```

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

If you send a player to jail via chat commands, are sent to jail for 60 minutes. Players sent via console can be adjusted to suit your needs.

## Net_Package_Detector
```xml
<Tool Name="Net_Package_Detector" Enable="False" />
```

### Attributes
* __Enable__  
### Description
```???No documentation```


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

Use the console commands while in game to add protection to an area or add zones via the xml

Use the console commands while in game to remove protection to an area or remove zones via the xml

You can set the area protection to false so that you do not have to remove it from the list

Use two opposing corner points to designate the protected space. This will form a square or rectangle depending on the locations chosen

Protected spaces do not allow for any damage to the blocks nor for anyone to build inside of it including admins.

You can list the protected spaces in console.

You can disable the protection of a space while keeping it on the list to make it easy to reactivate.

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
Players that violate PvE Lobby or Market space will be hit with a strike. If they get too many of them, it will apply a penalty.

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
Admin_Level controls the tier required for the tool to skip specific players and allow them to view inside blocks.
