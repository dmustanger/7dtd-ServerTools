# 7dtd-ServerTools
Server tools for 7 days to die dedicated servers<br>
<br>
# Installation
<br>
Download and extract the files.<br>
<br>
Copy the Mods folder to the root directory of your server.<br>
<br>
Start the server.<br>
<br>
The mod will auto create the config file in the game save directory.<br>
Enable each part of the mod you want via ..\your game save directory\ServerTools\ServerToolsConfig.xml<br>
Once a module is enabled, if it has a config it will auto create them in the ServerTools folder.<br>
<br>
Go to the releases tab and check for the latest files https://github.com/dmustanger/7dtd-ServerTools/releases <br>
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
Detects: flying above or below ground, hatch elevator, teleport, invalid items, invalid stack sized, family share accounts, <br>
player stats such as height, speed, jump strength, max stamina, max health, available skill points.<br>
<br>
Punish violators automatically with: Kill, Jail, Kick, Ban.<br>
<br>
Control the world radius and how far players can travel.<br>
<br>
Logs all player's inventory to a file for later review. Set the time between loggings. Default is once per minute.<br>
<br>
Records player's IP address and location to the inventory log.<br>
<br>
Logs of violators are auto created and deleted based on your settings.<br>
<br>
<br>
<br>
Other Tools<br>
<br>
<br>
Auction box: players can sell one item or stack at a time by placing it in a box and selling it through the auction system<br>
Players can buy it as they please with their wallet coins<br>
<br>
Shop: setup a list of items and prices for players to purchase inside or outside of a trader zone. Includes a wallet system<br>
<br>
Entity Cleanup: removed items in a pile, blocks if too many are dropping, fallen trees stuck at 0 hp<br>
<br>
Bike return: teleport a saved bike if close enough, back to the player<br>
<br>
Bag return: after dying players can teleport their bag items back to their inventory<br>
<br>
Vote: To kick or mute a player<br>
<br>
World Radius: control normal and donators world size as an anti cheat or feature for donators to travel further<br>
<br>
Day7 alerts: automatically receive in game alerts to the upcoming bloodmoon. Can set a custom horde night value<br>
<br>
Zone protection: set an area as a protected space. PvP inside is dealt with automatically. Alert upon entry/exit<br>
<br>
Weather vote: vote to change the weather<br>
<br>
Teleport to friend: request to teleport to a friend. They must accept the request in a time limit<br>
<br>
Shop and wallet: make a custom shop list for players to buy from. Wallet is calculated automatically. Can only shop in trader zone<br>
<br>
Vote rewards: for players voting your server at https://7daystodie-servers.com website<br>
<br>
Travel: for players to specific locations when in the right location. Can teleport a bike. Custom delay<br>
<br>
Starting items: new players receive all the items from the StartingItems.xml<br>
<br>
First claim block: players can type /claim to receive a single claim block. Prevents greifing<br>
<br>
Give item: give item directly into a players inventory or all online players. Drops to the ground if their inventory is full<br>
<br>
Give block: give block directly into a players inventory or all online players. Drops to the ground if their inventory is full<br>
<br>
Admin List: players can type trigger an ingame list of currently online admins and moderators<br>
<br>
Command delays: multiple commands have delay times. Players can not use the command until the delay has expired since last use<br>
<br>
Command delay reset: reset any player command delays via the console<br>
<br>
Temporary ban: console command tempban to allow a limited ban time for moderators or low tier admins<br>
<br>
Custom chat command triggers: commands use / and ! to initialize commands but they can be set to any symbol<br>
<br>
Stuck entity: automatically sends bikes, bags, and zombies to the world surface to prevent errors and server lag<br>
<br>
Admin alerts: detections that require manual inspection is sent to currently online admins<br>
<br>
Chat colors and prefixes: administators, moderators, donators/reserved players, special players can all get custom chat colors and prefix<br>
<br>
Auto shutdown: the server will auto shutdown after a scheduled time frame. Initiates shutdown system with alerts, world save, countdown<br>
<br>
Animal spawning: made to be like an animal tracking system. Players spawn a random animal from a list<br>
<br>
Stealh admins: while chat color and prefix is active, admins can disable their chat color to stay hidden<br>
<br>
Reserved players: automatically kick a non reserved player to let your reserved players in<br>
<br>
Custom commands: Make your own commands and set optional delays for the first ten entries<br>
<br>
Check next shutdown: Can check when the next scheduled shutdown period is<br>
<br>
Reservation check: players can check the expiration time for their reservation<br>
<br>
Gimme system: players can spawn a free item periodically with a custom delay<br>
<br>
Killme command: players can commit suicide with a custom delay<br>
<br>
Set home: save a home location so players can teleport to it with a custom delay<br>
<br>
Set home 2: a second home location that can be set for donators only<br>
<br>
High ping kicker: kicks players that have a ping too high<br>
<br>
Chat logger: saves all in game chat to a file for later review<br>
<br>
Bad word filter: replaces any words used in chat that matches ones from a list. Can also change bad player names to Invalid Name-No commands<br>
<br>
Message of the day: shows players a custom message upon entering the server<br>
<br>
Infoticker: automatically displays custom server messages. Can be set to display in a random order<br>
<br>
Auto world save: automatically saves the world based on custom delay time<br>
<br>
Watchlist: players listed here will be able to join but send online admins an alert when they are in game<br>
<br>
Custom phrases: any phrases list in the phrase file can be set a custom response<br>
<br>
Admin chat: admins can send all other online admins a direct private message<br>
<br>
Clan manager: create clan tags, add players, hire officers and rule the server as a elite crew. Clan name is set in chat<br>
<br>
Remove entity: remove a live entity from the game using its entity id via console<br>
<br>
Reset player profile: an easy console command will kick and delete the saved data of a player<br>
<br>
Stopserver command: easily shutdown your server with an alert system and countdown so players are aware<br>
<br>
Login notices : show unique alerts to players as a listed player joins the game<br>
<br>
Teleport new players : send new players to a designated location when they first join your server<br>
<br>
<br> 
<br>
