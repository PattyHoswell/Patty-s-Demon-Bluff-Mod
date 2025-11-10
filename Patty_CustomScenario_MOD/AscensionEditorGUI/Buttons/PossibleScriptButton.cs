using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using System;
using UnityEngine.EventSystems;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class PossibleScriptButton : ContentAmountButton
    {
        public PossibleScriptButton() : base(ClassInjector.DerivedConstructorPointer<PossibleScriptButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public PossibleScriptButton(IntPtr ptr) : base(ptr) { }
        public ScriptInfo TargetScriptInfo { get; internal set; }
        public bool IsSelected { get; internal set; }

        public override void OnPointerClick(BaseEventData data)
        {
            IsSelected = true;
            CustomScenarioPopup.Instance.SetScriptInfo(TargetScriptInfo);
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
            if (CustomScenarioPopup.Instance.CurrentScriptInfo == TargetScriptInfo)
            {
                return;
            }
            Initialize();
            IsSelected = true;
            CustomScenarioPopup.Instance.SetScriptInfo(TargetScriptInfo);
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
            NameLabel.text = $"{nameof(ScriptInfo)}: {transform.GetSiblingIndex()}";
        }
    }
}
