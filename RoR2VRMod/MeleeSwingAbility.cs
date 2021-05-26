using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VRMod
{
    internal class MeleeSwingAbility : MonoBehaviour
    {
        [SerializeField]
        private Transform weaponTip;

        [SerializeField]
        internal float speedThreshold;

        [SerializeField]
        private string[] activatedSkills;

        [SerializeField]
        private float activationTime;

        internal static bool[] skillStates = new bool[4];

        private static float[] timers = new float[4];

        private static CharacterBody body;

        private static List<MeleeSwingAbility> instanceList = new List<MeleeSwingAbility>();

        private Vector3[] tipPath = new Vector3[3];

        private bool firstFrame = true;

        private void OnEnable()
        {
            if (instanceList.Count() <= 0)
            {
                RoR2Application.onFixedUpdate += UpdateTimer;
            }

            instanceList.Add(this);
        }

        private void OnDisable()
        {
            instanceList.Remove(this);

            if (instanceList.Count() <= 0)
            {
                RoR2Application.onFixedUpdate -= UpdateTimer;
            }
        }

        private static void UpdateTimer()
        {
            for (int i = 0; i < 4; i++)
            {
                if (timers[i] > 0)
                {
                    timers[i] -= Time.fixedDeltaTime;
                }

                skillStates[i] = timers[i] > 0;
            }
        }

        private void FixedUpdate()
        {
            if (!VRCameraWrapper.instance) return;

            if (!body)
            {
                if ((body = LocalUserManager.GetFirstLocalUser().cachedBody) == null) return;
            }

            Vector3 newPoint = VRCameraWrapper.instance.transform.InverseTransformPoint(weaponTip.transform.position);

            if (newPoint != tipPath[0])
            {
                tipPath[2] = tipPath[1];
                tipPath[1] = tipPath[0];
                tipPath[0] = newPoint;

                if (firstFrame)
                {
                    firstFrame = false;
                    tipPath[2] = tipPath[1] = tipPath[0];
                }
            }

            bool swinging = (Time.fixedDeltaTime * speedThreshold) < (Vector3.Distance(tipPath[0], tipPath[2]) / 2);

            List<GenericSkill> skillsToActivate = body.skillLocator.allSkills.ToList().Where(x => activatedSkills.ToList().Contains(x.skillName) || activatedSkills.ToList().Contains(x.skillDef.skillName)).ToList();

            foreach (GenericSkill skill in skillsToActivate)
            {
                SkillSlot slot = body.skillLocator.FindSkillSlot(skill);

                if (slot != SkillSlot.None && swinging)
                    timers[(int)slot] = activationTime;
            }
        }
    }
}
