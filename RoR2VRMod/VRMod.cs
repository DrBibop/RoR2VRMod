using BepInEx;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Management;
using System;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "2.9.2")]
    [BepInDependency("com.Moffein.BanditTweaks", BepInDependency.DependencyFlags.SoftDependency)]
    public class VRMod : BaseUnityPlugin
    {
        internal static ManualLogSource StaticLogger;

        internal static AssetBundle VRAssetBundle;

        private void Awake()
        {
            StaticLogger = Logger;

            VRAssetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vrmodassets);

            ModConfig.Init();
            ActionAddons.Init();
            SettingsAddon.Init();
            UIFixes.Init();
            CameraFixes.Init();
            CutsceneFixes.Init();
            FocusChecker.Init();
            if (ModConfig.InitialMotionControlsValue)
            {
                RoR2.RoR2Application.isModded = true;
                MotionControls.Init();
                MotionControlledAbilities.Init();
                EntityStateAnimationParameter.Init();
            }

            RoR2.RoR2Application.onLoad += () =>
            {
                InitVR();
                RecenterController.Init();
                UIPointer.Init();
                Haptics.HapticsManager.Init();
                RoR2.RoR2Application.onNextUpdate += InitControllers;
            };
        }

        private void InitControllers()
        {
            Controllers.Init();
            ControllerGlyphs.Init();
        }

        private void InitVR()
        {
            var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();

            generalSettings.Manager = managerSettings;

            ((List<XRLoader>)managerSettings.activeLoaders).Clear();

            XRLoader xrLoader = ScriptableObject.CreateInstance<OpenXRLoader>();

            OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;

            managerSettings.m_Loaders.Add(xrLoader);

            managerSettings.InitializeLoaderSync();

            if (managerSettings.activeLoader == null)
            {
                StaticLogger.LogError("Failed to initialize OpenXR Loader. Is the VR headset ready?");
                return;
            }

            managerSettings.StartSubsystems();

            bool init = managerSettings.activeLoader.Initialize();
            bool start = managerSettings.activeLoader.Start();

            if (!init || !start)
            {
                StaticLogger.LogError("Failed to start OpenXR.");
            }

            List<XRInputSubsystem> xrSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(xrSubsystems);

            foreach (XRInputSubsystem xrSubsystem in xrSubsystems)
            {
                xrSubsystem.TrySetTrackingOriginMode(ModConfig.InitialRoomscaleValue ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
            }
        }
    }
}