using Il2CppDG.Tweening;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    public class ScrollableContentMenu : MonoBehaviour
    {
        public ScrollableContentMenu() : base(ClassInjector.DerivedConstructorPointer<ScrollableContentMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public ScrollableContentMenu(IntPtr ptr) : base(ptr) { }

        public Button AddOrCloseButton { get; internal set; }
        public TextMeshProUGUI ButtonLabel { get; internal set; }
        public GameObject HiddenContent { get; internal set; }
        public ScrollRect ScrollableArea { get; internal set; }
        public GameObject Prefab { get; internal set; }
        public List<ContentAmountButton> ContentButtons { get; internal set; } = new();

        private Lazy<Button.ButtonClickedEvent> AddItemEvent => new Lazy<Button.ButtonClickedEvent>(() =>
        {
            var buttonEvent = new Button.ButtonClickedEvent();
            buttonEvent.AddListener((UnityAction)(() => AddItem()));
            return buttonEvent;
        }, isThreadSafe: false);

        private Lazy<Button.ButtonClickedEvent> ToggleHiddenEvent => new Lazy<Button.ButtonClickedEvent>(() =>
        {
            var buttonEvent = new Button.ButtonClickedEvent();
            buttonEvent.AddListener((UnityAction)(HideContent));
            return buttonEvent;
        }, isThreadSafe: false);

        protected virtual void Awake()
        {
            HiddenContent = transform.GetChild(transform.childCount - 1).gameObject;
            AddOrCloseButton = transform.Find("Title/AddButton").GetComponent<Button>();
            if (AddOrCloseButton != null)
            {
                ButtonLabel = AddOrCloseButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            }
            ScrollableArea = GetComponentInChildren<ScrollRect>(includeInactive: true);
            AddOrCloseButton.onClick = AddItemEvent.Value;

            for (var i = 0; i < ScrollableArea.content.childCount; i++)
            {
                var child = ScrollableArea.content.GetChild(i);
                Prefab = child.gameObject;
                Prefab.gameObject.SetActive(false);
            }
            Prefab.transform.SetParent(transform);
            HiddenContent.transform.localScale = new Vector3(HiddenContent.transform.localScale.x, 0, HiddenContent.transform.localScale.z);
        }

        public void ShowContent()
        {
            ScrollableArea.transform.DOScaleY(0, 0.125f).onComplete = (TweenCallback)(() =>
            {
                HiddenContent.SetActive(true);
                HiddenContent.transform.DOScaleY(1, 0.15f);

                ScrollableArea.gameObject.SetActive(false);
                AddOrCloseButton.onClick = ToggleHiddenEvent.Value;
            });
            ButtonLabel.text = "Close";
            OnShowContent();
        }

        public void HideContent()
        {
            HiddenContent.transform.DOScaleY(0, 0.15f).onComplete = (TweenCallback)(() =>
            {
                ScrollableArea.gameObject.SetActive(true);
                ScrollableArea.transform.DOScaleY(1, 0.15f);

                HiddenContent.SetActive(false);
                AddOrCloseButton.onClick = AddItemEvent.Value;
            });
            ButtonLabel.text = "Add";
            OnHideContent();
        }

        /// <summary>
        /// Should be abstract but due to IL2CPP limitations. It is virtual instead.
        /// </summary>
        internal protected virtual void Setup()
        {
        }

        public virtual void ClearContent(bool immediate = false)
        {
            var content = ScrollableArea.content;
            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i).gameObject;
                if (immediate)
                {
                    DestroyImmediate(child);
                }
                else
                {
                    Destroy(child);
                }
            }
        }

        public virtual void SortButtons()
        {
            ContentButtons = ContentButtons.OrderBy(x => x.gameObject.transform.GetSiblingIndex()).ToList();
            for (var i = 0; i < ContentButtons.Count; i++)
            {
                var button = ContentButtons[i];
                button.name = $"Characters Count {ContentButtons.IndexOf(button)}";
                button.transform.SetSiblingIndex(i);
                button.UpdateLabelName();
            }
        }

        protected virtual void OnShowContent()
        {
        }

        protected virtual void OnHideContent()
        {
        }

        public virtual void Open()
        {
            ShowContent();
        }

        public virtual bool RemoveItem(ContentAmountButton contentAmountButton)
        {
            var contentButton = ContentButtons.Find(button => button == contentAmountButton);
            if (contentButton == null)
            {
                return false;
            }
            ContentButtons.Remove(contentButton);
            Destroy(contentButton.gameObject);
            return true;
        }

        public virtual GameObject AddItem(bool addToList = true)
        {
            var newItem = Instantiate(Prefab, ScrollableArea.content);
            newItem.gameObject.SetActive(true);
            if (addToList)
            {
                ContentButtons.Add(newItem.AddComponent<ContentAmountButton>());
            }
            return newItem;
        }
    }
}