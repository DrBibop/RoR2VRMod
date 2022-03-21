using RoR2;
using RoR2.UI;
using UnityEngine;

namespace VRMod
{
    internal class SmoothHUD : MonoBehaviour
    {
        private Quaternion smoothHUDRotation;

        private Transform referenceTransform;

        private int lastUpdateFrame = 0;

        internal void Init(Transform referenceTransform)
        {
            this.referenceTransform = referenceTransform;

            smoothHUDRotation = referenceTransform.rotation;
        }

        private void OnEnable()
        {
            UICamera.onUICameraPreRender += UpdateHUD;
        }

        private void OnDisable()
        {
            if (referenceTransform)
            {
                smoothHUDRotation = referenceTransform.rotation;

                TransformRect();
            }

            UICamera.onUICameraPreRender -= UpdateHUD;
        }

        private void UpdateHUD(UICamera camera)
        {
            if (!referenceTransform || lastUpdateFrame == Time.renderedFrameCount) return;

            lastUpdateFrame = Time.renderedFrameCount;

            Quaternion deriv = new Quaternion();
            smoothHUDRotation = smoothHUDRotation.SmoothDamp(referenceTransform.rotation, ref deriv, 0.07f);

            TransformRect();

            if (!ModConfig.InitialMotionControlsValue && camera.cameraRigController && camera.cameraRigController.hud)
            {
                CrosshairManager crosshairManager = camera.cameraRigController.hud.GetComponent<CrosshairManager>();

                if (crosshairManager)
                {
                    crosshairManager.container.position = camera.transform.position + (camera.transform.forward * 12.35f);
                    crosshairManager.container.rotation = camera.transform.rotation;
                    crosshairManager.hitmarker.transform.position = crosshairManager.container.position;
                    crosshairManager.hitmarker.transform.rotation = camera.transform.rotation;
                }
            }
        }

        private void TransformRect()
        {
            transform.rotation = Quaternion.LookRotation(smoothHUDRotation * Vector3.forward, referenceTransform.up);
            transform.position = referenceTransform.position + (transform.forward * 12.35f);
        }
    }
}
