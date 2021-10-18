using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace VRMod
{
    internal static class CutsceneFixes
    {
        internal static void Init()
        {
            On.RoR2.StartEvent.Start += SetupIntro;
        }

        private static void SetupIntro(On.RoR2.StartEvent.orig_Start orig, StartEvent self)
        {
            if (self.GetComponent<IntroCutsceneController>() != null)
            {
                GameObject cameraRigObject = GameObject.Find("Menu Main Camera");

                if (cameraRigObject)
                {
                    cameraRigObject.transform.localScale /= 6;

                    CameraRigController cameraRig = cameraRigObject.GetComponent<CameraRigController>();

                    Vector3 cameraOffset = new Vector3(-0.3387751f, 0.1797891f, -0.5321155f) * 5 / 6;
                    if (ModConfig.InitialRoomscaleValue) cameraOffset.y -= ModConfig.PlayerHeight.Value / 6;

                    cameraRig.desiredCameraState = new CameraState
                    {
                        position = cameraOffset,
                        rotation = Quaternion.identity
                    };

                    FixIntroCanvas(cameraRig);
                    AdjustIntroElements(cameraRig);
                }
            }
            orig(self);
        }

        private static void AdjustIntroElements(CameraRigController cameraRig)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            GameObject shipShot = allGameObjects.First(x => x.name == "cutscene intro");
            GameObject shipBackground = allGameObjects.First(x => x.name == "Set 1 - Space");

            if (shipShot && shipBackground)
            {
                shipShot.transform.Rotate(Vector3.up, -60);
                shipShot.transform.localScale = Vector3.one * 50;
                shipBackground.transform.Rotate(Vector3.up, -60);
            }

            GameObject captainShot = allGameObjects.First(x => x.name == "Set 2 - Cabin");

            if (captainShot)
            {
                Transform cabin = captainShot.transform.Find("CabinPosition");

                if (cabin) cabin.localScale = Vector3.one * 20;
            }

            GameObject spaceShot = allGameObjects.First(x => x.name == "Set 3 - Space, Small Planet");

            if (spaceShot)
            {
                spaceShot.transform.Rotate(Vector3.up, -60);
            }

            GameObject cargoShot = allGameObjects.First(x => x.name == "Set 4 - Cargo");

            if (cargoShot)
            {
                Transform pp = cargoShot.transform.Find("PP");

                PostProcessVolume ppVolume = pp.GetComponent<PostProcessVolume>();
                ppVolume.profile.RemoveSettings<DepthOfField>();
            }
        }

        private static void FixIntroCanvas(CameraRigController cameraRig)
        {
            GameObject canvasObject = GameObject.Find("Canvas");

            if (canvasObject)
            {
                GameObject fadeObject = new GameObject();
                fadeObject.layer = LayerIndex.ui.intVal;
                Canvas fadeCanvas = fadeObject.AddComponent<Canvas>();
                fadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                fadeCanvas.worldCamera = cameraRig.uiCam;
                fadeCanvas.planeDistance = 13;
                fadeObject.AddComponent<CanvasScaler>();

                Transform fade = canvasObject.transform.Find("MainArea/Fade");
                fade.SetParent(fadeObject.transform);
                fade.localPosition = Vector3.zero;
                fade.localRotation = Quaternion.identity;
                fade.localScale = Vector3.one * 3;

                canvasObject.transform.SetParent(cameraRig.uiCam.transform);

                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                RectTransform rectTransform = canvasObject.transform as RectTransform;
                rectTransform.sizeDelta = new Vector2(1200, 1000);
                rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f); ;
                rectTransform.SetParent(cameraRig.uiCam.transform);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localPosition = new Vector3(0, 0, 12.35f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                canvasObject.AddComponent<SmoothHUD>().Init(cameraRig);

                canvasObject.transform.Find("MainArea/BlackBarParent").gameObject.SetActive(false);
            }
        }
    }
}
