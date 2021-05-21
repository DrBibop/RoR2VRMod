
# Play Risk of Rain 2 in VR!
This mod was made possible with the joint effort of elliottate, MrPurple and me. You can contact us in our [Discord server](https://discord.gg/eQ7Fwac).

![](https://thumbs.gfycat.com/DeterminedSpryCassowary-size_restricted.gif)

In addition of adding VR functionality, this mod also tweaks multiple things to enhance the VR experience.

Playing in VR should be possible with any Oculus or SteamVR compatible devices. This includes WMR headsets with SteamVR. Make sure to disable game theatre mode in the game's properties.

If you want to support me and gain access to pre-release testing builds, you can head to my [Patreon](https://www.patreon.com/DrBibop)!

# Installation
It is recommended to use a mod manager such as [r2modman](https://thunderstore.io/package/ebkr/r2modman/) and press the "Install with Mod Manager" button. Once done, you can start the game using the "Start modded" button.

For manual download, make sure to have [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/) installed. You can then download the VR mod with the "Manual Download" button and copy the `plugins` and `patchers` folder into the `BepInEx` folder.

# Controls
Shoutout to laila, HutchyBen, Skarl1n, Geb, Popzix and Terrorcotta211 for helping me add support for all these controllers! Some controllers need special SteamVR bindings.

## Oculus Touch controllers
![](https://i.imgur.com/QRmmopA.png)

## Vive controllers
![](https://i.imgur.com/lINFo0A.png)

## WMR controllers with trackpads
![](https://i.imgur.com/W6XW6d1.png)

## Index Knuckles/Reverb G2 controllers
The Index Knuckles and the Reverb G2 controllers use the same binds as the Oculus Touch controllers but need special bindings in SteamVR in order to function properly. Their support in this version of Unity isn't well implemented out of the box. You can find the binding files below:

- [Download bindings for the Reverb G2 controllers](https://drive.google.com/uc?export=download&id=1nYZZR_wwwogffWNXotdZohRSigLbWvH4)
- [Download bindings for the Index Knuckles](https://drive.google.com/uc?export=download&id=1AJxT02TNW3qEndzXdoxu0Z5AiiKYDSbR)

With your file downloaded, open a file explorer and go to `C:/Users/%USERNAME%/Documents/steamvr/input/imports`. Create the `imports` folder if necessary. Drag the binding file you downloaded into that directory.

Now that the file is setup, you can open the game in VR and open the SteamVR overlay. Inside the overlay, you can go activate the custom binding:

![](https://thumbs.gfycat.com/GloriousComfortableFieldmouse-size_restricted.gif)

# FAQ
### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. We plan to make the VR experience better for players in the future and feedback regarding this issue would be very appreciated.

### Where are my VR hands?
Your hands will appear once you start a game. The menus don't use any motion controls at the moment. You can simply use the joysticks to navigate.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. You can play with vanilla players just fine too!

### Why play a 3rd person game in VR?
It honestly feels pretty great! This mod makes you play in first person by default but you can always come back to third person using the config editor.

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unclock it and click on the game. If that doesn't fix it, relaunching the game should work.

### The game won't launch in VR!
If you are using SteamVR and the game launches in game theatre mode, right click the game on Steam and go to `Properties > General`. You can then turn off the "Use Desktop Game Theatre while SteamVR is active" option. If it's still not working, there was likely an error when trying to activate VR. Ask for help on the [Discord server](https://discord.gg/eQ7Fwac).

### I have an Oculus headset and I don't want to use SteamVR.
No worries! You can switch to Oculus mode in the config file.
1. After launching the game in VR at least once, go to r2modman and click the `Config editor` on the left side.
2. Click on `VRMod > Edit Config`.
3. Set "Use Oculus mode" to `true`.
4. Click the `Save` button on the top-right.

To access the config file if you downloaded the mod manually, go to your game directory and head to `BepInEx/config`. You can then edit the "VRMod.cfg" file with a text editor.

### I'm still getting problems.
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
- Added a bindable key to recenter the HMD (Default: RCtrl/Dpad-Up).
- Added HUD config settings for UI scale and anchor placements.
- Fixed a bug that caused the map name to display too high up.
- Added MMHOOK Standalone as dependency (was previously included with the mod).

### 1.2.1
- Icons no longer have a fixed distance.
- Fixed a bug that caused icons to not correctly appear above targets.
- Fixed targeting indicator placements (Huntress primary, Engineer missile launcher, recycler, capacitor, etc.).

### 1.3.0
- Added first person config setting.
- Added snap turn and snap turn angle config settings.
- Added camera pitch lock config setting.
- Removed camera recoil effects.
- The pause menu now follows the camera rotation.
- Changed MMHOOK dependency to HookGenPatcher.

### 2.0.0
- Added motion controls support.
- Added a vignette during high-mobility abilities to reduce motion sickness (can be disabled).
- A dialog box now opens in the main menu telling the player how to recenter the HMD.
- The sprint icon on the bottom right turns yellow while sprinting to compensate for the lack of visual cues like the crosshair.
- The HUD should now appear at the same size no matter your resolution/FOV.
- Added HUD width and height config settings (HUD anchor settings need to be reset to default if you have downloaded a previous version).
- Fixed a bug that caused some targeting indicators to not face the camera properly.
- Fixed a bug that caused the dialog box in the pause menu to not follow the menu rotation.