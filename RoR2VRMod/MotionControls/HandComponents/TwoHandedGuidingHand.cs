using UnityEngine;

namespace VRMod
{
    internal class TwoHandedGuidingHand : MonoBehaviour
    {
        [SerializeField]
        internal Transform guidingTransform;

        [SerializeField]
        internal GameObject objectToDisableWhenTwoHanded;

        private HandController parentHand;

        private void Awake()
        {
            parentHand = GetComponentInParent<HandController>();
        }

        private void OnDisable()
        {
            if (parentHand) parentHand.stabilisePosition = false;
        }

        private void Update()
        {
            bool stabilise = objectToDisableWhenTwoHanded && !objectToDisableWhenTwoHanded.activeSelf;

            if (parentHand && parentHand.stabilisePosition != stabilise) parentHand.stabilisePosition = stabilise; 
        }
    }
}
