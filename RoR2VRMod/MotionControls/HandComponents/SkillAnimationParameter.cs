using RoR2;
using RoR2.Skills;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Animator))]
    internal class SkillAnimationParameter : MonoBehaviour
    {
        [SerializeField]
        private string skillName;

        [SerializeField]
        private SkillSlot forceSkillSlot;

        [SerializeField]
        private ParameterType parameterType;

        [SerializeField]
        private string parameterName;

        private Animator animator;

        private GenericSkill associatedSkill;

        private bool foundSkill;

        private bool wasAvailable;

        private CharacterBody body;

        private void Start()
        {
            body = MotionControls.currentBody;
            animator = GetComponent<Animator>();

            if (!body || !animator)
                return;

            if (parameterType == ParameterType.OnExecuteTrigger)
            {
                body.onSkillActivatedAuthority += OnActivatedAuthority;
                body.onSkillActivatedServer += OnActivatedServer;
            }
            else
            {
                if (forceSkillSlot == SkillSlot.None)
                {    
                    associatedSkill = body.skillLocator.FindSkill(skillName);

                    if (!associatedSkill)
                    { 
                        associatedSkill = body.skillLocator.allSkills.ToList().FirstOrDefault(x => x.skillDef.skillName == skillName || SkillCatalog.GetSkillName(x.skillDef.skillIndex) == skillName);
                    }
                }
                else
                {
                    associatedSkill = body.skillLocator.GetSkill(forceSkillSlot);
                }

                if (associatedSkill)
                {
                    foundSkill = true;
                    RoR2Application.onLateUpdate += UpdateBool;
                }
            }
        }

        private void OnDestroy()
        {
            if (parameterType == ParameterType.OnAvailableBool)
            {
                if (foundSkill)
                    RoR2Application.onLateUpdate -= UpdateBool;
            }
            else if (body)
            {
                body.onSkillActivatedAuthority -= OnActivatedAuthority;
                body.onSkillActivatedServer -= OnActivatedServer;
            }
        }

        private void OnActivatedAuthority(GenericSkill skill)
        {
            if (forceSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != forceSkillSlot) return;

            if (RoR2.Skills.SkillCatalog.GetSkillName(skill.skillDef.skillIndex) == skillName || skill.skillDef.skillName == skillName)
                animator.SetTrigger(parameterName);
        }

        private void OnActivatedServer(GenericSkill skill)
        {
            if (body.hasEffectiveAuthority || (forceSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != forceSkillSlot)) return;

            if (RoR2.Skills.SkillCatalog.GetSkillName(skill.skillDef.skillIndex) == skillName || skill.skillDef.skillName == skillName)
                animator.SetTrigger(parameterName);
        }

        private void UpdateBool()
        {
            bool available = associatedSkill != null && (associatedSkill.skillDef.skillName == skillName || associatedSkill.skillName == skillName || SkillCatalog.GetSkillName(associatedSkill.skillDef.skillIndex) == skillName) && associatedSkill.stock > 0;

            if (available != wasAvailable)
            {
                wasAvailable = available;

                animator.SetBool(parameterName, available);
            }
        }

        internal enum ParameterType
        {
            OnExecuteTrigger,
            OnAvailableBool
        }
    }
}
