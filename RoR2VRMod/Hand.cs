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
            if (muzzles == null || muzzles.Length <= 0)
                throw new NullReferenceException("This hand has no muzzle! Aiming won't be possible that way and other errors may appear.");

            animator = GetComponent<Animator>();

            currentMuzzle = muzzles[0];
        }

        internal void SetMuzzle(uint index)
        {
            if (index >= muzzles.Length)
                throw new ArgumentOutOfRangeException("Cannot set the muzzle using an out-of-range index.");

            currentMuzzle = muzzles[index];
        }
    }

    internal enum HandType
    {
        Both,
        Dominant,
        NonDominant
    }
}
