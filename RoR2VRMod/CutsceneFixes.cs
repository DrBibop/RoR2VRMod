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

            On.RoR2.OutroCutsceneController.OnEnable += SetupOutro;
        }

        private static void SetupOutro(On.RoR2.OutroCutsceneController.orig_OnEnable orig, OutroCutsceneController self)
        {
            GameObject cameraRoot = GameObject.Find("CutsceneEnabledObjects");

            if (cameraRoot)
            {
                Transform cameraTransform = cameraRoot.transform.Find("Camera Matcher/Menu Main Camera");

                if (cameraTransform)
                {
                    CameraRigController cameraRig = cameraTransform.GetComponent<CameraRigController>();

                    GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                    FixOutroCanvas(allGameObjects, cameraRig);
                    AdjustOutroElements(allGameObjects, cameraRig);
                }
            }
        }

        private static void AdjustOutroElements(GameObject[] allGameObjects, CameraRigController cameraRig)
        {
            GameObject spaceShot = allGameObjects.First(x => x.name == "Set 2 - Space");

            if (spaceShot)
            {
                spaceShot.transform.Translate(20, 0, 0);

                Transform dropShipParent = spaceShot.transform.Find("Dropship, Space");

                if (dropShipParent)
                {
                    dropShipParent.localScale = Vector3.one;
                }
            }
        }

        private static void FixOutroCanvas(GameObject[] allGameObjects, CameraRigController cameraRig)
        {
            GameObject subtitlesCanvas = allGameObjects.First(x => x.name == "Set 5 - Canvas");
            
            if (subtitlesCanvas)
            {
                Canvas canvas = subtitlesCanvas.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                RectTransform rectTransform = subtitlesCanvas.transform as RectTransform;
                rectTransform.SetParent(cameraRig.sceneCam.transform);
                rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                
                rectTransform.sizeDelta = new Vector2(6000, 5000);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localPosition = new Vector3(0, 0, 15f);

                RectTransform textCanvas = subtitlesCanvas.GetComponentInChildren<RoR2.UI.OutroFlavorTextController>().transform as RectTransform;
                textCanvas.anchorMin = new Vector2(0.42f, 0.42f);
                textCanvas.anchorMax = new Vector2(0.58f, 0.58f);
                textCanvas.gameObject.AddComponent<SmoothHUD>().Init(canvas.worldCamera.transform);
            }
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

                    Vector3 offset = new Vector3(-0.3387751f, 0.1797891f, -0.5321155f) * 5 / 6;

                    if (ModConfig.InitialRoomscaleValue) offset.y -= ModConfig.PlayerHeight.Value / 6;

                    cameraRig.desiredCameraState = new CameraState
                    {
                        position = offset,
                        rotation = Quaternion.Euler(3.899f, -180, 0)
                    };

                    cameraRig.currentCameraState = cameraRig.desiredCameraState;

                    GameObject.Destroy(cameraRig.sceneCam.GetComponent<MatchCamera>());

                    Transform cameraParent = cameraRig.sceneCam.transform.parent;
                    GameObject newSceneCamObject = GameObject.Instantiate(cameraRig.sceneCam.gameObject, cameraParent.position, cameraParent.rotation, cameraParent);
                    GameObject.Destroy(cameraRig.sceneCam.gameObject);

                    Camera newCam = newSceneCamObject.GetComponent<Camera>();
                    newCam.cullingMask = newCam.cullingMask & ~(1 << LayerIndex.ui.intVal);
                    cameraRig.sceneCam = newCam;
                    cameraRig.sprintingParticleSystem = newSceneCamObject.GetComponentInChildren<ParticleSystem>();

                    if (CameraFixes.liv) CameraFixes.liv.HMDCamera = newCam;

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
                GameObject fadeObject = new GameObject("Black Fade");
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

                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = cameraRig.uiCam;
                RectTransform rectTransform = canvasObject.transform as RectTransform;
                rectTransform.sizeDelta = new Vector2(1200, 1000);
                rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f); ;
                rectTransform.SetParent(cameraRig.uiCam.transform);
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localPosition = new Vector3(0, 0, 12.35f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                canvasObject.AddComponent<SmoothHUD>().Init(cameraRig.uiCam.transform);

                canvasObject.transform.Find("MainArea/BlackBarParent").gameObject.SetActive(false);
            }
        }
    }
}
