using RoR2;
using RoR2.UI;
using UnityEngine;

namespace VRMod
{
    internal class SmoothHUD : MonoBehaviour
    {
        private Quaternion smoothHUDRotation;

        private CameraRigController cameraRig;

        private Transform referenceTransform;

        internal void Init(Transform referenceTransform, CameraRigController cameraRig = null)
        {
            this.cameraRig = cameraRig;
            this.referenceTransform = referenceTransform;

            smoothHUDRotation = referenceTransform.rotation;
        }

        private void OnDisable()
        {
            if (referenceTransform)
            {
                smoothHUDRotation = referenceTransform.rotation;

                TransformRect();
            }
        }

        private void LateUpdate()
        {
            if (!referenceTransform) return;

            //This slerp code block was taken from idbrii in Unity answers and from an article by Rory
            float delta = Quaternion.Angle(smoothHUDRotation, referenceTransform.rotation);
            if (delta > 0f)
            {
                float t = Mathf.Lerp(delta, 0f, 1f - Mathf.Pow(0.03f, Time.unscaledDeltaTime));
                t = 1.0f - (t / delta);
                smoothHUDRotation = Quaternion.Slerp(smoothHUDRotation, referenceTransform.rotation, t);
            }

            TransformRect();

            if (!ModConfig.InitialMotionControlsValue && cameraRig)
            {
                CrosshairManager crosshairManager = cameraRig.hud.GetComponent<CrosshairManager>();

                if (crosshairManager)
                {
                    crosshairManager.container.position = cameraRig.uiCam.transform.position + (cameraRig.uiCam.transform.forward * 12.35f);
                    crosshairManager.container.rotation = cameraRig.uiCam.transform.rotation;
                    crosshairManager.hitmarker.transform.position = crosshairManager.container.position;
                    crosshairManager.hitmarker.transform.rotation = cameraRig.uiCam.transform.rotation;
                }
            }
        }

        private void TransformRect()
        {
            transform.rotation = smoothHUDRotation;
            transform.rotation = Quaternion.LookRotation(transform.forward, referenceTransform.up);
            transform.position = referenceTransform.position + (transform.forward * 12.35f);
        }
    }
}
