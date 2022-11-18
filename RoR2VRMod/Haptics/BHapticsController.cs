using Bhaptics.Tact;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VRMod.Haptics
{
    class BHapticsController : GenericHapticsController
    {
        private HapticPlayer hapticPlayer;

        private bool initialized = false;

        private bool shouldPlayHearthBeat = false;

        private Coroutine hearthBeatCoroutine;

        internal BHapticsController()
        {
            try
            {
                hapticPlayer = new HapticPlayer("RiskOfRain2VRMod", "RiskOfRain2VRMod");
                RegisterAllTactFiles();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                VRMod.StaticLogger.LogError("Could not initialize Bhaptics suit!");
            }
        }

        // Written by Astienth/Astien75
        private void RegisterAllTactFiles()
        {
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string assemblyFile = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(assemblyFile);
            string configPath = myPath + "\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    hapticPlayer.RegisterTactFileStr(prefix, tactFileStr);
                }
                catch (Exception e) 
                {
                    VRMod.StaticLogger.LogError("Failed to register tact pattern: " + prefix);
                    Debug.LogException(e);
                }
            }
            initialized = true;
        }

        private void PlayDamagePattern(string pattern, float intensity, float duration)
        {
            hapticPlayer.SubmitRegistered(pattern, new ScaleOption(intensity, duration / 50));
        }

        internal override bool Initialized()
        {
            return initialized;
        }

        internal override void Update(bool isHealthLow)
        {
            if (hearthBeatCoroutine == null && isHealthLow)
                hearthBeatCoroutine = RoR2.RoR2Application.instance.StartCoroutine(HearthBeatCoroutine());

            shouldPlayHearthBeat = isHealthLow;
        }

        internal override void OnHealthSpentOnInteractable(float healthPercent)
        {
            PlayDamagePattern("HearthDamage", 1, healthPercent * 1000);
        }

        internal override void OnFallDamageTaken(float healthPercent)
        {
            PlayDamagePattern(hapticPlayer.IsActive(PositionType.FootL) ? "FallDamage_Feet" : "FallDamage_Vest", 1, healthPercent * 800);
        }

        internal override void OnDirectionalDamageTaken(Vector3 localDirection, float healthPercent)
        {
            Vector3 reverseDirection = -localDirection;
            Vector3 flatDirection = reverseDirection;
            flatDirection.y = 0;
            flatDirection.Normalize();
            float angle = flatDirection == Vector3.zero ? 0f : ((-Vector3.SignedAngle(Vector3.forward, flatDirection, Vector3.up) + 360) % 360);

            ScaleOption scaleOption = new ScaleOption(0.3f + healthPercent * 0.7f, 2 + healthPercent * 14);
            RotationOption rotationOption = new RotationOption(angle, reverseDirection.y / 2);

            hapticPlayer.SubmitRegisteredVestRotation("Damage", "Damage", rotationOption, scaleOption);
        }

        internal override void OnEnvironmentDamageTaken(float healthPercent)
        {
            PlayDamagePattern("HearthDamage", 0.5f, healthPercent * 1000);
        }

        internal override void OnDeath()
        {
            hapticPlayer.SubmitRegistered("Death_Vest");

            if (hapticPlayer.IsActive(PositionType.ForearmL))
                hapticPlayer.SubmitRegistered("Death_Arms");
            if (hapticPlayer.IsActive(PositionType.HandL))
                hapticPlayer.SubmitRegistered("Death_Hands");
            if (hapticPlayer.IsActive(PositionType.FootL))
                hapticPlayer.SubmitRegistered("Death_Feet");
        }

        private IEnumerator HearthBeatCoroutine()
        {
            do
            {
                hapticPlayer.SubmitRegistered("Heartbeat");

                yield return new WaitForSeconds(0.75f);
            }
            while (shouldPlayHearthBeat);

            hearthBeatCoroutine = null;
        }
    }
}
