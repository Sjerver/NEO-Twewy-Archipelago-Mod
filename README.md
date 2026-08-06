# NEOTwewyArchipelagoMod
This is a mod for [NEO: The World Ends with You](https://store.steampowered.com/app/1647550/NEO_The_World_Ends_with_You/) that integrates it into [Archipelago.gg](https://archipelago.gg/).

## What it Does
This mod adds Archipelago support to NEO: The World Ends with You and allows it to participate in Archipelago multiworld games:

- Replaces Story/Scenario Rewards and Shop items up to the end of Week 1 Day 4.
- Adds rewards for beating a day the first time.
- Connects to the Archipelago server on boot.

The goal of this randomizer is to reach Week 1 Day 5. In order to proceed from one day to the next you need at least as many Secret Reports as the day you are currently on.
So you need 1 Secret Report to reach Day 2 for example. Once you reach Day 5 the seed will be *complete*.

If you complete a day without enough Secret Reports to progress, the day will repeat.

In this case, you can use the configured skip button to skip to the end of the day.
The default key is F5 and can be changed in the config file.


## What is Required

The Steam version of NEO: The World Ends with You is required in order to play this game.

You can find the corresponding APWorld in this repository: https://github.com/Sjerver/Archipelago-NEO-TWEWY/tree/main.

# Installing this Mod

1. Download the [latest release](https://github.com/Sjerver/NEO-Twewy-Archipelago-Mod/releases) of this mod.
2. Download MelonLoader.Installer.exe from https://github.com/LavaGang/MelonLoader/releases/latest and run it.
3. Select NEO: The World Ends with You in the list of games.
   If NEO: The World Ends with You does not appear, click add game manually then browse to NEO: The World Ends with You.exe.
4. Click install. If the latest version is not working: Untick Latest and select 0.7.3
5. Download NEOTwewyArchipelagoMod.zip.
6. Head to the NEOTwewyArchipelagoMod folder and open up /mods/. If this folder does not exist, run the game and it should appear.
7. Extract the contents of the .zip file into mods. Ensure the files are not in a subfolder, this mod is not setup for that yet.
8. Edit `Mods/NeoTwewyArchipelago/NEOTwewyArchipelagoConfig.json` to match the Archipelago room you want to connect to.
9. Run the game.

# Using this Mod
1. When starting a new game, you will need to be connected to an archipelago server. If you are not connected, the game will not start.
2. The mod will replace the rewards for the first 4 days of the game with items from your Archipelago world.
3. After you have initially started a save file while connected to a server, it is possible to play the game without being connected to a server. 
   However, you will not be able to receive any new items from the Archipelago world until you reconnect.
4. If you are connected to a room, whose seed does not match the last used seed, the game won't receive items or send location checks.
   You will need to start a new game to resync the seed. The mod will warn you if this happens.

# Known Issues
- Currently, there is no way to tell which Archipelago items from other worlds you collect in game via scenario/quest rewards. It is recommended to keep an eye on the MelonLoader console while playing. 

# Building this Mod

## Requirements

- Visual Studio 2022
- .NET 6 SDK
- [Melon Loader v0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3)

## Setup

1. Install MelonLoader v0.7.3 and run the game once.
2. Clone this repository.
3. Open `NEOTwewyArchipelagoMod.sln`.

## GamePath configuration

The project uses the `GamePath` MSBuild property to locate the MelonLoader assemblies.

Create a `Directory.Build.props` file next to NEOTwewyArchipelago.csproj:

<Project>
  <PropertyGroup>
    <GamePath>C:\Path\To\NEO The World Ends with You</GamePath>
  </PropertyGroup>
</Project>
