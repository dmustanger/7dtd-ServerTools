# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>

# Installation
Go to the releases tab and check for the latest files.<br>
Or click this link https://github.com/dmustanger/7dtd-ServerTools/releases <br>
<br>
Download and extract the files.<br>
Copy the Mods folder to the root directory of your server.<br>
<br>
Start the server.<br>
<br>
The mod will auto create the necessary config file in the game's save directory. Enable each part of the mod you want via ..\your game save directory\ServerTools\ServerToolsConfig.xml<br>
Once a module is enabled, if it has a config or folder, it will auto create them in the ServerTools folder.<br>

# Current Features
Multiple anticheat systems. One for flying above ground, one for below ground. Player stats monitor. Player position logger.<br>
<br>
Vote reward system linked into the https://7daystodie-servers.com website.<br>
<br>
Chat command /admin to show players online administrators and moderators.<br>
<br>
Can reset all player's command delays via console.<br>
<br>
Temporary ban command for 1 to 60 min ban via console.<br>
<br>
Default chat command triggers / and ! can be set to a custom instead.<br>
<br>
Administrator alerts when entities are detected stuck underground.<br>
<br>
Administrators can turn off their chat color to remain stealthy via console.<br>
<br>
Special players can be given a unique prefix and color for chat.<br>
<br>
Automatically teleports entities found underground to the surface such as bikes and backpacks. This drastically reduces lag on busy servers.<br>
<br>
Console command restart check for remaining time until auto restart initiates. Chat command /restart does the same.<br>
<br>
Auto restart. Set a time and countdown amount. This runs stopserver command automatically upon timer expiration<br>
<br>
Animal spawning with /trackanimal or /track. Delay timer optional. Choice of available animals via servertools config<br>
<br>
Reservation expiration check with /reserved in chat.<br>
<br>
Donator chat colors and prefix. Adjustable colors via chat command /doncolor. Players must be listed in the reserved list.<br>
<br>
Custom chat commands with custom color. Add your own commands via config. All commands are added to the chat commands /commands /info /help automatically. All chat commands can use "!" or "/". If "/gimme" is used it returns a private message to the player. If "!gimme" is used it returns a message to the entire server.<br>
<br>
/Gimme with adjustable timer and items via config.<br>
<br>
/Killme with adjustable timer via config.<br>
<br>
/home /sethome /delhome with adjustable timer for /home via config.<br>
<br>
High ping kicker with ping immunity. Can add players to the immunity list via config or console command. Can also set it so that it takes samples before a kick occurs. If you set SamplesNeeded to 2 in the config, a player would have to have a high ping for 2 checks before the player is kicked.<br>
<br>
Ban or kick players for invalid items/blocks in their inventory such as reinforced concrete blocks. Select what items/blocks are invalid via config.<br>
<br>
Alert players of Invalid Item stack numbers in their inventory.<br>
<br>
Chat logger. Saves all ingame chat to a log file in the game save directory.<br>
<br>
Bad word filter. Replaces bad words with "*****". Can add bad words via config.<br>
<br>
Motd adjustable via config.<br>
<br>
InfoTicker/Scrolling messages adjustable via config.<br>
<br>
Auto save the world every x amount of minutes adjustable via config.<br>
<br>
Watchlist You can add suspect players to this list and when they join the server it will alert any online admins that they are online.<br>
<br>
All chat responses can be edited via config.<br>
<br>
Admin chat commands @admins @all /mute /unmute. @admins \<message\> will send a message to all admins and only admins can use this command. @all \<message\> is just like a say command. /mute \<playerName\> will keep said player from posting in the chat. /unmute \<playerName\> will allow the player to post in the chat again.<br>
<br>
Clan Tag Protection. Only the person that created the clan can delete the clan and promote/demote members to/from officers. Only clan owners and officers can invite and remove members.<br>
to make a clan type /clanadd \<clanTag\><br>
to delete a clan type /clandel<br>
to add members type /claninvite \<playerName\><br>
to remove a member type /clanremove \<playerName\><br>
to promote a member to officer type /clanpromote \<playerName\><br>
to demote a member from officer to member type /clandemote \<playerName\><br>
to accept a clan invite type /clanaccept<br>
to decline a clan invite type /clandecline<br>
to leave a clan type /clanleave<br>
<br>
Chat commands /pm or /w for players to pm other players.<br>
Usage: /w \<playername\> \<message\> or /pm \<playername\> \<message\><br>
<br>
Admin name coloring and prefixes<br>
<br>
Console command to stop the game server. Usage: stopserver \<minutes\><br>
<br>
Console command to remove a entity. Usage: entityremove \<entityId\><br>
<br>
Console command to reset a players profile. Usage: rp \<steamId\> or resetplayer \<steamId\>
