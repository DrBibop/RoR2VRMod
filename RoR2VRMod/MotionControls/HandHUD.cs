using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace VRMod
{
    internal class HandHUD : MonoBehaviour
    {
        [SerializeField]
        private bool faceCamera;

        [SerializeField]
        private bool hideWhenFacingAway;

        [SerializeField]
        private GameObject[] hudsToHideWhenVisible;

        private Canvas canvas;

        private CameraRigController cameraRig;

        private List<RectTransform> hudClusters = new List<RectTransform>();

        private Vector3 origScale;

        private float scaleLerp = 1f;

        private void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            canvas.transform.SetParent(null);

            origScale = transform.localScale;
        }

        private void OnDestroy()
        {
            if (canvas)
                GameObject.Destroy(canvas.gameObject);
        }

        internal void Init(CameraRigController rig)
        {
            cameraRig = rig;
            canvas.worldCamera = cameraRig.uiCam;
        }

        public void AddHUDCluster(RectTransform hudClusterTransform)
        {
            if (hudClusterTransform)
            {
                hudClusters.Add(hudClusterTransform);
                hudClusterTransform.SetParent(canvas.transform);
                hudClusterTransform.localPosition = Vector3.zero;
                hudClusterTransform.localRotation = Quaternion.identity;
                hudClusterTransform.localScale = Vector3.one;
            }
        }

        private void LateUpdate()
        {
            if (!cameraRig || !VRCameraWrapper.instance) return;

            Vector3 position = VRCameraWrapper.instance.transform.InverseTransformPoint(transform.position);
            canvas.transform.position = position;
            canvas.transform.rotation = faceCamera ? Quaternion.LookRotation(position - cameraRig.uiCam.transform.position, cameraRig.uiCam.transform.up) : Quaternion.LookRotation(VRCameraWrapper.instance.transform.InverseTransformVector(transform.forward), VRCameraWrapper.instance.transform.InverseTransformVector(transform.up));

            if (hideWhenFacingAway && cameraRig && canvas)
            {
                bool show = Vector3.Angle(transform.forward, transform.position - cameraRig.sceneCam.transform.position) <= 45;

                if (scaleLerp != (show ? 1f : 0f))
                {
                    scaleLerp = Mathf.Clamp(scaleLerp + (show ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 6, 0f, 1f);

                    canvas.transform.localScale = Vector3.Lerp(Vector3.zero, origScale, scaleLerp);

                    foreach (GameObject hud in hudsToHideWhenVisible)
                    {
                        if (hud.activeSelf == show)
                        {
                            hud.SetActive(!show);
                        }
                    }
                }
            }
        }
    }
}
