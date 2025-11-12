using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    public class PossibleScriptMenu : ScrollableContentMenu
    {
        public PossibleScriptMenu() : base(ClassInjector.DerivedConstructorPointer<PossibleScriptMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public PossibleScriptMenu(IntPtr ptr) : base(ptr) { }

        public new List<PossibleScriptButton> ContentButtons { get; internal set; } = new();
        public bool IsHighlighted { get; set; }
        public ScriptInfo TargetScriptInfo { get; set; }

        protected override void Awake()
        {
            base.Awake();

            CustomScenarioPopup.Instance.OnAscensionChanged += Instance_OnAscensionChanged;
            CustomScenarioPopup.Instance.OnScriptInfoChanged += Instance_OnScriptInfoChanged;
            CustomScenarioPopup.Instance.OnCustomScriptDataChanged += Instance_OnCustomScriptDataChanged;

            Setup();
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

        private void Instance_OnScriptInfoChanged(ScriptInfo obj)
        {
            foreach (var button in ContentButtons)
            {
                if (button.TargetScriptInfo == obj)
                {
                    button.IsSelected = true;
                    button.Highlight();
                    continue;
                }
                button.Deselect();
            }
        }

        private void Instance_OnCustomScriptDataChanged(CustomScriptData obj)
        {
            if (obj == null)
            {
                return;
            }
            CustomScenarioPopup.Instance.SetScriptInfo(null!);
        }
        public override void SortButtons()
        {
            var orderedButton = ContentButtons.OrderBy(x => x.gameObject.transform.GetSiblingIndex()).ToList();
            for (var i = 0; i < orderedButton.Count; i++)
            {
                var button = orderedButton[i];
                button.name = $"{nameof(ScriptInfo)}: {i}";
                button.transform.SetSiblingIndex(i);
                button.UpdateLabelName();
            }
        }

        public override bool RemoveItem(ContentAmountButton contentAmountButton)
        {
            var shouldChange = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.Length <= 1;
            if (shouldChange && CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.Length <= 0)
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
            CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.Where(x =>
            {
                if (!found && x.Equals(contentButton.TargetScriptInfo))
                {
                    CustomScenario.Logger.Msg("Removed an item from possible scripts");
                    found = true;
                    return false;
                }
                return true;
            }).ToArray();
            ContentButtons.Remove(contentButton);
            Destroy(contentButton.gameObject);
            SortButtons();
            if (isSelected && CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.Length > 0)
            {
                CustomScenarioPopup.Instance.SetScriptInfo(CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.First());
            }
            if (shouldChange)
            {
                CustomScenarioPopup.Instance.SetCustomScriptData(CustomScenarioPopup.Instance.CurrentAscensionData.possibleScriptsData.First());
            }

            return true;
        }

        internal protected override void Setup()
        {
            ClearContent(true);
            var possibleScripts = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts;
            for (var i = 0; i < possibleScripts.Count; i++)
            {
                if (possibleScripts[i] == null) continue;
                var possibleScriptButton = AddItem(false).GetComponent<PossibleScriptButton>();
                possibleScriptButton.TargetScriptInfo = possibleScripts[i];
                possibleScriptButton.UpdateLabelName();

                if (CustomScenarioPopup.Instance.CurrentScriptInfo == possibleScripts[i])
                {
                    possibleScriptButton.Highlight();
                }
            }
            SortButtons();
        }

        public override GameObject AddItem(bool addToList = true)
        {
            var newItem = Instantiate(Prefab, ScrollableArea.content);
            newItem.gameObject.SetActive(true);
            var contentButton = newItem.AddComponent<PossibleScriptButton>();
            contentButton.name = $"Possible Script {contentButton.transform.GetSiblingIndex()}";
            contentButton.TargetScrollableContent = this;
            contentButton.TargetScriptInfo = GameUtility.CreateDefaultScriptInfo();
            contentButton.UpdateLabelName();
            ContentButtons.Add(contentButton);
            if (addToList)
            {
                CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts = CustomScenarioPopup.Instance.CurrentAscensionData.possibleScripts.Append(contentButton.TargetScriptInfo).ToArray();
            }
            return contentButton.gameObject;
        }
    }
}
