
# Play Risk of Rain 2 in VR!
This mod was made possible with the joint effort of elliottate, MrPurple and me. You can contact us in our [Discord server](https://discord.gg/eQ7Fwac).

![](https://thumbs.gfycat.com/UnfitAdoredBooby-size_restricted.gif)

In addition of adding VR functionality, this mod also tweaks multiple things to enhance the VR experience.

Playing in VR should be possible with any Oculus or SteamVR compatible devices. Motion controls are currently not supported. A gamepad is therefore recommended.

If you want to support me and gain access to pre-release testing builds, you can head to my [Patreon](https://www.patreon.com/DrBibop)!

# Installation
It is recommended to use a mod manager such as [r2modman](https://thunderstore.io/package/ebkr/r2modman/) and press the "Install with Mod Manager" button. Once done, you can start the game using the "Start modded" button.

For manual download, make sure to have [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed. You can then download the VR mod with the "Manual Download" button and copy the `plugins` and `patchers` folder into the `BepInEx` folder.

# FAQ
### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. The occasionnal camera shake also doesn't help. We plan to make the VR experience better for players in the future and feedback regarding this issue would be very appreciated.

### Where are my VR hands?
Motion controls are currently not supported. We would love to add this in the future but it will require a lot of work and time. Meanwhile, we recommend using a gamepad such as an XBox controller.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. Other players do still need to have a modded client in order to have a matching game version. Make sure everybody has [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed with any other mods that require to be installed on every client.

### Why play a 3rd person game in VR?
It honestly feels pretty great! You can still play in first person using the [First Person mod](https://thunderstore.io/package/mistername/FirstPersonView/). Keep in mind that there isn't really a way to know which way is forward using this mod. Keep your body straight and use your joystick to make large turns. We plan on implementing our own first person system that works better for VR in the future.

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unclock it and click on the game.

### The game won't launch in VR!
If you are using SteamVR and the game launches in game theatre mode, right click the game on Steam and go to `Properties > General`. You can then turn off the "Use Desktop Game Theatre while SteamVR is active" option.

If the game keeps launching in desktop mode, try adding `-vrmode Oculus` or `-vrmode OpenVR` as a launch option depending on your device.
- If you're using r2modman, you can add launch options in `Settings > Debugging > Set launch parameters`.
- If you installed the mod manually, right click the game on Steam and click on `Properties > General` to access the launch options field.

### I disabled/uninstalled the VR mod and the game still launches in VR.
The proper way to disable VR is not to disable or uninstall the mod but to edit the mod's config file.
1. After launching the game in VR at least once, go to r2modman and click the `Config editor` on the left side.
2. Click on `VRMod > Edit Config`.
3. Set "VR Enabled" to `false`.
4. Click the `Save` button on the top-right.

To access the config file if you downloaded the mod manually, go to your game directory and head to `BepInEx/config`. You can then edit the "VRMod.cfg" file with a text editor.

With this setting disabled, the game should launch normally in desktop mode. After launching the game with VR off, you should be able to safely disable or uninstall the mod without any problems.

If that doesn't fix the problem, you can verify you game file integrity on Steam. Right click the game and click on `Propreties > Local Files > Verify inegrity of game files`.

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