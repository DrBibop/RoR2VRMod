using UnityEngine;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;

namespace VRMod
{
    internal class Hand : MonoBehaviour
    {
        [SerializeField]
        internal HandType handType;

        [SerializeField]
        internal Muzzle[] muzzles;

        [SerializeField]
        internal string bodyName;

        [SerializeField]
        internal bool useRay;

        [SerializeField]
        internal CharacterModel.RendererInfo[] rendererInfos;

        [SerializeField]
        internal List<HandSkinDef> skins = new List<HandSkinDef>();
		
		[SerializeField]
        internal bool copyMaterialsFromCharacterModelIfNoSkin = false;

        internal Animator animator { get; private set; }

        internal Muzzle currentMuzzle;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();

            if (muzzles == null || muzzles.Length <= 0)
                throw new NullReferenceException("This hand has no muzzle! Aiming won't be possible that way and other errors may appear.");

            currentMuzzle = muzzles[0];
        }

        private void OnEnable()
        {
            MotionControls.onSkinApplied += ApplySkin;
        }

        private void OnDisable()
        {
            MotionControls.onSkinApplied -= ApplySkin;
        }

        internal void AddSkin(HandSkinDef skinDef)
        {
            if (skins.Contains(skinDef))
            {
                VRMod.StaticLogger.LogError("The hand skin definition " + skinDef.originalSkinNameToken + " has already been added!");
            }

            skins.Add(skinDef);
        }

        private void ApplySkin(SkinDef skinDef)
        {
            HandSkinDef selectedSkin = skins.FirstOrDefault(x => x.originalSkinNameToken == skinDef.nameToken);

            if (selectedSkin != null)
            {
                selectedSkin.Apply(this);
            }
            else if (copyMaterialsFromCharacterModelIfNoSkin)
            {
                for (uint i = 0; i < rendererInfos.Length; i++)
                {
                    CopyMaterialFromModel(i);
                }
            }
        }

        internal void CopyMaterialFromModel(uint rendererInfoIndex)
        {
            CharacterModel model = MotionControls.currentBody.modelLocator.modelTransform.GetComponent<CharacterModel>();

            var rendererInfo = rendererInfos[rendererInfoIndex];

            string materialName = rendererInfo.renderer.material.name;

            if (materialName.EndsWith(" (Instance)"))
            {
                materialName = materialName.Remove(materialName.IndexOf(" (Instance)"));
            }

            CharacterModel.RendererInfo? matchingRendererInfo = model.baseRendererInfos.FirstOrDefault(x => x.defaultMaterial && (x.defaultMaterial.name.Replace(" (Instance)", "") == materialName || x.defaultMaterial.name.Replace("Alt", "").Replace(" (Instance)", "") == materialName));

            if (!matchingRendererInfo.HasValue || matchingRendererInfo.Value.defaultMaterial == null)
            {
                if (rendererInfo.renderer is MeshRenderer || rendererInfo.renderer is SkinnedMeshRenderer)
                {
                    matchingRendererInfo = model.baseRendererInfos.FirstOrDefault(x => (x.renderer is MeshRenderer || x.renderer is SkinnedMeshRenderer) && x.renderer.name == rendererInfo.renderer.name);

                    if (matchingRendererInfo == null) return;
                }
                else
                {
                    return;
                }
            }

            CharacterModel.RendererInfo bodyRendererInfo = matchingRendererInfo.Value;

            if (bodyRendererInfo.defaultMaterial)
            {
                VRMod.StaticLogger.LogInfo("Applying body material " + bodyRendererInfo.defaultMaterial.name + " on renderer " + rendererInfo.renderer.gameObject.name);
                rendererInfo.renderer.material = bodyRendererInfo.defaultMaterial;
                rendererInfo.defaultMaterial = bodyRendererInfo.defaultMaterial;

                rendererInfos[rendererInfoIndex] = rendererInfo;
            }
            else
            {
                VRMod.StaticLogger.LogWarning("No material replacement found for " + materialName + ".");
            }
        }
    }

    public enum HandType
    {
        Both,
        Dominant,
        NonDominant
    }
}
