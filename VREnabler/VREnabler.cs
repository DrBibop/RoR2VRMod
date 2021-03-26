using AssetsTools.NET;
using AssetsTools.NET.Extra;
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
        internal static string VRPatcherPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string ManagedPath => Paths.ManagedPath;
		internal static string PluginsPath => Path.Combine(VREnabler.ManagedPath, "../Plugins");

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
			string path = Path.Combine(VREnabler.PluginsPath, "x86_64");
			DirectoryInfo directoryInfo;
			if (!Directory.Exists(path))
			{
				directoryInfo = new DirectoryInfo(VREnabler.PluginsPath);
			}
			else
			{
				directoryInfo = new DirectoryInfo(path);
			}
			string[] array = new string[]
			{
				"AudioPluginOculusSpatializer.dll",
				"openvr_api.dll",
				"OVRGamepad.dll",
				"OVRPlugin.dll"
			};
			FileInfo[] files = directoryInfo.GetFiles();
			bool flag = false;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string name = executingAssembly.GetName().Name;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string pluginName = array2[i];
				if (!Array.Exists<FileInfo>(files, (FileInfo file) => pluginName == file.Name))
				{
					flag = true;
					using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name + ".Plugins." + pluginName))
					{
						using (FileStream fileStream = new FileStream(Path.Combine(directoryInfo.FullName, pluginName), FileMode.Create, FileAccess.Write, FileShare.Delete))
						{
							VREnabler.Logger.LogInfo("Copying " + pluginName);
							manifestResourceStream.CopyTo(fileStream);
						}
					}
				}
			}
			if (flag)
			{
				VREnabler.Logger.LogInfo("Successfully copied VR plugins!");
				return;
			}
			VREnabler.Logger.LogInfo("VR plugins already present");
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000221C File Offset: 0x0000041C
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
