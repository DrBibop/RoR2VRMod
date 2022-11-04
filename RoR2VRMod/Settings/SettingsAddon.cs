using BepInEx.Configuration;
using MonoMod.Cil;
using Rewired;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
                {
                    SetupVRSettings(self.submenuPanelInstance);
                }
            };

            On.RoR2.UI.MainMenu.SubmenuMainMenuScreen.OnExit += (orig, self, controller) =>
            {
                orig(self, controller);
                SaveSettings();
            };

            On.RoR2.UI.PauseScreenController.OpenSettingsMenu += (orig, self) =>
            {
                orig(self);
                SetupVRSettings(self.submenuObject);
            };

            IL.RoR2.UI.PauseScreenController.Update += SaveOnClose;

            On.RoR2.UI.BaseSettingsControl.GetCurrentValue += GetVRSettingValue;

            On.RoR2.UI.BaseSettingsControl.SubmitSettingInternal += SubmitConfig;

            On.RoR2.UI.BaseSettingsControl.Awake += StopError;

            On.RoR2.UI.HGButton.OnSelect += ScrollToButton;
        }

        private static void ScrollToButton(On.RoR2.UI.HGButton.orig_OnSelect orig, HGButton self, BaseEventData eventData)
        {
            orig(self, eventData);
            if (self.gameObject.name.Contains("VRModSetting"))
            {
                HGScrollRectHelper scrollHelper = self.GetComponentInParent<HGScrollRectHelper>();
                scrollHelper.ScrollToShowMe(self);
            }
        }

        private static void StopError(On.RoR2.UI.BaseSettingsControl.orig_Awake orig, BaseSettingsControl self)
        {
            if (self.gameObject.name.Contains("VRModSetting"))
            {
                self.eventSystemLocator = self.GetComponent<MPEventSystemLocator>();
                if (self.nameLabel && !string.IsNullOrEmpty(self.nameToken))
                {
                    self.nameLabel.token = self.nameToken;
                }
                return;
            }
            orig(self);
        }

        private static void SubmitConfig(On.RoR2.UI.BaseSettingsControl.orig_SubmitSettingInternal orig, BaseSettingsControl self, string newValue)
        {
            if (self.gameObject.name.Contains("VRModSetting"))
            {
                ModConfig.ConfigSetting setting;
                if (ModConfig.settings.TryGetValue(self.settingName, out setting))
                {
                    ConfigEntryBase entry = setting.entry;
                    if (entry.SettingType == typeof(bool))
                    {
                        (entry as ConfigEntry<bool>).Value = newValue == "1";
                    }
                    else if (entry.SettingType == typeof(int))
                    {
                        float parsedValue = float.Parse(newValue);
                        (entry as ConfigEntry<int>).Value = (int)parsedValue;
                    }
                    else if (entry.SettingType == typeof(float))
                    {
                        float parsedValue = float.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
                        (entry as ConfigEntry<float>).Value = parsedValue;
                    }
                    else if (entry.SettingType == typeof(string))
                    {
                        (entry as ConfigEntry<string>).Value = newValue;
                    }
                }
                RoR2.RoR2Application.onNextUpdate += self.OnUpdateControls;
                return;
            }
            orig(self, newValue);
        }

        private static string GetVRSettingValue(On.RoR2.UI.BaseSettingsControl.orig_GetCurrentValue orig, BaseSettingsControl self)
        {
            if (self.gameObject.name.Contains("VRModSetting"))
            {
                ModConfig.ConfigSetting setting;
                if (ModConfig.settings.TryGetValue(self.settingName, out setting))
                {
                    ConfigEntryBase entry = setting.entry;
                    if (entry.SettingType == typeof(bool))
                    {
                        return (entry as ConfigEntry<bool>).Value ? "1" : "0";
                    }
                    else if (entry.SettingType == typeof(int))
                    {
                        return TextSerialization.ToStringInvariant((entry as ConfigEntry<int>).Value);
                    }
                    else if (entry.SettingType == typeof(float))
                    {
                        return TextSerialization.ToStringInvariant((entry as ConfigEntry<float>).Value);
                    }
                    else if (entry.SettingType == typeof(string))
                    {
                        string value = (entry as ConfigEntry<string>).Value;

                        if (Array.Exists((self as CarouselController).choices, x => x.convarValue == value))
                        {
                            return value;
                        }
                        else
                        {
                            if (self.settingName == "vr_ray_color")
                                return "#FFFFFF";
                            else if (self.settingName == "vr_haptics_suit")
                                return "None";
                        }
                    }
                }
                else
                {
                    return "0";
                }
            }
            return orig(self);
        }

        private static void SaveOnClose(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchCallvirt<GameObject>("SetActive"));

            c.Index++;
            c.EmitDelegate<Action>(() =>
            {
                SaveSettings();
            });
        }

        private static void SaveSettings()
        {
            ModConfig.Save();
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

            GameObject subPanelInstance = GameObject.Instantiate(subPanelToInstantiate, parent);

            Transform instanceLayout = subPanelInstance.transform.Find("Scroll View/Viewport/VerticalLayout");

            foreach (Transform child in instanceLayout)
            {
                GameObject.Destroy(child.gameObject);
            }

            GameObject sliderSetting = subPanelToInstantiate.transform.Find("Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Slider (Look Scale X)").gameObject;
            GameObject boolSetting = subPanelToInstantiate.transform.Find("Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Bool (Invert X)").gameObject;
            GameObject carouselSetting = parent.transform.Find("SettingsSubPanel, Video/Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Carousel (Vsync)").gameObject;

            LanguageTextMeshController descriptionText = parent.transform.Find("GenericDescriptionPanel/ContentSizeFitter/DescriptionText").GetComponent<LanguageTextMeshController>();

            bool first = true;
            foreach (KeyValuePair<string, ModConfig.ConfigSetting> keyValuePair in ModConfig.settings)
            {
                ModConfig.ConfigSetting setting = keyValuePair.Value;

                BaseSettingsControl settingInstance;
                if (setting.entry.SettingType == typeof(float) || setting.entry.SettingType == typeof(int))
                {
                    settingInstance = GameObject.Instantiate(sliderSetting, instanceLayout).GetComponent<BaseSettingsControl>();

                    SettingsSlider slider = settingInstance as SettingsSlider;
                    slider.minValue = setting.minValue;
                    slider.maxValue = setting.maxValue;
                    slider.formatString = setting.entry.SettingType == typeof(float) ? "{0:0.00}" : "{0:N0}";
                }
                else if (setting.entry.SettingType == typeof(bool))
                {
                    settingInstance = GameObject.Instantiate(boolSetting, instanceLayout).GetComponent<BaseSettingsControl>();
                }
                else
                {
                    settingInstance = GameObject.Instantiate(carouselSetting, instanceLayout).GetComponent<BaseSettingsControl>();

                    CarouselController carousel = settingInstance as CarouselController;
                    List<CarouselController.Choice> choices = new List<CarouselController.Choice>();
                    if (keyValuePair.Key == "vr_ray_color")
                    {
                        string[] choiceStrings = new string[]
                        {
                            "White",
                            "Green",
                            "Red",
                            "Blue",
                            "Yellow",
                            "Magenta",
                            "Cyan",
                            "Lime",
                            "Black"
                        };

                        string[] hexStrings = new string[]
                        {
                            "#FFFFFF",
                            "#008000",
                            "#FF0000",
                            "#0000FF",
                            "#FFFF00",
                            "#FF00FF",
                            "#00FFFF",
                            "#00FF00",
                            "#000000"
                        };

                        for (int i = 0; i < choiceStrings.Length; i++)
                        {
                            CarouselController.Choice choice = new CarouselController.Choice();
                            choice.convarValue = hexStrings[i];
                            choice.suboptionDisplayToken = choiceStrings[i];
                            choices.Add(choice);
                        }
                    }
                    else if (keyValuePair.Key == "vr_haptics_suit")
                    {
                        string[] choiceStrings = new string[]
                        {
                            "None",
                            "Shockwave"
                        };

                        for (int i = 0; i < choiceStrings.Length; i++)
                        {
                            CarouselController.Choice choice = new CarouselController.Choice();
                            choice.convarValue = choiceStrings[i];
                            choice.suboptionDisplayToken = choiceStrings[i];
                            choices.Add(choice);
                        }
                    }

                    carousel.choices = choices.ToArray();
                }

                settingInstance.settingSource = BaseSettingsControl.SettingSource.ConVar;
                settingInstance.nameToken = setting.entry.Definition.Key;
                settingInstance.settingName = keyValuePair.Key;
                settingInstance.gameObject.name = "VRModSetting, " + settingInstance.nameToken;

                HGButton button = settingInstance.GetComponent<HGButton>();
                if (button)
                {
                    string prefixString = "";

                    if (setting.settingUpdate == ModConfig.ConfigSetting.SettingUpdate.NextStage)
                        prefixString = "[WILL APPLY NEXT STAGE] ";
                    else if (setting.settingUpdate == ModConfig.ConfigSetting.SettingUpdate.AfterRestart)
                        prefixString = "[RESTART REQUIRED] ";

                    button.updateTextOnHover = true;
                    button.hoverToken = prefixString + setting.entry.Description.Description;
                    button.hoverLanguageTextMeshController = descriptionText;

                    button.defaultFallbackButton = first;
                    first = false;
                }
            }

            GameObject controllerBindingSetting = subPanelToInstantiate.transform.Find("Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Binding (Jump)").gameObject;
            GameObject keyboardBindingSetting = parent.Find("SettingsSubPanel, Controls (M&KB)/Scroll View/Viewport/VerticalLayout/SettingsEntryButton, Binding (Jump)").gameObject;

            ActionAddons.ActionDef[] actionDefs = ActionAddons.actionDefs;
            foreach (var actionDef in actionDefs)
            {
                if (actionDef.keyboardMap != KeyboardKeyCode.None)
                    AddBindingSetting(actionDef, keyboardBindingSetting, instanceLayout);
                if (actionDef.joystickMap != ActionAddons.ControllerInput.None)
                    AddBindingSetting(actionDef, controllerBindingSetting, instanceLayout);
            }

            subPanelInstance.transform.Find("Scroll View").gameObject.AddComponent<ScrollToSelection>();

            return subPanelInstance;
        }

        private static void AddBindingSetting(ActionAddons.ActionDef actionDef, GameObject settingToInstantiate, Transform panelLayout)
        {
            GameObject settingInstance = GameObject.Instantiate(settingToInstantiate, panelLayout);

            InputBindingControl inputBindingControl = settingInstance.GetComponent<InputBindingControl>();
            inputBindingControl.actionName = actionDef.actionName;
            inputBindingControl.axisRange = AxisRange.Full;
            inputBindingControl.Awake();

            settingInstance.name = string.Format("SettingsEntryButton, {1} Binding ({0})", actionDef.actionName, inputBindingControl.inputSource == MPEventSystem.InputSource.MouseAndKeyboard ? "M&K" : "Gamepad");
        }

        private static GameObject SetupHeader(Transform parent)
        {
            GameObject categoryToInstantiate = parent.Find("GenericHeaderButton (Graphics)").gameObject;

            GameObject headerInstance = GameObject.Instantiate(categoryToInstantiate, parent);

            headerInstance.transform.SetSiblingIndex(parent.childCount - 2);
            headerInstance.name = "GenericHeaderButton (VR)";

            return headerInstance;
        }
    }
}
