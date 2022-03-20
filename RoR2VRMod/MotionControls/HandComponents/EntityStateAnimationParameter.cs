using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Animator))]
    class EntityStateAnimationParameter : MonoBehaviour
    {
        [SerializeField]
        private string[] stateNames;

        [SerializeField]
        private EntityStateParameterType parameterType;

        [SerializeField]
        private string parameterName;

        private static List<EntityStateAnimationParameter> instances;

        private CharacterBody body;

        private Animator animator;

        private EntityStateMachine[] bodyStateMachines;

        internal static void Init()
        {
            On.RoR2.EntityStateMachine.SetState += CheckStateMachine;
            instances = new List<EntityStateAnimationParameter>();
        }

        private static void CheckStateMachine(On.RoR2.EntityStateMachine.orig_SetState orig, EntityStateMachine self, EntityState newState)
        {
            if (self.commonComponents.characterBody.IsLocalBody())
            {
                foreach (EntityStateAnimationParameter animationParameter in instances)
                {
                    animationParameter.DoUpdate(self, newState);
                }
            }
            orig(self, newState);
        }

        private void OnEnable()
        {
            instances.Add(this);
        }

        private void OnDisable()
        {
            instances.Remove(this);
        }

        private void Start()
        {
            body = MotionControls.currentBody;
            animator = GetComponent<Animator>();

            if (!body || !animator)
            {
                enabled = false;
                return;
            }

            bodyStateMachines = body.GetComponents<EntityStateMachine>();
        }

        private void DoUpdate(EntityStateMachine stateMachine, EntityState newState)
        {
            if (!body || !animator)
            {
                enabled = false;
                return;
            }

            bool isCurrentStateValid = stateNames.Contains(stateMachine.state.GetType().Name);
            bool isNextStateValid = stateNames.Contains(newState.GetType().Name);

            if ((parameterType == EntityStateParameterType.OnStateEnterTrigger && isNextStateValid) || (parameterType == EntityStateParameterType.OnStateExitTrigger && isCurrentStateValid))
            {
                animator.SetTrigger(parameterName);
            }
            else if (parameterType == EntityStateParameterType.IsInStateBool)
            {
                if (isNextStateValid)
                    animator.SetBool(parameterName, true);
                else if (isCurrentStateValid)
                    animator.SetBool(parameterName, false);
            }
        }

        internal enum EntityStateParameterType
        {
            IsInStateBool,
            OnStateEnterTrigger,
            OnStateExitTrigger
        }
    }
}
