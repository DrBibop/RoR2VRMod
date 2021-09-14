using BepInEx;
using BepInEx.Configuration;
using System.Globalization;
using UnityEngine;

namespace VRMod
{
    public static class ModConfig
    {
        private const string CONFIG_FILE_NAME = "VRMod.cfg";

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME), true);
        internal static ConfigEntry<bool> OculusMode { get; private set; }
        internal static ConfigEntry<bool> FirstPerson { get; private set; }
        internal static ConfigEntry<bool> ConfortVignette { get; private set; }
        internal static ConfigEntry<bool> Roomscale { get; private set; }
        internal static ConfigEntry<float> PlayerHeight { get; private set; }

        internal static ConfigEntry<string> RayColorHex { get; private set; }
        internal static ConfigEntry<float> RayOpacity { get; private set; }
        internal static ConfigEntry<bool> CommandoDualWield { get; private set; }
        internal static ConfigEntry<float> BanditWeaponGripSnapAngle { get; private set; }
        internal static ConfigEntry<float> MercSwingSpeedThreshold { get; private set; }
        internal static ConfigEntry<float> LoaderSwingSpeedThreshold { get; private set; }
        internal static ConfigEntry<float> AcridSwingSpeedThreshold { get; private set; }
        internal static Color RayColor = Color.white;

        internal static ConfigEntry<bool> WristHUD { get; private set; }
        internal static ConfigEntry<bool> WatchHUD { get; private set; }
        internal static ConfigEntry<bool> BetterHealthBar { get; private set; }
        internal static ConfigEntry<bool> SmoothHUD { get; private set; }
        internal static ConfigEntry<int> HUDWidth { get; private set; }
        internal static ConfigEntry<int> HUDHeight { get; private set; }
        internal static ConfigEntry<float> BottomAnchor { get; private set; }
        internal static ConfigEntry<float> TopAnchor { get; private set; }
        internal static ConfigEntry<float> LeftAnchor { get; private set; }
        internal static ConfigEntry<float> RightAnchor { get; private set; }
        internal static Vector2 AnchorMin { get; private set; }
        internal static Vector2 AnchorMax { get; private set; }

        internal static ConfigEntry<bool> SnapTurn { get; private set; }
        internal static ConfigEntry<float> SnapTurnAngle { get; private set; }
        internal static ConfigEntry<float> SnapTurnHoldDelay { get; private set; }
        internal static ConfigEntry<bool> LockedCameraPitch { get; private set; }
        internal static ConfigEntry<bool> UseMotionControls { get; private set; }
        internal static ConfigEntry<bool> LeftDominantHand { get; private set; }
        internal static ConfigEntry<bool> ControllerMovementDirection { get; private set; }

        internal static void Init()
        {
            OculusMode = configFile.Bind<bool>(
                "VR Settings",
                "Use Oculus mode",
                false,
                "Launches the game in Oculus mode if you don't like using SteamVR."
            );
            FirstPerson = configFile.Bind<bool>(
                "VR Settings",
                "First Person",
                true,
                "Experience the game in a first person POV."
            );
            ConfortVignette = configFile.Bind<bool>(
                "VR Settings",
                "Confort Vignette",
                true,
                "Adds a black vignette during high-mobility abilities to reduce motion sickness."
            );
            Roomscale = configFile.Bind<bool>(
                "VR Settings",
                "Roomscale Tracking Space",
                false,
                "EXPERIMENTAL: Changes the tracking space to roomscale. Your real height will be used in-game to scale the view properly. This should also fix a rare gray screen glitch."
            );
            PlayerHeight = configFile.Bind<float>(
                "VR Settings",
                "Player Height in meters",
                1.82f,
                "EXPERIMENTAL: Used for roomscale tracking. Your view scale will be adjusted to make you feel as tall as the survivor you're playing. Most survivors have a height of 1.82 meters which means keeping the default value will keep your view scale multiplier at 1 on most survivors"
            );

            RayColorHex = configFile.Bind<string>(
                "Survivor Settings",
                "General: Aim ray hex color",
                "FFFFFF",
                "Changes the color of aim rays. You can use Google's color picker to find your desired color's hex value."
            );
            RayOpacity = configFile.Bind<float>(
                "Survivor Settings",
                "General: Aim ray opacity",
                0.6f,
                "Sets the aim ray opacity between 0 (invisible) and 1 (opaque)."
            );
            CommandoDualWield = configFile.Bind<bool>(
                "Survivor Settings",
                "Commando: Dual wield",
                true,
                "TRUE: Double Tap and Phase Blast alternate between the left and right pistol for each bullet. FALSE: Double Tap only uses the dominant pistol while Phase Blast uses the non-dominant pistol."
            );
            BanditWeaponGripSnapAngle = configFile.Bind<float>(
                "Survivor Settings",
                "Bandit: Weapon grip snap angle",
                40,
                "Angle in which the non-dominant hand can grip the weapon."
            );
            MercSwingSpeedThreshold = configFile.Bind<float>(
                "Survivor Settings",
                "Mercenary: Swing speed threshold",
                22,
                "The sword tip speed required to trigger an attack."
            );
            LoaderSwingSpeedThreshold = configFile.Bind<float>(
                "Survivor Settings",
                "Loader: Swing speed threshold",
                18,
                "The mech fist speed required to trigger an attack."
            );
            AcridSwingSpeedThreshold = configFile.Bind<float>(
                "Survivor Settings",
                "Acrid: Swing speed threshold",
                12,
                "The claw tip speed required to trigger an attack."
            );

            string hexString = RayColorHex.Value;

            if (hexString.StartsWith("#") && hexString.Length > 1)
            {
                hexString = hexString.Substring(1);
            }

            if (hexString.Length == 6)
            {
                string[] hexValues = new string[]
                {
                    hexString.Substring(0, 2),
                    hexString.Substring(2, 2),
                    hexString.Substring(4, 2)
                };

                for (int i = 0; i < 3; i++)
                {
                    int colorChannel = 255;
                    if (int.TryParse(hexValues[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out colorChannel))
                    {
                        switch (i)
                        {
                            case 0:
                                RayColor.r = (float)colorChannel / 255f;
                                break;
                            case 1:
                                RayColor.g = (float)colorChannel / 255f;
                                break;
                            case 2:
                                RayColor.b = (float)colorChannel / 255f;
                                break;
                        }
                    }
                }
            }

            RayColor.a = RayOpacity.Value;

            WristHUD = configFile.Bind<bool>(
                "HUD Settings",
                "Wrist HUD",
                true,
                "TRUE: Attaches the healthbar, money and cooldowns to the wrists. FALSE: Attaches the healthbar, money and cooldowns to the camera."
            );
            WatchHUD = configFile.Bind<bool>(
                "HUD Settings",
                "Watch HUD",
                true,
                "TRUE: Attaches the inventory, chat, objective and allies to a watch that appears when looking at it. FALSE: Attaches the inventory, chat, objective and allies to the camera."
            );
            BetterHealthBar = configFile.Bind<bool>(
                "HUD Settings",
                "Camera Health Bar",
                true,
                "TRUE: Makes the health bar more visible by placing it on the bottom-middle of the camera HUD. FALSE: Places the healthbar on the left wrist HUD (if the wrist HUD setting is enabled)."
            );
            SmoothHUD = configFile.Bind<bool>(
                "HUD Settings",
                "Smooth HUD",
                true,
                "TRUE: The camera HUD will follow the camera smoothly making it lag behind when moving the headset. FALSE: The camera HUD will follow the camera directly without smoothing."
            );
            HUDWidth = configFile.Bind<int>(
                "HUD Settings",
                "HUD Width",
                1200,
                "Width of the camera HUD."
            );
            HUDHeight = configFile.Bind<int>(
                "HUD Settings",
                "HUD Height",
                1000,
                "Height of the camera HUD."
            );
            BottomAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Bottom anchor",
                1f,
                "Position of the bottom anchor between 0 and 1 (Middle to bottom edge of the screen)."
            );
            TopAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Top anchor",
                0.7f,
                "Position of the top anchor between 0 and 1 (Middle to top edge of the screen)."
            );
            LeftAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Left anchor",
                1f,
                "Position of the left anchor between 0 and 1 (Middle to left edge of the screen)."
            );
            RightAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Right anchor",
                1f,
                "Position of the right anchor between 0 and 1 (Middle to right edge of the screen)."
            );

            AnchorMin = new Vector2((1 - Mathf.Clamp(LeftAnchor.Value, 0, 1)) / 2, (1 - Mathf.Clamp(BottomAnchor.Value, 0, 1)) / 2);
            AnchorMax = new Vector2((1 + Mathf.Clamp(RightAnchor.Value, 0, 1)) / 2, (1 + Mathf.Clamp(TopAnchor.Value, 0, 1)) / 2);

            SnapTurn = configFile.Bind<bool>(
                "Controls",
                "Snap turn",
                true,
                "TRUE: Rotate the camera in increments (unavailable in third person).  FALSE: Smooth camera rotation."
            );
            SnapTurnAngle = configFile.Bind<float>(
                "Controls",
                "Snap turn angle",
                45,
                "Rotation in degrees of each snap turn."
            );
            SnapTurnHoldDelay = configFile.Bind<float>(
                "Controls",
                "Snap turn hold delay",
                0.33f,
                "Time in seconds between each snap turn when holding a direction."
            );

            LockedCameraPitch = configFile.Bind<bool>(
                "Controls",
                "Locked camera pitch",
                true,
                "Prevents the camera from rotating vertically (cannot disable when snap turn is on)."
            );
            UseMotionControls = configFile.Bind<bool>(
                "Controls",
                "Use motion controls",
                true,
                "Enables motion controls for VR controllers. They act as a simple gamepad when set to false."
            );
            LeftDominantHand = configFile.Bind<bool>(
                "Controls",
                "Set left hand as dominant",
                false,
                "Swaps left/right triggers and grips. The aiming hand for each skill is also swapped as well as hand models."
            );
            ControllerMovementDirection = configFile.Bind<bool>(
                "Controls",
                "Use controller direction for movement",
                false,
                "When enabled, pushing forward on the joystick will move the character towards the direction the controller is pointing instead of the head."
            );

            if (!FirstPerson.Value)
                UseMotionControls.Value = false;

            if (SnapTurn.Value || UseMotionControls.Value)
                LockedCameraPitch.Value = true;

            if (!UseMotionControls.Value)
            {
                WristHUD.Value = false;
                WatchHUD.Value = false;
            }
        }

        public static bool MotionControlsEnabled => UseMotionControls.Value;
        public static bool LeftHanded => LeftDominantHand.Value;
    }
}
