using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons
{
    /// <summary>
    /// Button handler for Characters Amount inside of <see cref="CharacterCountMenu"/>
    /// </summary>
    public class ContentAmountButton : MonoBehaviour
    {
        public ContentAmountButton() : base(ClassInjector.DerivedConstructorPointer<ContentAmountButton>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public ContentAmountButton(IntPtr ptr) : base(ptr) { }

        // Dark Purple default highlighted Color
        public Color32 HighlightedColor { get; set; } = new Color32(28, 14, 30, 100);

        public ScrollableContentMenu TargetScrollableContent { get; internal set; }

        #region User Interface
        public CanvasGroup CanvasGroupInfo { get; internal set; }
        public Image BackgroundImg { get; internal set; }
        public TextMeshProUGUI NameLabel { get; internal set; }
        public TextButton RemoveButton { get; internal set; }
        public EventTrigger Trigger { get; internal set; }
        #endregion

        private bool isInitialized, isAboutToBeDestroyed;

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            BackgroundImg = GetComponent<Image>();
            NameLabel = BackgroundImg.GetComponentInChildren<TextMeshProUGUI>(true);
            CanvasGroupInfo = GetComponent<CanvasGroup>();
            Trigger = gameObject.AddComponent<EventTrigger>();
            RemoveButton = transform.Find("RemoveButton").gameObject.AddComponent<TextButton>();
            RemoveButton.Button.onClick.AddListener((UnityAction)(RemoveItem));
            SetButtonTriggers();
        }

        void RemoveItem()
        {
            TargetScrollableContent.RemoveItem(this);
        }

        void SetButtonTriggers()
        {
            gameObject.AddTrigger(EventTriggerType.PointerEnter, (UnityAction<BaseEventData>)OnItemHoverEntered, Trigger);
            gameObject.AddTrigger(EventTriggerType.PointerExit, (UnityAction<BaseEventData>)OnItemHoverExited, Trigger);
            gameObject.AddTrigger(EventTriggerType.PointerClick, (UnityAction<BaseEventData>)OnItemPressed, Trigger);
            gameObject.AddTrigger(EventTriggerType.InitializePotentialDrag, (UnityAction<BaseEventData>)OnInitializePotentialDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.BeginDrag, (UnityAction<BaseEventData>)OnBeginDrag, Trigger);
            gameObject.AddTrigger(EventTriggerType.EndDrag, (UnityAction<BaseEventData>)OnEndDrag, Trigger);
        }

        protected void OnItemPressed(BaseEventData data)
        {
            if (isAboutToBeDestroyed)
            {
                return;
            }
            OnPointerClick(data);
        }

        protected void OnItemHoverEntered(BaseEventData data)
        {
            if (isAboutToBeDestroyed)
            {
                return;
            }
            AscensionPopup.Instance.HoveredItem = this;
            OnPointerEnter(data);
        }

        protected void OnItemHoverExited(BaseEventData data)
        {
            if (isAboutToBeDestroyed)
            {
                return;
            }

            AscensionPopup.Instance.HoveredItem = null;
            OnPointerExit(data);
        }

        protected void OnInitializePotentialDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            TargetScrollableContent.ScrollableArea.OnInitializePotentialDrag(pointerData);
        }
        protected void OnDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            TargetScrollableContent.ScrollableArea.OnDrag(pointerData);
        }
        protected void OnBeginDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            TargetScrollableContent.ScrollableArea.OnBeginDrag(pointerData);
        }
        protected void OnEndDrag(BaseEventData data)
        {
            var pointerData = data.TryCast<PointerEventData>();
            if (pointerData is null)
            {
                return;
            }
            TargetScrollableContent.ScrollableArea.OnEndDrag(pointerData);
        }

        void OnDestroy()
        {
            isAboutToBeDestroyed = true;
        }

        public virtual void OnPointerClick(BaseEventData data)
        {
            TargetScrollableContent.Open();
        }

        public virtual void OnPointerEnter(BaseEventData data)
        {
            Highlight();
        }

        public virtual void OnPointerExit(BaseEventData data)
        {
            UnHighlight();
        }

        public void Highlight()
        {
            Initialize();
            BackgroundImg.color = HighlightedColor;
            CanvasGroupInfo.alpha = 0.6f;
        }

        public void UnHighlight()
        {
            Initialize();
            BackgroundImg.color = Color.clear;
            CanvasGroupInfo.alpha = 1f;
        }

        public virtual void UpdateLabelName()
        {
            Initialize();
            NameLabel.text = $"Content Idx: {transform.GetSiblingIndex()}";
        }
    }
}
