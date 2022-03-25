using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Hand))]
    public class TwoHandedMainHand : MonoBehaviour
    {
        [SerializeField]
        public bool twoHandedAimActive = true;

        [SerializeField]
        public float snapAngle = 40f;

        [SerializeField]
        private Transform guide;

        [SerializeField]
        private Transform objectToGuide;

        [SerializeField]
        private GameObject objectToEnableWhenTwoHanded;

        private bool isHandWithinRange;

        private TwoHandedGuidingHand targetHand;

        private Vector3 guidingVector;

        private Quaternion originalRotation;

        private HandController parentHand;

        private void Awake()
        {
            parentHand = GetComponentInParent<HandController>();
            originalRotation = objectToGuide.localRotation;
        }

        private void OnDisable()
        {
            if (parentHand) parentHand.stabilisePosition = false;
        }

        private void Update()
        {
            HandController hand = GetComponentInParent<HandController>();

            if (!targetHand)
            {
                if (hand)
                {
                    targetHand = hand.oppositeHand.currentHand.GetComponent<TwoHandedGuidingHand>();
                }

                if (!targetHand) return;
            }
            
            if (twoHandedAimActive && targetHand.guidingTransform)
            {
                guidingVector = targetHand.guidingTransform.position - guide.position;
                float angleBetweenHands = Vector3.Angle(guide.forward, guidingVector);

                isHandWithinRange = angleBetweenHands <= snapAngle;
            }
            else if (isHandWithinRange)
            {
                isHandWithinRange = false;
                objectToGuide.rotation = originalRotation;
            }

            if (isHandWithinRange)
            {
                objectToGuide.rotation = Quaternion.LookRotation(guidingVector, guide.up);

                if (!objectToEnableWhenTwoHanded.activeSelf)
                {
                    objectToEnableWhenTwoHanded.SetActive(true);
                    targetHand.objectToDisableWhenTwoHanded.SetActive(false);
                }
            }
            else
            {
                if (objectToGuide.localRotation != originalRotation)
                    objectToGuide.localRotation = originalRotation;

                if (objectToEnableWhenTwoHanded.activeSelf)
                {
                    objectToEnableWhenTwoHanded.SetActive(false);
                    targetHand.objectToDisableWhenTwoHanded.SetActive(true);
                }
            }

            if (parentHand && parentHand.stabilisePosition != isHandWithinRange) parentHand.stabilisePosition = isHandWithinRange;
        }
    }
}
