using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

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

        public CharacterModel.RendererInfo[] rendererInfos => currentHand.rendererInfos;

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

        internal bool hasAimableEquipment;

        internal bool hasAimableHeresySkill;

        internal bool stabilisePosition;

        private Hand uiHand;

        private bool _uiMode;

        private Vector3 lastPosition;

        private Quaternion lastRotation;

        private bool rayActive => uiMode ? (this == MotionControls.GetHandByDominance(true)) : (currentHand.useRay || hasAimableHeresySkill || (hasAimableEquipment && MotionControls.currentBody?.equipmentSlot?.stock > 0));

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
                    Utils.SetLayerRecursive(currentHand.gameObject, LayerIndex.noDraw.intVal);
                    uiHand.gameObject.SetActive(isActiveAndEnabled);
                    ray.material.color = Color.white;
                }
                else
                {
                    ray.gameObject.layer = 0;
                    ray.sortingOrder = 0;
                    Utils.SetLayerRecursive(currentHand.gameObject, 0);
                    uiHand.gameObject.SetActive(false);
                    UpdateRayColor();
                }
            }
        }

        private List<GameObject> handPrefabs;

        private void Awake()
        {
            SetCurrentHand(pointerHand);

            uiHand = Object.Instantiate(pointerHand.gameObject).GetComponent<Hand>();
            uiHand.gameObject.SetLayerRecursive(LayerIndex.ui.intVal);
            uiHand.useRay = false;
            uiHand.gameObject.SetActive(false);

            ray.material.color = ModConfig.RayColor;

            lastPosition = Vector3.zero;
            lastRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            if (uiMode && uiHand && !uiHand.isActiveAndEnabled)
                uiHand.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (uiHand && uiHand.isActiveAndEnabled)
                uiHand.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (uiHand)
                uiHand.gameObject.name = string.Format("UI Hand ({0})", xrNode == XRNode.LeftHand ? "Left" : "Right");
        }

        private void OnDestroy()
        {
            if(uiHand)
                GameObject.Destroy(uiHand.gameObject);
        }

        private void Update()
        {
            uiMode = Utils.isUsingUI;

            if (transform.parent != Camera.main.transform.parent)
                transform.SetParent(Camera.main.transform.parent);

            Vector3 handPosition = Vector3.zero;
            Quaternion handRotation = Quaternion.identity;

            InputDevice device = InputDevices.GetDeviceAtXRNode(xrNode);

            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 controllerPosition))
            {
                handPosition = controllerPosition;
            }

            if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion controllerRotation))
            {
                handRotation = controllerRotation;
            }

            handRotation *= Quaternion.Euler(Vector3.right * 40);
            handPosition += handRotation * Vector3.down * 0.03f;
            handPosition += handRotation * Vector3.back * 0.05f;

            if (!uiMode && stabilisePosition)
            {
                Vector3 posVel = new Vector3();
                handPosition = Vector3.SmoothDamp(lastPosition, handPosition, ref posVel, 0.05f * ModConfig.AimStabiliserAmount.Value, int.MaxValue, Time.unscaledDeltaTime);
            }

            Quaternion deriv = new Quaternion();
            handRotation = lastRotation.SmoothDamp(handRotation, ref deriv, 0.05f * ModConfig.AimStabiliserAmount.Value);

            transform.localPosition = handPosition;
            transform.localRotation = handRotation;

            uiHand.transform.position = handPosition;
            uiHand.transform.rotation = handRotation;

            lastPosition = handPosition;
            lastRotation = handRotation;
        }

        private void LateUpdate()
        {
            if (!currentHand) return;

            bool active = rayActive;

            if (ray.gameObject.activeSelf != active)
                ray.gameObject.SetActive(active);

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

        internal void SetCurrentHand(string bodyName)
        {
            foreach (GameObject handPrefab in handPrefabs)
            {
                Hand hand = handPrefab.GetComponent<Hand>();

                if (!hand)
                    continue;

                if (hand.bodyName == bodyName)
                {
                    Hand newHand = Object.Instantiate(handPrefab, transform).GetComponent<Hand>();
                    SetCurrentHand(newHand);

                    return;
                }
            }

            VRMod.StaticLogger.LogWarning(string.Format("Could not find hand with name \'{0}\'. This character is likely not VR supported and some abilities might not work as intended. Using default pointer.", bodyName));
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
                GameObject.Destroy(currentHand.gameObject);

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
