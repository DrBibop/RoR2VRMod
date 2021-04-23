using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace VRMod
{
    public class MotionControls
    {
        internal static bool HandsReady => dominantHand != null && nonDominantHand != null && dominantHand.currentHand != null && nonDominantHand.currentHand != null;

        private static GameObject handSelectorPrefab;

        private static HandSelector dominantHand;
        private static HandSelector nonDominantHand;

        private static List<GameObject> handPrefabs = new List<GameObject>();

        internal static void Init()
        {
            On.RoR2.CameraRigController.Start += SetupVRHands;

            RoR2.PauseManager.onPauseStartGlobal += ResetToPointer;

            RoR2.PauseManager.onPauseEndGlobal += () =>
            {
                CharacterBody body = LocalUserManager.GetFirstLocalUser().cachedBody;
                if (body)
                    SetHandPair(body.name.Substring(0, body.name.IndexOf("Body")));
            };

            AssetBundle assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vrmodassets);

            List<GameObject> prefabs = new List<GameObject>()
            {
                assetBundle.LoadAsset<GameObject>("CommandoPistol"),
                assetBundle.LoadAsset<GameObject>("HuntressBow"),
                assetBundle.LoadAsset<GameObject>("HuntressHand")
            };

            handSelectorPrefab = assetBundle.LoadAsset<GameObject>("VRHand");

            foreach (GameObject prefab in prefabs)
            {
                AddHandPrefab(prefab);
            }
        }

        /// <summary>
        /// Add a hand prefab to be instanciated when the matching body is being used by the local player.
        /// </summary>
        /// <param name="handPrefab">The hand prefab from your asset bundle.</param>
        public static void AddHandPrefab(GameObject handPrefab)
        {
            if (!handPrefab)
            {
                throw new ArgumentNullException("VR Mod: Cannot add null as a hand prefab.");
            }

            Hand handComponent = handPrefab.GetComponent<Hand>();

            if (!handComponent)
            {
                throw new ArgumentException("VR Mod: The prefab is missing a Hand component and cannot be added as a hand prefab.");
            }

            if (handPrefabs.Exists((x) => CheckPrefabMatch(handComponent, x)))
            {
                throw new ArgumentException("VR Mod: A hand with the same body name and hand type has already been added. Make sure to only add one hand of type \'Both\' or one \'Dominant\' and \'Non-Dominant\' hand for the same body.");
            }

            handPrefabs.Add(handPrefab);
        }

        /// <summary>
        /// Returns the animator component of the chosen hand.
        /// </summary>
        /// <param name="dominant">Return the component of the dominant or non-dominant hand.</param>
        /// <returns></returns>
        public static Animator GetHandAnimator(bool dominant)
        {
            if (!HandsReady)
                throw new NullReferenceException("VR Mod: Cannot retrieve the animator of a hand that doesn't exist.");

            return (dominant ? dominantHand : nonDominantHand).currentHand.animator;
        }

        /// <summary>
        /// Returns the generated ray from the chosen hand's muzzle.
        /// </summary>
        /// <param name="dominant">Return the ray of the dominant or non-dominant hand.</param>
        /// <returns></returns>
        public static Ray GetHandRay(bool dominant)
        {
            if (!HandsReady)
                throw new NullReferenceException("VR Mod: Cannot retrieve the ray of a hand that doesn't exist.");

            return (dominant ? dominantHand : nonDominantHand).GetRay();
        }

        private static bool CheckPrefabMatch(Hand hand, GameObject prefab)
        {
            Hand prefabHand = prefab.GetComponent<Hand>();

            return hand.bodyName == prefabHand.bodyName && (prefabHand.handType == HandType.Both || hand.handType == HandType.Both || prefabHand.handType == hand.handType);
        }

        private static void SetupVRHands(On.RoR2.CameraRigController.orig_Start orig, RoR2.CameraRigController self)
        {
            orig(self);

            HandSelector leftHand = GameObject.Instantiate(handSelectorPrefab).GetComponent<HandSelector>();
            leftHand.SetXRNode(XRNode.LeftHand);
            Vector3 mirroredScale = leftHand.transform.localScale;
            mirroredScale.x = -mirroredScale.x;
            leftHand.transform.localScale = mirroredScale;

            HandSelector rightHand = GameObject.Instantiate(handSelectorPrefab).GetComponent<HandSelector>();
            rightHand.SetXRNode(XRNode.RightHand);

            dominantHand = ModConfig.LeftDominantHand.Value ? leftHand : rightHand;
            nonDominantHand = ModConfig.LeftDominantHand.Value ? rightHand : leftHand;

            dominantHand.SetPrefabs(handPrefabs.Where((x) => CheckDominance(x, true)).ToList());
            nonDominantHand.SetPrefabs(handPrefabs.Where((x) => CheckDominance(x, false)).ToList());

            CharacterBody localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            if (localBody)
                SetHandPair(localBody.name.Substring(0, localBody.name.IndexOf("Body")));
        }

        private static bool CheckDominance(GameObject prefab, bool dominant)
        {
            Hand hand = prefab.GetComponent<Hand>();

            return hand.handType == HandType.Both || (dominant == (hand.handType == HandType.Dominant));
        }

        internal static void SetHandPair(string bodyName)
        {
            if (!HandsReady) return;

            dominantHand.SetCurrentHand(bodyName);
            nonDominantHand.SetCurrentHand(bodyName);

            CharacterBody localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            if (localBody)
            {
                localBody.aimOriginTransform = dominantHand.currentHand.muzzle;

                ChildLocator childLocator = localBody.modelLocator.modelTransform.GetComponent<ChildLocator>();

                if (childLocator)
                {
                    for (int i = 0; i < childLocator.transformPairs.Length; i++)
                    {
                        if (childLocator.transformPairs[i].name.Contains("Muzzle"))
                        {
                            if (childLocator.transformPairs[i].name.Contains("Left"))
                                childLocator.transformPairs[i].transform = nonDominantHand.currentHand.muzzle;
                            else if (!childLocator.transformPairs[i].name.Contains("Center"))
                                childLocator.transformPairs[i].transform = dominantHand.currentHand.muzzle;
                        }
                    }
                }
            }
        }

        internal static void ResetToPointer()
        {
            dominantHand.ResetToPointer();
            nonDominantHand.ResetToPointer();
        }
    }
}
