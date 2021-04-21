using UnityEngine;
using UnityEngine.XR;

namespace VRMod
{
    public class HandSelector : MonoBehaviour
    {
        [SerializeField]
        private Hand[] hands;

        [SerializeField]
        private LineRenderer ray;

        private XRNode xrNode;

        public Hand currentHand { get; private set; }

        private void Update()
        {
            if (transform.parent != Camera.main.transform.parent)
                transform.SetParent(Camera.main.transform.parent);

            transform.localPosition = InputTracking.GetLocalPosition(xrNode);
            transform.localRotation = InputTracking.GetLocalRotation(xrNode);

            if (currentHand && ray.gameObject.activeSelf)
            {
                ray.SetPosition(0, currentHand.aimOrigin.position);
                ray.SetPosition(1, GetRayHitPosition());
            }
        }

        public void SetCurrentHand(HandType type)
        {
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i].handType == type)
                {
                    if (currentHand)
                        currentHand.gameObject.SetActive(false);

                    currentHand = hands[i];
                    currentHand.gameObject.SetActive(true);
                    break;
                }
            }
        }

        public void ShowRay(bool active)
        {
            ray.gameObject.SetActive(active);
        }

        public Vector3 GetRayHitPosition()
        {
            if (!currentHand)
                return Vector3.zero;

            RaycastHit hitInfo;
            if (Physics.Raycast(currentHand.aimOrigin.transform.position, currentHand.aimOrigin.transform.forward, out hitInfo, 300))
            {
                return hitInfo.point;
            }
            return currentHand.aimOrigin.transform.position + (currentHand.aimOrigin.transform.forward * 300);
        }

        public void SetXRNode(XRNode node)
        {
            xrNode = node;
        }
    }
}
