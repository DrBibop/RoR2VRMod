
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

### Currently supported mods:
[**ExtraSkillSlots**](https://thunderstore.io/package/KingEnderBrine/ExtraSkillSlots/): Skills 1-4\
[**VoiceChat**](https://thunderstore.io/package/Evaisa/Voicechat/): Push-to-talk\
[**SkillsPlusPlus**](https://thunderstore.io/package/Volvary/SkillsPlusPlus_UnofficialRelease/): Buy menu\
[**ProperSave**](https://thunderstore.io/package/KingEnderBrine/ProperSave/): Load

# FAQ

### Wouldn't it be nauseating to play in VR?
Each person has a different level of tolerance regarding motion sickness and VR. Despite that, Risk of Rain 2 was not intended to be played in VR. This means that getting motion sick is more likely, especially with high mobility characters such as Loader or Mercenary. We plan to make the VR experience better for players in the future and feedback regarding this issue would be very appreciated.

### Can I play using the VR Mod in multiplayer with my non-VR friends?
Yes! The mod is only required for VR players. You can play with vanilla players just fine too!

### Where can I configure the mod like turning off snap turning?
The VR Mod settings can be accessed with the in-game settings in the "VR" tab. You can also use the mod manager instead while the game is closed. On the left, click on `Config Editor > VRMod > Edit Config`. You can then change the settings to your liking. Once you're done, make sure to save your changes with the `Save` button on the top-right.

### Why play a 3rd person game in VR?
This mod makes you play in first person by default but you can always come back to third person by going in the settings or the config editor. It's honestly not a bad experience!

### I can't press any buttons when I launch the game.
This is likely because the game is not in focus on your PC. Make sure to click on the game window to pull it to the front. If your cursor is stuck in place, you can press the Windows key or Ctrl+Escape to unlock it and click on the game. If that doesn't fix it, relaunching the game should work.

### The game won't launch in VR!
If you are using SteamVR and the game launches in game theatre mode, right click the game on Steam and go to `Properties > General`. You can then turn off the "Use Desktop Game Theatre while SteamVR is active" option. If you're using an Oculus headset, try enabling the "Use Oculus mode" setting in the config editor to bypass SteamVR. Finally, make sure your Steam and game folder are correctly set in the mod manager's settings. If it's still not fixed, ask for help on the [Discord server](https://discord.gg/eQ7Fwac).

### Can I change my controller binds?
You can change your binds using SteamVR's binding system. Note that changing your binds won't change the icons shown in-game. There is currently no way to change your binds when using the Oculus Runtime instead of SteamVR.

### The game is lagging. What can I do to improve performance?
Disabling SSAO and Bloom in the game's settings should improve the performance. Make sure you also don't have too many other applications running in the background. You can also try the [OpenVR FSR mod](https://github.com/fholger/openvr_fsr) which adds AMD's upscaling technology to the game.

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
**Daerst:** Fixed the biggest and oldest bug of this mod. Thank you so much.

# Changelog
### 1.0.0
- Initial release of the mod.

### 1.1.0
- All menus are now visible in VR.
- Enemy health bars are now correctly positioned above enemies.
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

### 2.0.1
- Reduced the size of multiple muzzle flashes and effects.
- New setting: "Hide broken decal textures".
- Added "Survivor Settings" config category:
	- New setting: "Commando: Dual wield".
	- New setting: "Bandit: Weapon grip snap angle".
	- New setting: "Mercenary: Swing speed threshold".
	- New setting: "Loader: Swing speed threshold".
	- New setting: "Acrid: Swing speed threshold".
- The pop-up appearing when selecting a lobby in multiplayer now correctly appears in the headset.
- The black transition screens now correctly appear in the headset.
- The credits now appear in the headset (it is currently stuck on the headset but this will change soon).
- Fixed a bug that caused inputs to not register when no profiles are selected.
- Fixed a bug that caused the profile creation pop-up to be uninteractable.

### 2.1.0
- Added a wrist HUD setting that attaches the health bar, money display and skills to the wrist.
- Added a watch HUD setting that attaches the inventory, chat, difficulty, objective and allies to a watch-like HUD.
- Added a smooth HUD setting that adds smoothing to the camera HUD when moving the headset.
- Added a spectator screen that appears in front of you when spectating players.
- Added "Ray color" and "Ray opacity" settings to customize the aim ray.
- Added more detailed models for Loader's hands, Bandit's shotgun and Bandit's revolver.
- Removed "UI scale" setting as it already exists in-game.
- Removed the center smoke effect on Bandit's stealth ability for improved visibility.
- Possibly fixed a bug that caused the Heretic wings to appear on the wrong player which would break some abilities.
- Fixed a bug that caused Heretic's primary skill projectiles to not appear from the hand after transforming.
- Fixed a bug that caused Vive Cosmos controllers to use the standard Vive controller binds.

### 2.1.1
- The credits no longer stick to the camera.
- The spectator screen is now fully opaque.
- Loader's aim rays have been aligned better with the mech arms.
- Fixed a bug that caused the spectator screen to not appear in multiplayer for non-host players.
- Fixed a bug that caused the shield effect to appear abnormally large on Bandit's new weapon models.
- Fixed a bug that caused MUL-T's left hand animations to break when activating power mode right before transport mode.

### 2.2.0
- Compatibility with the new VR API which adds the possibility of VR compatible mods such as custom characters.
- New hand models for all survivors.
- Equipments, items and body effects that were obstructing vision are now hidden for better visibility.
- Fixed a bug that made bullets and projectiles no longer appear from weapon muzzles after reviving.
- Fixed a bug that made bullets no longer appear from the main weapon's muzzle on Bandit when disabling Commando's dual wield setting.

### 2.2.1
- Fixed a bug that caused the Smooth HUD config to be ineffective.
- Fixed a bug that caused floating equipments to re-appear when teleporting to a new stage.

### 2.3.0
- The scoreboard and the profile menu can now be accessed by holding the menu button.
- Snap turns will now repeat when holding a direction.
- Added a "Snap Turn Hold Delay" setting.
- Added a "Camera Health Bar" setting which puts the health bar at the bottom-middle of the camera HUD for better visibility.
- Added a new EXPERIMENTAL "Roomscale Tracking Space" setting.
- Added a new EXPERIMENTAL "Player Height" setting.
- The aim ray will now activate on the appropriate hand when you have an aimable equipment or heresy skills.
- The Soulbound Catalyst and the Frost Relic no longer appear around the player.
- Removed "Hide broken decal textures" config.
- Fixed a bug that caused decals to only render on the left eye.
- Fixed a bug that caused the camera HUD to freeze in place while paused.
- Fixed a bug that caused parts of the multiplayer menu to not render properly creating an offset.

### 2.4.0
- You can now freely bind your controls with SteamVR.
	- Every single action can be bound to separate inputs.
	- This new system removes the need to download binds for Index Knuckles and Reverb G2 controllers.
	- A few extra bindable actions have been added for these mods: ExtraSkillSlots, VoiceChat, SkillsPlusPlus and ProperSave.
- Added haptic feedback (controller rumble). The intensity can be adjusted with the in-game gamepad setting.
- In-game "cutscenes" that take control of the camera have been improved to reduce risks of nausea and clipping.
- Fixed a bug that caused inputs to break after selecting a profile.
- Fixed a bug that placed and scaled enemy health bars and some indicators incorrectly when using the roomscale tracking setting.

### 2.5.0
- Added the LIV SDK for XR capture support.
	- Only available with SteamVR and with the "Roomscale tracking space" setting enabled.
	- A setting has been added to display the classic HUD on the XR camera.
- All mod configs can now be edited with the in-game settings menu. Some will only be applied on the next stage or after restarting.
- Elements in the intro cutscene and the escape cutscene have been scaled and placed in a more realistic way.
- The grip sensitivity has been reduced on the default Index Knuckles bindings.
- The "Roomscale tracking space" setting is no longer in an experimental stage and is now enabled by default.
- The default value of Loader's melee swing speed threshold has been slightly reduced.
- Reduced the size of the charging Nano-Bomb effect on Artificer's hand for better visibility.
- Fixed a bug that prevented cutscene subtitles to display correctly in VR.
- Fixed a bug that caused the camera to be placed too high in menus when using roomscale tracking.
- Fixed a bug that caused the Visions on Heresy skill to be activated by swinging your controller when equipped on melee survivors.

### 2.5.1
- The pickup notification and the spectator label have been moved up above the central health bar to prevent them from being hidden behind status icons.
- Fixed a bug that caused some setting sliders to parse decimals incorrectly when using certain languages.

### 2.5.2
- Fixed a bug that caused the credits to appear during the escape cutscene.
- Fixed a bug that prevented the LIV plugin from being correctly copied into the game directory during the patching process.

### 2.6.0
- The UI navigation system has been revamped to use your dominant hand as a pointer instead of using gamepad controls.
- The player and character height can now also scale the camera view with the roomscale tracking space setting disabled.
- The mouse can no longer move the camera when using motion controls.
- Fixed a bug that prevented the default Vive Cosmos controller binds from loading correctly which made the Vive Cosmos controllers unusable.
- Fixed a bug that would sometimes break inputs with the Oculus Mode setting enabled.
- Fixed a bug that caused the SteamVR overlay to pause the game which created sync issues in multiplayer.
- Fixed a bug that caused corruption of some menu backgrounds when using LIV XR capture.
- Fixed a bug that caused the hand tracking to be slightly inaccurate when using SteamVR.
- Possibly fixed a bug that prevented the spectator screen from appearing in some occasions.

### 2.6.1
- Fixed a bug that prevented the spectator screen from appearing.
- The spectator screen will now always render on the foreground.
- Fixed a bug that broke some hand animations after using the command or scrapper panel.
- Fixed a bug that prevented the kick message from displaying correctly.
- Fixed a bug that made the buttons in the lobby details panel unclickable.
- Fixed a bug that broke all inputs when no profile has been created.

### 2.6.2
- Fixed a bug that caused some menus to have unreachable buttons near the edges when playing with a lower resolution per eye.
- Fixed a bug that prevented the watch HUD from appearing or disappearing while paused.
- Fixed a bug that caused the spectator camera to have the wrong field of view.
- Fixed a bug that caused Bandit's revolver animation to cancel by mistake when other Bandit players in the lobby would start sprinting.

### 2.6.3
- The momentum direction is now controlled by the non-dominant hand when using Loader's grapple hook.
- A warning now appears when the game window loses focus.
- Fixed a bug that prevented the use of the triggers or the X button to interact with menus on Oculus Touch controllers when Oculus mode is enabled.
- Fixed a bug that showed the wrong control glyphs when playing with Vive or WMR controllers.

### 2.6.4
- VR settings that depend on other settings will no longer get forcibly switched. For example:
	- Turning off first person deactivates motion controls, but the motion controls setting will stay intact so it stays enabled when re-enabling first person.
	- Turning off motion controls means the wrist and watch HUDs cannot be used, but the settings will now stay enabled.
- Fixed a bug that broke controller inputs when the controllers are no longer detected with Oculus mode on.