
# Play Risk of Rain 2 in VR!
Experience Risk of Rain 2 in virtual reality with full motion controls support! Join the [Flatscreen to VR Discord server](https://discord.gg/eQ7Fwac) to stay up to date with the development.

![](https://i.imgur.com/Z6R7Rli.gif)

Playing in VR should be possible with any Oculus or SteamVR compatible devices. This includes WMR headsets as well as the Quest/Quest 2 if connected to a PC via a Link cable, AirLink or Virtual Desktop. Make sure to disable game theatre mode in the game's properties.

If you want to support me and gain access to pre-release testing builds, you can head to my [Patreon](https://www.patreon.com/DrBibop)!

# Installation
It is strongly recommended to use a mod manager such as the [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager) or [r2modman](https://thunderstore.io/package/ebkr/r2modman/) and press the "Install with Mod Manager" button. Once done, you can start the game using the "Start modded" button.

For manual download, make sure to have [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/) installed. You can then download the VR mod with the "Manual Download" button and copy the `plugins` and `patchers` folder into the `BepInEx` folder.

# Default controls
Shoutout to laila, HutchyBen, Skarl1n, Geb, Popzix and Terrorcotta211 for helping me add support for all these controllers!

## Oculus Touch/Reverb G2/Vive Cosmos controllers
![](https://i.imgur.com/EdPkhQD.png)

## Index Knuckles
![](https://i.imgur.com/9eLpOgc.png)

## Vive controllers
![](https://i.imgur.com/SFrR5pP.png)

## WMR controllers
![](https://i.imgur.com/h75xsb3.png)

## Inputs for mods
The VR Mod uses the SteamVR binding system to bind controls. Some binds have been added to support certain mods.

### Currently supported inputs for mods:
[**ExtraSkillSlots**](https://thunderstore.io/package/KingEnderBrine/ExtraSkillSlots/): Skills 1-4\
[**VoiceChat**](https://thunderstore.io/package/Evaisa/Voicechat/): Push-to-talk\
[**SkillsPlusPlus**](https://thunderstore.io/package/Volvary/SkillsPlusPlus_UnofficialRelease/): Buy menu\
[**ProperSave**](https://thunderstore.io/package/KingEnderBrine/ProperSave/): Load

# Custom characters
Custom characters can receive full VR support using the [VRAPI](https://thunderstore.io/package/DrBibop/VRAPI/). You should still be able to play any custom characters without the API and aim with your dominant hand but the default pointer will be used for the hand model.

### List of fully VR supported custom characters as of September 2023:
- [Samus](https://thunderstore.io/package/dgosling/dgoslings_Samus_Mod/) by dgosling
- [Enforcer and Nemesis Enforcer](https://thunderstore.io/package/EnforcerGang/Enforcer/) by EnforcerGang
- [Paladin](https://thunderstore.io/package/Paladin_Alliance/PaladinMod/) by Paladin_Alliance
- [Tesla Trooper](https://thunderstore.io/package/TheTimesweeper/Tesla_Trooper/) by TheTimesweeper
- [Playable Void Jailer](https://thunderstore.io/package/Xan/VoidJailerPlayerCharacter/) by Xan

# Haptics
The mod supports the Bhaptics and Shockwave suits which provide more immersive gameplay.

Follow these steps to enable haptics for your suit:
1. Open the game with the VR mod installed.
2. Open the settings menu and navigate to the VR tab.
3. Scroll to the very bottom to find the "Haptics suit" setting.
4. Click the arrows to select the suit you're wearing.
5. Relaunch the game.

# FAQ

### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. Feedback regarding this issue would be very appreciated.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. You can play with vanilla players just fine too!

### Where can I configure the mod like turning off snap turning?
The VR Mod settings can be accessed with the in-game settings in the "VR" tab. You can also use the mod manager instead while the game is closed. On the left, click on `Config Editor > VRMod > Edit Config`. You can then change the settings to your liking. Once you're done, make sure to save your changes with the `Save` button on the top-right.

### Why play a 3rd person game in VR?
This mod makes you play in first person by default but you can always come back to third person by going in the settings or the config editor. It still plays well but currently doesn't support motion controls.

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unlock it and click on the game. If that doesn't fix it, relaunching the game should work.

### The game won't launch in VR!
If you are using SteamVR and the game launches in game theatre mode, right click the game on Steam and go to `Properties > General`. You can then turn off the "Use Desktop Game Theatre while SteamVR is active" option. If you're using an Oculus headset, try enabling the "Use Oculus mode" setting in the config editor to bypass SteamVR. Finally, make sure your Steam and game folder are correctly set in the mod manager's settings. If it's still not fixed, ask for help on the [Discord server](https://discord.gg/eQ7Fwac).

### Can I change my controller binds?
You can change your binds using SteamVR's binding system. Note that changing your binds won't change the icons shown in-game. There is currently no way to change your binds when using the Oculus Runtime instead of SteamVR.

### The game is lagging. What can I do to improve performance?
Disabling SSAO and Bloom in the game's settings should improve the performance. Make sure you also don't have too many other applications running in the background. You can also try the [VR Performance Toolkit](https://github.com/fholger/vrperfkit) which adds upscaling technology like FSR and NIS to the game.

### I have an Oculus headset and I don't want to use SteamVR.
No worries! You can switch to Oculus mode with the in-game settings.
If you really don't want to launch the game with SteamVR, you can instead edit the config file.
1. After launching the game in VR at least once, go to r2modman and click the `Config editor` on the left side.
2. Click on `VRMod > Edit Config`.
3. Set "Use Oculus mode" to `true`.
4. Click the `Save` button on the top-right.

To access the config file if you downloaded the mod manually, go to your game directory and head to `BepInEx/config`. You can then edit the "VRMod.cfg" file with a text editor.

### I'm still getting problems.
You can ask for help in our [Discord server](https://discord.gg/eQ7Fwac). You can also check our [issues list](https://github.com/DrBibop/RoR2VRMod/issues) to see what problems are known.

# Credits

**DrBibop:** Mod/patcher programmer, animator\
**MrPurple6411:** Patcher programmer\
**dotflare:** 3D artist\
**Ncognito:** 3D artist\
**eliotttate:** Original creator, code assist\
**HutchyBen:** Code assist\
**AmadeusMop:** Code assist\
**Daerst:** Fixed the biggest and oldest bug of this mod. Thank you so much.