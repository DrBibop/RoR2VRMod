using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using RoR2;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    internal static class CameraFixes
    {
        private static VRCameraWrapper camWrapper;

        private static float snapTurnYaw;

        private static bool isTurningLeft;
        private static bool wasTurningLeft;

        private static bool isTurningRight;
        private static bool wasTurningRight;

        private static bool justTurnedLeft => isTurningLeft && !wasTurningLeft;
        private static bool justTurnedRight => isTurningRight && !wasTurningRight;

        internal static void Init()
        {
            On.RoR2.MatchCamera.Awake += (orig, self) =>
            {
                self.matchFOV = false;
                orig(self);
            };

            IL.RoR2.CameraRigController.SetCameraState += RemoveFOVChange;
            On.RoR2.CameraRigController.SetCameraState += SetCameraStateOverride;


            if (ModConfig.FirstPerson.Value)
            {
                On.RoR2.Run.Update += SetBodyInvisible;

                On.RoR2.CameraRigController.OnDestroy += (orig, self) =>
                {
                    orig(self);
                    if (camWrapper)
                        Object.Destroy(camWrapper.gameObject);
                };
            }

            On.RoR2.CameraRigController.GetCrosshairRaycastRay += GetVRCrosshairRaycastRay;
        }

        private static void SetBodyInvisible(On.RoR2.Run.orig_Update orig, Run self)
        {
            Renderer[] renderers = PlayerCharacterMasterController.instances[0]?.master?.GetBody()?.modelLocator?.modelTransform?.gameObject.GetComponentsInChildren<Renderer>(true);
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.gameObject.activeSelf)
                    {
                        renderer.gameObject.SetActive(false);
                    }
                }
            }
            orig(self);
        }

        private static void SetCameraStateOverride(On.RoR2.CameraRigController.orig_SetCameraState orig, CameraRigController self, CameraState cameraState)
        {
            if (Run.instance && self.cameraMode == CameraRigController.CameraMode.PlayerBasic)
            {
                if (ModConfig.SnapTurn.Value && ModConfig.FirstPerson.Value)
                {
                    Player player = self.localUserViewer?.inputPlayer;

                    if (player != null)
                    {
                        float joystickAxis = player.GetAxisRaw(16);
                        float scrollAxis = player.GetAxisRaw(26);

                        wasTurningLeft = isTurningLeft;
                        wasTurningRight = isTurningRight;

                        isTurningLeft = joystickAxis < -0.8f || scrollAxis < -0.8f;
                        isTurningRight = joystickAxis > 0.8f || scrollAxis > 0.8f;

                        if (justTurnedLeft)
                            snapTurnYaw = (snapTurnYaw - ModConfig.SnapTurnAngle.Value) % 360;
                        else if (justTurnedRight)
                            snapTurnYaw = (snapTurnYaw + ModConfig.SnapTurnAngle.Value) % 360;

                        cameraState.rotation = Quaternion.Euler(0, snapTurnYaw, 0);
                    }
                }
                else if (ModConfig.LockedCameraPitch.Value)
                {
                    cameraState.rotation = Quaternion.Euler(0, self.yaw, 0);
                }

                if (ModConfig.FirstPerson.Value)
                {
                    if (!camWrapper)
                    {
                        GameObject wrapperObject = new GameObject("VR Camera Wrapper");
                        camWrapper = wrapperObject.AddComponent<VRCameraWrapper>();
                        camWrapper.Init(self);
                    }
                    camWrapper.ManualUpdate(cameraState);

                    cameraState.rotation = self.sceneCam.transform.rotation;

                    Vector3 pos = camWrapper.transform.position;

                    if (self.targetBody)
                    {
                        pos = self.targetBody.transform.position;

                        CapsuleCollider collider = self.targetBody.GetComponent<CapsuleCollider>();

                        if (collider)
                            pos.y += (collider.height) / 2;
                    }

                    camWrapper.transform.position = pos;
                }
            }

            orig(self, cameraState);
        }

        private static void RemoveFOVChange(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<CameraState>("fov")
                );

            ILLabel breakLabel = c.IncomingLabels.ToList().First<ILLabel>();

            while (c.Next.OpCode.Code != Code.Ret)
            {
                c.Remove();
            }

            ILLabel retLabel = c.MarkLabel();

            c.GotoPrev(
                x => x.MatchBr(breakLabel)
                );
            c.Remove();
            c.Emit(OpCodes.Br_S, retLabel);
        }

        private static Ray GetVRCrosshairRaycastRay(On.RoR2.CameraRigController.orig_GetCrosshairRaycastRay orig, RoR2.CameraRigController self, Vector2 crosshairOffset, Vector3 raycastStartPlanePoint)
        {
            if (!self.sceneCam)
            {
                return default(Ray);
            }
            float fieldOfView = self.sceneCam.fieldOfView;
            float num = fieldOfView * self.sceneCam.aspect;
            Quaternion quaternion = Quaternion.Euler(crosshairOffset.y * fieldOfView, crosshairOffset.x * num, 0f);
            quaternion = self.sceneCam.transform.rotation * quaternion;
            return new Ray(Vector3.ProjectOnPlane(self.sceneCam.transform.position - raycastStartPlanePoint, self.sceneCam.transform.rotation * Vector3.forward) + raycastStartPlanePoint, quaternion * Vector3.forward);
        }
    }
}
