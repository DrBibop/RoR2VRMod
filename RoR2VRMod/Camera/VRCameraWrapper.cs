using Rewired;
using RoR2;
using UnityEngine;

namespace VRMod
{
    internal class VRCameraWrapper : MonoBehaviour
    {
        public static VRCameraWrapper instance;


        public void Init(CameraRigController cameraRigController)
        {
            instance = this;
            this.cameraRigController = cameraRigController;
            Vector3 localCamPos = cameraRigController.sceneCam.transform.localPosition;
            while(cameraRigController.transform.childCount > 0)
            {
                cameraRigController.transform.GetChild(cameraRigController.transform.childCount - 1).SetParent(transform);
            }

            cameraRigController.sceneCam.transform.localPosition = localCamPos;
        }

        internal void UpdateRotation(CameraState cameraState)
        {
            transform.rotation = cameraState.rotation;
        }

        internal CameraRigController cameraRigController { get; private set; }
    }
}
