using Il2Cpp;
using Il2CppDG.Tweening;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    public class CharacterPoolPopup : MonoBehaviour
    {

        public CharacterPoolPopup() : base(ClassInjector.DerivedConstructorPointer<CharacterPoolPopup>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public CharacterPoolPopup(IntPtr ptr) : base(ptr) { }

        public static CharacterPoolPopup Instance { get; private set; }

        public Button CloseButton { get; private set; }
        public CharacterButton HoveredCharacter { get; set; }

        public CompendiumCharacter characterPrefab;
        public TextMeshProUGUI title;
        public CharacterPoolMenu currentPool, allCharactersPool;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CloseButton = transform.Find("CloseButton").GetComponent<Button>();
            CloseButton.onClick.AddListener((UnityAction)CloseMenu);
            title = transform.Find("Title/Label").GetComponent<TextMeshProUGUI>();

            SetupPrefab();
            SetupPool();
        }

        public void OpenMenu(EAlignment alignment, ECharacterType characterType)
        {
            title.text = $"{characterType.ToString()} Pool";
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

            var scriptInfo = CustomScenarioPopup.Instance.GetScriptInfo();
            var characters = new List<CharacterData>();
            if (characterType == ECharacterType.Villager)
            {
                characters = scriptInfo.startingTownsfolks;
            }
            else if (characterType == ECharacterType.Outcast)
            {
                characters = scriptInfo.startingOutsiders;
            }
            else if (characterType == ECharacterType.Minion)
            {
                characters = scriptInfo.startingMinions;
            }
            else if (characterType == ECharacterType.Demon)
            {
                characters = scriptInfo.startingDemons;
            }
            IEnumerator InitalizeAllCharacters(EAlignment alignment, ECharacterType characterType)
            {
                currentPool.DestroyAllContent();
                allCharactersPool.DestroyAllContent();

                yield return YieldCache.WaitForEndOfFrame.Value;

                currentPool.SetDropdown(alignment, characterType);
                allCharactersPool.SetDropdown(alignment, characterType);
                var allCharacters = new List<CharacterData>(CustomScenario.allCharacterData.Value.Length);
                foreach (var data in CustomScenario.allCharacterData.Value)
                {
                    allCharacters.Add(data);
                }
                yield return currentPool.OpenMenu(CustomScenario.AllAlignment, CustomScenario.AllCharacterType, characters);
                yield return allCharactersPool.OpenMenu(alignment, characterType, allCharacters);
            }
            MelonCoroutines.Start(InitalizeAllCharacters(alignment, characterType));
        }

        public void OpenMustIncludePool()
        {
            title.text = "Must Include Pool";
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

            var scriptInfo = CustomScenarioPopup.Instance.GetScriptInfo();
            var characters = scriptInfo.mustInclude;
            IEnumerator InitalizeAllCharacters()
            {
                currentPool.DestroyAllContent();
                allCharactersPool.DestroyAllContent();

                yield return YieldCache.WaitForEndOfFrame.Value;

                currentPool.SetDropdown(CustomScenario.AllAlignment, CustomScenario.AllCharacterType);
                allCharactersPool.SetDropdown(CustomScenario.AllAlignment, CustomScenario.AllCharacterType);
                var allCharacters = new List<CharacterData>(CustomScenario.allCharacterData.Value.Length);
                foreach (var data in CustomScenario.allCharacterData.Value)
                {
                    allCharacters.Add(data);
                }
                yield return currentPool.OpenMenu(CustomScenario.AllAlignment, CustomScenario.AllCharacterType, characters);
                yield return allCharactersPool.OpenMenu(CustomScenario.AllAlignment, CustomScenario.AllCharacterType, allCharacters);
            }
            MelonCoroutines.Start(InitalizeAllCharacters());
        }

        public void CloseMenu()
        {
            transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
        }

        public void AddCharacter(CharacterData data, bool isCurrentPool)
        {
            if (isCurrentPool)
            {
                currentPool.AddCharacter(data, sort: true);
            }
            else
            {
                allCharactersPool.AddCharacter(data, sort: true);
            }
        }

        public void RemoveCharacter(CompendiumCharacter chaacter, bool isCurrentPool)
        {
            if (isCurrentPool)
            {
                currentPool.RemoveCharacter(chaacter);
            }
            else
            {
                allCharactersPool.RemoveCharacter(chaacter);
            }
        }

        void SetupPrefab()
        {
            characterPrefab = GameObject.FindObjectOfType<CompendiumCharacter>(true);
            characterPrefab = Instantiate(characterPrefab);
            characterPrefab.gameObject.SetActive(false);
            characterPrefab.transform.Find("Icon/Card/Backside").localScale = Vector3.zero;
            characterPrefab.transform.Find("Icon/Card/Shadow").localScale = Vector3.zero;
            DontDestroyOnLoad(characterPrefab.gameObject);
        }

        void SetupPool()
        {
            var content = transform.Find("Content");
            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child.name == "Current Pool")
                {
                    currentPool = child.gameObject.AddComponent<CharacterPoolMenu>();
                    currentPool.isCurrentPool = true;
                }
                else if (child.name == "AllCharactersPool")
                {
                    allCharactersPool = child.gameObject.AddComponent<CharacterPoolMenu>();
                    allCharactersPool.isCurrentPool = false;
                }
            }
        }
        public void OnGUI()
        {
            if (HoveredCharacter == null)
            {
                return;
            }
            Event e = Event.current;
            if (!e.isScrollWheel)
            {
                return;
            }
            Vector2 scrollDelta = e.delta;
            if (scrollDelta.y == 0)
            {
                return;
            }
            var pointerData = new PointerEventData(EventSystem.current);
            pointerData.scrollDelta = new Vector2(
                scrollDelta.x,
                (scrollDelta.y * -1) / 3
            );
            HoveredCharacter.PoolMenu.scrollPool.OnScroll(pointerData);
        }
    }
}