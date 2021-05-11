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
        private float speedThreshold;

        [SerializeField]
        private string[] activatedSkills;

        [SerializeField]
        private float activationTime;

        internal static bool[] skillStates = new bool[4];

        private float timer;

        private Vector3[] tipPath = new Vector3[3];

        private bool firstFrame = true;

        private CharacterBody body;

        private void FixedUpdate()
        {
            if (timer > 0)
                timer -= Time.fixedDeltaTime;

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

                VRMod.StaticLogger.LogInfo(tipPath[0]);

                if (firstFrame)
                {
                    firstFrame = false;
                    tipPath[2] = tipPath[1] = tipPath[0];
                }

                VRMod.StaticLogger.LogInfo(Vector3.Distance(tipPath[0], tipPath[2]) / 2);
            }

            bool swinging = (Time.fixedDeltaTime * speedThreshold) < (Vector3.Distance(tipPath[0], tipPath[2]) / 2);

            if (swinging)
                timer = activationTime;

            VRMod.StaticLogger.LogInfo(timer);

            List<GenericSkill> skillsToActivate = body.skillLocator.allSkills.ToList().Where(x => activatedSkills.ToList().Contains(x.skillDef.skillName)).ToList();

            foreach (GenericSkill skill in skillsToActivate)
            {
                SkillSlot slot = body.skillLocator.FindSkillSlot(skill);

                bool activate = timer > 0;

                switch (slot)
                {
                    case SkillSlot.Primary:
                        skillStates[0] = activate;
                        break;
                    case SkillSlot.Secondary:
                        skillStates[1] = activate;
                        break;
                    case SkillSlot.Utility:
                        skillStates[2] = activate;
                        break;
                    case SkillSlot.Special:
                        skillStates[3] = activate;
                        break;
                    default:
                        continue;
                }
            }
        }
    }
}
