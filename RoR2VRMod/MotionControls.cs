using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

            On.RoR2.CharacterMaster.TransformBody += (orig, self, bodyName) =>
            {
                orig(self, bodyName);
                if (bodyName.Contains("Heretic"))
                    SetHandPair(bodyName.Substring(0, bodyName.IndexOf("Body")));
            };

            handSelectorPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("VRHand");

            string[] prefabNames = new string[]
            {
                "CommandoPistol",
                "HuntressBow",
                "HuntressHand"
            };

            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = VRMod.VRAssetBundle.LoadAsset<GameObject>(prefabName);

                if (prefab)
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
        /// Returns the generated ray from the chosen hand's muzzle by hand dominance.
        /// </summary>
        /// <param name="dominant">Return the ray of the dominant or non-dominant hand.</param>
        /// <returns></returns>
        public static Ray GetHandRayByDominance(bool dominant)
        {
            if (!HandsReady)
                throw new NullReferenceException("VR Mod: Cannot retrieve the ray of a hand that doesn't exist.");

            return (dominant ? dominantHand : nonDominantHand).GetRay();
        }

        /// <summary>
        /// Returns the muzzle transform from the chosen hand.
        /// </summary>
        /// <param name="dominant"></param>
        /// <returns>Return the muzzle of the dominant or non-dominant hand.</returns>
        public static Transform GetHandMuzzle(bool dominant)
        {
            if (!HandsReady)
                throw new NullReferenceException("VR Mod: Cannot retrieve the muzzle of a hand that doesn't exist.");

            return (dominant ? dominantHand : nonDominantHand).currentHand.muzzle;
        }

        internal static Ray GetHandRayBySide(bool left)
        {
            return (left == ModConfig.LeftDominantHand.Value ? dominantHand : nonDominantHand).GetRay();
        }

        private static bool CheckPrefabMatch(Hand hand, GameObject prefab)
        {
            Hand prefabHand = prefab.GetComponent<Hand>();

            return hand.bodyName == prefabHand.bodyName && (prefabHand.handType == HandType.Both || hand.handType == HandType.Both || prefabHand.handType == hand.handType);
        }

        private static void SetupVRHands(On.RoR2.CameraRigController.orig_Start orig, RoR2.CameraRigController self)
        {
            orig(self);

            if (!Run.instance) return;

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

            RoR2Application.onFixedUpdate += CheckForLocalBody;
        }

        private static void CheckForLocalBody()
        {
            CharacterBody localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            if (localBody)
            {
                RoR2Application.onFixedUpdate -= CheckForLocalBody;
                VRMod.StaticLogger.LogInfo(String.Format("Local cached body \'{0}\' found. Applying hand pair.", localBody.name));
                SetHandPair(localBody.name.Substring(0, localBody.name.IndexOf("Body")));
            }
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
                    if (bodyName == "Captain")
                    {
                        List<ChildLocator.NameTransformPair> transformPairList = childLocator.transformPairs.ToList();
                        transformPairList.Add(new ChildLocator.NameTransformPair() { name = "MuzzleLeft", transform = nonDominantHand.currentHand.muzzle });
                        childLocator.transformPairs = transformPairList.ToArray();
                    }
                    for (int i = 0; i < childLocator.transformPairs.Length; i++)
                    {
                        string name = childLocator.transformPairs[i].name;
                        if (name.Contains("Muzzle"))
                        {
                            if (bodyName == "Engi")
                            {
                                if (name.Contains("Center"))
                                    childLocator.transformPairs[i].transform = nonDominantHand.currentHand.muzzle;
                            }
                            else
                            {
                                if (name.Contains("Left") || name.Contains("HandL") || name == "DualWieldMuzzleR" || (bodyName == "Mage" && name.Contains("Between")))
                                    childLocator.transformPairs[i].transform = nonDominantHand.currentHand.muzzle;
                                else if (!name.Contains("Center"))
                                    childLocator.transformPairs[i].transform = dominantHand.currentHand.muzzle;
                            }
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
