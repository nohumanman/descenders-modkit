# Descenders-Split-Timer

This repository contains an assortment of C# scripts that can be implemented in the game "Descenders" using [Unity 2017.4.9f1 Modding](https://descenders.mod.io/guides/descenders-modding-guide).


## Tools & Features
Note: Most of these feature rely on an external server, *uptime is not guaranteed*.

### Split Timer
- A timer system that has split times integrated.
- Timer has configurable 'boundary' such that a player's time is invalidated when they exit the boundary.
- Has a leaderboard.
- Note: Relies on an external server, *uptime is not guaranteed*.
- Note II: Requires externally loaded DLL as described later.

- A physical leaderboard in the game that displays the top 10 fastest times on that trail on speedrun.com
- Physical Leaderboard model courtesy of BBB171
- Note: Relies on an external server, *uptime is not guaranteed*.
- Note II: Requires externally loaded DLL as described later.
- When you enter the Box Collider, the speed of the player is set to that of the "speedToSet" variable you assign.
- Doesn't kill the player.
![image](assets/Speed%20Modifier.png)

### Banners
- Banner model courtesy of [RageSquid](https://ragesquid.com/).
    - [Toy Elf](https://www.youtube.com/c/ToyElf)
    - [Ovanny](https://www.youtube.com/channel/UCd1LjvaKUITm8WXhnWy_d5A)
    - [BBB171](https://www.youtube.com/channel/UCfOIARENIJQd34lY06SCsiA)
    - myself
- These banner materials use [
Ciconia Studio](https://assetstore.unity.com/packages/vfx/shaders/free-double-sided-shaders-23087)'s double sided shaders.


### Bike Switcher
- Allows you to switch between Hardtail, Downhill, and Enduro bike types via UI.
- Note: Requires externally loaded DLL as described later.

### Riders Gate
- A BMX-Style start gate.
- **Synced with other networked players**.
- Has audio accompaniment for auditory queues.
- When 'G' is pressed, the gate is opened permanently.

![image](assets/Riders%20Gate.png)


### Teleport Pad
- Gives you the ability to teleport players using a box collider.
- Gives you the ability to freeze the player on teleport unlike many conventional teleporter scripts.

![image](assets/Teleport%20Pad.png)

### Boundary Tool
-

### Cable Tool
-


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