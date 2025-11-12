using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{

    public class CharacterButton : MonoBehaviour
    {
        public CharacterButton() : base(ClassInjector.DerivedConstructorPointer<CharacterButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CharacterButton(IntPtr ptr) : base(ptr) { }

        public CharacterPoolMenu PoolMenu { get; internal set; }
        public EventTrigger Trigger { get; internal set; }
        public CompendiumCharacter Character { get; internal set; }
        public bool IsCurrentPool { get; internal set; }

        private bool isDragging, isHovered;

        void Awake()
        {
            if (!gameObject.TryGetComponent(out EventTrigger eventTrigger))
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            Trigger = eventTrigger;
            SetButtonTriggers();
        }

        void SetButtonTriggers()
        {
            gameObject.AddTrigger(EventTriggerType.PointerEnter, (UnityAction<BaseEventData>)OnPointerEnter, Trigger);
            gameObject.AddTrigger(EventTriggerType.PointerExit, (UnityAction<BaseEventData>)OnPointerExit, Trigger);
            gameObject.AddTrigger(EventTriggerType.PointerUp, (UnityAction<BaseEventData>)OnPointerUp, Trigger);
            gameObject.AddTrigger(EventTriggerType.PointerDown, (UnityAction<BaseEventData>)OnPointerDown, Trigger);

            gameObject.AddTrigger(EventTriggerType.InitializePotentialDrag, (UnityAction<BaseEventData>)OnInitializePotentialDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.BeginDrag, (UnityAction<BaseEventData>)OnBeginDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.EndDrag, (UnityAction<BaseEventData>)OnEndDrag, Trigger);
        }
        protected void OnPointerEnter(BaseEventData data)
        {
            isHovered = true;
            CharacterPoolPopup.Instance.HoveredCharacter = this;
        }
        protected void OnPointerExit(BaseEventData data)
        {
            isHovered = false;
            CharacterPoolPopup.Instance.HoveredCharacter = null;
        }
        protected void OnPointerUp(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            if (!isDragging && pointerData.button == PointerEventData.InputButton.Left)
            {
                if (IsCurrentPool)
                {
                    CharacterPoolPopup.Instance.RemoveCharacter(Character, true);
                }
                else
                {
                    CharacterPoolPopup.Instance.AddCharacter(Character.GetData(), true);
                }
            }
            else if (!isDragging && pointerData.button == PointerEventData.InputButton.Right)
            {
                CustomScenarioPopup.Instance.PopupCharacterInfo(Character.character);
            }
            isDragging = false;
        }
        protected void OnPointerDown(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            isDragging = false;
        }
        protected void OnInitializePotentialDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            PoolMenu.scrollPool.OnInitializePotentialDrag(pointerData);
        }
        protected void OnDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            PoolMenu.scrollPool.OnDrag(pointerData);
        }
        protected void OnBeginDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            isDragging = true;
            PoolMenu.scrollPool.OnBeginDrag(pointerData);
        }
        protected void OnEndDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            isDragging = false;
            PoolMenu.scrollPool.OnEndDrag(pointerData);
        }
    }
}
