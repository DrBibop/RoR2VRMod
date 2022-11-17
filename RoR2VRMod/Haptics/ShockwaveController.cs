using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRMod.Haptics
{
    class ShockwaveController : GenericHapticsController
    {
        private bool shouldPlayHearthBeat = false;

        private Coroutine hearthBeatCoroutine;

        private static KeyValuePair<Vector3, ShockwaveManager.HapticGroup>[] directionToHapticGroup = new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>[]
        {
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.2f, 1f, -0.2f).normalized, ShockwaveManager.HapticGroup.RIGHT_HIP_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.2f, 1f, 0.2f).normalized, ShockwaveManager.HapticGroup.RIGHT_HIP_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.2f, 1f, -0.2f).normalized, ShockwaveManager.HapticGroup.LEFT_HIP_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.2f, 1f, 0.2f).normalized, ShockwaveManager.HapticGroup.LEFT_HIP_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.6f, 0.5f, -0.6f).normalized, ShockwaveManager.HapticGroup.RIGHT_WAIST_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.6f, 0.5f, 0.6f).normalized, ShockwaveManager.HapticGroup.RIGHT_WAIST_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.6f, 0.5f, -0.6f).normalized, ShockwaveManager.HapticGroup.LEFT_WAIST_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.6f, 0.5f, 0.6f).normalized, ShockwaveManager.HapticGroup.LEFT_WAIST_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-1f, 0f, -1f).normalized, ShockwaveManager.HapticGroup.RIGHT_SPINE_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-1f, 0f, 1f).normalized, ShockwaveManager.HapticGroup.RIGHT_SPINE_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(1f, 0f, -1f).normalized, ShockwaveManager.HapticGroup.LEFT_SPINE_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(1f, 0f, 1f).normalized, ShockwaveManager.HapticGroup.LEFT_SPINE_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.6f, -0.5f, -0.6f).normalized, ShockwaveManager.HapticGroup.RIGHT_CHEST_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.6f, -0.5f, 0.6f).normalized, ShockwaveManager.HapticGroup.RIGHT_CHEST_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.6f, -0.5f, -0.6f).normalized, ShockwaveManager.HapticGroup.LEFT_CHEST_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.6f, -0.5f, 0.6f).normalized, ShockwaveManager.HapticGroup.LEFT_CHEST_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.2f, -1f, -0.2f).normalized, ShockwaveManager.HapticGroup.RIGHT_SHOULDER_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(-0.2f, -1f, 0.2f).normalized, ShockwaveManager.HapticGroup.RIGHT_SHOULDER_BACK),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.2f, -1f, -0.2f).normalized, ShockwaveManager.HapticGroup.LEFT_SHOULDER_FRONT),
            new KeyValuePair<Vector3, ShockwaveManager.HapticGroup>(new Vector3(0.2f, -1f, 0.2f).normalized, ShockwaveManager.HapticGroup.LEFT_SHOULDER_BACK)
        };

        internal ShockwaveController()
        {
            ShockwaveManager.Instance.enableBodyTracking = false;
            ShockwaveManager.Instance.InitializeSuit();

            if (!ShockwaveManager.Instance.Ready)
            {
                VRMod.StaticLogger.LogError("Could not initialize Shockwave suit!");
            }
        }

        internal override bool Initialized()
        {
            return ShockwaveManager.Instance.Ready;
        }

        internal override void Update(bool isHealthLow)
        {
            if (hearthBeatCoroutine == null && isHealthLow)
                hearthBeatCoroutine = RoR2.RoR2Application.instance.StartCoroutine(HearthBeatCoroutine());

            shouldPlayHearthBeat = isHealthLow;
        }

        internal override void OnHealthSpentOnInteractable(float healthPercent)
        {
            // Pulse on the hearth
            ShockwaveManager.Instance.sendHapticsPulse(24, 2, healthPercent * 1000);
        }

        internal override void OnFallDamageTaken(float healthPercent)
        {
            // Pulse on lower legs
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.LEFT_LOWER_LEG, 2, (int)(healthPercent * 800));
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.RIGHT_LOWER_LEG, 2, (int)(healthPercent * 800));
        }

        internal override void OnDirectionalDamageTaken(Vector3 localDirection, float healthPercent)
        {
            var chosenPair = directionToHapticGroup[0];

            for (int i = 1; i < directionToHapticGroup.Length; i++)
            {
                if (Vector3.Angle(localDirection, directionToHapticGroup[i].Key) < Vector3.Angle(localDirection, chosenPair.Key))
                    chosenPair = directionToHapticGroup[i];
            }

            ShockwaveManager.Instance.SendHapticGroup(chosenPair.Value, 0.5f + healthPercent * 2f, 100 + (int)(healthPercent * 700));
        }

        internal override void OnEnvironmentDamageTaken(float healthPercent)
        {
            // Pulse on the hearth
            ShockwaveManager.Instance.sendHapticsPulse(24, 1, healthPercent * 1000);
        }

        internal override void OnDeath()
        {
            RoR2.RoR2Application.instance.StartCoroutine(DeathCoroutine());
        }

        private IEnumerator HearthBeatCoroutine()
        {
            do
            {
                ShockwaveManager.Instance.sendHapticsPulse(24, 0.5f, 10);

                yield return new WaitForSeconds(0.25f);

                ShockwaveManager.Instance.sendHapticsPulse(24, 0.3f, 10);

                yield return new WaitForSeconds(0.5f);
            }
            while (shouldPlayHearthBeat);

            hearthBeatCoroutine = null;
        }

        private IEnumerator DeathCoroutine()
        {
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.SHOULDERS, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.CHEST, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.LEFT_BICEP, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.RIGHT_BICEP, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.SPINE, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.LEFT_FOREARM, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.RIGHT_FOREARM, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.WAIST, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.HIP, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.LEFT_THIGH, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.RIGHT_THIGH, 2, 200);

            yield return new WaitForSeconds(0.2f);

            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.LEFT_LOWER_LEG, 2, 200);
            ShockwaveManager.Instance.SendHapticGroup(ShockwaveManager.HapticGroup.RIGHT_LOWER_LEG, 2, 200);
        }
    }
}
