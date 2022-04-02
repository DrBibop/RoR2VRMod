using HG;
using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace VRMod
{
    internal class SniperScopeController : MonoBehaviour
    {
        [SerializeField]
        private Transform scopeCameraParent;

        [SerializeField]
        private RenderTexture scopeRenderTexture;

        [SerializeField]
        private MeshRenderer scopeViewRenderer;

        [SerializeField]
        private Canvas overlayCanvas;

        private Camera scopeCamera;

        private Camera scopeUICamera;

        private Material renderMaterial;

        private float targetFOV;

        private PointViewer pointViewer;

        private RenderTexture scopeUIRenderTexture;

        private static Material clearMaterial;

        private static Material uiTransparentMaterial;

        private void Awake()
        {
            UpdateDominance(ModConfig.LeftDominantHand.Value);

            renderMaterial = scopeViewRenderer.material;

            if (!clearMaterial)
            {
                clearMaterial = VRMod.VRAssetBundle.LoadAsset<Material>("ScopeViewCleared");
            }

            if (!uiTransparentMaterial)
            {
                uiTransparentMaterial = VRMod.VRAssetBundle.LoadAsset<Material>("UnlitTransparentMat");
            }

            overlayCanvas.transform.SetParent(null);
            overlayCanvas.planeDistance = 12.5f;
            overlayCanvas.gameObject.SetLayerRecursive(LayerIndex.enemyBody.intVal);

            scopeUIRenderTexture = new RenderTexture(scopeRenderTexture);
            scopeUIRenderTexture.Create();

            scopeViewRenderer.material = clearMaterial;

            On.RoR2.UI.PointViewer.FindCamera += DidntAsk;
            On.RoR2.UI.PointViewer.UpdateAllElementPositions += ReplaceWithScopeCameras;
            On.RoR2.UI.PointViewer.AddElement += ReplaceLayer;

            RoR2Application.onLateUpdate += FindAndInstantiateCamera;
        }

        private void FindAndInstantiateCamera()
        {
            if (Utils.localCameraRig && Utils.localCameraRig.hud)
            {
                Camera cameraReference = Utils.localCameraRig.sceneCam;

                bool cameraReferenceEnabled = cameraReference.enabled;
                if (cameraReferenceEnabled)
                {
                    cameraReference.enabled = false;
                }
                bool cameraReferenceActive = cameraReference.gameObject.activeSelf;
                if (cameraReferenceActive)
                {
                    cameraReference.gameObject.SetActive(false);
                }

                scopeCamera = GameObject.Instantiate(cameraReference.gameObject, null).GetComponent<Camera>();
                Component[] components = scopeCamera.GetComponents<Component>();
                Behaviour.Destroy(scopeCamera.GetComponent<OutlineHighlight>());
                foreach (Component component in components)
                {
                    if (!(component is Transform) && !(component is Camera) && !(component is PostProcessLayer) && !(component is RoR2.PostProcess.SobelCommandBuffer && !(component is ThreeEyedGames.DecaliciousRenderer)))
                    {
                        Component.Destroy(component);
                    }
                }

                scopeCamera.stereoTargetEye = StereoTargetEyeMask.None;

                if (cameraReferenceActive != cameraReference.gameObject.activeSelf)
                {
                    cameraReference.gameObject.SetActive(cameraReferenceActive);
                }
                if (cameraReferenceEnabled != cameraReference.enabled)
                {
                    cameraReference.enabled = cameraReferenceEnabled;
                }

                scopeCamera.gameObject.SetActive(true);

                scopeCamera.transform.SetParent(scopeCameraParent);
                scopeCamera.transform.localPosition = Vector3.zero;
                scopeCamera.transform.localRotation = Quaternion.identity;
                scopeCamera.targetTexture = scopeRenderTexture;

                foreach (Transform child in scopeCamera.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                GameObject uiCamObject = new GameObject("Weakpoints Camera");
                uiCamObject.transform.SetParent(scopeCameraParent.transform);
                uiCamObject.transform.localPosition = Vector3.zero;
                uiCamObject.transform.localRotation = Quaternion.identity;
                uiCamObject.transform.localScale = Vector3.one;

                scopeUICamera = uiCamObject.AddComponent<Camera>();
                scopeUICamera.cullingMask = (1 << RoR2.LayerIndex.enemyBody.intVal);
                scopeUICamera.clearFlags = CameraClearFlags.SolidColor;
                scopeUICamera.backgroundColor = new Color(0, 0, 0, 0);
                scopeUICamera.rect = new Rect(0, 0, 1, 1);
                scopeUICamera.depth = 1;
                scopeUICamera.stereoTargetEye = StereoTargetEyeMask.None;
                scopeUICamera.allowHDR = false;
                scopeUICamera.allowMSAA = false;
                scopeUICamera.enabled = false;
                scopeUICamera.fieldOfView = 80;
                scopeUICamera.targetTexture = scopeUIRenderTexture;
                scopeUICamera.gameObject.SetActive(true);

                overlayCanvas.worldCamera = scopeUICamera;

                RoR2Application.onLateUpdate -= FindAndInstantiateCamera;
            }
        }

        private void DidntAsk(On.RoR2.UI.PointViewer.orig_FindCamera orig, PointViewer self)
        {
            if (self != pointViewer) orig(self);
        }

        private GameObject ReplaceLayer(On.RoR2.UI.PointViewer.orig_AddElement orig, PointViewer self, PointViewer.AddElementRequest request)
        {
            GameObject instance = orig(self, request);

            if (instance && self == pointViewer)
                instance.SetLayerRecursive(LayerIndex.enemyBody.intVal);

            return instance;
        }

        private void ReplaceWithScopeCameras(On.RoR2.UI.PointViewer.orig_UpdateAllElementPositions orig, RoR2.UI.PointViewer self)
        {
            if (self != pointViewer || !scopeCamera)
            {
                orig(self);
                return;
            }

            Vector2 size = self.rectTransform.rect.size;
            float num = scopeCamera.fieldOfView * 0.0548311367f;
            float num2 = 1f / num;
            foreach (KeyValuePair<UnityObjectWrapperKey<GameObject>, StructAllocator<PointViewer.ElementInfo>.Ptr> keyValuePair in self.elementToElementInfo)
            {
                StructAllocator<PointViewer.ElementInfo>.Ptr value = keyValuePair.Value;
                ref PointViewer.ElementInfo @ref = ref self.elementInfoAllocator.GetRef(value);
                if (@ref.targetTransform)
                {
                    @ref.targetLastKnownPosition = @ref.targetTransform.position;
                }
                Vector3 targetLastKnownPosition = @ref.targetLastKnownPosition;
                targetLastKnownPosition.y += @ref.targetWorldVerticalOffset;
                Vector3 vector = scopeCamera.WorldToViewportPoint(targetLastKnownPosition);
                vector.x -= 0.5f;
                vector.y -= 0.5f;
                float z = vector.z;
                Vector2 sizeDelta = self.rectTransform.sizeDelta;
                Vector3 localPosition = new Vector3(vector.x * sizeDelta.x, vector.y * sizeDelta.y, 0);
                localPosition.z = ((z >= 0f) ? 0f : -1f);
                @ref.elementRectTransform.localPosition = localPosition;
                if (@ref.scaleWithDistance)
                {
                    float d = @ref.targetWorldDiameter * num2 / z;
                    @ref.elementRectTransform.sizeDelta = d * size;
                }
            }
        }

        private void OnDestroy()
        {
            scopeUIRenderTexture.Release();

            On.RoR2.UI.PointViewer.FindCamera -= DidntAsk;
            On.RoR2.UI.PointViewer.UpdateAllElementPositions -= ReplaceWithScopeCameras;
            On.RoR2.UI.PointViewer.AddElement -= ReplaceLayer;
            Object.Destroy(overlayCanvas);

            if (!scopeCamera) RoR2Application.onLateUpdate -= FindAndInstantiateCamera;
        }

        private void OnEnable()
        {
            if (!scopeCamera)
            {
                return;
            }

            scopeCamera.fieldOfView = targetFOV;

            scopeViewRenderer.material = renderMaterial;
        }

        private void OnDisable()
        {
            if (!ModConfig.RailgunnerKeepScopeVisible.Value)
            {
                scopeViewRenderer.material = clearMaterial;
            } else
            {
                this.enabled = true;
            }
        }

        private void LateUpdate()
        {
            StartCoroutine(RenderScopeView());
        }

        private IEnumerator RenderScopeView()
        {
            yield return new WaitForEndOfFrame();
            scopeCamera.Render();
            scopeUICamera.Render();
            Graphics.Blit(scopeUIRenderTexture, scopeRenderTexture, uiTransparentMaterial);
        }

        internal void SetFOV(float fov)
        {
            targetFOV = fov;
            if (scopeCamera) scopeCamera.fieldOfView = targetFOV;
        }

        internal void UpdateDominance(bool leftHanded)
        {
            Vector3 scale = scopeViewRenderer.transform.localScale;

            if ((scale.x < 0) != leftHanded) scale.x = -scale.x;

            scopeViewRenderer.transform.localScale = scale;
        }

        internal void SetOverlay(GameObject overlay)
        {
            overlay.transform.SetParent(overlayCanvas.transform);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localScale = Vector3.one;
            overlay.SetLayerRecursive(LayerIndex.enemyBody.intVal);

            pointViewer = overlay.GetComponentInChildren<PointViewer>();
            pointViewer.rectTransform.sizeDelta = (overlayCanvas.transform as RectTransform).sizeDelta;
        }
    }
}
