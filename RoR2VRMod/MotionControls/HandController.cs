using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace VRMod
{
    public class HandController : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer ray;

        [SerializeField]
        private Hand pointerHand;

        [SerializeField]
        internal HandHUD smallHud;

        [SerializeField]
        internal HandHUD watchHud;

        public Animator animator => currentHand.animator;

        public Ray aimRay
        {
            get
            {
                return new Ray(muzzle.position, muzzle.forward);
            }
        }

        internal Ray uiRay
        {
            get
            {
                return new Ray(uiHand.currentMuzzle.transform.position, uiHand.currentMuzzle.transform.forward);
            }
        }

        public Transform muzzle => currentHand.currentMuzzle.transform;

        internal Hand currentHand { get; private set; }

        internal XRNode xrNode;

        internal HandController oppositeHand;

        internal bool forceRay;

        private Hand uiHand;

        private bool _uiMode;

        private bool rayActive => currentHand.useRay || forceRay || uiMode;

        internal bool uiMode
        {
            get { return _uiMode; }
            set
            {
                if (_uiMode == value) return;

                _uiMode = value;

                if (_uiMode)
                {
                    ray.gameObject.layer = LayerIndex.ui.intVal;
                    ray.sortingOrder = 999;
                    currentHand.gameObject.SetActive(false);
                    uiHand.gameObject.SetActive(true);
                }
                else
                {
                    ray.gameObject.layer = 0;
                    ray.sortingOrder = 0;
                    currentHand.gameObject.SetActive(true);
                    uiHand.gameObject.SetActive(false);
                }
            }
        }

        private List<GameObject> handPrefabs;

        private List<Hand> instantiatedHands = new List<Hand>();

        private void Awake()
        {
            SetCurrentHand(pointerHand);

            uiHand = Object.Instantiate(pointerHand.gameObject).GetComponent<Hand>();
            uiHand.gameObject.name = string.Format("UI Hand ({0})", xrNode == XRNode.LeftHand ? "Left" : "Right");
            uiHand.gameObject.SetLayerRecursive(LayerIndex.ui.intVal);
            uiHand.gameObject.SetActive(false);

            ray.material.color = ModConfig.RayColor;
        }

        private void Update()
        {
            uiMode = Utils.isUIOpen;

            if (!uiMode && transform.parent != Camera.main.transform.parent)
                transform.SetParent(Camera.main.transform.parent);
            else if (uiMode && transform.parent)
                transform.SetParent(null);

            Vector3 handPosition = InputTracking.GetLocalPosition(xrNode);
            Quaternion handRotation = InputTracking.GetLocalRotation(xrNode);

            if (!ModConfig.InitialOculusModeValue)
            {
                handRotation *= Quaternion.Euler(Vector3.right * 40);
                handPosition += handRotation * Vector3.down * 0.03f;
                handPosition += handRotation * Vector3.back * 0.05f;
            }

            transform.localPosition = handPosition;
            transform.localRotation = handRotation;

            uiHand.transform.position = handPosition;
            uiHand.transform.rotation = handRotation;
        }

        private void LateUpdate()
        {
            if (!currentHand) return;

            if (ray.gameObject.activeSelf != rayActive)
                ray.gameObject.SetActive(rayActive);

            if (ray.gameObject.activeSelf)
            {
                ray.SetPosition(0, (uiMode ? uiHand.currentMuzzle.transform : muzzle).position);
                ray.SetPosition(1, GetRayHitPosition());
            }
        }

        /// <summary>
        /// Returns the muzzle transform at the specified index.
        /// </summary>
        /// <param name="index">The index of the muzzle.</param>
        /// <returns>The chosen muzzle's transform.</returns>
        public Transform GetMuzzleByIndex(uint index)
        {
            return currentHand.muzzles[index].transform;
        }

        internal void ResetToPointer()
        {
            SetCurrentHand(pointerHand);
        }

        internal void SetCurrentHand(string bodyName)
        {
            Hand matchingInstantiatedHands = instantiatedHands.FirstOrDefault((hand) => hand.bodyName == bodyName);

            if (matchingInstantiatedHands)
            {
                SetCurrentHand(matchingInstantiatedHands);
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

        private void SetCurrentHand(Hand hand)
        {
            if (currentHand)
                currentHand.gameObject.SetActive(false);

            currentHand = hand;
            currentHand.gameObject.SetActive(true);

            ray.gameObject.SetActive(rayActive);
        }

        private Vector3 GetRayHitPosition()
        {
            if (!currentHand)
                return Vector3.zero;

            Ray ray = uiMode ? uiRay : aimRay;

            LayerMask mask = uiMode ? LayerIndex.ui.mask : LayerIndex.ragdoll.mask;

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 300, mask))
            {
                return hitInfo.point;
            }

            return ray.origin + (ray.direction * 300);
        }

        internal void UpdateRayColor()
        {
            ray.material.color = ModConfig.RayColor;
        }
    }
}
