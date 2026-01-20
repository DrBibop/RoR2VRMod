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
using UnityEngine.XR.OpenXR.Features;

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

            SetupControllerProfiles();

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

        private void SetupControllerProfiles()
        {
            StaticLogger.LogInfo("SetupControllerProfiles: Setting up OpenXR controller profiles...");
            try
            {
                var openXRSettings = OpenXRSettings.Instance;

                if (openXRSettings.features.Length > 0) 
                {
                    StaticLogger.LogInfo($"SetupControllerProfiles: Features already configured ({openXRSettings.features.Length} features)");
                    return;
                }

                // Try to create controller profiles
                var features = new List<OpenXRFeature>();

                Type[] profileTypes = {
                    typeof(UnityEngine.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile),
                    typeof(UnityEngine.XR.OpenXR.Features.Interactions.ValveIndexControllerProfile),
                    typeof(UnityEngine.XR.OpenXR.Features.Interactions.HTCViveControllerProfile),
                    typeof(UnityEngine.XR.OpenXR.Features.Interactions.MicrosoftMotionControllerProfile),
                    typeof(UnityEngine.XR.OpenXR.Features.Interactions.KHRSimpleControllerProfile)
                };

                foreach (var profileType in profileTypes)
                {
                    try
                    {
                        var profile = (OpenXRFeature)ScriptableObject.CreateInstance(profileType);
                        profile.enabled = true;
                        features.Add(profile);
                        StaticLogger.LogInfo($"  Created profile: {profileType.Name}");
                    }
                    catch (Exception e)
                    {
                        StaticLogger.LogWarning($"  Failed to create {profileType.Name}: {e.Message}");
                    }
                }

                if (features.Count > 0)
                {
                    openXRSettings.features = features.ToArray();
                    StaticLogger.LogInfo($"SetupControllerProfiles: Added {features.Count} controller profiles");
                }
                else
                {
                    StaticLogger.LogWarning("SetupControllerProfiles: No controller profiles could be created");
                }
            }
            catch (Exception e)
            {
                StaticLogger.LogError($"SetupControllerProfiles failed: {e}");
            }
        }
    }
}