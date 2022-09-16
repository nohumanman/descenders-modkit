# Descenders-Split-Timer

This repository contains an assortment of C# scripts that can be implemented in the game "Descenders" using [Unity 2017.4.9f1 Modding](https://descenders.mod.io/guides/descenders-modding-guide).


## Tools & Features

### Split Timer
- A timer system that has split times integrated.
- Timer has configurable 'boundary' such that a player's time is invalidated when they exit the boundary.
- Has a leaderboard.
- Note: Relies on an external server, *uptime is not guaranteed*.
- Note II: Requires externally loaded DLL as described later.

### Speedrun.com Leaderboard
- A physical leaderboard in the game that displays the top 10 fastest times on that trail on speedrun.com
- Physical Leaderboard model courtesy of BBB171
- Note: Relies on an external server, *uptime is not guaranteed*.
- Note II: Requires externally loaded DLL as described later.

### Bike Switcher
- Allows you to switch between Hardtail, Downhill, and Enduro bike types via UI.
- Note: Requires externally loaded DLL as described later.

### Riders Gate
- A BMX-Style start gate **that is synced with other networked players**.
- Has audio accompaniment for auditory queues.
- Note: Relies on an external server, *uptime is not guaranteed*.
- Note II: Requires externally loaded DLL as described later.

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


### Boundary Tool

### Cable Tool

## Implementations
This timer is implemented, to some capacity, on 
- **[Igloo Bike Park](https://mod.io/g/descenders/m/igloo-bike-park)** by *[antgrass](https://mod.io/g/descenders/u/antgrass)* (50K+ downloads)
- **[Montcerf](https://mod.io/g/descenders/m/montcerf)** by *[antgrass](https://mod.io/g/descenders/u/antgrass)*
- **[Hedgecock](https://mod.io/g/descenders/m/hedgecock)** by *[antgrass](https://mod.io/g/descenders/u/antgrass)*
- **[4x Dobrany](https://mod.io/g/descenders/m/4x-dobrany)** by *[BBB1711](https://mod.io/g/descenders/u/bbb1711)*
- **Ced's Downhill Park (unpublished)** by *[antgrass](https://mod.io/g/descenders/u/antgrass)*
- **[Igloo Bike Park](https://mod.io/g/descenders/m/igloo-bike-park)** by *[antgrass](https://mod.io/g/descenders/u/antgrass)*
- **[MTB Paradise](https://mod.io/g/descenders/m/mtb-paradise)** by *[KAMUS](https://mod.io/g/descenders/u/kamus)*
- **[Red Bull Hardline 2021](https://mod.io/g/descenders/m/rbhl21)** by *[
BI0S0CK](https://mod.io/g/descenders/u/bi0s0ck)*
- **[MTR BMX Track](https://mod.io/g/descenders/m/mtr-bmx-track)** by *[dragonkiller37](https://mod.io/g/descenders/u/dragonkiller37)*