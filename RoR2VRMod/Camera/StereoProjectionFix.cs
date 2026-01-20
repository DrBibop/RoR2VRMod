using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;

namespace VRMod
{
    /// <summary>
    /// Fixes stereo projection matrices by maintaining a reference camera that has
    /// correct VR projection matrices, then copying them to the main camera.
    /// This prevents the game from overriding the headset's projection matrices.
    /// </summary>
    internal class StereoProjectionFix : MonoBehaviour
    {
        private Camera _mainCamera;
        private Camera _referenceCamera;
        private GameObject _referenceCameraObject;

        private void Awake()
        {
            _mainCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            // Create a reference camera that will maintain correct VR stereo projection matrices
            _referenceCameraObject = new GameObject("VR Stereo Reference Camera");
            _referenceCameraObject.transform.SetParent(transform, false);
            _referenceCameraObject.transform.localPosition = Vector3.zero;
            _referenceCameraObject.transform.localRotation = Quaternion.identity;

            _referenceCamera = _referenceCameraObject.AddComponent<Camera>();
            _referenceCamera.CopyFrom(_mainCamera);

            // Disable rendering on the reference camera - it's only used to get correct matrices
            _referenceCamera.cullingMask = 0;
            _referenceCamera.clearFlags = CameraClearFlags.Nothing;
            _referenceCamera.depth = -100;
            _referenceCamera.enabled = false; // Don't render anything

            // Add pose tracking to reference camera so it gets correct VR matrices
            TrackedPoseDriver poseDriver = _referenceCameraObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Head);
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
        }

        private void OnPreCull()
        {
            ApplyCorrectProjectionMatrices();
        }

        private void OnPreRender()
        {
            ApplyCorrectProjectionMatrices();
        }

        private void ApplyCorrectProjectionMatrices()
        {
            if (_mainCamera == null || _referenceCamera == null || !XRSettings.enabled)
                return;

            // Temporarily enable reference camera to get its matrices
            _referenceCamera.enabled = true;

            // Get the current eye being rendered
            var eye = _mainCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left
                ? Camera.StereoscopicEye.Left
                : Camera.StereoscopicEye.Right;

            // Apply the correct stereo projection matrix from our reference camera
            _mainCamera.projectionMatrix = _referenceCamera.GetStereoProjectionMatrix(eye);

            _referenceCamera.enabled = false;
        }

        private void OnDestroy()
        {
            if (_referenceCameraObject != null)
            {
                Destroy(_referenceCameraObject);
            }
        }
    }
}
