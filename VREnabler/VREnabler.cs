using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VRPatcher
{
    //Class by MrPurple, adapted by DrBibop

    /// <summary>
    /// A patcher which runs ahead of UnityPlayer to enable VR in the Global Game Manager.
    /// </summary>
    public static class VREnabler
    {
        internal static string VRPatcherPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string ManagedPath => Paths.ManagedPath;
        internal static string PluginsPath => Path.Combine(VREnabler.ManagedPath, "../Plugins");
        internal static string SteamVRPath => Path.Combine(VREnabler.ManagedPath, "../StreamingAssets/SteamVR");

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("VREnabler");
        

        /// <summary>
        /// Called from BepInEx while patching, our entry point for patching.
        /// Do not change the method name as it is identified by BepInEx. Method must remain public.
        /// </summary>
        [Obsolete("Should not be used!", true)]
        public static void Initialize()
        {
            if (!VREnabler.EnableVROptions(Path.Combine(VREnabler.ManagedPath, "../globalgamemanagers")))
            {
                return;
            }
            VREnabler.Logger.LogInfo("Checking for VR plugins...");
            string pluginsPath = Path.Combine(VREnabler.PluginsPath, "x86_64");
            if (!Directory.Exists(pluginsPath))
            {
                pluginsPath = VREnabler.PluginsPath;
            }

            string[] plugins = new string[]
            {
                "AudioPluginOculusSpatializer.dll",
                "openvr_api.dll",
                "OVRGamepad.dll",
                "OVRPlugin.dll",
                "LIV_Bridge.dll",
                "ShockWaveIMU.dll",
                "bhaptics_library.dll"
            };
            string[] managedLibraries = new string[]
            {
                "SteamVR.dll",
                "SteamVR_Actions.dll",
                "ShockwaveManager.dll",
                "BHaptics.dll"
            };

            bool copyPluginsResult = CopyFiles(pluginsPath, plugins, "Plugins.");
            bool copyManagedLibrariesResult = CopyFiles(VREnabler.ManagedPath, managedLibraries, "Plugins.");

            if (copyPluginsResult || copyManagedLibrariesResult)
                VREnabler.Logger.LogInfo("Successfully copied VR plugins!");
            else
                VREnabler.Logger.LogInfo("VR plugins already present");

            VREnabler.Logger.LogInfo("Checking for binding files...");


            if (!Directory.Exists(SteamVRPath))
            {
                try
                {
                    Directory.CreateDirectory(SteamVRPath);
                }
                catch (Exception e)
                {
                    VREnabler.Logger.LogError("Could not create SteamVR folder in StreamingAssets: " + e.Message);
                    VREnabler.Logger.LogError(e.StackTrace);
                    return;
                }
            }

            string[] bindingFiles = new string[]
            {
                "actions.json",
                "binding_holographic_hmd.json",
                "binding_index_hmd.json",
                "binding_rift.json",
                "binding_vive.json",
                "binding_vive_cosmos.json",
                "binding_vive_pro.json",
                "binding_vive_tracker_camera.json",
                "bindings_holographic_controller.json",
                "bindings_knuckles.json",
                "bindings_logitech_stylus.json",
                "bindings_oculus_touch.json",
                "bindings_vive_controller.json",
                "bindings_vive_cosmos_controller.json"
            };

            if (CopyFiles(SteamVRPath, bindingFiles, "Binds.", true))
                VREnabler.Logger.LogInfo("Successfully copied binding files!");
            else
                VREnabler.Logger.LogInfo("Binding files already present");
        }

        private static bool EnableVROptions(string path)
        {
            AssetsManager assetsManager = new AssetsManager();
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(path, false, "");
            assetsManager.LoadClassDatabase(Path.Combine(VREnabler.VRPatcherPath, "cldb.dat"));
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
                                    File.WriteAllBytes(path, memoryStream2.ToArray());
                                }
                            }
                            return true;
                        }
                    }
                }
                catch
                {
                }
                num++;
            }
            VREnabler.Logger.LogError("VR enable location not found!");
            return false;
        }

        private static bool CopyFiles(string destinationPath, string[] fileNames, string embedFolder, bool replaceIfDifferent = false)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(destinationPath);
            FileInfo[] files = directoryInfo.GetFiles();
            bool flag = false;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string name = executingAssembly.GetName().Name;
            string[] array = fileNames;
            for (int i = 0; i < array.Length; i++)
            {
                string fileName = array[i];
                if (!Array.Exists<FileInfo>(files, (FileInfo file) => fileName == file.Name))
                {
                    flag = true;
                    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name + "." + embedFolder + fileName))
                    {
                        using (FileStream fileStream = new FileStream(Path.Combine(directoryInfo.FullName, fileName), FileMode.Create, FileAccess.ReadWrite, FileShare.Delete))
                        {
                            VREnabler.Logger.LogInfo("Copying " + fileName);
                            manifestResourceStream.CopyTo(fileStream);
                        }
                    }
                }
                else if (replaceIfDifferent)
                {
                    string resourceFileContent;
                    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name + "." + embedFolder + fileName))
                    {
                        using (StreamReader reader = new StreamReader(manifestResourceStream))
                        {
                            resourceFileContent = reader.ReadToEnd();
                        }
                    }

                    FileInfo installedFile = files.First(file => file.Name == fileName);
                    string installedFileContent = File.ReadAllText(@installedFile.FullName);

                    if (resourceFileContent != installedFileContent)
                    {
                        flag = true;
                        VREnabler.Logger.LogInfo("Overwriting " + fileName);
                        File.WriteAllText(installedFile.FullName, resourceFileContent);
                    }
                }
            }
            return flag;
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
