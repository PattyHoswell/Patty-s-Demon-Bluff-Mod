using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using MelonLoader;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.Enums;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    public class CharacterPoolMenu : MonoBehaviour
    {
        public CharacterPoolMenu() : base(ClassInjector.DerivedConstructorPointer<CharacterPoolMenu>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        public CharacterPoolMenu(IntPtr ptr) : base(ptr) { }

        public TextMeshProUGUI label;
        public List<TMP_Dropdown> dropdowns = new List<TMP_Dropdown>();
        public ScrollRect scrollPool;
        public EAlignment targetAlignment;
        public ECharacterType targetCharacterType;
        public EMenuType menuType = EMenuType.All;
        public List<CharacterData> availableChaactersData = new List<CharacterData>();
        public List<CompendiumCharacter> allCharacters = new List<CompendiumCharacter>();
        public bool isCurrentPool;

        void Awake()
        {
            var labelAndOptions = transform.Find("LabelAndOptions");
            label = labelAndOptions.Find("Label").GetComponent<TextMeshProUGUI>();
            foreach (var dropdown in labelAndOptions.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                dropdowns.Add(dropdown);
            }
            scrollPool = transform.Find("Scroll View").GetComponent<ScrollRect>();

            SetupDropdowns();
            DestroyAllContent();
        }

        internal void DestroyAllContent()
        {
            for (var i = 0; i < scrollPool.content.childCount; i++)
            {
                Destroy(scrollPool.content.GetChild(i).gameObject);
            }
        }

        void SetupDropdowns()
        {
            for (var i = 0; i < dropdowns.Count; i++)
            {
                var dropdown = dropdowns[i];
                dropdown.template.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                dropdown.ClearOptions();
                switch (dropdown.name)
                {

                    case "TypeDropdown":
                        foreach (var menuType in Enumeration.GetAll<EMenuType>())
                        {
                            var option = new TMP_Dropdown.OptionData();
                            option.text = menuType.Name;
                            dropdown.options.Add(option);
                        }
                        break;

                    case "ModDropdown":
                        foreach (var name in MelonBase.RegisteredMelons.Select(x => x.MelonAssembly.Assembly))
                        {
                            var option = new TMP_Dropdown.OptionData();
                            option.text = Path.GetFileNameWithoutExtension(name.Location);
                            dropdown.options.Add(option);
                        }
                        break;
                    case "AlignmentDropdown":
                        foreach (var name in Il2CppSystem.Enum.GetNames(Il2CppType.Of<EAlignment>()).OrderBy(x => x))
                        {
                            var option = new TMP_Dropdown.OptionData();
                            option.text = name;
                            dropdown.options.Add(option);
                        }
                        break;
                    case "CharacterTypeDropdown":
                        foreach (var name in Il2CppSystem.Enum.GetNames(Il2CppType.Of<ECharacterType>()).OrderBy(x => x))
                        {
                            var option = new TMP_Dropdown.OptionData();
                            option.text = name;
                            dropdown.options.Add(option);
                        }
                        break;
                }
                dropdown.onValueChanged.AddListener((UnityAction<int>)(index =>
                {
                    OnDropdownChanged(dropdown, index);
                }));
            }
        }

        public IEnumerator OpenMenu(EAlignment alignment, ECharacterType characterType, List<CharacterData> characterDatas)
        {
            targetAlignment = alignment;
            targetCharacterType = characterType;
            availableChaactersData = characterDatas;
            DestroyAllContent();
            yield return YieldCache.WaitForEndOfFrame.Value;
            yield return AddContents(alignment, characterType, characterDatas);
        }

        internal void SetDropdown(EAlignment alignment, ECharacterType characterType)
        {
            if (TryGetDropdown("TypeDropdown", out var dropdown))
            {
                Func<TMP_Dropdown.OptionData, bool> match = x => x.text == EMenuType.All.Name;
                dropdown.SetValueWithoutNotify(dropdown.options.FindIndex(match));
            }
            if (TryGetDropdown("AlignmentDropdown", out dropdown))
            {
                Func<TMP_Dropdown.OptionData, bool> match = x => x.text == alignment.ToString();
                dropdown.SetValueWithoutNotify(dropdown.options.FindIndex(match));
            }
            if (TryGetDropdown("CharacterTypeDropdown", out dropdown))
            {
                Func<TMP_Dropdown.OptionData, bool> match = x => x.text == characterType.ToString();
                dropdown.SetValueWithoutNotify(dropdown.options.FindIndex(match));
            }
        }

        public bool TryGetDropdown(string name, out TMP_Dropdown? dropdown)
        {
            Func<TMP_Dropdown, bool> findFunc = x => x.name == name;
            dropdown = dropdowns.Find(findFunc);
            return dropdown != null;
        }

        public void AddCharacter(CharacterData data, bool addToList = true, bool sort = false)
        {
            var newCharacter = Instantiate(CharacterPoolPopup.Instance.characterPrefab, scrollPool.content, false);
            newCharacter.name = data.name;
            newCharacter.gameObject.SetActive(true);
            newCharacter.transform.localScale = Vector3.one;
            newCharacter.transform.Find("Icon/Card/Backside").localScale = Vector3.zero;
            newCharacter.transform.Find("Icon/Card/Shadow").GetComponent<Image>().enabled = false;
            newCharacter.Init(data, ECompendiumCardState.Unlocked);
            var trigger = newCharacter.gameObject.AddComponent<CharacterButton>();
            trigger.PoolMenu = this;
            trigger.Character = newCharacter;
            trigger.IsCurrentPool = isCurrentPool;
            allCharacters.Add(newCharacter);
            if (addToList)
                availableChaactersData.Add(data);
            if (sort)
                SortCharacters();
        }

        public bool RemoveCharacter(CompendiumCharacter character)
        {
            if (character == null || !allCharacters.Contains(character))
            {
                return false;
            }
            availableChaactersData.Remove(character.GetData());
            allCharacters.Remove(character);
            Destroy(character.gameObject);
            return true;
        }

        public void OnDropdownChanged(TMP_Dropdown tmpDropdown, int index)
        {
            System.Collections.IEnumerator OnDropdownChanged_Internal(TMP_Dropdown tmpDropdown, int index)
            {
                DestroyAllContent();
                yield return null;
                switch (tmpDropdown.name)
                {
                    case "TypeDropdown":
                        menuType = EMenuType.Parse(index);
                        /*
                        if (TryGetDropdown("ModDropdown", out var modDropdown))
                        {
                            modDropdown.gameObject.SetActive(menuType == MenuType.Modded);
                        }*/
                        break;
                    /*case "ModDropdown":
                        modTarget = tmpDropdown.options[index].text;
                        break;*/
                    case "AlignmentDropdown":
                        targetAlignment = (EAlignment)Enum.Parse(typeof(EAlignment), tmpDropdown.options[index].text);
                        break;
                    case "CharacterTypeDropdown":
                        targetCharacterType = (ECharacterType)Enum.Parse(typeof(ECharacterType), tmpDropdown.options[index].text);
                        break;
                }
                yield return AddContents(targetAlignment, targetCharacterType, availableChaactersData);
            }


            MelonCoroutines.Start(OnDropdownChanged_Internal(tmpDropdown, index));
        }

        public void SortCharacters()
        {
            var sortedCharacters = SortCharacters(allCharacters).ToList();
            for (var i = 0; i < sortedCharacters.Count; i++)
            {
                sortedCharacters[i].transform.SetSiblingIndex(i);
            }
        }

        public IOrderedEnumerable<CompendiumCharacter> SortCharacters(List<CompendiumCharacter> characters)
        {
            var managedList = new System.Collections.Generic.List<CompendiumCharacter>(characters.Count);
            foreach (var character in characters)
            {
                if (character == null)
                    continue;

                managedList.Add(character);
            }
            return managedList.OrderBy(x => x.GetData().type).ThenBy(x => x.GetData().startingAlignment).ThenBy(x => x.GetData().name);
        }
        public IOrderedEnumerable<CharacterData> SortCharacterDatas(List<CharacterData> characterDatas)
        {
            var managedList = new System.Collections.Generic.List<CharacterData>(characterDatas.Count);
            foreach (var data in characterDatas)
                managedList.Add(data);
            return managedList.OrderBy(x => x.type).ThenBy(x => x.startingAlignment).ThenBy(x => x.name);
        }

        internal IEnumerator AddContents(EAlignment alignment, ECharacterType characterType, List<CharacterData> characterDatas)
        {
            foreach (var characterData in SortCharacterDatas(characterDatas))
            {
                if (menuType == EMenuType.Modded && Path.GetFileName(characterData.role?.GetType().Assembly.Location) == "Assembly-CSharp.dll")
                {
                    continue;
                }
                else if (menuType == EMenuType.Vanilla && Path.GetFileName(characterData.role?.GetType().Assembly.Location) != "Assembly-CSharp.dll")
                {
                    continue;
                }
                if (characterData.startingAlignment != alignment && alignment != CustomScenario.AllAlignment)
                {
                    continue;
                }
                if (characterData.type != characterType && characterType != CustomScenario.AllCharacterType)
                {
                    continue;
                }
                AddCharacter(characterData, false);
                yield return YieldCache.WaitForEndOfFrame.Value;
            }
        }
    }
}