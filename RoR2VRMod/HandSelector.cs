using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace VRMod
{
    internal class HandSelector : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer ray;

        [SerializeField]
        private Hand pointerHand;

        private List<GameObject> handPrefabs;

        internal Hand currentHand { get; private set; }

        private XRNode xrNode;

        private List<Hand> instantiatedHands = new List<Hand>();

        private void Awake()
        {
            SetCurrentHand(pointerHand);
        }

        private void Update()
        {
            if (transform.parent != Camera.main.transform.parent)
                transform.SetParent(Camera.main.transform.parent);

            transform.localPosition = InputTracking.GetLocalPosition(xrNode);
            transform.localRotation = InputTracking.GetLocalRotation(xrNode);

            if (currentHand && ray.gameObject.activeSelf)
            {
                ray.SetPosition(0, currentHand.muzzle.position);
                ray.SetPosition(1, GetRayHitPosition());
            }
        }

        internal void ResetToPointer()
        {
            SetCurrentHand(pointerHand);
        }

        internal void SetCurrentHand(string bodyName)
        {
            List<Hand> matchingInstantiatedHands = instantiatedHands.Where((hand) => hand.bodyName == bodyName).ToList();

            if (matchingInstantiatedHands.Count > 0)
            {
                SetCurrentHand(matchingInstantiatedHands.First());
                return;
            }

            foreach (GameObject handPrefab in handPrefabs)
            {
                Hand hand = handPrefab.GetComponent<Hand>();

                if (!hand)
                    continue;

                if (hand.bodyName == bodyName)
                {
                    Hand newHand = Object.Instantiate(handPrefab, transform).GetComponent<Hand>();
                    instantiatedHands.Add(newHand);

                    SetCurrentHand(newHand);

                    return;
                }
            }

            VRMod.StaticLogger.LogWarning(string.Format("Could not find hand with name \'{0}\'. Using default pointers.", bodyName));
        }

        internal void SetXRNode(XRNode node)
        {
            xrNode = node;
        }

        internal void SetPrefabs(List<GameObject> prefabs)
        {
            if (prefabs == null)
                return;

            handPrefabs = prefabs;
        }

        internal Ray GetRay()
        {
            return new Ray(currentHand.muzzle.position, currentHand.muzzle.forward);
        }

        private void SetCurrentHand(Hand hand)
        {
            if (currentHand)
                currentHand.gameObject.SetActive(false);

            currentHand = hand;
            currentHand.gameObject.SetActive(true);

            ray.gameObject.SetActive(currentHand.useRay);
        }

        private Vector3 GetRayHitPosition()
        {
            if (!currentHand)
                return Vector3.zero;

            RaycastHit hitInfo;
            if (Physics.Raycast(GetRay(), out hitInfo, 300))
            {
                return hitInfo.point;
            }
            return currentHand.muzzle.transform.position + (currentHand.muzzle.transform.forward * 300);
        }
    }
}
