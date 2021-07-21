using RoR2;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    [RequireComponent(typeof(Hand))]
    class MaterialReplacer : MonoBehaviour
    {
        private void OnEnable()
        {
            MotionControls.onSkinApplied += ApplySkin;
        }

        private void OnDisable()
        {
            MotionControls.onSkinApplied -= ApplySkin;
        }

        private void ApplySkin()
        {
            Hand hand = GetComponent<Hand>();

            CharacterModel model = MotionControls.currentBody.modelLocator.modelTransform.GetComponent<CharacterModel>();

            for (int i = 0; i < hand.rendererInfos.Length; i++)
            {
                var rendererInfo = hand.rendererInfos[i];

                string name = rendererInfo.renderer.material.name;

                if (!name.StartsWith("mat")) continue;

                if (name.EndsWith(" (Instance)"))
                {
                    name = name.Remove(name.IndexOf(" (Instance)"));
                }

                var bodyRendererInfos = model.baseRendererInfos.Where(x => x.defaultMaterial && (x.defaultMaterial.name == name || x.defaultMaterial.name == (name + "Alt")));

                if (bodyRendererInfos == null || bodyRendererInfos.Count() <= 0)
                {
                    if (rendererInfo.renderer is SkinnedMeshRenderer)
                    {
                        bodyRendererInfos = model.baseRendererInfos.Where(x => x.renderer is SkinnedMeshRenderer && x.renderer.name == rendererInfo.renderer.name);

                        if (bodyRendererInfos == null || bodyRendererInfos.Count() <= 0) continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                var bodyRendererInfo = bodyRendererInfos.First();

                if (bodyRendererInfo.defaultMaterial)
                {
                    rendererInfo.renderer.material = bodyRendererInfo.defaultMaterial;
                    rendererInfo.defaultMaterial = bodyRendererInfo.defaultMaterial;

                    hand.rendererInfos[i] = rendererInfo;
                }
                else
                {
                    VRMod.StaticLogger.LogWarning("No replacement found for " + name + ".");
                }
            }
        }
    }
}
