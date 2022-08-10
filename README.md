# OSU-Top2Collection V2
A tool that fetches the top X from gamemode X from users and add's them to your Collection 

## USAGE:
- Download ZIP from Releases
- Edit "top2collection.json" (see below)
- *OPTIONAL* Set as desktop Shortcut
- PROFIT
- you may need .NET 6 for Console Applications if you download release 1.0
- you may need .NET 6 for Desktop Apps if you download 1.1
https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime

### PLEASE NOTE 
Peppy gives us an fully open and usally unlimited API to work with. So please dont fetch like 200 users every time you want to start up osu. Public API's are not cheap to maintain so use it with love and care!
## Config File
The config file is called ``top2collection.json`` and needs to be at the same place as the exe file. 
```
[{
  "osupath": "G:\\osu!\\",
  "mode": [ "0", "0", "0", "0", "0" ],
  "apikey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "targets": [ "Dynam1cNET", "9704883", "4382588", "4014879", "3390230" ],
  "limits": [ "100", "100", "100", "100", "100" ],
  "collectionnames": [ "I Suck at Farming", "Kalle Sucks at Farming", "Waterbottle Sucks at Farming", "--Pants Sucks at Farming", "###Liz Sucks at Farming" ],
  "backupCollectionsDB": true,
  "startosuaftersync": true,
  "generateMissingMaps": true
}]
```
* **osupath:** The path to your osu! folder. Please note that we need to escape "\\" with another "\\" so we have "\\\\". **Also its important that we add the extra "\\\\" at the end!**
* **mode:** Here we specify the gamemode. ``0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania`` We need to set a mode for every user we add
* **apikey:** Insert your osu API key here. You get it from ``https://osu.ppy.sh/p/api`` note that we sont have a "/" at the end of the link. if you get redirected to the forum site after you logged in you have to paste that link again with the "/" at the end.
* **targets:** Here we insert our users of wich we want the top plays from. As you can see in the Example you can use osu names and user id's. I would recommend user ID's. You get the user id from the profile link. eg. https://osu.ppy.sh/users/ **12540789**
* **limits:** here we specify how deep we want to get the top plays. So if you only need the top 20 then inster 20 here. We also need to do this for every user we add.
* **collectionnames:** The name of the Collections (ingame) We need to again do this for every user. Each name has to be unique. 
* **backupCollectionsDB:** Backs up your original collection.db in a new folder called ``"collecionBackups"`` inside your osu! folder
* **startosuaftersync:** Starts osu after finishing. May be interesting if you want to run this before every start of osu. 
* **generateMissingMaps:** Will generate a folder inside the osu! folder called ``"MissingMaps"`` inside that folder you will find html files of every collection where you have maps missing. 
