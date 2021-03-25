using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VRPatcher
{
    //Class by MrPurple, adapted by DrBibop

    /// <summary>
    /// A patcher which runs ahead of UnityPlayer to enable VR in the Global Game Manager.
    /// </summary>
    public static class VREnabler
    {
        internal static string VREnablerPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string GameRootPath => Paths.GameRootPath;
        internal static string ManagedPath => Paths.ManagedPath;

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("VREnabler");
        

        /// <summary>
        /// Called from BepInEx while patching, our entry point for patching.
        /// Do not change the method name as it is identified by BepInEx. Method must remain public.
        /// </summary>
        [Obsolete("Should not be used!", true)]
        public static void Initialize()
        {
            SetupVR(Path.Combine(ManagedPath, "../Plugins"));
        }

        private static void SetupVR(string pluginsPath)
        {
              Logger.LogInfo("Checking for VR plugins...");

              DirectoryInfo gamePluginsDirectory = new DirectoryInfo(pluginsPath);

              string[] pluginNames = new string[]
              {
                  "AudioPluginOculusSpatializer.dll",
                  "openvr_api.dll",
                  "OVRGamepad.dll",
                  "OVRPlugin.dll"
              };

              FileInfo[] gamePluginFiles = gamePluginsDirectory.GetFiles();

              bool hasCopied = false;

              foreach (string pluginName in pluginNames)
              {
                  if (!Array.Exists<FileInfo>(gamePluginFiles, (file) => pluginName == file.Name))
                  {
                      hasCopied = true;
                      using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("VREnabler.VRPlugins." + pluginName))
                      {
                          using (var file = new FileStream(Path.Combine(gamePluginsDirectory.FullName, pluginName), FileMode.Create, FileAccess.Write, FileShare.Delete))
                          {
                              Logger.LogInfo("Copying " + pluginName);
                              resource.CopyTo(file);
                          }
                      }
                  }
              }

              if (hasCopied)
                  Logger.LogInfo("Successfully copied VR plugins!");
              else
                  Logger.LogInfo("VR plugins already present");
        }

        /// <summary>
        /// For BepInEx to identify your patcher as a patcher, it must match the patcher contract as outlined in the BepInEx docs:
        /// https://bepinex.github.io/bepinex_docs/v5.0/articles/dev_guide/preloader_patchers.html#patcher-contract
        /// It must contain a list of managed assemblies to patch as a public static <see cref="IEnumerable{T}"/> property named TargetDLLs
        /// </summary>
        [Obsolete("Should not be used!", true)]
        public static IEnumerable<string> TargetDLLs { get; } = new string[0];

        /// <summary>
        /// For BepInEx to identify your patcher as a patcher, it must match the patcher contract as outlined in the BepInEx docs:
        /// https://bepinex.github.io/bepinex_docs/v5.0/articles/dev_guide/preloader_patchers.html#patcher-contract
        /// It must contain a public static void method named Patch which receives an <see cref="AssemblyDefinition"/> argument,
        /// which patches each of the target assemblies in the TargetDLLs list.
        /// 
        /// We don't actually need to patch any of the managed assemblies, so we are providing an empty method here.
        /// </summary>
        /// <param name="ad"></param>
        [Obsolete("Should not be used!", true)]
        public static void Patch(AssemblyDefinition ad) { }
    }
}
