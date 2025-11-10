using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using System;
using UnityEngine.EventSystems;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class AllCharacterMenuButton : ContentAmountButton
    {
        public AllCharacterMenuButton() : base(ClassInjector.DerivedConstructorPointer<AllCharacterMenuButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public AllCharacterMenuButton(IntPtr ptr) : base(ptr) { }

        public CharactersCount TargetCharCount { get; internal set; }

        public override void OnPointerClick(BaseEventData data)
        {
            if (TargetScrollableContent is CharacterCountMenu characterCountContent)
            {
                OnItemHoverExited(data);
                characterCountContent.Open(this);
            }
            else
            {
                CustomScenario.Logger.Error($"{nameof(TargetScrollableContent)} isn't {nameof(CharacterCountMenu)}");
            }
        }

        public override void UpdateLabelName()
        {
            Initialize();
            NameLabel.text = $"Villager:{TargetCharCount.town}, Outcast:{TargetCharCount.outs}, Minion:{TargetCharCount.minion}, Demon:{TargetCharCount.demon}";
        }
    }
}
