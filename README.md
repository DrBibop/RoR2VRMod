# Play Risk of Rain 2 in VR!
This mod was made possible with the joint effort of elliottate, MrPurple and me. You can contact us in our [Discord server](https://discord.gg/eQ7Fwac).

In addition of adding VR functionality, this mod also tweaks multiple things to enhance the VR experience.

Playing in VR should be possible with any Oculus or SteamVR compatible devices. Motion controls are currently not supported. A gamepad is therefore recommended.

# Installation
It is recommended to use a mod manager such as [r2modman](https://thunderstore.io/package/ebkr/r2modman/) and press the "Install with Mod Manager" button. Once done, you can start the game using the "Start modded" button.

For manual download, make sure to have [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed. You can then download the VR mod with the "Manual Download" button and copy the `plugins` and `patchers` folder into the `BepInEx` folder.

# FAQ
### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. The occasionnal camera shake also doesn't help. We plan to make the VR experience better for players in the future and feedback regarding this issue would be very appreciated.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. Other players do still need to have a modded client in order to have a matching game version. Make sure everybody has [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/) installed with any other mods that require to be installed on every client.

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unclock it and click on the game.

### Why can't I see the menu?
We're working on it! Meanwhile, you can either use the menu on your PC or open a virtual desktop in your headset if supported.

### The character selection menu is too big!
We're also working on that! The controls are still functional to navigate.

### The game won't launch in VR!
If the game keeps launching in desktop mode, try adding `-vrmode Oculus` or `-vrmode OpenVR` as a launch option depending on your device.
- If you're using r2modman, you can add launch options in `Settings > Debugging > Set launch parameters`.
- If you installed the mod manually, right click the game on Steam and click on `Properties > General` to access the launch options field.

### I disabled/uninstalled the VR mod and the menu looks weird.
If you encounter a menu with a black background with a cursor leaving a trail after disabling/uninstalling the mod, try setting `-vrmode None` in the launch options.
- If you're using r2modman, you can add launch options in `Settings > Debugging > Set launch parameters`.
- If you installed the mod manually, right click the game on Steam and click on `Properties > General` to access the launch options field.

If that doesn't fix the problem, you can verify you game file integrity on Steam. Right click the game and click on `Propreties > Local Files > Verify inegrity of game files`.

### I'm still getting problems
You can ask for help in our [Discord server](https://discord.gg/eQ7Fwac). You can also check our [issues list](https://github.com/DrBibop/RoR2VRMod/issues) to see what problems are known.


# Changelog
### 1.0.0
Initial release of the mod.