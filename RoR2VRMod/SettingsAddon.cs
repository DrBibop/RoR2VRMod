using Rewired;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRMod
{
    //Thank you KingEnderBrine. Your code from ExtraSkillSlots have been greatly helpful for this part.
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

            On.RoR2.UI.PauseScreenController.OpenSettingsMenu += (orig, self) =>
            {
                orig(self);
                SetupVRSettings(self.submenuObject);
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
            GameObject subPanelToInstantiate = parent.Find("SettingsSubPanel, Controls (Gamepad)").gameObject;

            GameObject subPanelInstance = Object.Instantiate(subPanelToInstantiate, parent);

            Transform instanceLayout = subPanelInstance.transform.Find("Scroll View/Viewport/VerticalLayout");

            foreach (Transform child in instanceLayout)
            {
                Object.Destroy(child.gameObject);
            }

            GameObject controllerBindingSetting = subPanelToInstantiate.transform.Find("Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Binding (Jump)").gameObject;
            GameObject keyboardBindingSetting = parent.Find("SettingsSubPanel, Controls (M&KB)/Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Binding (Jump)").gameObject;

            Inputs.ActionDef[] actionDefs = Inputs.actionDefs;

            foreach (var actionDef in actionDefs)
            {
                if (actionDef.keyboardMap != KeyboardKeyCode.None)
                    AddBindingSetting(actionDef, keyboardBindingSetting, instanceLayout, subPanelInstance);
                if (actionDef.joystickMap != Inputs.ControllerInput.None)
                    AddBindingSetting(actionDef, controllerBindingSetting, instanceLayout, subPanelInstance);
            }

            return subPanelInstance;
        }

        private static void AddBindingSetting(Inputs.ActionDef actionDef, GameObject settingToInstantiate, Transform panelLayout, GameObject subPanel)
        {
            GameObject settingInstance = Object.Instantiate(settingToInstantiate, panelLayout);

            InputBindingControl inputBindingControl = settingInstance.GetComponent<InputBindingControl>();
            inputBindingControl.actionName = actionDef.actionName;
            inputBindingControl.axisRange = AxisRange.Full;
            inputBindingControl.Awake();

            settingInstance.name = string.Format("SettingsEntryButton, {1} Binding ({0})", actionDef.actionName, inputBindingControl.inputSource == MPEventSystem.InputSource.MouseAndKeyboard ? "M&K" : "Gamepad");

            HGButtonHistory buttonHistory = subPanel.GetComponent<HGButtonHistory>();

            if (buttonHistory && !buttonHistory.lastRememberedGameObject)
                buttonHistory.lastRememberedGameObject = settingInstance;
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
