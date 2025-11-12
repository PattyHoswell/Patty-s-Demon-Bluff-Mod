using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class CharacterPoolButton : MonoBehaviour
    {
        public CharacterPoolButton() : base(ClassInjector.DerivedConstructorPointer<CharacterPoolButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CharacterPoolButton(IntPtr ptr) : base(ptr) { }

        public EAlignment TargetAlignment { get; private set; }
        public ECharacterType TargetCharacterType { get; private set; }
        public Button? AttachedButton { get; internal set; }
        // Use this for initialization
        void Start()
        {
            TargetCharacterType = Enum.TryParse<ECharacterType>(name, out var characterType) ? characterType : ECharacterType.None;
            TargetAlignment = TargetCharacterType switch
            {
                ECharacterType.Villager or ECharacterType.Outcast => EAlignment.Good,
                ECharacterType.Minion or ECharacterType.Demon => EAlignment.Evil,
                _ => EAlignment.None,
            };
            AttachedButton = GetComponentInChildren<Button>(includeInactive: true);
            if (AttachedButton == null)
            {
                MelonLogger.Error("CharacterPoolButton must be attached to a Button GameObject.");
                return;
            }
            AttachedButton.onClick.AddListener((UnityAction)OnClickButton);
        }

        void OnClickButton()
        {
            if (CharacterPoolPopup.Instance != null)
            {
                CharacterPoolPopup.Instance.OpenMenu(TargetAlignment, TargetCharacterType);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}