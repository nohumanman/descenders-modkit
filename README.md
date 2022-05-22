# Descenders-Split-Timer

This repository contains an assortment of C# scripts that can be implemented in the game "Descenders" using [Unity 2017.4.9f1 Modding](https://descenders.mod.io/guides/descenders-modding-guide).


## Tools & Features

### Split Timer
- A timer system that has split times integrated.
- Timer has configurable 'boundary' such that a player's time is invalidated when they exit the boundary.
- Has a leaderboard.
  - Note: Relies on an external server, *uptime is not guaranteed*.
  - Note II: Requires externally loaded DLL as described later.

### Map Navigation
- Player Tracking - allows both networked players and local players to be visually represented on the map, with the name displayed.
- Player Teleportation - allows you to teleport to the players that are visually represented on the map.
- Uses an orthographic camera to render the map, not a fixed photo or texture.
- Fast Travel Locations - scrollable fast travel locations on the map UI.

### Bike Switcher
- Allows you to switch between Hardtail, Downhill, and Enduro bike types via UI.
  - Note: Requires externally loaded DLL as described later.

### Splash Screen
- Allows you to load a video file of your choice and start it when the map is loaded.
- The video will stop and dissapear when any key is pressed.
- The video will be looped.

### Riders Gate
- A BMX-Style start gate **that is synced with other networked players**.
- Has audio accompaniment for auditory queues.
  - Note: Relies on an external server, *uptime is not guaranteed*.
  - Note II: Requires externally loaded DLL as described later.

### Ice Mod
- Adds the same functionality as in Bikeout, where the rider is extremely slippery.
  - Note: Requires externally loaded DLL as described later.

### Teleport Pad
- Gives you the ability to teleport players using a box collider.
- Gives you the ability to freeze the player on teleport unlike many conventional teleporter scripts.

## Externally Loaded DLLs
For some features it is required to use an externally loaded DLL, this DLL (MapTools.dll) can be installed as follows:
1. Copy the MapTools.dll file.
2. Locate to your Mod folder (the same folder as the .info file is located).
3. Paste the MapTools.dll file into this folder.
4. Restart Descenders and load the mod - the DLL is now loaded..

Note: The DLL file will be deleted every time you export your unity project.

