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

        [SerializeField]
        internal HandHUD smallHud;

        [SerializeField]
        internal HandHUD watchHud;

        internal Hand currentHand { get; private set; }

        internal XRNode xrNode;

        internal HandSelector oppositeHand;

        internal bool forceRay;

        private List<GameObject> handPrefabs;

        private List<Hand> instantiatedHands = new List<Hand>();

        private void Awake()
        {
            SetCurrentHand(pointerHand);
            ray.material.color = ModConfig.RayColor;
        }

        private void Update()
        {
            if (transform.parent != Camera.main.transform.parent)
                transform.SetParent(Camera.main.transform.parent);

            transform.localPosition = InputTracking.GetLocalPosition(xrNode);
            transform.localRotation = InputTracking.GetLocalRotation(xrNode);

            if (XRSettings.loadedDeviceName == "OpenVR")
            {
                transform.Rotate(new Vector3(35, 0, 0), Space.Self);
                transform.Translate(new Vector3(0, -0.04f, -0.02f), Space.Self);
            }
        }

        private void LateUpdate()
        {
            if (!currentHand) return;

            if (!forceRay)
            {
                if (ray.gameObject.activeSelf != currentHand.useRay)
                    ray.gameObject.SetActive(currentHand.useRay);
            }
            else if (!ray.gameObject.activeSelf)
            {
                ray.gameObject.SetActive(true);
            }

            if (ray.gameObject.activeSelf)
            {
                ray.SetPosition(0, currentHand.currentMuzzle.transform.position);
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

            VRMod.StaticLogger.LogWarning(string.Format("Could not find hand with name \'{0}\'. Using default pointer.", bodyName));
        }

        internal void SetPrefabs(List<GameObject> prefabs)
        {
            if (prefabs == null)
                return;

            handPrefabs = prefabs;
        }

        internal Ray GetRay()
        {
            Transform muzzle = currentHand.currentMuzzle.transform;
            return new Ray(muzzle.position, muzzle.forward);
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
            if (Physics.Raycast(GetRay(), out hitInfo, 300, LayerMask.GetMask("Ragdoll")))
            {
                return hitInfo.point;
            }

            Transform muzzle = currentHand.currentMuzzle.transform;
            return muzzle.position + (muzzle.forward * 300);
        }
    }
}
