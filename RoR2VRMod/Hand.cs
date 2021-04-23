using UnityEngine;

namespace VRMod
{
    internal class Hand : MonoBehaviour
    {
        [SerializeField]
        internal HandType handType;

        [SerializeField]
        internal Transform muzzle;

        [SerializeField]
        internal string bodyName;

        [SerializeField]
        internal bool useRay;

        internal Animator animator { get; private set; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
    }

    internal enum HandType
    {
        Both,
        Dominant,
        NonDominant
    }
}
