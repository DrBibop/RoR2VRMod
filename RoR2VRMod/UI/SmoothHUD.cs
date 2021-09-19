using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;

namespace VRMod
{
    internal class SmoothHUD : MonoBehaviour
    {
        private Quaternion smoothHUDRotation;

        private CameraRigController cameraRig;

        internal void Init(CameraRigController cameraRig)
        {
            this.cameraRig = cameraRig;

            smoothHUDRotation = cameraRig.uiCam.transform.rotation;
        }

        private void LateUpdate()
        {
            //This slerp code block was taken from idbrii in Unity answers and from an article by Rory
            float delta = Quaternion.Angle(smoothHUDRotation, cameraRig.uiCam.transform.rotation);
            if (delta > 0f)
            {
                float t = Mathf.Lerp(delta, 0f, 1f - Mathf.Pow(0.03f, Time.unscaledDeltaTime));
                t = 1.0f - (t / delta);
                smoothHUDRotation = Quaternion.Slerp(smoothHUDRotation, cameraRig.uiCam.transform.rotation, t);
            }

            Transform mainContainer = cameraRig.hud.mainContainer.transform;

            mainContainer.rotation = smoothHUDRotation;
            mainContainer.rotation = Quaternion.LookRotation(mainContainer.forward, cameraRig.uiCam.transform.up);
            mainContainer.position = cameraRig.uiCam.transform.position + (mainContainer.forward * 12.35f);

            if (!ModConfig.UseMotionControls.Value)
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
    }
}
