using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    public class ConfortVignette : MonoBehaviour
    {
        private static ConfortVignette instance;

        private static Material vignette;

        private const float INITIAL_RADIUS = 0.7f;
        private const float TARGET_RADIUS = 0.4f;
        private const float RADIUS_CHANGE_PER_SECOND = 3f;

        private static float currentRadius = INITIAL_RADIUS;
        private static float currentTargetRadius = INITIAL_RADIUS;

        private static bool activated;
        private static bool wasActivated;

        private List<EntityStateMachine> stateMachines;

        private bool foundStateMachine;

        private static List<Type> vignetteAbilities = new List<Type>()
        {
            typeof(EntityStates.Commando.DodgeState),
            typeof(EntityStates.Commando.SlideState),
            typeof(EntityStates.Huntress.BlinkState),
            typeof(EntityStates.Huntress.MiniBlinkState),
            typeof(EntityStates.Toolbot.ToolbotDash),
            typeof(EntityStates.Mage.FlyUpState),
            typeof(EntityStates.Merc.Assaulter2),
            typeof(EntityStates.Merc.EvisDash),
            typeof(EntityStates.Merc.FocusedAssaultDash),
            typeof(EntityStates.Merc.EvisDash),
            typeof(EntityStates.Loader.SwingChargedFist),
            typeof(EntityStates.Loader.SwingZapFist),
            typeof(EntityStates.Loader.GroundSlam),
            typeof(EntityStates.Croco.Leap),
            typeof(EntityStates.Croco.ChainableLeap)
        };

        private void Awake()
        {
            instance = this;

            if (!vignette)
                vignette = VRMod.VRAssetBundle.LoadAsset<Material>("VignetteMaterial");
        }

        private void OnEnable()
        {
            if (!foundStateMachine)
                RoR2Application.onUpdate += FindStateMachine;
        }

        private void OnDisable()
        {
            if (!foundStateMachine)
                RoR2Application.onUpdate -= FindStateMachine;
        }

        /// <summary>
        /// Adds to the list of states that activates the confort vignette. Recommended for mobility skills.
        /// </summary>
        /// <param name="stateType">The state type that will use the vignette.</param>
        public static void AddVignetteState(Type stateType)
        {
            if (!stateType.IsSubclassOf(typeof(EntityStates.EntityState)))
            {
                VRMod.StaticLogger.LogWarning("The type " + stateType.ToString() + " doesn't inherit from EntityState and cannot be added as a vignette state.");
                return;
            }

            if (vignetteAbilities.Contains(stateType))
            {
                VRMod.StaticLogger.LogWarning("The state type " + stateType.ToString() + " is already set as a vignette state and cannot be added twice.");
                return;
            }

            vignetteAbilities.Add(stateType);
        }

        private void FindStateMachine()
        {
            CharacterBody body = LocalUserManager.GetFirstLocalUser().cachedBody;

            if (body)
            {
                stateMachines = body.GetComponents<EntityStateMachine>().ToList();
                foundStateMachine = true;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (foundStateMachine)
            {
                stateMachines.RemoveAll(sm => sm == null);

                activated = stateMachines != null && stateMachines.Count > 0 && stateMachines.Exists(sm => vignetteAbilities.Contains(sm.state.GetType()));

                if (activated != wasActivated)
                {
                    wasActivated = activated;
                    currentTargetRadius = activated ? TARGET_RADIUS : INITIAL_RADIUS;
                }

                if (currentRadius != currentTargetRadius)
                {
                    float difference = Mathf.Abs(currentTargetRadius - currentRadius);
                    float unclampedChange = Time.deltaTime * (currentRadius < currentTargetRadius ? RADIUS_CHANGE_PER_SECOND : -RADIUS_CHANGE_PER_SECOND);
                    float radiusToChange = Mathf.Clamp(unclampedChange, -difference, difference);

                    currentRadius += radiusToChange;

                    vignette.SetFloat("_VRadius", currentRadius);
                }
            }

            Graphics.Blit(source, destination, vignette);
        }
    }
}
