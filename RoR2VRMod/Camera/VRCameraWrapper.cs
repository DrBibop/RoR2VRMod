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
            while(cameraRigController.transform.childCount > 0)
            {
                Transform child = cameraRigController.transform.GetChild(cameraRigController.transform.childCount - 1);
                child.SetParent(transform);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
            }

            if (CameraFixes.liv)
            {
                CameraFixes.liv.stage = transform;
            }
        }

        internal void UpdateRotation(CameraState cameraState)
        {
            transform.rotation = cameraState.rotation;
        }

        internal CameraRigController cameraRigController { get; private set; }
    }
}
