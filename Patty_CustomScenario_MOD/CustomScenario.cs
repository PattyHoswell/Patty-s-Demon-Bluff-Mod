using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using MelonLoader.Utils;
using Patty_CustomScenario_MOD;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomScenario), "Patty_CustomScenario_MOD", "1.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonPriority(100)]

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

        public static MelonLogger.Instance Logger;

        public string BasePath => Path.GetDirectoryName(MelonAssembly.Location);

        public ScriptInfo targetScriptInfo;
        public CharactersCount targetCharCount;
        public System.Collections.Generic.Dictionary<string, AscensionScenario_Data> customScenarios = new();
        public AscensionScenario_Data currentScenario;
        public MelonPreferences_Category configCategory;

        public static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);

        public override void OnLateInitializeMelon()
        {
            Logger = LoggerInstance;

            configCategory = MelonPreferences.CreateCategory(MAIN_CATEGORY, "Custom Scenario Settings");
            configCategory.CreateEntry(DEBUG, false, description: "When enabled, always start a scenario with debug data");
            configCategory.CreateEntry(EXTRACT_ORIGINAL_FILES, true, description: "When enabled, extract scenario data from the game if the file doesn't exist yet");
            configCategory.CreateEntry(ADD_CUSTOM_ENDLESS_SCENARIO, true, description: "When enabled, load custom scenario data from Endless folder");
            configCategory.CreateEntry(LOAD_UNREGISTERED_SCENARIO_DATA, false, description: "When enabled, load scenario data that's not registered in Endless folder\n" +
                                                                                            "May added the game scenario as well. Only enable if you know what you're doing");
            configCategory.CreateEntry(REPLACE_DEBUG_SCENARIO, true, description: "When enabled, replace debug scenario with file specified on Debug folder");
            configCategory.CreateEntry(REPLACE_ENDLESS_SCENARIO, true, description: "When enabled, replace endless scenario with file specified on Endless folder");
            configCategory.CreateEntry(REPLACE_NORMAL_MODE_SCENARIO, true, description: "When enabled, replace normal mode scenario with file specified on Ascension folder");
            configCategory.CreateEntry(REPLACE_ROGUELIKE_SCENARIO, true, description: "When enabled, replace roguelike scenario with file specified on Roguelike folder");
            configCategory.CreateEntry(REPLACE_ROGUELIKE_STANDARD_SCENARIO, true, description: "When enabled, replace roguelike standard scenario with file specified on RoguelikeStandard");
            configCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "CustomScenarioSettings.cfg"));
            configCategory.SaveToFile();

            GameData.DebugAscension = configCategory.GetEntry<bool>(DEBUG).Value;

            InitializeScenario();

            if (configCategory.GetEntry<bool>(ADD_CUSTOM_ENDLESS_SCENARIO).Value)
                LoadCustomScenarios();

            WritesCharacterName();
        }

        public static CharacterData FindCharacter(string name)
        {
            Func<CharacterData, bool> value = c => c.name == name;
            var charData = ProjectContext.Instance.gameData.allCharacterData.Find(value);
            if (charData == null)
            {
                foreach (var characterData in allCharacterData.Value)
                {
                    if (characterData.name == name)
                        return characterData;
                }
            }
            return charData;
        }

        public AscensionScenario_Data ExtractAscension(AscensionsData data, string path, bool fromScript)
        {
            var scenarioData = new AscensionScenario_Data();
            if (fromScript)
                scenarioData.InitializeFromScriptInfo(data.currentPickedScript, data.currentPickedScript.characterCounts);
            else
                scenarioData.InitializeFromAscension(data, data.characterCounts);
            var json = JsonSerializer.Serialize(scenarioData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            return scenarioData;
        }

        public AscensionScenario_Data ExtractAscension(ScriptInfo data, string path)
        {
            var scenarioData = new AscensionScenario_Data();
            scenarioData.InitializeFromScriptInfo(data, data.characterCounts);
            var json = JsonSerializer.Serialize(scenarioData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            return scenarioData;
        }


        public bool InitializeAscension(AscensionsData original, string path, bool assignData, bool initializeFromScript, out AscensionScenario_Data result)
        {
            result = null!;
            if (configCategory.GetEntry<bool>(EXTRACT_ORIGINAL_FILES).Value && !File.Exists(path))
            {
                result = ExtractAscension(original, path, initializeFromScript);
            }
            else if (File.Exists(path))
            {
                LoadCustomScenarioFile(path, out result);
            }
            return result != null;
        }

        public bool InitializeAscension(CustomScriptData original, string path, bool assignData, out AscensionScenario_Data result)
        {
            result = null!;
            if (configCategory.GetEntry<bool>(EXTRACT_ORIGINAL_FILES).Value && !File.Exists(path))
            {
                result = ExtractAscension(original.scriptInfo, path);
            }
            else if (File.Exists(path))
            {
                LoadCustomScenarioFile(path, out result);
            }
            return result != null;
        }

        public bool LoadCustomScenarioFile(string path, out AscensionScenario_Data result)
        {
            try
            {
                result = JsonSerializer.Deserialize<AscensionScenario_Data>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                result = null!;
                Logger.BigError($"Cannot load file {Path.GetFileName(path)}, reason: {(ex.InnerException ?? ex).Message}");
            }
            return result != null;
        }

        private void WritesCharacterName()
        {
            var allRegistered = ProjectContext.Instance.gameData.allCharacterData;
            var allChars = new HashSet<CharacterData>(allRegistered.Count);

            foreach (var charData in allRegistered)
                allChars.Add(charData);
            foreach (var charData in allCharacterData.Value)
                allChars.Add(charData);

            var charNamesBuilder = new StringBuilder();
            charNamesBuilder.AppendLine("Villager:");
            foreach (var characterData in allChars)
            {
                if (characterData.type == ECharacterType.Villager)
                    charNamesBuilder.Append("- ").AppendLine(characterData.name);
            }
            charNamesBuilder.AppendLine();

            charNamesBuilder.AppendLine("Outcast:");
            foreach (var characterData in allChars)
            {
                if (characterData.type == ECharacterType.Outcast)
                    charNamesBuilder.Append("- ").AppendLine(characterData.name);
            }
            charNamesBuilder.AppendLine();

            charNamesBuilder.AppendLine("Minion:");
            foreach (var characterData in allChars)
            {
                if (characterData.type == ECharacterType.Minion)
                    charNamesBuilder.Append("- ").AppendLine(characterData.name);
            }
            charNamesBuilder.AppendLine();

            charNamesBuilder.AppendLine("Demon:");
            foreach (var characterData in allChars)
            {
                if (characterData.type == ECharacterType.Demon)
                    charNamesBuilder.Append("- ").AppendLine(characterData.name);
            }
            charNamesBuilder.AppendLine();

            charNamesBuilder.AppendLine("Unspecified:");
            foreach (var characterData in allChars)
            {
                if (characterData.type != ECharacterType.Villager &&
                    characterData.type != ECharacterType.Outcast &&
                    characterData.type != ECharacterType.Minion &&
                    characterData.type != ECharacterType.Demon)
                    charNamesBuilder.Append("- ").AppendLine(characterData.name);
            }
            charNamesBuilder.AppendLine();


            File.WriteAllText(Path.Combine(BasePath, "AllCharacterName.txt"), charNamesBuilder.ToString());
        }

        private void InitializeScenario()
        {
            DirectoryInfo advancedFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Endless"));
            foreach (var customScriptData in ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData)
            {
                var customScriptPath = Path.Combine(advancedFolder.FullName, $"{customScriptData.name}.json");
                if (customScenarios.ContainsKey(customScriptPath))
                    continue;

                if (InitializeAscension(customScriptData,
                                        customScriptPath,
                                        configCategory.GetEntry<bool>(REPLACE_ENDLESS_SCENARIO).Value,
                                        out var advancedAscension))
                {
                    customScenarios.Add(customScriptPath, advancedAscension);
                }
            }

            DirectoryInfo debugFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Debug"));
            var debugJson = Path.Combine(debugFolder.FullName, $"{ProjectContext.Instance.gameData.debugAscension.name}.json");
            if (InitializeAscension(ProjectContext.Instance.gameData.debugAscension,
                                    debugJson,
                                    configCategory.GetEntry<bool>(REPLACE_DEBUG_SCENARIO).Value,
                                    initializeFromScript: true,
                                    out var debugAscension))
            {
                if (!customScenarios.ContainsKey(debugJson))
                    customScenarios.Add(debugJson, debugAscension);

                if (configCategory.GetEntry<bool>(REPLACE_DEBUG_SCENARIO).Value)
                {
                    targetScriptInfo = ProjectContext.Instance.gameData.debugAscension.currentPickedScript;
                    currentScenario = debugAscension;

                    LoggerInstance.Msg("Loaded Debug Scenario: Debug.json");


                    try
                    {
                        targetCharCount = ProjectContext.Instance.gameData.debugAscension.characterCounts[0];

                        SetEligibleTownsfolk();
                        SetEligibleOutcasts();
                        SetEligibleMinions();
                        SetEligibleDemons();

                        SetTownsfolkAmount();
                        SetOutcastAmount();
                        SetMinionAmount();
                        SetDemonAmount();
                    }
                    catch (Exception ex)
                    {
                        LoggerInstance.Msg($"Error setting up debug scenario, reason: {(ex.InnerException ?? ex).Message}");
                    }
                }
            }

            DirectoryInfo ascensionFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Ascension"));
            foreach (var ascension in ProjectContext.Instance.gameData.standardAscensions)
            {
                var ascensionPath = Path.Combine(ascensionFolder.FullName, $"{ascension.name}.json");
                if (customScenarios.ContainsKey(ascensionPath))
                    continue;

                if (InitializeAscension(ascension,
                                        ascensionPath,
                                        configCategory.GetEntry<bool>(REPLACE_NORMAL_MODE_SCENARIO).Value,
                                        initializeFromScript: false,
                                        out var ascensionData))
                {
                    customScenarios.Add(ascensionPath, ascensionData);
                }
            }

            DirectoryInfo roguelikeFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Roguelike"));
            foreach (var roguelike in ProjectContext.Instance.gameData.roguelikeAscensions)
            {
                var roguelikePath = Path.Combine(roguelikeFolder.FullName, $"{roguelike.name}.json");
                if (InitializeAscension(roguelike,
                                        roguelikePath,
                                        configCategory.GetEntry<bool>(REPLACE_ROGUELIKE_SCENARIO).Value,
                                        initializeFromScript: false,
                                        out var roguelikeData))
                {
                    customScenarios.Add(roguelikePath, roguelikeData);
                }
            }
            DirectoryInfo roguelikeStandardFolder = Directory.CreateDirectory(Path.Combine(BasePath, "RoguelikeStandard"));
            foreach (var roguelikeStandard in ProjectContext.Instance.gameData.roguelikeAscensions)
            {
                var roguelikeStandardPath = Path.Combine(roguelikeStandardFolder.FullName, $"{roguelikeStandard.name}.json");
                if (customScenarios.ContainsKey(roguelikeStandardPath))
                    continue;

                if (InitializeAscension(roguelikeStandard,
                                        roguelikeStandardPath,
                                        configCategory.GetEntry<bool>(REPLACE_ROGUELIKE_STANDARD_SCENARIO).Value,
                                        initializeFromScript: false,
                                        out var roguelikeStandardData))
                {
                    customScenarios.Add(roguelikeStandardPath, roguelikeStandardData);
                }
            }
        }

        public void LoadCustomScenarios()
        {
            var scriptsArray = ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData;
            var newArray = new List<CustomScriptData>();

            DirectoryInfo advancedFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Endless"));
            foreach (var file in advancedFolder.GetFiles("*.json"))
            {
                AscensionScenario_Data result = null!;
                if (customScenarios.TryGetValue(file.FullName, out result) || LoadCustomScenarioFile(file.FullName, out result))
                {
                    if (!customScenarios.ContainsKey(file.FullName))
                        customScenarios.Add(file.FullName, result);

                    Logger.Warning($"Adding custom scenario for Endless mode from file '{file.Name}'");
                    var scriptInfo = new ScriptInfo();
                    var customScript = ScriptableObject.CreateInstance<CustomScriptData>();
                    customScript.name = Path.GetFileNameWithoutExtension(file.Name);
                    customScript.scriptInfo = scriptInfo;
                    result.AssignData(customScript.scriptInfo);
                    newArray.Add(customScript);
                }
            }

            if (configCategory.GetEntry<bool>(LOAD_UNREGISTERED_SCENARIO_DATA).Value)
            {
                foreach (var script in scriptsArray)
                {
                    Func<CustomScriptData, bool> find = c => c.name == script.name;
                    if (!newArray.Exists(find))
                    {
                        Logger.Warning($"Adding unregistered {nameof(CustomScriptData)}, possibly from a mod '{script.name}'");
                        newArray.Add(script);
                    }
                }
            }

            ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData = (Il2CppReferenceArray<CustomScriptData>)newArray.ToArray();
        }

        private void SetEligibleTownsfolk()
        {
            targetScriptInfo.startingTownsfolks = new List<CharacterData>(currentScenario.Characters.Villagers.Count);
            for (var i = currentScenario.Characters.Villagers.Count - 1; i >= 0; i--)
            {
                var charName = currentScenario.Characters.Villagers[i];
                var charData = FindCharacter(charName);
                if (charData == null)
                {
                    LoggerInstance.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                targetScriptInfo.startingTownsfolks.Add(charData);
            }
        }

        private void SetEligibleOutcasts()
        {
            targetScriptInfo.startingOutsiders = new List<CharacterData>(currentScenario.Characters.Outcasts.Count);
            for (var i = currentScenario.Characters.Outcasts.Count - 1; i >= 0; i--)
            {
                var charName = currentScenario.Characters.Outcasts[i];
                var charData = FindCharacter(charName);
                if (charData == null)
                {
                    LoggerInstance.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                targetScriptInfo.startingOutsiders.Add(charData);
            }
        }

        private void SetEligibleMinions()
        {
            targetScriptInfo.startingMinions = new List<CharacterData>(currentScenario.Characters.Minions.Count);
            for (var i = currentScenario.Characters.Minions.Count - 1; i >= 0; i--)
            {
                var charName = currentScenario.Characters.Minions[i];
                var charData = FindCharacter(charName);
                if (charData == null)
                {
                    LoggerInstance.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                targetScriptInfo.startingMinions.Add(charData);
            }
        }

        private void SetEligibleDemons()
        {
            targetScriptInfo.startingDemons = new List<CharacterData>(currentScenario.Characters.Demons.Count);
            for (var i = currentScenario.Characters.Demons.Count - 1; i >= 0; i--)
            {
                var charName = currentScenario.Characters.Demons[i];
                var charData = FindCharacter(charName);
                if (charData == null)
                {
                    LoggerInstance.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                targetScriptInfo.startingDemons.Add(charData);
            }
        }

        private void SetTownsfolkAmount()
        {
            targetCharCount.dTown = currentScenario.CharactersAmount[0].TownsfolkAmount;
            targetCharCount.town = currentScenario.CharactersAmount[0].TownsfolkAmount;
        }

        private void SetOutcastAmount()
        {
            targetCharCount.dOuts = currentScenario.CharactersAmount[0].OutcastAmount;
            targetCharCount.outs = currentScenario.CharactersAmount[0].OutcastAmount;
        }

        private void SetMinionAmount()
        {
            targetCharCount.dMinion = currentScenario.CharactersAmount[0].MinionAmount;
            targetCharCount.minion = currentScenario.CharactersAmount[0].MinionAmount;
        }

        private void SetDemonAmount()
        {
            targetCharCount.dDemon = currentScenario.CharactersAmount[0].DemonAmount;
            targetCharCount.demon = currentScenario.CharactersAmount[0].DemonAmount;
        }
    }

}
