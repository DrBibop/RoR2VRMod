using System.Linq;
using UnityEngine;

namespace VRMod
{
    internal class FocusChecker : MonoBehaviour
    {
        internal static FocusChecker instance;

        static GameObject focusCanvasPrefab;

        Canvas focusCanvas;

        internal static void Init()
        {
            focusCanvasPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("FocusCanvas");
            RoR2.RoR2Application.onLoad += () =>
            {
                RoR2.RoR2Application.instance.gameObject.AddComponent<FocusChecker>();
            };
        }

        private void Awake()
        {
            instance = this;

            if (!Application.isFocused && !focusCanvas)
            {
                CreateCanvas();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && !focusCanvas && focusCanvasPrefab)
            {
                CreateCanvas();
            }

            if (focusCanvas) focusCanvas.gameObject.SetActive(!focused);
        }

        private void CreateCanvas()
        {
            focusCanvas = GameObject.Instantiate(focusCanvasPrefab).GetComponent<Canvas>();
            GameObject.DontDestroyOnLoad(focusCanvas.gameObject);
            focusCanvas.gameObject.AddComponent<SmoothHUD>();
            UpdateCameraRig(RoR2.CameraRigController.instancesList.First());
        }

        internal void UpdateCameraRig(RoR2.CameraRigController rig)
        {
            if (!rig || !focusCanvas) return;

            focusCanvas.worldCamera = rig.uiCam;
            focusCanvas.GetComponent<SmoothHUD>().Init(rig.uiCam.transform);
        }
    }
}
