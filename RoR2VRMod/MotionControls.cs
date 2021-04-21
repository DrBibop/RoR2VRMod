using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace VRMod
{
    internal class MotionControls
    {
        private static GameObject handPrefab;

        private static HandSelector leftHand;
        private static HandSelector rightHand;

        internal static bool HandsReady => leftHand != null && rightHand != null && leftHand.currentHand != null && rightHand.currentHand != null;

        private static HandSelector dominantHand;
        private static HandSelector nonDominantHand;

        internal static void Init()
        {
            On.RoR2.CameraRigController.Start += SetupVRHands;

            RoR2.PauseManager.onPauseStartGlobal += () =>
            {
                SetHandPair(HandPair.Pointer);
            };

            RoR2.PauseManager.onPauseEndGlobal += () =>
            {
                CharacterBody body = LocalUserManager.GetFirstLocalUser().cachedBody;
                if (body)
                    SetHandPairFromCharacterName(body.name);
            };
        }

        private static void SetHandPairFromCharacterName(string characterName)
        {
            string name = characterName;
            if (name.Contains("Body")) name = name.Substring(0, characterName.IndexOf("Body"));

            VRMod.StaticLogger.LogInfo(characterName);

            switch (name)
            {
                case "Huntress":
                    SetHandPair(HandPair.Huntress);
                    break;
                case "Bandit2":
                    SetHandPair(HandPair.Bandit);
                    break;
                case "HAND":
                    SetHandPair(HandPair.MULT);
                    break;
                case "Engi":
                    SetHandPair(HandPair.Engi);
                    break;
                case "Mage":
                    SetHandPair(HandPair.Arti);
                    break;
                case "Merc":
                    SetHandPair(HandPair.Merc);
                    break;
                case "Treebot":
                    SetHandPair(HandPair.Rex);
                    break;
                case "Loader":
                    SetHandPair(HandPair.Loader);
                    break;
                case "Croco":
                    SetHandPair(HandPair.Acrid);
                    break;
                case "Captain":
                    SetHandPair(HandPair.Captain);
                    break;
                default:
                    SetHandPair(HandPair.Commando);
                    break;
            }
        }

        private static void SetupVRHands(On.RoR2.CameraRigController.orig_Start orig, RoR2.CameraRigController self)
        {
            orig(self);

            if (!handPrefab)
            {
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vrmodassets);

                handPrefab = assetBundle.LoadAsset<GameObject>("VRHand");
            }

            leftHand = GameObject.Instantiate(handPrefab).GetComponent<HandSelector>();
            leftHand.SetXRNode(XRNode.LeftHand);
            Vector3 mirroredScale = leftHand.transform.localScale;
            mirroredScale.x = -mirroredScale.x;
            leftHand.transform.localScale = mirroredScale;

            rightHand = GameObject.Instantiate(handPrefab).GetComponent<HandSelector>();
            rightHand.SetXRNode(XRNode.RightHand);

            dominantHand = rightHand;
            nonDominantHand = leftHand;

            leftHand.ShowRay(true);
            rightHand.ShowRay(true);

            CharacterBody localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            if (localBody)
                SetHandPairFromCharacterName(localBody.name);
            else
                SetHandPair(HandPair.Pointer);
        }

        internal static void SetHandPair(HandPair handPair)
        {
            if (handPair != HandPair.Pointer)
                handPair = HandPair.Commando;

            switch(handPair)
            {
                case HandPair.Pointer:
                    leftHand.SetCurrentHand(HandType.Pointer);
                    rightHand.SetCurrentHand(HandType.Pointer);
                    break;
                case HandPair.Commando:
                    leftHand.SetCurrentHand(HandType.Commando);
                    rightHand.SetCurrentHand(HandType.Commando);
                    break;
                case HandPair.Huntress:
                    leftHand.SetCurrentHand(HandType.HuntressHand);
                    rightHand.SetCurrentHand(HandType.HuntressBow);
                    break;
                case HandPair.Bandit:
                    leftHand.SetCurrentHand(HandType.BanditRevolver);
                    rightHand.SetCurrentHand(HandType.BanditRifle);
                    break;
                case HandPair.MULT:
                    leftHand.SetCurrentHand(HandType.MULTHand);
                    rightHand.SetCurrentHand(HandType.MULTNail);
                    break;
                case HandPair.Engi:
                    leftHand.SetCurrentHand(HandType.Engi);
                    rightHand.SetCurrentHand(HandType.Engi);
                    break;
                case HandPair.Arti:
                    leftHand.SetCurrentHand(HandType.Arti);
                    rightHand.SetCurrentHand(HandType.Arti);
                    break;
                case HandPair.Merc:
                    leftHand.SetCurrentHand(HandType.MercHand);
                    rightHand.SetCurrentHand(HandType.MercSword);
                    break;
                case HandPair.Rex:
                    leftHand.SetCurrentHand(HandType.Rex);
                    rightHand.SetCurrentHand(HandType.Rex);
                    break;
                case HandPair.Loader:
                    leftHand.SetCurrentHand(HandType.Loader);
                    rightHand.SetCurrentHand(HandType.Loader);
                    break;
                case HandPair.Acrid:
                    leftHand.SetCurrentHand(HandType.Acrid);
                    rightHand.SetCurrentHand(HandType.Acrid);
                    break;
                case HandPair.Captain:
                    leftHand.SetCurrentHand(HandType.CaptainHand);
                    rightHand.SetCurrentHand(HandType.CaptainShotgun);
                    break;
            }

            if (handPair == HandPair.Pointer)
                return;

            CharacterBody localBody = LocalUserManager.GetFirstLocalUser().cachedBody;
            if (localBody)
            {
                localBody.aimOriginTransform = dominantHand.currentHand.aimOrigin;

                ChildLocator childLocator = localBody.modelLocator.modelTransform.GetComponent<ChildLocator>();

                if (childLocator)
                {
                    for (int i = 0; i < childLocator.transformPairs.Length; i++)
                    {
                        if (childLocator.transformPairs[i].name.Contains("Muzzle"))
                        {
                            if (childLocator.transformPairs[i].name.Contains("Left"))
                                childLocator.transformPairs[i].transform = nonDominantHand.currentHand.aimOrigin;
                            else if (!childLocator.transformPairs[i].name.Contains("Center"))
                                childLocator.transformPairs[i].transform = dominantHand.currentHand.aimOrigin;
                        }
                    }
                }
            }
        }

        internal static Ray GetHandRay(HandSide handSide = HandSide.Dominant)
        {
            HandSelector selectedHand;
            switch (handSide)
            {
                case HandSide.Left:
                    selectedHand = leftHand;
                    break;
                case HandSide.Right:
                    selectedHand = rightHand;
                    break;
                case HandSide.NonDominant:
                    selectedHand = nonDominantHand;
                    break;
                default:
                    selectedHand = dominantHand;
                    break;
            }

            return GetHandRay(selectedHand);
        }

        internal static Ray GetHandRay(HandSelector hand)
        {
            return new Ray(hand.currentHand.aimOrigin.position, hand.currentHand.aimOrigin.forward);
        }

        internal enum HandPair
        {
            Pointer,
            Commando,
            Huntress,
            Bandit,
            MULT,
            Engi,
            Arti,
            Merc,
            Rex,
            Loader,
            Acrid,
            Captain
        }

        internal enum HandSide
        {
            Left,
            Right,
            Dominant,
            NonDominant
        }
    }
}
