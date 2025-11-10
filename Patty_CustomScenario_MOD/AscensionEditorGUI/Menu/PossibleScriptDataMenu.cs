using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    internal class PossibleScriptDataMenu : ScrollableContentMenu
    {
        public PossibleScriptDataMenu() : base(ClassInjector.DerivedConstructorPointer<PossibleScriptDataMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public PossibleScriptDataMenu(IntPtr ptr) : base(ptr) { }

        public TextButton LoadButton { get; private set; }
        public new List<PossibleScriptDataButton> ContentButtons { get; internal set; } = new();
        public bool IsHighlighted { get; set; }
        public ScriptInfo TargetScriptInfo { get; set; }

        protected override void Awake()
        {
            base.Awake();

            LoadButton = transform.Find("Title/LoadButton").gameObject.AddComponent<TextButton>();
            LoadButton.Button.onClick.AddListener(new Action(LoadCustomScenarioData));
            CustomScenarioPopup.Instance.OnAscensionChanged += Instance_OnAscensionChanged;
            CustomScenarioPopup.Instance.OnScriptInfoChanged += Instance_OnScriptInfoChanged;
            CustomScenarioPopup.Instance.OnCustomScriptDataChanged += Instance_OnCustomScriptDataChanged;
            AscensionPopup.Instance.OnChangeCustomScriptDataName += Ascension_OnChangeCustomScriptDataName;

            Setup();
        }

        public List<PossibleScriptDataButton> GetSelecteds()
        {
            return ContentButtons.FindAll(x => x.IsSelected);
        }

        private void Instance_OnAscensionChanged(AscensionsData obj)
        {
            IEnumerator ClearAndSetup()
            {
                ClearContent();
                ContentButtons.Clear();
                yield return YieldCache.WaitForEndOfFrame.Value;
                Setup();
            }
            MelonCoroutines.Start(ClearAndSetup());
        }

        private void Ascension_OnChangeCustomScriptDataName(string obj)
        {
            foreach (var button in GetSelecteds())
            {
                button.UpdateLabelName();
            }
        }

        public void LoadCustomScenarioData()
        {
            var filter = "JSON Files (*.json)\0*.json\0\0";
            var initialDirectory = CustomScenario.CustomScriptDataFolder;
            WindowsFileDialog.OpenSingleFile((filePath) =>
            {
                if (!File.Exists(filePath))
                {
                    return;
                }
                if (!CustomScenario.customScripts.TryGetValue(filePath, out CustomScriptData_Json result))
                {
                    if (!UniversalUtility.LoadJson(filePath, out result))
                    {
                        return;
                    }
                    CustomScenario.customScripts.Add(filePath, result);
                }
                var customScriptData = GameUtility.FindCustomScriptData(result.Name) ?? ScriptableObject.CreateInstance<CustomScriptData>();
                result.Assign(customScriptData);
                var possibleScriptDataButton = AscensionPopup.Instance.PossibleScriptDataMenu.AddItem().GetComponent<PossibleScriptDataButton>();
                possibleScriptDataButton.TargetCustomScriptData = customScriptData;
                possibleScriptDataButton.UpdateLabelName();
                if (CustomScenarioPopup.Instance.CurrentCustomScriptData == customScriptData)
                {
                    possibleScriptDataButton.IsSelected = true;
                    possibleScriptDataButton.Highlight();
                }

            }, "Load Custom Scenario Data", filter, initialDirectory, defaultExt: ".json");
        }

        private void Instance_OnScriptInfoChanged(ScriptInfo obj)
        {
            if (obj == null)
            {
                return;
            }
            CustomScenarioPopup.Instance.SetCustomScriptData(null!);
        }

        private void Instance_OnCustomScriptDataChanged(CustomScriptData obj)
        {
            foreach (var button in ContentButtons)
            {
                if (button.TargetCustomScriptData == obj)
                {
                    button.IsSelected = true;
                    button.Highlight();
                    continue;
                }
                button.Deselect();
            }
        }

        public override bool RemoveItem(ContentAmountButton contentAmountButton)
        {
            var shouldChange = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.Length <= 1;
            if (shouldChange && CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.Length <= 0)
            {
                CustomScenario.Logger.Warning("Cannot remove because its the last script available");
                return false;
            }
            var contentButton = ContentButtons.Find(button => button == contentAmountButton);
            if (contentButton == null)
            {
                return false;
            }
            var isSelected = contentButton.IsSelected;
            var found = false;
            CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.Where(x =>
            {
                if (!found && x.Equals(contentButton.TargetCustomScriptData))
                {
                    found = true;
                    return false;
                }
                return true;
            }).ToArray();
            ContentButtons.Remove(contentButton);
            Destroy(contentButton.gameObject);
            if (isSelected && CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.Length > 0)
            {
                CustomScenarioPopup.Instance.SetCustomScriptData(CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.First());
            }
            if (shouldChange)
            {
                CustomScenarioPopup.Instance.SetScriptInfo(CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.First());
            }
            return true;
        }

        internal protected override void Setup()
        {
            ClearContent(true);
            var possibleScripts = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData;
            for (var i = 0; i < possibleScripts.Count; i++)
            {
                if (possibleScripts[i] == null) continue;
                var possibleScriptDataButton = AddItem(false).GetComponent<PossibleScriptDataButton>();
                possibleScriptDataButton.TargetCustomScriptData = possibleScripts[i];
                possibleScriptDataButton.UpdateLabelName();

                if (CustomScenarioPopup.Instance.CurrentCustomScriptData == possibleScripts[i])
                {
                    possibleScriptDataButton.IsSelected = true;
                    possibleScriptDataButton.Highlight();
                }
            }
            SortButtons();
        }

        public override GameObject AddItem(bool addToList = true)
        {
            var newItem = Instantiate(Prefab, ScrollableArea.content);
            newItem.gameObject.SetActive(true);
            var contentButton = newItem.AddComponent<PossibleScriptDataButton>();
            contentButton.name = $"Custom Script Data {contentButton.transform.GetSiblingIndex()}";
            contentButton.TargetScrollableContent = this;
            contentButton.TargetCustomScriptData = GameUtility.CreateDefaultCustomScriptData();
            contentButton.TargetCustomScriptData.name = $"{contentButton.TargetCustomScriptData.name}_{contentButton.transform.GetSiblingIndex()}";
            contentButton.UpdateLabelName();
            ContentButtons.Add(contentButton);
            if (addToList)
            {
                CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.Append(contentButton.TargetCustomScriptData).ToArray();
            }
            return contentButton.gameObject;
        }
    }
}
