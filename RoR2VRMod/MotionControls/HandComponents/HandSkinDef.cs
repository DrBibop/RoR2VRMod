using UnityEngine;
using UnityEngine.Rendering;

namespace VRMod
{
    [CreateAssetMenu(menuName = "VRMod/HandSkinDef")]
    public class HandSkinDef : ScriptableObject
    {
        [Tooltip("The name of the character body prefab.")]
        public string characterBodyName;

        [Tooltip("The name token of the original character model skin. If your original skin uses KingEnderBrine's skin builder, write the token in full upper-case like so by replacing the spaces surrounded with \"*\": *SKINAUTHOR*_SKIN_*SKINDEFINITIONNAME*_NAME")]
        public string originalSkinNameToken;

        [Tooltip("The hand on which to apply the override.")]
        public HandType handType;

        public RendererInfoOverride[] rendererInfoOverrides;

        [System.Serializable]
        public struct RendererInfoOverride
        {
            [Tooltip("The index of the renderer info to override. For indexes of vanilla characters, check out the RoR2VRMod wiki on GitHub.")]
            public uint rendererInfoIndex;

            [Tooltip("The new mesh that will replace the one stored in the associated renderer. Leave this field empty to keep the original mesh.")]
            public Mesh meshOverride;

            [Tooltip("The new material that will replace the one stored in the associated renderer. Leave this field empty to keep the original material.")]
            public Material materialOverride;

            [Tooltip("If enabled, the material override will be skipped and instead will attempt to copy the material from the character model by checking matching material or renderer object names. For this to work, the character needs a ModelLocator and a CharacterModel component.")]
            public bool copyMaterialFromCharacterModel;

            public ShadowCastingMode defaultShadowCastingMode;

            public bool ignoreOverlays;

            public bool hideOnDeath;
        }

        internal void Apply(Hand hand)
        {
            for (int i = 0; i < rendererInfoOverrides.Length; i++)
            {
                RendererInfoOverride rendererInfoOverride = rendererInfoOverrides[i];

                if (rendererInfoOverride.rendererInfoIndex >= hand.rendererInfos.Length)
                {
                    VRMod.StaticLogger.LogError("The hand skin " + originalSkinNameToken + " is attempting to override a renderer info that doesn't exist!");
                    return;
                }

                RoR2.CharacterModel.RendererInfo origRendererInfo = hand.rendererInfos[rendererInfoOverride.rendererInfoIndex];

                origRendererInfo.defaultShadowCastingMode = rendererInfoOverride.defaultShadowCastingMode;
                origRendererInfo.ignoreOverlays = rendererInfoOverride.ignoreOverlays;
                origRendererInfo.hideOnDeath = rendererInfoOverride.hideOnDeath;

                hand.rendererInfos[rendererInfoOverride.rendererInfoIndex] = origRendererInfo;

                if (rendererInfoOverride.meshOverride)
                {
                    if (origRendererInfo.renderer is SkinnedMeshRenderer)
                    {
                        (origRendererInfo.renderer as SkinnedMeshRenderer).sharedMesh = rendererInfoOverride.meshOverride;
                    }
                    else if (origRendererInfo.renderer is MeshRenderer)
                    {
                        MeshFilter meshFilter = origRendererInfo.renderer.GetComponent<MeshFilter>();

                        if (meshFilter)
                            meshFilter.sharedMesh = rendererInfoOverride.meshOverride;
                    }
                    else
                    {
                        VRMod.StaticLogger.LogError("The hand skin " + originalSkinNameToken + " is attempting to override a mesh on a renderer that doesn't support meshes!");
                    }
                }

                if (rendererInfoOverride.copyMaterialFromCharacterModel)
                {
                    hand.CopyMaterialFromModel(rendererInfoOverride.rendererInfoIndex);
                }
                else if (rendererInfoOverride.materialOverride)
                {
                    origRendererInfo.defaultMaterial = rendererInfoOverride.materialOverride;
                    hand.rendererInfos[rendererInfoOverride.rendererInfoIndex] = origRendererInfo;
                }
            }
        }
    }
}
