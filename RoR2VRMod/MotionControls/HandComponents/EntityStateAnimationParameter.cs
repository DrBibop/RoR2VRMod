using RoR2;
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

        private CharacterBody body;

        private Animator animator;

        private EntityStateMachine[] bodyStateMachines;

        private bool previousBoolValue;

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

        private void LateUpdate()
        {
            if (!body || !animator)
            {
                enabled = false;
                return;
            }

            bool foundState = false;
            foreach (EntityStateMachine stateMachine in bodyStateMachines)
            {
                if (stateNames.Contains(stateMachine.state.GetType().Name))
                {
                    foundState = true;
                    break;
                }
            }

            if (previousBoolValue != foundState)
            {
                if ((foundState && parameterType == EntityStateParameterType.OnStateEnterTrigger) || (!foundState && parameterType == EntityStateParameterType.OnStateExitTrigger))
                {
                    animator.SetTrigger(parameterName);
                }
                else if (parameterType == EntityStateParameterType.IsInStateBool)
                {
                    animator.SetBool(parameterName, foundState);
                }
                previousBoolValue = foundState;
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
