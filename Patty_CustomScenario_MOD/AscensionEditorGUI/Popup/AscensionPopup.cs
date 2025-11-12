using Il2Cpp;
using Il2CppDG.Tweening;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Buttons;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    internal class AscensionPopup : MonoBehaviour
    {

        public AscensionPopup() : base(ClassInjector.DerivedConstructorPointer<AscensionPopup>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public AscensionPopup(IntPtr ptr) : base(ptr) { }

        public static AscensionPopup Instance { get; private set; }

        public CharacterCountMenu CharacterCountMenu { get; private set; }
        public AllCharacterMenu AllCharacterMenu { get; private set; }
        public PossibleScriptMenu PossibleScriptMenu { get; private set; }
        public PossibleScriptDataMenu PossibleScriptDataMenu { get; private set; }

        public Button CloseButton { get; private set; }
        public ContentAmountButton HoveredItem { get; set; }

        public (TMP_InputField inputField, TextMeshProUGUI label, Image background) title;

        public event Action<string> OnChangeCustomScriptDataName;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            title = (transform.Find("Title/InputField (TMP)").GetComponent<TMP_InputField>(),
                     transform.Find("Title/Text (TMP)").GetComponent<TextMeshProUGUI>(),
                     transform.Find("Title").GetComponent<Image>());
            title.inputField.gameObject.SetActive(false);
            title.label.gameObject.SetActive(true);
            title.inputField.onSubmit.AddListener(new Action<string>(OnInputFieldSubmit));

            var titleTransform = title.background.transform;
            titleTransform.gameObject.AddTrigger(EventTriggerType.PointerEnter, new Action<BaseEventData>(OnPointerEnter));
            titleTransform.gameObject.AddTrigger(EventTriggerType.PointerExit, new Action<BaseEventData>(OnPointerExit));
            titleTransform.gameObject.AddTrigger(EventTriggerType.PointerClick, new Action<BaseEventData>(OnPointerClick));

            CustomScenarioPopup.Instance.OnAscensionChanged += Instance_OnAscensionChanged;
            CustomScenarioPopup.Instance.OnCustomScriptDataChanged += Instance_OnCustomScriptDataChanged;

            CloseButton = transform.Find("CloseButton").GetComponent<Button>();
            CloseButton.onClick.AddListener((UnityAction)CloseMenu);

            var content = transform.Find("Content");
            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i).gameObject;
                if (child.name == "Characters Amt")
                {
                    CharacterCountMenu = child.AddComponent<CharacterCountMenu>();
                }
                else if (child.name == "All Characters")
                {
                    AllCharacterMenu = child.AddComponent<AllCharacterMenu>();
                }
                else if (child.name == "Possible Scripts")
                {
                    PossibleScriptMenu = child.AddComponent<PossibleScriptMenu>();
                }
                else if (child.name == "Possible Scripts Data")
                {
                    PossibleScriptDataMenu = child.AddComponent<PossibleScriptDataMenu>();
                }
                else
                {
                    child.AddComponent<ScrollableContentMenu>();
                }
            }

            var bottomScrollRect = transform.Find("Bottom Buttons").GetComponent<ScrollRect>();
            var button = bottomScrollRect.content.Find("Save As").GetComponent<Button>();
            button.onClick.AddListener(new Action(() =>
            {
                var filter = "JSON Files (*.json)\0*.json\0\0";
                var initialDirectory = CustomScenario.AscensionDataFolder;
                WindowsFileDialog.SaveSingleFile((filePath) =>
                {
                    CustomScenario.Logger.Msg($"Saving {nameof(AscensionsData)} to path {filePath}");
                    var scenarioData = new AscensionsData_Json();
                    scenarioData.Initialize(CustomScenarioPopup.Instance.CurrentAscensionData);
                    scenarioData.Name = Path.GetFileNameWithoutExtension(filePath);

                    UniversalUtility.SerializeJson(filePath, scenarioData);
                    scenarioData.SerializeCustomScriptData(CustomScenario.CustomScriptDataFolder, true);

                }, "Save Ascension Data", filter, initialDirectory, CustomScenarioPopup.Instance.CurrentAscensionData.name, defaultExt: ".json");
            }));

            button = bottomScrollRect.content.Find("Load").GetComponent<Button>();
            button.onClick.AddListener(new Action(() =>
            {
                var filter = "JSON Files (*.json)\0*.json\0" +
                             "All Files\0*.*\0\0";
                var initialDirectory = CustomScenario.AscensionDataFolder;
                WindowsFileDialog.OpenSingleFile((filePath) =>
                {
                    if (!File.Exists(filePath))
                    {
                        return;
                    }
                    if (!CustomScenario.customAscensions.TryGetValue(filePath, out AscensionsData_Json result))
                    {
                        if (!UniversalUtility.LoadJson(filePath, out result))
                        {
                            return;
                        }
                        CustomScenario.customAscensions.Add(filePath, result);
                    }
                    var unloadedScriptData = result.PossibleScriptsDataName.FindAll(x => CustomScenario.customScripts.Values.All(y => x != y.Name));
                    if (unloadedScriptData.Count > 0)
                    {
                        string[] files = Directory.GetFiles(CustomScenario.CustomScriptDataFolder, "*.json", SearchOption.AllDirectories);
                        foreach (var scenario in unloadedScriptData)
                        {
                            CustomScenario.Logger.Msg($"Looking for unloaded script data {scenario}");
                            foreach (var file in files)
                            {
                                if (CustomScenario.customScripts.ContainsKey(file))
                                {
                                    continue;
                                }
                                if (UniversalUtility.LoadJson(file, out CustomScriptData_Json scriptDataResult))
                                {
                                    CustomScenario.customScripts.Add(file, scriptDataResult);
                                }
                            }
                        }
                    }
                    var customScriptData = GameUtility.FindAscensionData(result.Name) ?? ScriptableObject.CreateInstance<AscensionsData>();
                    result.Assign(customScriptData);
                    CustomScenarioPopup.Instance.SetAscensionData(customScriptData);

                }, "Load Ascension Data", filter, initialDirectory, defaultExt: ".json");
            }));


            button = bottomScrollRect.content.Find("Save").GetComponent<Button>();
            button.onClick.AddListener(new Action(() =>
            {
                UniversalUtility.ExtractAscension(CustomScenarioPopup.Instance.CurrentAscensionData, true);
            }));

            button = bottomScrollRect.content.Find("Must Include").GetComponent<Button>();
            button.onClick.AddListener(new Action(() =>
            {
                CharacterPoolPopup.Instance.OpenMustIncludePool();
            }));
        }

        private void Instance_OnAscensionChanged(AscensionsData obj)
        {
            ChangeTitle(CustomScenarioPopup.Instance.CurrentCustomScriptData != null ? CustomScenarioPopup.Instance.CurrentCustomScriptData.name : "Ascension Editor");
        }

        void OnInputFieldSubmit(string newTitle)
        {
            title.label.gameObject.SetActive(true);
            title.inputField.gameObject.SetActive(false);
            if (CustomScenarioPopup.Instance.CurrentCustomScriptData == null)
            {
                return;
            }
            CustomScenarioPopup.Instance.CurrentCustomScriptData.name = newTitle;
            ChangeTitle(newTitle);
            OnChangeCustomScriptDataName?.Invoke(newTitle);
        }

        void OnPointerEnter(BaseEventData data)
        {
            if (CustomScenarioPopup.Instance.CurrentCustomScriptData == null)
            {
                return;
            }
            var newColor = title.background.color / 2;
            newColor.a = 1;
            title.background.color = newColor;
        }

        void OnPointerExit(BaseEventData data)
        {
            if (CustomScenarioPopup.Instance.CurrentCustomScriptData == null)
            {
                return;
            }
            title.background.color = Color.white;
        }

        void OnPointerClick(BaseEventData data)
        {
            if (CustomScenarioPopup.Instance.CurrentCustomScriptData == null)
            {
                return;
            }
            title.label.gameObject.SetActive(false);
            title.inputField.gameObject.SetActive(true);
            title.inputField.Select();
        }

        private void Instance_OnCustomScriptDataChanged(Il2Cpp.CustomScriptData obj)
        {
            ChangeTitle(obj != null ? obj.name : "Ascension Editor");
        }

        public void ChangeTitle(string newTitle)
        {
            title.label.SetText(newTitle);
        }

        public void OpenMenu()
        {
            CustomScenarioPopup.Instance.RaycastBlocker.gameObject.SetActive(true);
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOLocalMoveX(0, 0.25f).SetEase(Ease.OutBack));
            sequence.Join(transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutSine));
        }

        public void CloseMenu()
        {
            var sequence = DOTween.Sequence();
            var moveX = transform.DOLocalMoveX(Screen.currentResolution.width + 200f, 0.25f);
            moveX.SetEase(Ease.InBack).onComplete = new Action(() =>
            {
                CustomScenarioPopup.Instance.RaycastBlocker.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
            sequence.Append(moveX);
            sequence.Join(transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InSine));
        }

        void OnGUI()
        {
            if (HoveredItem == null)
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
            HoveredItem.TargetScrollableContent.ScrollableArea.OnScroll(pointerData);
        }
    }
}
