using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Utils;
using Patty_CustomScenario_MOD;
using Patty_CustomScenario_MOD.AscensionEditorGUI.Menu;
using Patty_CustomScenario_MOD.Patch;
using Patty_CustomScenario_MOD.QoL;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[assembly: MelonInfo(typeof(CustomScenario), "Patty_CustomScenario_MOD", "2.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonPriority(100)]
[assembly: HarmonyDontPatchAll]
namespace Patty_CustomScenario_MOD
{
    public class CustomScenario : MelonMod
    {
        public const string MAIN_CATEGORY = "CustomScenario",
                            DEBUG = "Debug",
                            EXTRACT_ORIGINAL_FILES = "ExtractOriginalFiles",
                            ADD_CUSTOM_ENDLESS_SCENARIO = "AddCustomEndlessScenario",
                            LOAD_UNREGISTERED_SCENARIO_DATA = "LoadUnregisteredScenarioData",
                            REPLACE_DEBUG_SCENARIO = "ReplaceDebugScenario",
                            REPLACE_ENDLESS_SCENARIO = "ReplaceEndlessScenario",
                            REPLACE_NORMAL_MODE_SCENARIO = "ReplaceNormalModeScenario",
                            REPLACE_ROGUELIKE_SCENARIO = "ReplaceRoguelikeScenario",
                            REPLACE_ROGUELIKE_STANDARD_SCENARIO = "ReplaceRoguelikeStandardScenario";

        public static readonly EAlignment AllAlignment = (EAlignment)(-900);
        public static readonly ECharacterType AllCharacterType = (ECharacterType)(-900);

        public static string AscensionDataFolder { get; internal set; }
        public static string CustomScriptDataFolder { get; internal set; }
        public static MelonLogger.Instance Logger { get; internal set; }
        public static string BasePath { get; internal set; }

        public static System.Collections.Generic.Dictionary<string, AscensionsData_Json> customAscensions = new();
        public static System.Collections.Generic.Dictionary<string, CustomScriptData_Json> customScripts = new();
        public MelonPreferences_Category configCategory;

        public static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);

        public string base64string;

        public override void OnLateInitializeMelon()
        {
            BasePath = Path.GetDirectoryName(MelonAssembly.Location);
            AscensionDataFolder = Path.Combine(BasePath, "Scenario", "AscensionDatas");
            CustomScriptDataFolder = Path.Combine(BasePath, "Scenario", "CustomScriptDatas");

            Directory.CreateDirectory(AscensionDataFolder);
            Directory.CreateDirectory(CustomScriptDataFolder);

            Logger = LoggerInstance;
            try
            {
                HarmonyInstance.PatchAll(typeof(SimpleEnumPatcher));
                HarmonyInstance.Patch(AccessTools.DeclaredMethod(typeof(Enum), "TryParse", new Type[]
                {
                    typeof(Type),
                    typeof(ReadOnlySpan<char>),
                    typeof(bool),
                    typeof(bool),
                    typeof(object).MakeByRefType()
                }), prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(SimpleEnumPatcher), "Enum_TryParse")));
            }
            catch (HarmonyException ex)
            {
                LoggerInstance.Error((ex.InnerException ?? ex).Message);
            }

            foreach (var type in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
            {
                if (type == null)
                    continue;
                if (typeof(Il2CppSystem.Object).IsAssignableFrom(type) &&
                    (!type.ContainsGenericParameters && !type.IsGenericType) &&
                    !ClassInjector.IsTypeRegisteredInIl2Cpp(type))
                {
                    ClassInjector.RegisterTypeInIl2Cpp(type);
                }
            }

            UniversalUtility.AddEnum<EAlignment>("All", AllAlignment);
            UniversalUtility.AddEnum<ECharacterType>("All", AllCharacterType);

            configCategory = MelonPreferences.CreateCategory(MAIN_CATEGORY, "Custom Scenario Settings");
            configCategory.CreateEntry(DEBUG, false, description: "When enabled, always start a scenario with debug data");
            configCategory.CreateEntry(EXTRACT_ORIGINAL_FILES, true, description: "When enabled, extract scenario data from the game if the file doesn't exist yet");
            configCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "CustomScenarioSettings.cfg"));
            configCategory.SaveToFile();

            GameData.DebugAscension = configCategory.GetEntry<bool>(DEBUG).Value;

            if (configCategory.GetEntry<bool>(EXTRACT_ORIGINAL_FILES).Value)
            {
                ExtractScenarios();
            }
            LoadCustomScriptData();
            LoadAscensionData();
            AssignCustomScriptData();
            AssignAllAscension();

            MelonCoroutines.Start(InitializeAfter2Frame());
        }


        private IEnumerator InitializeAfter2Frame()
        {
            yield return YieldCache.WaitForEndOfFrame.Value;
            yield return YieldCache.WaitForEndOfFrame.Value;
            CreateEditorGUI();
        }

        public void CreateEditorGUI()
        {
            var asset = AssetBundle.LoadFromFile(Path.Combine(BasePath, "CustomScenarioGUI.bundle"));
            var scenarioEditor = GameObject.Instantiate(asset.LoadAsset<GameObject>("Assets/Prefabs/Scenario Editor.prefab"), Vector3.zero, Quaternion.identity);
            var canvas = scenarioEditor.GetComponentInChildren<Canvas>(true);
            canvas.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
            canvas.gameObject.AddComponent<CustomScenarioPopup>();
            if (canvas.worldCamera == null)
            {
                Logger.Warning("The canvas has not been assigned camera, assigning to overlay...");
                canvas.worldCamera = scenarioEditor.transform.Find("Overlay Camera").GetComponent<Camera>();
            }
            var canvasCamera = canvas.worldCamera;
            canvasCamera.cullingMask = 2;
            Camera.main.gameObject.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(scenarioEditor.GetComponentInChildren<Camera>(true));

            asset.Unload(unloadAllLoadedObjects: false);
        }

        private void ExtractScenarios()
        {
            UniversalUtility.ExtractAscension(ProjectContext.Instance.gameData.advancedAscension);
            UniversalUtility.ExtractAscension(ProjectContext.Instance.gameData.debugAscension);
            for (int i = 0; i < ProjectContext.Instance.gameData.standardAscensions.Count; i++)
            {
                UniversalUtility.ExtractAscension(ProjectContext.Instance.gameData.standardAscensions[i]);
            }
            for (int i = 0; i < ProjectContext.Instance.gameData.roguelikeAscensions.Count; i++)
            {
                UniversalUtility.ExtractAscension(ProjectContext.Instance.gameData.roguelikeAscensions[i]);
            }
            for (int i = 0; i < ProjectContext.Instance.gameData.roguelikeStandardAscensions.Count; i++)
            {
                AscensionsList ascensionList = ProjectContext.Instance.gameData.roguelikeStandardAscensions[i];
                for (var j = 0; j < ascensionList.ascensions.Count; j++)
                {
                    UniversalUtility.ExtractAscension(ascensionList.ascensions[i]);
                }
            }
            UniversalUtility.ExtractAscension(ProjectContext.Instance.gameData.allCharactersAscension);
        }

        private void LoadCustomScriptData()
        {
            var allDataFiles = Directory.GetFiles(CustomScriptDataFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in allDataFiles)
            {
                if (customScripts.ContainsKey(file))
                {
                    Logger.Warning($"Skipping {file} because it's already loaded");
                    continue;
                }
                if (UniversalUtility.LoadJson(file, out CustomScriptData_Json result))
                {
                    customScripts.Add(file, result);
                }
            }
        }

        private void LoadAscensionData()
        {
            var allDataFiles = Directory.GetFiles(AscensionDataFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in allDataFiles)
            {
                if (customAscensions.ContainsKey(file))
                {
                    Logger.Warning($"Skipping {file} because it's already loaded");
                    continue;
                }
                if (UniversalUtility.LoadJson(file, out AscensionsData_Json result))
                {
                    customAscensions.Add(file, result);
                }
            }
        }

        private void AssignCustomScriptData()
        {
            foreach (var scenarioJson in customScripts)
            {
                var originalAscension = GameUtility.FindCustomScriptData(scenarioJson.Value.Name) ?? ScriptableObject.CreateInstance<CustomScriptData>();
                scenarioJson.Value.Assign(originalAscension);
                Logger.Msg($"Assigning Script Data {Path.GetFileName(scenarioJson.Key)} to {scenarioJson.Value.Name}");
            }
        }

        private void AssignAllAscension()
        {
            foreach (var ascensionJson in customAscensions)
            {
                var originalAscension = GameUtility.FindAscensionData(ascensionJson.Value.Name) ?? ScriptableObject.CreateInstance<AscensionsData>();
                ascensionJson.Value.Assign(originalAscension);
                Logger.Msg($"Assigning {Path.GetFileName(ascensionJson.Key)} to {ascensionJson.Value.Name}");
            }
        }
    }
}
