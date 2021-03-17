using BepInEx;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace DrBibop
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "1.0.0")]
    public class VRMod : BaseUnityPlugin
    {
        private void Awake()
        {
            On.RoR2.UI.HUD.Awake += AdjustHUDAnchors;
            On.RoR2.CameraRigController.GetCrosshairRaycastRay += GetVRCrosshairRaycastRay;
            On.RoR2.MatchCamera.Awake += (orig, self) =>
            {
                self.matchFOV = false;
                orig(self);
            };
            IL.RoR2.CameraRigController.SetCameraState += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<CameraState>("fov")
                    );
                
                ILLabel breakLabel = c.IncomingLabels.ToList().First<ILLabel>();

                while(c.Next.OpCode.Code != Mono.Cecil.Cil.Code.Ret)
                {
                    c.Remove();
                }

                ILLabel retLabel = c.MarkLabel();

                c.GotoPrev(
                    x => x.MatchBr(breakLabel)
                    );
                c.Remove();
                c.Emit(Mono.Cecil.Cil.OpCodes.Br_S, retLabel);
            };
        }

        private void AdjustHUDAnchors(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            
            RectTransform mainRect = self.mainContainer.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.2f, 0.25f);
            mainRect.anchorMax = new Vector2(0.8f, 0.75f);
            CanvasScaler scaler = self.canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.scaleFactor = 0.8f;

            Transform uiArea = mainRect.transform.Find("MainUIArea");

            if (uiArea)
            {
                Transform[] uiElementsToLower = new Transform[3]
                {
                    uiArea.Find("UpperRightCluster"),
                    uiArea.Find("UpperLeftCluster"),
                    uiArea.Find("TopCenterCluster")
                };

                foreach (Transform uiTransform in uiElementsToLower)
                {
                    if (!uiTransform)
                        continue;

                    RectTransform rect = uiTransform.GetComponent<RectTransform>();
                    Vector3 newPos = rect.position;
                    newPos.y -= 150;
                    rect.position = newPos;
                }
            }
        }

        private Ray GetVRCrosshairRaycastRay(On.RoR2.CameraRigController.orig_GetCrosshairRaycastRay orig, RoR2.CameraRigController self, Vector2 crosshairOffset, Vector3 raycastStartPlanePoint)
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