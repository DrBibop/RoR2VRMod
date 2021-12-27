using RoR2;
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
            body = Utils.localBody;
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
                        try
                        {
                            associatedSkill = body.skillLocator.allSkills.ToList().First(x => x.skillDef.skillName == skillName);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    if (associatedSkill)
                    {
                        foundSkill = true;
                        RoR2Application.onUpdate += UpdateBool;
                    }
                }
                else
                {
                    associatedSkill = body.skillLocator.GetSkill(forceSkillSlot);
                }
            }
        }

        private void OnDestroy()
        {
            if (parameterType == ParameterType.OnAvailableBool)
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
            if ((forceSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != forceSkillSlot) || skill.skillDef.skillName != skillName) return;

            animator.SetTrigger(parameterName);
        }

        private void OnActivatedServer(GenericSkill skill)
        {
            if (body.hasEffectiveAuthority || (forceSkillSlot != SkillSlot.None && body.skillLocator.FindSkillSlot(skill) != forceSkillSlot) || skill.skillDef.skillName != skillName) return;

            animator.SetTrigger(parameterName);
        }

        private void UpdateBool()
        {
            bool available = associatedSkill != null && (associatedSkill.skillDef.skillName == skillName || associatedSkill.skillName == skillName) && associatedSkill.stock > 0;

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
