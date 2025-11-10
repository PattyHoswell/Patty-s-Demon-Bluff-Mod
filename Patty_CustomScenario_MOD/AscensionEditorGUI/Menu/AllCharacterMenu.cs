using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using System;
using UnityEngine;
namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    /// <summary>
    /// The main class that holds the Character Pool menu in Ascension Menu
    /// </summary>
    public class AllCharacterMenu : MonoBehaviour
    {
        public AllCharacterMenu() : base(ClassInjector.DerivedConstructorPointer<AllCharacterMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public AllCharacterMenu(IntPtr ptr) : base(ptr) { }

        public TextMeshProUGUI Title { get; internal set; }
        void Awake()
        {
            Title = transform.Find("Title").GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

            var characters = transform.Find("GameObject/Characters");
            for (var i = 0; i < characters.childCount; i++)
            {
                var child = characters.GetChild(i);
                child.gameObject.AddComponent<CharacterPoolButton>();
            }
        }
    }
}
