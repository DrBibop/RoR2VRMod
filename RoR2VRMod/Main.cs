using BepInEx;
using HarmonyLib;
using R2API.Utils;
using RoR2;
using System;
using System.Globalization;
using System.IO;
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
    [BepInPlugin("com.DrBibop.VRFixes", "VR Fixes", "1.0.0")]
    public class VRFixes : BaseUnityPlugin
    {
        private void Awake()
        {
            On.RoR2.UI.HUD.Awake += AdjustHUDAnchors;
            On.RoR2.CameraRigController.GetCrosshairRaycastRay += GetVRCrosshairRaycastRay;

            //On.RoR2.UI.MainMenu.MainMenuController.Start += EnableVR;
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

        /*
        private void EnableVR(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);

            //Credit to x753 for the bytes operations

            string[] oldHexStrings = new string[]
            {
                "0000049000588F8800000011000010000000000032303138",
                "726C642E756E6974790000000000000000000000",
                "9C1401000A0000000C0000000000000050600200940000000B0000000D00000000000000E8600200080D56000C0000000E00000000000000F06D5800840100000D0000000F00000000000000786F5800140100000E000000110000000000000090705800EC0D00000F0000001200000000000000807E5800D4000000100000001300000000000000587F58000000000011000000140000000000000058"
            };

            string[] newHexStrings = new string[]
            {
                "0000049000588FA800000011000010000000000032303138",
                "726C642E756E6974790000000000000003000000060000004F63756C75730000060000004F70656E56520000040000004E6F6E65",
                "BC1401000A0000000C0000000000000070600200940000000B0000000D0000000000000008610200080D56000C0000000E00000000000000106E5800840100000D0000000F00000000000000986F5800140100000E0000001100000000000000B0705800EC0D00000F0000001200000000000000A07E5800D4000000100000001300000000000000787F58000000000011000000140000000000000078"
            };

            byte[] allBytes = File.ReadAllBytes(Environment.CurrentDirectory + @"\Risk of Rain 2_Data\globalgamemanagers");

            for (int i = 0; i < 3; i++)
            {
                byte[] oldBytePattern = BytePatternUtilities.ConvertHexStringToByteArray(oldHexStrings[i]);

                byte[] newBytePattern = BytePatternUtilities.ConvertHexStringToByteArray(newHexStrings[i]);

                allBytes = BytePatternUtilities.ReplaceBytes(allBytes, oldBytePattern, newBytePattern);

                if (allBytes == null)
                    break;
            }

            if (allBytes != null)
            {
                File.WriteAllBytes(Environment.CurrentDirectory + @"\Risk of Rain 2_Data\globalgamemanagers", resultBytes);
                GameObject restartWarning = new GameObject("RestartWarning");
                restartWarning.AddComponent<RestartWarning>();
                DontDestroyOnLoad(restartWarning);
            }
            else
            {
                var harmony = new Harmony("com.DrBibop.VRFixes");
                harmony.PatchAll();
            }
        }
        */

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

    /*
    public class RestartWarning : MonoBehaviour
    {
        void OnGUI()
        {
            GUI.Label(new Rect(0f, 0f, 600f, 80f), "The Unity Audio Engine has been restored, please restart the game once for it to take effect.", GUI.skin.label);
        }
    }
    

    //By x753
    public static class BytePatternUtilities
    {
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            byte[] data = new byte[hexString.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                string byteValue = hexString.Substring(i * 2, 2);
                data[i] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        private static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            byte[] temp = null;
            int index = FindBytes(src, search);
            while (index >= 0)
            {
                if (temp == null)
                    temp = src;
                else
                    temp = dst;

                dst = new byte[temp.Length - search.Length + repl.Length];

                Buffer.BlockCopy(temp, 0, dst, 0, index);
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                Buffer.BlockCopy(temp, index + search.Length, dst, index + repl.Length, temp.Length - (index + search.Length));

                index = FindBytes(dst, search);
            }
            return dst;
        }
    }
    */
}