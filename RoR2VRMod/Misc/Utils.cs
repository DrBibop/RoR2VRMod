using RoR2;
using UnityEngine;

namespace VRMod
{
    internal static class Utils
    {
        internal static bool isPickerPanelOpen;

        internal static bool isUsingUI => !Run.instance || MPEventSystemManager.primaryEventSystem.isCursorVisible || !MotionControls.currentBody;

        private static CharacterMaster _localMaster;

        internal static CharacterMaster localMaster
        {
            get
            {

                if (!_localMaster)
                {
                    LocalUser user = LocalUserManager.GetFirstLocalUser();
                    if (user != null)
                        _localMaster = user.cachedMaster;
                }

                return _localMaster;
            }
        }

        private static CharacterBody _localBody;

        internal static CharacterBody localBody
        {
            get
            {
                if (!_localBody)
                {
                    LocalUser user = LocalUserManager.GetFirstLocalUser();
                    if (user != null)
                        _localBody = user.cachedBody;
                }

                return _localBody;
            }
        }

        private static CameraRigController _localCameraRig;

        internal static CameraRigController localCameraRig
        {
            get
            {
                if (!_localCameraRig)
                {
                    LocalUser user = LocalUserManager.GetFirstLocalUser();
                    if (user != null)
                        _localCameraRig = user.cameraRigController;
                }

                return _localCameraRig;
            }
        }

        private static Rewired.Player _localInputPlayer;

        internal static Rewired.Player localInputPlayer
        {
            get
            {
                if (_localInputPlayer == null)
                {
                    LocalUser user = LocalUserManager.GetFirstLocalUser();
                    if (user != null)
                        _localInputPlayer = user.inputPlayer;
                }

                return _localInputPlayer;
            }
        }

        private static UserProfile _localUserProfile;

        internal static UserProfile localUserProfile
        {
            get
            {
                if (_localUserProfile == null)
                {
                    LocalUser user = LocalUserManager.GetFirstLocalUser();
                    if (user != null)
                        _localUserProfile = user.userProfile;
                }

                return _localUserProfile;
            }
        }

        internal static void SetLayerRecursive(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursive(layer);
            }
        }

        internal static bool IsLocalBody(this CharacterBody body)
        {
            return body && body == localBody;
        }

        internal static bool IsLocalMaster(this CharacterMaster master)
        {
            return master && master == localMaster;
        }
    }
}
