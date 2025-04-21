using BepInEx;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;
using Unity.XR.Oculus;
using System;
using UnityEngine.XR;

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
                RoR2.RoR2Application.onNextUpdate += InitSteamVR;
                RoR2.RoR2Application.onNextUpdate += InitControllers;
            };
        }

        private void InitSteamVR()
        {
            if (ModConfig.InitialOculusModeValue) return;

            SteamVR_Settings.instance.trackingSpace = ModConfig.InitialRoomscaleValue ? ETrackingUniverseOrigin.TrackingUniverseStanding : ETrackingUniverseOrigin.TrackingUniverseSeated;
            SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
            SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;
            SteamVR.Initialize();
            SteamVR_Input.IdentifyActionsFile();
            SteamVR_Actions.gameplay.Activate();
            SteamVR_Actions.ui.Activate();
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

            XRLoader xrLoader = null;

            if (ModConfig.InitialOculusModeValue)
            {
                xrLoader = ScriptableObject.CreateInstance<OculusLoader>();
                managerSettings.m_Loaders.Add(xrLoader);

                OculusSettings.s_Settings = ScriptableObject.CreateInstance<OculusSettings>();
                OculusSettings.s_Settings.m_StereoRenderingModeDesktop = OculusSettings.StereoRenderingModeDesktop.MultiPass;
                OculusSettings.s_Settings.DepthSubmission = false;

                NativeMethods.LoadOVRPlugin("");
                generalSettings.InitXRSDK();
                generalSettings.StartXRSDK();
            }
            else
            {
                xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();
                managerSettings.m_Loaders.Add(xrLoader);

                OpenVRSettings.s_Settings = ScriptableObject.CreateInstance<OpenVRSettings>();
                OpenVRSettings.s_Settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;
                OpenVRSettings.s_Settings.InitializationType = OpenVRSettings.InitializationTypes.Scene;

                generalSettings.InitXRSDK();
                generalSettings.StartXRSDK();
            }

            List<XRInputSubsystem> xrSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(xrSubsystems);

            foreach (XRInputSubsystem xrSubsystem in xrSubsystems)
            {
                xrSubsystem.TrySetTrackingOriginMode(ModConfig.InitialRoomscaleValue ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
            }

            /*
            XRSettings.LoadDeviceByName(useOculus ? "Oculus" : "OpenVR");
            yield return null;
            if (XRSettings.loadedDeviceName != (useOculus ? "Oculus" : "OpenVR")) yield break;
            
            XRSettings.enabled = true;
            List<XRInputSubsystem> xrSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(xrSubsystems);

            foreach (XRInputSubsystem xrSubsystem in xrSubsystems)
            {
                xrSubsystem.TrySetTrackingOriginMode(ModConfig.InitialRoomscaleValue ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
            }

            if (!useOculus)
            {
                //SteamVR_Settings.instance.trackingSpace = ModConfig.InitialRoomscaleValue ? ETrackingUniverseOrigin.TrackingUniverseStanding : ETrackingUniverseOrigin.TrackingUniverseSeated;
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
                SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;
                SteamVR.Initialize();
                SteamVR_Actions.gameplay.Activate();
                SteamVR_Actions.ui.Activate();
            }
            Controllers.Init();
            ControllerGlyphs.Init();*/
        }
    }
}