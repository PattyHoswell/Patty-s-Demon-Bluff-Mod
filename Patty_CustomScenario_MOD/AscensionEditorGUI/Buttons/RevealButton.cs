using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    public class RevealButton : MonoBehaviour
    {
        public RevealButton() : base(ClassInjector.DerivedConstructorPointer<RevealButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public RevealButton(IntPtr ptr) : base(ptr) { }

        public List<GameObject> ObjectsToHide { get; internal set; } = new List<GameObject>();
        public GameObject? GameObjectToShow { get; internal set; }
        public Button? AttachedButton { get; internal set; }

        // Use this for initialization
        void Start()
        {
            AttachedButton = GetComponentInChildren<Button>(includeInactive: true);
            if (AttachedButton == null)
            {
                MelonLogger.Error("RevealButton must be attached to a Button GameObject.");
                return;
            }
            AttachedButton.onClick.AddListener((UnityAction)OnClickButton);
        }

        void OnClickButton()
        {
            foreach (var obj in ObjectsToHide)
            {
                if (obj == null || obj == gameObject)
                    continue;

                obj.SetActive(!obj.activeSelf);
            }
            if (GameObjectToShow != null)
            {
                GameObjectToShow.gameObject.SetActive(!GameObjectToShow.gameObject.activeSelf);
            }
        }
    }
}