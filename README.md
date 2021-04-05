
# Play Risk of Rain 2 in VR!
This mod was made possible with the joint effort of elliottate, MrPurple and me. You can contact us in our [Discord server](https://discord.gg/eQ7Fwac).

![](https://thumbs.gfycat.com/UnfitAdoredBooby-size_restricted.gif)

In addition of adding VR functionality, this mod also tweaks multiple things to enhance the VR experience.

Playing in VR should be possible with any Oculus or SteamVR compatible devices. Motion controls are currently not supported. A gamepad is therefore recommended.

If you want to support me and gain access to pre-release testing builds, you can head to my [Patreon](https://www.patreon.com/DrBibop)!

# Installation
It is recommended to use a mod manager such as [r2modman](https://thunderstore.io/package/ebkr/r2modman/) and press the "Install with Mod Manager" button. Once done, you can start the game using the "Start modded" button.

For manual download, make sure to have [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/), [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed. You can then download the VR mod with the "Manual Download" button and copy the `plugins` and `patchers` folder into the `BepInEx` folder.

# FAQ
### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. The occasionnal camera shake also doesn't help. We plan to make the VR experience better for players in the future and feedback regarding this issue would be very appreciated.

### Where are my VR hands?
Motion controls are currently not supported. We would love to add this in the future but it will require a lot of work and time. Meanwhile, we recommend using a gamepad such as an XBox controller.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. Your friends still need a modded client with [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/), [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed.

### Why play a 3rd person game in VR?
It honestly feels pretty great! You can still play in first person using the [First Person mod](https://thunderstore.io/package/mistername/FirstPersonView/). Keep in mind that there isn't really a way to know which way is forward using this mod. Keep your body straight and use your joystick to make large turns. We plan on implementing our own first person system that works better for VR in the future.

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unclock it and click on the game.

### The game won't launch in VR!
If you are using SteamVR and the game launches in game theatre mode, right click the game on Steam and go to `Properties > General`. You can then turn off the "Use Desktop Game Theatre while SteamVR is active" option.

### I have an Oculus headset and I don't want to use SteamVR.
No worries! You can switch to Oculus mode in the config file.
1. After launching the game in VR at least once, go to r2modman and click the `Config editor` on the left side.
2. Click on `VRMod > Edit Config`.
3. Set "Use Oculus mode" to `true`.
4. Click the `Save` button on the top-right.

To access the config file if you downloaded the mod manually, go to your game directory and head to `BepInEx/config`. You can then edit the "VRMod.cfg" file with a text editor.

### I'm still getting problems
You can ask for help in our [Discord server](https://discord.gg/eQ7Fwac). You can also check our [issues list](https://github.com/DrBibop/RoR2VRMod/issues) to see what problems are known.


# Changelog
### 1.0.0
- Initial release of the mod.

### 1.1.0
- All menus are now visible in VR.
- Enemy healthbars are now correctly positioned above enemies.
- Ping icons have been pushed further away from the camera.
- Added a config setting to disable VR.

### 1.1.1
- Fixed some indicator icons that were too large.

### 1.1.2
- Fixed yet another oversized indicator.
- Removed R2API dependency.
- Lowered the top part of the HUD (was reverted due to the anniversary update).
- Fixed a bug that caused the game to launch in VR after disabling or uninstalling the mod.
- Removed the need for launch options (this causes the game to launch in SteamVR by default).
- Added a config setting to launch in Oculus mode.
- Removed the "Enable VR" setting.

### 1.2.0
- Added a bindable key to recenter the HMD