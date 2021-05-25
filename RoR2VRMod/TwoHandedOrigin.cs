using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Hand))]
    internal class TwoHandedOrigin : MonoBehaviour
    {
        [SerializeField]
        internal bool twoHandedAimActive = true;

        [SerializeField]
        internal float angleTolerance = 40f;

        [SerializeField]
        private Transform originTransform;

        [SerializeField]
        private Transform targetToRotate;

        [SerializeField]
        private GameObject objectToActivateWhenTwoHanded;

        private bool isHandWithinRange;

        private TwoHandedAimTarget targetHand;

        private Vector3 guidingVector;

        private Quaternion originalTargetRotation;

        private void Awake()
        {
            originalTargetRotation = targetToRotate.localRotation;
        }

        private void Update()
        {
            if (!targetHand)
            {
                HandSelector hand = GetComponentInParent<HandSelector>();

                if (hand)
                {
                    targetHand = hand.oppositeHand.currentHand.GetComponent<TwoHandedAimTarget>();
                }

                if (!targetHand) return;
            }
            
            if (twoHandedAimActive && targetHand.guidingTransform)
            {
                guidingVector = targetHand.guidingTransform.position - originTransform.position;
                float angleBetweenHands = Vector3.Angle(originTransform.forward, guidingVector);

                isHandWithinRange = angleBetweenHands <= angleTolerance;
            }
            else if (isHandWithinRange)
            {
                isHandWithinRange = false;
                targetToRotate.rotation = originalTargetRotation;
            }

            if (isHandWithinRange)
            {
                targetToRotate.rotation = Quaternion.LookRotation(guidingVector, originTransform.up);

                if (!objectToActivateWhenTwoHanded.activeSelf)
                {
                    objectToActivateWhenTwoHanded.SetActive(true);
                    targetHand.objectToDisableWhenTwoHanded.SetActive(false);
                }
            }
            else
            {
                if (targetToRotate.localRotation != originalTargetRotation)
                    targetToRotate.localRotation = originalTargetRotation;

                if (objectToActivateWhenTwoHanded.activeSelf)
                {
                    objectToActivateWhenTwoHanded.SetActive(false);
                    targetHand.objectToDisableWhenTwoHanded.SetActive(true);
                }
            }
        }
    }
}
