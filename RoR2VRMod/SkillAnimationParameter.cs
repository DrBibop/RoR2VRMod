using RoR2;
using System;
using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Animator))]
    internal class SkillAnimationParameter : MonoBehaviour
    {
        [SerializeField]
        private string skillName;

        [SerializeField]
        private SkillSlot filterSkillSlot;

        [SerializeField]
        private TriggerType triggerType;

        [SerializeField]
        private string parameterName;

        private Animator animator;

        private GenericSkill associatedSkill;

        private bool foundSkill;

        private bool wasAvailable;

        private CharacterBody body;

        private void Start()
        {
            body = LocalUserManager.GetFirstLocalUser().cachedBody;
            animator = GetComponent<Animator>();

            if (!body || !animator)
                return;

            if (triggerType == TriggerType.OnExecuteTrigger)
            {
                body.onSkillActivatedAuthority += OnActivatedAuthority;
                body.onSkillActivatedServer += OnActivatedServer;
            }
            else
            {
                if (filterSkillSlot == SkillSlot.None)
                {
                    GenericSkill[] skills = body.skillLocator.allSkills;

                    foreach (GenericSkill skill in skills)
                    {
                        if (skill.skillDef.skillName == skillName)
                        {
                            foundSkill = true;
                            associatedSkill = skill;
                            RoR2Application.onUpdate += UpdateBool;
                            break;
                        }
                    }
                }
                else
                {
                    associatedSkill = body.skillLocator.GetSkill(filterSkillSlot);
                }
            }
        }

        private void OnDestroy()
        {
            if (triggerType == TriggerType.OnAvailableBool)
            {
                if (foundSkill)
                    RoR2Application.onUpdate -= UpdateBool;
            }
            else if (body)
            {
                body.onSkillActivatedAuthority -= OnActivatedAuthority;
                body.onSkillActivatedServer -= OnActivatedServer;
            }
        }

        private void OnActivatedAuthority(GenericSkill skill)
        {
            if ((filterSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != filterSkillSlot) || skill.skillDef.skillName != skillName) return;

            animator.SetTrigger(parameterName);
        }

        private void OnActivatedServer(GenericSkill skill)
        {
            if (body.hasEffectiveAuthority || (filterSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != filterSkillSlot) || skill.skillDef.skillName != skillName) return;

            animator.SetTrigger(parameterName);
        }

        private void UpdateBool()
        {

            bool available = associatedSkill != null && associatedSkill.skillDef.skillName == skillName && associatedSkill.stock > 0;

            if (available != wasAvailable)
            {
                wasAvailable = available;

                animator.SetBool(parameterName, available);
            }
        }

        internal enum TriggerType
        {
            OnExecuteTrigger,
            OnAvailableBool
        }
    }
}
