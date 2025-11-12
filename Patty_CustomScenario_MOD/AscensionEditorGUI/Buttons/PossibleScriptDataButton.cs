using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using Patty_CustomScenario_MOD.QoL;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class PossibleScriptDataButton : ContentAmountButton
    {
        public PossibleScriptDataButton() : base(ClassInjector.DerivedConstructorPointer<PossibleScriptDataButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public PossibleScriptDataButton(IntPtr ptr) : base(ptr) { }
        public CustomScriptData TargetCustomScriptData { get; internal set; }
        public bool IsSelected { get; internal set; }
        public TextButton LoadButton { get; internal set; }

        protected override void Awake()
        {
            base.Awake();
            LoadButton = transform.Find("SaveAsButton").gameObject.AddComponent<TextButton>();
            LoadButton.Button.onClick.AddListener((UnityAction)(SaveCustomScenarioData));
        }
        public void SaveCustomScenarioData()
        {
            var filter = "JSON Files (*.json)\0*.json\0\0";
            var initialDirectory = CustomScenario.CustomScriptDataFolder;
            WindowsFileDialog.SaveSingleFile((filePath) =>
            {
                CustomScenario.Logger.Msg($"Saving to path {filePath}");
                var scriptDataJson = new CustomScriptData_Json();
                scriptDataJson.Initialize(TargetCustomScriptData);
                UniversalUtility.SerializeJson(filePath, scriptDataJson);

            }, "Save Custom Scenario Data", filter, initialDirectory, TargetCustomScriptData.name, defaultExt: ".json");
        }

        public override void OnPointerClick(BaseEventData data)
        {
            Select();
        }

        public override void OnPointerEnter(BaseEventData data)
        {
            if (IsSelected)
            {
                return;
            }

            base.OnPointerEnter(data);
        }

        public override void OnPointerExit(BaseEventData data)
        {
            if (IsSelected)
            {
                return;
            }

            base.OnPointerExit(data);
        }

        public void Select()
        {
            if (CustomScenarioPopup.Instance.CurrentCustomScriptData == TargetCustomScriptData)
            {
                return;
            }
            Initialize();
            Highlight();
            IsSelected = true;
            CustomScenarioPopup.Instance.SetCustomScriptData(TargetCustomScriptData);
        }

        public void Deselect()
        {
            Initialize();
            IsSelected = false;
            OnPointerExit(null!);
        }

        public override void UpdateLabelName()
        {
            Initialize();
            NameLabel.text = $"{TargetCustomScriptData.name}";
        }
    }
}
