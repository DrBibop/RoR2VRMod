using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using BepInEx.Configuration;
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
            SetupVR(true);
        }

        public static void SetupVR(bool enabled = true)
        {
            SetupVR(Path.Combine(ManagedPath, "../globalgamemanagers"), Path.Combine(ManagedPath, "../Plugins"), enabled);
        }

        private static void SetupVR(string assetsPath, string pluginsPath, bool enabled = true)
        {
            AssetsManager assetsManager = new AssetsManager();
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(assetsPath, false, "");
            assetsManager.LoadClassDatabase(Path.Combine(VREnablerPath, "cldb.dat"));
            int num = 0;
            while ((long)num < (long)((ulong)assetsFileInstance.table.assetFileInfoCount))
            {
                try
                {
                    AssetFileInfoEx assetInfo = assetsFileInstance.table.GetAssetInfo((long)num);
                    AssetTypeInstance ati = assetsManager.GetATI(assetsFileInstance.file, assetInfo, false);
                    AssetTypeValueField assetTypeValueField = (ati != null) ? ati.GetBaseField(0) : null;
                    AssetTypeValueField assetTypeValueField2 = (assetTypeValueField != null) ? assetTypeValueField.Get("enabledVRDevices") : null;
                    if (assetTypeValueField2 != null)
                    {
                        AssetTypeValueField assetTypeValueField3 = assetTypeValueField2.Get("Array");
                        if (assetTypeValueField3 != null)
                        {
                            AssetTypeValueField assetTypeValueField4 = ValueBuilder.DefaultValueFieldFromArrayTemplate(assetTypeValueField3);
                            assetTypeValueField4.GetValue().Set("Oculus");
                            AssetTypeValueField assetTypeValueField5 = ValueBuilder.DefaultValueFieldFromArrayTemplate(assetTypeValueField3);
                            assetTypeValueField5.GetValue().Set("OpenVR");
                            AssetTypeValueField assetTypeValueField6 = ValueBuilder.DefaultValueFieldFromArrayTemplate(assetTypeValueField3);
                            assetTypeValueField6.GetValue().Set("None");
                            assetTypeValueField3.SetChildrenList(new AssetTypeValueField[]
                            {
                                assetTypeValueField6,
                                assetTypeValueField4,
                                assetTypeValueField5
                            });
                            byte[] array;
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                using (AssetsFileWriter assetsFileWriter = new AssetsFileWriter(memoryStream))
                                {
                                    assetsFileWriter.bigEndian = false;
                                    AssetWriters.Write(assetTypeValueField, assetsFileWriter, 0);
                                    array = memoryStream.ToArray();
                                }
                            }
                            List<AssetsReplacer> list = new List<AssetsReplacer>
                            {
                                new AssetsReplacerFromMemory(0, (long)num, (int)assetInfo.curFileType, ushort.MaxValue, array)
                            };
                            using (MemoryStream memoryStream2 = new MemoryStream())
                            {
                                using (AssetsFileWriter assetsFileWriter2 = new AssetsFileWriter(memoryStream2))
                                {
                                    assetsFileInstance.file.Write(assetsFileWriter2, 0UL, list, 0U, null);
                                    assetsFileInstance.stream.Close();
                                    File.WriteAllBytes(assetsPath, memoryStream2.ToArray());
                                }
                            }
                            return;
                        }
                    }

                    Logger.LogInfo("VR Mod setup complete!");

                    if (!enabled)
                        return;

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
                    return;
                }
                catch { }
                num++;
            }

            Logger.LogError("VR enable location not found!");

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
