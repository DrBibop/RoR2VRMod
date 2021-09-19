using UnityEngine;
using RoR2;
using System;

namespace VRMod
{
    internal class Hand : MonoBehaviour
    {
        [SerializeField]
        internal HandType handType;

        [SerializeField]
        internal Muzzle[] muzzles;

        [SerializeField]
        internal string bodyName;

        [SerializeField]
        internal bool useRay;

        [SerializeField]
        internal CharacterModel.RendererInfo[] rendererInfos;

        internal Animator animator { get; private set; }

        internal Muzzle currentMuzzle;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();

            if (muzzles == null || muzzles.Length <= 0)
                throw new NullReferenceException("This hand has no muzzle! Aiming won't be possible that way and other errors may appear.");

            currentMuzzle = muzzles[0];
        }
    }

    internal enum HandType
    {
        Both,
        Dominant,
        NonDominant
    }
}
