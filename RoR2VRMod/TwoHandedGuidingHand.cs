using UnityEngine;

namespace VRMod
{
    internal class TwoHandedGuidingHand : MonoBehaviour
    {
        [SerializeField]
        internal Transform guidingTransform;

        [SerializeField]
        internal GameObject objectToDisableWhenTwoHanded;
    }
}
