using RoR2;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    internal static class Utils
    {
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
                    if (MotionControls.HandsReady)
                    {
                        _localBody = MotionControls.currentBody;
                    }
                    else
                    {
                        LocalUser user = LocalUserManager.GetFirstLocalUser();
                        if (user != null)
                            _localBody = user.cachedBody;
                    }
                }

                return _localBody;
            }
        }

        private static CameraRigController _localCameraRig;

        internal static CameraRigController localCameraRig
        {
            get
            {
                if (!_localCameraRig || !_localCameraRig.isActiveAndEnabled)
                {
                    _localCameraRig = CameraRigController.instancesList.FirstOrDefault(x => x.isActiveAndEnabled == true);
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

                    if (_localInputPlayer == null)
                        _localInputPlayer = LocalUserManager.GetRewiredMainPlayer();
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

        //Made by maxattack on GitHub
        internal static Quaternion SmoothDamp(this Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.unscaledDeltaTime < Mathf.Epsilon) return rot;
            if (time == 0f) return target;

            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;

            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time, int.MaxValue, Time.unscaledDeltaTime),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time, int.MaxValue, Time.unscaledDeltaTime)
            ).normalized;

            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }
    }
}
