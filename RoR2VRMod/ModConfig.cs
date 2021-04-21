using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace VRMod
{
    internal static class ModConfig
    {
        private const string CONFIG_FILE_NAME = "VRMod.cfg";

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME), true);
        internal static ConfigEntry<bool> ConfigUseOculus { get; private set; }
        internal static ConfigEntry<bool> FirstPerson { get; private set; }

        internal static ConfigEntry<float> UIScale { get; private set; }
        internal static ConfigEntry<float> BottomAnchor { get; private set; }
        internal static ConfigEntry<float> TopAnchor { get; private set; }
        internal static ConfigEntry<float> LeftAnchor { get; private set; }
        internal static ConfigEntry<float> RightAnchor { get; private set; }
        internal static Vector2 AnchorMin { get; private set; }
        internal static Vector2 AnchorMax { get; private set; }

        internal static ConfigEntry<bool> SnapTurn { get; private set; }
        internal static ConfigEntry<float> SnapTurnAngle { get; private set; }
        internal static ConfigEntry<bool> LockedCameraPitch { get; private set; }

        internal static void Init()
        {
            ConfigUseOculus = configFile.Bind<bool>(
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

            UIScale = configFile.Bind<float>(
                "HUD Settings",
                "UI scale",
                0.8f,
                "Scale of UI elements in the HUD."
            );

            BottomAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Bottom anchor",
                0.5f,
                "Position of the bottom anchor between 0 and 1 (Middle to bottom edge of the screen)."
            );
            TopAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Top anchor",
                0.3f,
                "Position of the top anchor between 0 and 1 (Middle to top edge of the screen)."
            );
            LeftAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Left anchor",
                0.5f,
                "Position of the left anchor between 0 and 1 (Middle to left edge of the screen)."
            );
            RightAnchor = configFile.Bind<float>(
                "HUD Settings",
                "Right anchor",
                0.5f,
                "Position of the right anchor between 0 and 1 (Middle to right edge of the screen)."
            );

            AnchorMin = new Vector2((1 - Mathf.Clamp(LeftAnchor.Value, 0, 1)) / 2, (1 - Mathf.Clamp(BottomAnchor.Value, 0, 1)) / 2);
            AnchorMax = new Vector2((1 + Mathf.Clamp(RightAnchor.Value, 0, 1)) / 2, (1 + Mathf.Clamp(TopAnchor.Value, 0, 1)) / 2);

            SnapTurn = configFile.Bind<bool>(
                "Controls",
                "Snap turn",
                false,
                "TRUE: Rotate the camera in increments (unavailable in third person).  FALSE: Smooth camera rotation."
            );
            SnapTurnAngle = configFile.Bind<float>(
                "Controls",
                "Snap turn angle",
                45,
                "Rotation in degrees of each snap turn."
            );

            LockedCameraPitch = configFile.Bind<bool>(
                "Controls",
                "Locked camera pitch",
                true,
                "Prevents the camera from rotating vertically (cannot disable when snap turn is on)."
            );
        }
    }
}
