using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class TextButton : MonoBehaviour
    {
        public TextButton() : base(ClassInjector.DerivedConstructorPointer<TextButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public TextButton(IntPtr ptr) : base(ptr) { }

        public TextMeshProUGUI TextMeshPro { get; internal set; }
        public Button Button { get; internal set; }

        void Awake()
        {
            Button = GetComponentInChildren<Button>(true);
            TextMeshPro = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}
