using System;
using UnityEngine;

namespace VRAPI
{
    public static class MotionControls
    {
        public delegate void SetHandPairEventHandler(RoR2.CharacterBody body);

        public static SetHandPairEventHandler onHandPairSet;

        private static HandController _dominantHand;

        private static HandController _nonDominantHand;

        private static bool? _leftHanded;

        private static bool leftHanded
        {
            get
            {
                if (_leftHanded == null)
                {
                    _leftHanded = VRMod.ModConfig.LeftHanded;
                }
                return (bool)_leftHanded;
            }
        }

        /// <summary>
        /// The dominant hand controlled by the local player. Can be the right or left hand depending on their settings.
        /// </summary>
        public static HandController dominantHand
        {
            get
            {
                if (_dominantHand == null)
                {
                    _dominantHand = new HandController();
                }

                if (_dominantHand.handController == null)
                {
                    _dominantHand.handController = VRMod.MotionControls.GetHandByDominance(true);
                }
                return _dominantHand;
            }
        }

        /// <summary>
        /// The non-dominant hand controlled by the local player. Can be the right or left hand depending on their settings.
        /// </summary>
        public static HandController nonDominantHand
        {
            get
            {
                if (_nonDominantHand == null)
                {
                    _nonDominantHand = new HandController();
                }

                if (_nonDominantHand.handController == null)
                {
                    _nonDominantHand.handController = VRMod.MotionControls.GetHandByDominance(false);
                }
                return _nonDominantHand;
            }
        }

        /// <summary>
        /// The left hand controlled by the local player. This hand won't change no matter which hand is set as dominant.
        /// </summary>
        public static HandController leftHand
        {
            get
            {
                return leftHanded ? dominantHand : nonDominantHand;
            }
        }

        /// <summary>
        /// The right hand controlled by the local player. This hand won't change no matter which hand is set as dominant.
        /// </summary>
        public static HandController rightHand
        {
            get
            {
                return leftHanded ? nonDominantHand : dominantHand;
            }
        }

        private static bool? _enabled;

        /// <summary>
        /// Returns true if the user has the VR Mod with motion controls enabled.
        /// </summary>
        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = VRMod.ModConfig.MotionControlsEnabled;
                }
                return (bool)_enabled;
            }
        }

        /// <summary>
        /// Adds a hand prefab to use when the associated character is being played in VR. This must only be called once per prefab.
        /// </summary>
        /// <param name="handPrefab">The hand prefab containing a hand script.</param>
        public static void AddHandPrefab(GameObject handPrefab)
        {
            VRMod.MotionControls.AddHandPrefab(handPrefab);
        }

        /// <summary>
        /// Adds a hand prefab to use when the associated character is being played in VR. This must only be called once per prefab.
        /// </summary>
        /// <param name="handPrefab">The hand prefab containing a hand script.</param>
        public static void AddHandSkin(ScriptableObject handSkinDef)
        {
            if (!(handSkinDef is VRMod.HandSkinDef))
            {
                VRAPI.StaticLogger.LogError("Cannot add hand skin: The scriptable object isn't of type HandSkinDef.");
            }
            VRMod.MotionControls.AddHandSkin(handSkinDef as VRMod.HandSkinDef);
        }

        /// <summary>
        /// Edits the binding of skills. When the associated character is played, The selected binds will replace the default ones. This can be useful to change which hand controls a certain ability.
        /// </summary>
        /// <param name="bodyName">The name of the character body object.</param>
        /// <param name="dominantTrigger">The skill to activate with the dominant hand trigger (default is primary).</param>
        /// <param name="nonDominantTrigger">The skill to activate with the non-dominant hand trigger (default is secondary).</param>
        /// <param name="nonDominantGrip">The skill to activate with the non-dominant hand grip (default is utility).</param>
        /// <param name="dominantGrip">The skill to activate with the dominant hand grip (default is special).</param>
        public static void AddSkillBindingOverride(string bodyName, RoR2.SkillSlot dominantTrigger, RoR2.SkillSlot nonDominantTrigger, RoR2.SkillSlot nonDominantGrip, RoR2.SkillSlot dominantGrip)
        {
            VRMod.Controllers.AddSkillBindingOverride(bodyName, dominantTrigger, nonDominantTrigger, nonDominantGrip, dominantGrip);
        }

        /// <summary>
        /// (This method is deprecated. Please use AddSkillBindingOverride instead) Adds a remap instruction between two skills. When the associated character is played, the mapped buttons will be swapped between the two skills. This can be useful to change which hand controls a certain ability.
        /// </summary>
        /// <param name="bodyName">The name of the character body object.</param>
        /// <param name="skill1">The first skill to be remapped.</param>
        /// <param name="skill2">The second skill to be remapped.</param>
        [Obsolete("Deprecated. Use AddSkillBindingOverride instead.")]
        public static void AddSkillRemap(string bodyName, RoR2.SkillSlot skill1, RoR2.SkillSlot skill2)
        {
            VRMod.Controllers.AddSkillRemap(bodyName, skill1, skill2);
        }

        public class HandController
        {
            internal VRMod.HandController handController;
            public Transform transform => handController.transform;
            public Transform muzzle => handController.muzzle;
            public Ray aimRay => handController.aimRay;
            public Animator animator => handController.animator;
            public RoR2.CharacterModel.RendererInfo[] rendererInfos => handController.rendererInfos;

            public Transform GetMuzzleByIndex(uint index)
            {
                return handController.GetMuzzleByIndex(index);
            }
        }
    }
}
