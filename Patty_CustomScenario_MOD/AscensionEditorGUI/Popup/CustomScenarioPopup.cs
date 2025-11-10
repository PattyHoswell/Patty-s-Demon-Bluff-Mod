using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Other;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Patty_CustomScenario_MOD.AscensionEditorGUI.Menu
{
    internal class CustomScenarioPopup : MonoBehaviour
    {
        public CustomScenarioPopup() : base(ClassInjector.DerivedConstructorPointer<CustomScenarioPopup>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public CustomScenarioPopup(IntPtr ptr) : base(ptr) { }

        public static CustomScenarioPopup Instance { get; private set; }

        public Canvas Canvas { get; private set; }
        public CharacterPoolPopup CharacterPool { get; private set; }
        public AscensionPopup Ascension { get; private set; }
        public Image RaycastBlocker { get; internal set; }
        public SimpleUIInfo SimpleUIInfo { get; private set; }

        public AscensionsData CurrentAscensionData { get; private set; }
        public ScriptInfo CurrentScriptInfo { get; private set; }
        public CardExplanationUI CardExplanationPopup { get; private set; }
        public CustomScriptData CurrentCustomScriptData { get; private set; }

        public event Action<AscensionsData> OnAscensionChanged;
        public event Action<ScriptInfo> OnScriptInfoChanged;
        public event Action<CustomScriptData> OnCustomScriptDataChanged;

        public Dictionary<GameObject, CameraOverlayInfo> OriginalOverlayDatas = new Dictionary<GameObject, CameraOverlayInfo>();
        public Dictionary<GameObject, CameraOverlayInfo> NewOverlayDatas = new Dictionary<GameObject, CameraOverlayInfo>();
        private bool isOpeningInfo;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentAscensionData = ProjectContext.Instance.gameData.advancedAscension;
            SetScriptInfoFromAscensionData();

            Canvas = GetComponent<Canvas>();
            Canvas.scaleFactor = 0.77f;

            RaycastBlocker = transform.Find("RaycastBlocker").GetComponent<Image>();
            Ascension = transform.Find("Ascension Editor").gameObject.AddComponent<AscensionPopup>();
            CharacterPool = transform.Find("Character Pool Editor").gameObject.AddComponent<CharacterPoolPopup>();
            CharacterPool.gameObject.SetActive(true);

            SimpleUIInfo = FindObjectOfType<SimpleUIInfo>();
            CardExplanationPopup = SimpleUIInfo.GetComponentInChildren<CardExplanationUI>(true);

            var overlayCamera = transform.parent.Find("Overlay Camera").GetComponent<Camera>();
            foreach (var canvas in SimpleUIInfo.gameObject.GetComponentsInChildren<Canvas>(true))
            {
                OriginalOverlayDatas[canvas.gameObject] = new CameraOverlayInfo
                {
                    Camera = canvas.worldCamera,
                    LayerMask = canvas.gameObject.layer,
                    Canvas = canvas
                };
                NewOverlayDatas[canvas.gameObject] = new CameraOverlayInfo
                {
                    Camera = overlayCamera,
                    LayerMask = LayerMask.NameToLayer("TransparentFX"),
                    Canvas = canvas
                };
            }

            if (CurrentCustomScriptData is not null)
            {
                SetCustomScriptData(CurrentCustomScriptData);
            }
            else if (CurrentScriptInfo is not null)
            {
                SetScriptInfo(CurrentScriptInfo);
            }
            SetupMenuButton();
            HideScreen();
        }

        void SetScriptInfoFromAscensionData()
        {
            if (CurrentAscensionData.possibleScriptsData != null && CurrentAscensionData.possibleScriptsData.Length > 0)
            {
                CurrentCustomScriptData = CurrentAscensionData.possibleScriptsData.First();
            }
            else if (CurrentAscensionData.possibleScripts != null && CurrentAscensionData.possibleScripts.Length > 0)
            {
                CurrentScriptInfo = CurrentAscensionData.possibleScripts.First();
            }
            if (CurrentCustomScriptData == null && CurrentScriptInfo == null)
            {
                CurrentCustomScriptData = GameUtility.CreateDefaultCustomScriptData();
                CurrentAscensionData.possibleScriptsData = CurrentAscensionData.possibleScriptsData.Append(CurrentCustomScriptData).ToArray();
            }
        }

        void SetupMenuButton()
        {
            var creditsButton = GameObject.Find("Game/Menu/Content/Menu/Credits").GetComponent<GenericButton>();
            var ascensionEditorButton = Instantiate(creditsButton, creditsButton.transform.parent);
            ascensionEditorButton.name = "Ascension Editor";
            var rectTransform = ascensionEditorButton.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1, rectTransform.anchorMax.y);
            rectTransform.anchorMin = new Vector2(1, rectTransform.anchorMin.y);
            rectTransform.localPosition = new Vector3((creditsButton.transform.localPosition.x * -1), creditsButton.transform.localPosition.y, creditsButton.transform.localPosition.z);
            ascensionEditorButton.onClick = new UnityEngine.Events.UnityEvent();
            ascensionEditorButton.onClick.AddListener(new Action(OpenMenu));
            ascensionEditorButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Ascension Editor");
        }

        void HideScreen()
        {
            CharacterPool.transform.localScale = Vector3.zero;
            RaycastBlocker.gameObject.SetActive(false);
            Ascension.transform.localScale = Vector3.zero;
            var ascensionPos = Ascension.transform.localPosition;
            ascensionPos.x = Screen.currentResolution.width + 200;
            Ascension.transform.localPosition = ascensionPos;
        }

        void OpenMenu()
        {
            Canvas.scaleFactor = 0.77f;
            RaycastBlocker.gameObject.SetActive(true);
            Ascension.gameObject.SetActive(true);
            Ascension.OpenMenu();
        }

        void CloseMenu()
        {
            Ascension.CloseMenu();
        }

        void OnGUI()
        {
            if (!isOpeningInfo)
            {
                return;
            }
            Event e = Event.current;
            if (e.isMouse || e.type == EventType.KeyDown)
            {
                if (!CardExplanationPopup.transform.Find("Panel").gameObject.activeSelf)
                {
                    ResetCharacterExplanationPopup();
                }
            }
        }

        public void PopupCharacterInfo(Character character)
        {
            isOpeningInfo = true;
            foreach (var overlayData in NewOverlayDatas.Values)
            {
                overlayData.Canvas.gameObject.layer = overlayData.LayerMask;
                overlayData.Canvas.worldCamera = overlayData.Camera;
            }
            CardExplanationPopup.Init(character);
        }

        public void ResetCharacterExplanationPopup()
        {
            isOpeningInfo = false;
            foreach (var overlayData in OriginalOverlayDatas.Values)
            {
                overlayData.Canvas.gameObject.layer = overlayData.LayerMask;
                overlayData.Canvas.worldCamera = overlayData.Camera;
            }
        }

        public void SetAscensionData(AscensionsData data)
        {
            CurrentAscensionData = data;
            CurrentCustomScriptData = null;
            CurrentScriptInfo = null;
            SetScriptInfoFromAscensionData();
            OnAscensionChanged?.Invoke(CurrentAscensionData);
            if (CurrentCustomScriptData != null)
            {
                CustomScenario.Logger.Warning("Setting to custom script");
                SetCustomScriptData(CurrentCustomScriptData);
            }
            else if (CurrentScriptInfo != null)
            {
                CustomScenario.Logger.Warning("Setting to script info");
                SetScriptInfo(CurrentScriptInfo);
            }
            else
            {
                CustomScenario.Logger.BigError("It should've never reached this");
            }
        }

        public void SetScriptInfo(ScriptInfo scriptInfo)
        {
            CurrentScriptInfo = scriptInfo;
            OnScriptInfoChanged?.Invoke(CurrentScriptInfo);
        }

        public void SetCustomScriptData(CustomScriptData customScriptData)
        {
            CurrentCustomScriptData = customScriptData;
            OnCustomScriptDataChanged?.Invoke(CurrentCustomScriptData);
        }

        public ScriptInfo GetScriptInfo()
        {
            if (CurrentCustomScriptData != null)
            {
                return CurrentCustomScriptData.scriptInfo;
            }
            else if (CurrentScriptInfo != null)
            {
                return CurrentScriptInfo;
            }
            else
            {
                CustomScenario.Logger.BigError($"{nameof(ScriptInfo)} should never be null. Something is wrong");
                return null;
            }
        }

        public List<CharactersCount> GetCharactersCounts(bool isOriginalRef)
        {
            List<CharactersCount> newList = !isOriginalRef ? new List<CharactersCount>() : null!;
            if (CurrentCustomScriptData != null)
            {
                if (CurrentCustomScriptData.scriptInfo == null)
                {
                    CurrentCustomScriptData.scriptInfo = GameUtility.CreateDefaultScriptInfo();
                }
                if (isOriginalRef)
                {
                    return CurrentCustomScriptData.scriptInfo.characterCounts;
                }
                foreach (var characterCount in CurrentCustomScriptData.scriptInfo.characterCounts)
                {
                    newList.Add(characterCount);
                }
            }
            else if (CurrentScriptInfo != null)
            {
                if (CurrentScriptInfo.characterCounts == null || CurrentScriptInfo.characterCounts.Count <= 0)
                {
                    CurrentScriptInfo.characterCounts = new List<CharactersCount>();
                    CurrentScriptInfo.characterCounts.Add(GameUtility.CreateDefaultCharactersCount());
                }
                if (isOriginalRef)
                {
                    return CurrentScriptInfo.characterCounts;
                }
                foreach (var characterCount in CurrentScriptInfo.characterCounts)
                {
                    newList.Add(characterCount);
                }
            }
            else
            {
                if (isOriginalRef)
                {
                    return CurrentAscensionData.characterCounts;
                }
                foreach (var characterCount in CurrentAscensionData.characterCounts)
                {
                    newList.Add(characterCount);
                }
            }
            return newList;
        }
    }
}
