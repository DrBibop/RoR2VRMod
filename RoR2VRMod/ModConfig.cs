using BepInEx;
using BepInEx.Configuration;

namespace VRMod
{
    internal static class ModConfig
    {
        private const string CONFIG_FILE_NAME = "VRMod.cfg";

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME), true);
        internal static ConfigEntry<bool> ConfigUseOculus { get; private set; }

        internal static void Init()
        {
            ConfigUseOculus = configFile.Bind<bool>(
                "VR Settings",
                "Use Oculus mode",
                false,
                "Launches the game in Oculus mode if you don't like using SteamVR."
            );
        }
    }
}
