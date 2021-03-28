using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    internal static class SettingsAddon
    {
        internal static void Init()
        {
            On.RoR2.UI.MainMenu.SubmenuMainMenuScreen.OnEnter += (orig, self, controller) =>
            {
                orig(self, controller);
                if (self.submenuPanelPrefab.name == "SettingsPanel")
                    SetupVRSettings(self.submenuPanelInstance);
            };
        }

        internal static void SetupVRSettings(GameObject panel)
        {
            HGHeaderNavigationController controller = panel.GetComponent<HGHeaderNavigationController>();

            if (!controller)
                return;

            Transform header = panel.transform.Find("SafeArea/HeaderContainer/Header (JUICED)");
            Transform subPanelArea = panel.transform.Find("SafeArea/SubPanelArea");

            if (!header || !subPanelArea)
                return;

            GameObject subPanelInstance = SetupSubPanel(subPanelArea);

            GameObject headerInstance = SetupHeader(header);

            LanguageTextMeshController text = headerInstance.GetComponent<LanguageTextMeshController>();
            text.token = "VR";

            HGHeaderNavigationController.Header headerInfo = new HGHeaderNavigationController.Header();
            headerInfo.headerButton = headerInstance.GetComponent<HGButton>();
            headerInfo.headerName = "VR";
            headerInfo.tmpHeaderText = headerInstance.GetComponentInChildren<HGTextMeshProUGUI>();
            headerInfo.headerRoot = subPanelInstance;

            List<HGHeaderNavigationController.Header> headerList = controller.headers.ToList();

            headerList.Add(headerInfo);

            controller.headers = headerList.ToArray();
        }

        private static GameObject SetupSubPanel(Transform parent)
        {
            GameObject subPanelToInstantiate = parent.Find("SettingsSubPanel, Graphics").gameObject;

            GameObject subPanelInstance = Object.Instantiate(subPanelToInstantiate, parent);

            Transform instanceLayout = subPanelInstance.transform.Find("Scroll View/Viewport/VerticalLayout");

            foreach (Transform child in instanceLayout)
            {
                Object.Destroy(child.gameObject);
            }

            return subPanelInstance;
        }

        private static GameObject SetupHeader(Transform parent)
        {
            GameObject categoryToInstantiate = parent.Find("GenericHeaderButton (Graphics)").gameObject;

            GameObject headerInstance = Object.Instantiate(categoryToInstantiate, parent);

            headerInstance.transform.SetSiblingIndex(parent.childCount - 2);
            headerInstance.name = "GenericHeaderButton (VR)";

            return headerInstance;
        }
    }
}
