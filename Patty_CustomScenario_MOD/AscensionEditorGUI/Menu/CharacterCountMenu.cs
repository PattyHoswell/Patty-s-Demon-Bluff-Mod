using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.Enums;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    public class CharacterCountMenu : ScrollableContentMenu
    {
        public CharacterCountMenu() : base(ClassInjector.DerivedConstructorPointer<CharacterCountMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CharacterCountMenu(IntPtr ptr) : base(ptr) { }
        public CharactersCount TargetCharCount { get; internal set; }
        public new List<AllCharacterMenuButton> ContentButtons { get; internal set; } = new();
        public Dictionary<ECharactersCountType, List<CharacterCountButton>> CharactersCountButton = new()
        {
            { ECharactersCountType.Characters, new List<CharacterCountButton>() },
            { ECharactersCountType.Deck, new List<CharacterCountButton>() },
        };

        protected override void Awake()
        {
            base.Awake();
            var hiddenContentTransform = HiddenContent.transform;

            void ProcessChild(Transform parent, ECharactersCountType countType, bool isDeck = false)
            {
                for (var j = 0; j < parent.childCount; j++)
                {
                    var button = parent.GetChild(j);
                    if (button.name == "Text (TMP)")
                    {
                        continue;
                    }
                    var charType = Enum.Parse<ECharacterType>(button.name);
                    var characterCountButton = button.gameObject.AddComponent<CharacterCountButton>();
                    characterCountButton.CharacterType = charType;
                    characterCountButton.IsDeck = isDeck;
                    CharactersCountButton[countType].Add(characterCountButton);
                }
            }

            for (var i = 0; i < hiddenContentTransform.childCount; i++)
            {
                var child = hiddenContentTransform.GetChild(i);
                if (child.name == "Characters")
                {
                    ProcessChild(child, ECharactersCountType.Characters);
                }
                else if (child.name == "Deck View")
                {
                    ProcessChild(child, ECharactersCountType.Deck, isDeck: true);
                }
            }

            foreach (var buttonList in CharactersCountButton.Values)
            {
                foreach (var button in buttonList)
                {
                    button.OnAmountChanged += CharacterCountButton_OnAmountChanged;
                }
            }

            CustomScenarioPopup.Instance.OnAscensionChanged += Instance_OnAscensionChanged;
            CustomScenarioPopup.Instance.OnScriptInfoChanged += Instance_OnScriptInfoChanged;
            CustomScenarioPopup.Instance.OnCustomScriptDataChanged += Instance_OnCustomScriptDataChanged;
            Setup();
        }

        private void Instance_OnAscensionChanged(AscensionsData obj)
        {
            ClearContent(true);
            ContentButtons.Clear();
            Setup();
        }

        private void Instance_OnScriptInfoChanged(ScriptInfo obj)
        {
            ClearContent(true);
            ContentButtons.Clear();
            Setup();
        }

        private void Instance_OnCustomScriptDataChanged(CustomScriptData obj)
        {
            ClearContent(true);
            ContentButtons.Clear();
            Setup();
        }

        private void CharacterCountButton_OnAmountChanged(CharacterCountButton instance, int previousAmount, int newAmount)
        {
            if (!instance.initialized)
            {
                return;
            }
            if (instance.IsDeck)
            {
                var characterToShowButton = GetCharacterCountButton(instance.CharacterType, ECharactersCountType.Characters);
                if (characterToShowButton == null || !characterToShowButton.initialized)
                {
                    return;
                }
                if (newAmount < characterToShowButton.Amount)
                {
                    instance.SetValue(characterToShowButton.Amount);
                }
            }
            else
            {
                var deckButton = GetCharacterCountButton(instance.CharacterType, ECharactersCountType.Deck);
                if (deckButton == null || !deckButton.initialized)
                {
                    return;
                }
                if (newAmount > deckButton.Amount)
                {
                    deckButton.SetValue(newAmount);
                }
            }
        }

        internal protected override void Setup()
        {
            Il2CppSystem.Collections.Generic.List<CharactersCount> characterCounts = CustomScenarioPopup.Instance.GetCharactersCounts(isOriginalRef: false);
            for (var i = 0; i < characterCounts.Count; i++)
            {
                if (characterCounts[i] == null) continue;
                var allCharacterMenuButton = AddItem(addToList: false).GetComponent<AllCharacterMenuButton>();
                allCharacterMenuButton.TargetCharCount = characterCounts[i];
            }
            SortButtons();
        }


        public override void ClearContent(bool immediate = false)
        {
            foreach (var button in ContentButtons)
            {
                if (immediate)
                {
                    DestroyImmediate(button.gameObject);
                }
                else
                {
                    Destroy(button.gameObject);
                }
            }
            ContentButtons.Clear();
        }

        public override void SortButtons()
        {
            var orderedButton = ContentButtons.OrderBy(x => x.gameObject.transform.GetSiblingIndex()).ToList();
            for (var i = 0; i < orderedButton.Count; i++)
            {
                var button = orderedButton[i];
                button.name = $"Characters Count {orderedButton.IndexOf(button)}";
                button.transform.SetSiblingIndex(i);
                button.UpdateLabelName();
            }
        }

        public override bool RemoveItem(ContentAmountButton contentAmountButton)
        {
            if (ContentButtons.Count <= 1)
            {
                CustomScenario.Logger.Warning("Cannot remove because its the last character count available");
                return false;
            }
            var contentButton = ContentButtons.Find(button => button == contentAmountButton);
            if (contentButton == null)
            {
                MelonLogger.Msg("Not found button to remove");
                return false;
            }
            CustomScenarioPopup.Instance.GetCharactersCounts(isOriginalRef: true).Remove(contentButton.TargetCharCount);
            ContentButtons.Remove(contentButton);
            Destroy(contentButton.gameObject);
            SortButtons();
            return true;
        }

        public override GameObject AddItem(bool addToList = true)
        {
            var newItem = Instantiate(Prefab, ScrollableArea.content);
            newItem.gameObject.SetActive(true);
            var contentButton = newItem.AddComponent<AllCharacterMenuButton>();
            contentButton.name = $"Characters Count {contentButton.transform.GetSiblingIndex()}";
            contentButton.TargetScrollableContent = this;
            contentButton.TargetCharCount = GameUtility.CreateDefaultCharactersCount();
            contentButton.UpdateLabelName();
            ContentButtons.Add(contentButton);
            if (addToList)
            {
                CustomScenarioPopup.Instance.GetCharactersCounts(isOriginalRef: true).Add(contentButton.TargetCharCount);
            }
            return contentButton.gameObject;
        }

        public CharacterCountButton? GetCharacterCountButton(ECharacterType characterType, ECharactersCountType countType)
        {
            return CharactersCountButton[countType].FirstOrDefault(x => x.CharacterType == characterType);
        }

        public void Open(AllCharacterMenuButton characterAmtButton)
        {
            TargetCharCount = characterAmtButton.TargetCharCount;
            foreach (var (charCountType, buttons) in CharactersCountButton)
            {
                foreach (var button in buttons)
                {
                    button.ParentContentButton = characterAmtButton;
                    button.TargetCharCount = TargetCharCount;
                    button.Setup();
                }
            }
            ShowContent();
        }
    }
}
