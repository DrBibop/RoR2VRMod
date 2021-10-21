using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LIV.SDK.Unity
{
    static class SDKShaders
    {
        public static readonly int LIV_COLOR_MASK = Shader.PropertyToID("_LivColorMask");
        public static readonly int LIV_TESSELLATION_PROPERTY = Shader.PropertyToID("_LivTessellation");
        public static readonly int LIV_CLIP_PLANE_HEIGHT_MAP_PROPERTY = Shader.PropertyToID("_LivClipPlaneHeightMap");

        public const string LIV_MR_FOREGROUND_KEYWORD = "LIV_MR_FOREGROUND";
        public const string LIV_MR_BACKGROUND_KEYWORD = "LIV_MR_BACKGROUND";
        public const string LIV_MR_KEYWORD = "LIV_MR";

        public static Material clipPlaneSimpleMaterial { get; private set; }
        public static Material clipPlaneSimpleDebugMaterial { get; private set; }
        public static Material clipPlaneComplexMaterial { get; private set; }
        public static Material clipPlaneComplexDebugMaterial { get; private set; }
        public static Material writeOpaqueToAlphaMaterial { get; private set; }
        public static Material combineAlphaMaterial { get; private set; }
        public static Material writeMaterial { get; private set; }
        public static Material forceForwardRenderingMaterial { get; private set; }

        public static void LoadShaders()
        {
            clipPlaneSimpleMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_ClipPlaneSimpleMat");
            clipPlaneSimpleDebugMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_ClipPlaneSimpleDebugMat");
            clipPlaneComplexMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_ClipPlaneComplexMat");
            clipPlaneComplexDebugMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_ClipPlaneComplexDebugMat");
            writeOpaqueToAlphaMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_WriteOpaqueToAlphaMat");
            combineAlphaMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_CombineAlphaMat");
            writeMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_WriteMat");
            forceForwardRenderingMaterial = VRMod.VRMod.VRAssetBundle.LoadAsset<Material>("LIV_ForceForwardRenderingMat");
        }

        public static void StartRendering()
        {
            Shader.EnableKeyword(LIV_MR_KEYWORD);
        }

        public static void StopRendering()
        {
            Shader.DisableKeyword(LIV_MR_KEYWORD);
        }

        public static void StartForegroundRendering()
        {
            Shader.EnableKeyword(LIV_MR_FOREGROUND_KEYWORD);
        }

        public static void StopForegroundRendering()
        {
            Shader.DisableKeyword(LIV_MR_FOREGROUND_KEYWORD);
        }

        public static void StartBackgroundRendering()
        {
            Shader.EnableKeyword(LIV_MR_BACKGROUND_KEYWORD);
        }

        public static void StopBackgroundRendering()
        {
            Shader.DisableKeyword(LIV_MR_BACKGROUND_KEYWORD);
        }
    }
}
