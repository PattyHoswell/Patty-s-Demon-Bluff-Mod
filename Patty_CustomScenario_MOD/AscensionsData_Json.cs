using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using Patty_CustomScenario_MOD.QoL;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using UnityEngine;
namespace Patty_CustomScenario_MOD
{
    public class AscensionsData_Json
    {
        public string Name { get; set; } = "";
        public System.Collections.Generic.List<ScriptInfo_Json> PossibleScripts { get; set; } = new();

        [JsonIgnore]
        public System.Collections.Generic.List<CustomScriptData_Json> PossibleScriptsData { get; set; } = new();

        public System.Collections.Generic.List<string> PossibleScriptsDataName { get; set; } = new();

        public void Initialize(AscensionsData ascension)
        {
            Name = ascension.name;
            if (ascension.possibleScripts != null)
            {
                for (var i = 0; i < ascension.possibleScripts.Count; i++)
                {
                    var newScript = new ScriptInfo_Json();
                    newScript.Initialize(ascension.possibleScripts[i]);
                    PossibleScripts.Add(newScript);
                }
            }
            if (ascension.possibleScriptsData != null)
            {
                for (var i = 0; i < ascension.possibleScriptsData.Count; i++)
                {
                    var newScript = new CustomScriptData_Json();
                    newScript.Initialize(ascension.possibleScriptsData[i]);
                    PossibleScriptsData.Add(newScript);
                    PossibleScriptsDataName.Add(newScript.Name);
                }
            }
        }

        public void Assign(AscensionsData data)
        {
            data.name = Name;
            data.possibleScripts = new ScriptInfo[0];
            for (var i = 0; i < PossibleScripts.Count; i++)
            {
                var newItem = new ScriptInfo();
                data.possibleScripts = data.possibleScripts.Append(newItem).ToArray();
                PossibleScripts[i].Assign(newItem);
            }
            AssignCustomScenarioData(data);
        }

        void AssignCustomScenarioData(AscensionsData data)
        {
            data.possibleScriptsData = new CustomScriptData[0];
            for (int i = 0; i < PossibleScriptsDataName.Count; i++)
            {
                var customScriptJson = GameUtility.FindCustomScenarioData_Json(PossibleScriptsDataName[i]);
                if (customScriptJson == null)
                {
                    CustomScenario.Logger.Error($"Cannot find {nameof(CustomScriptData)} named {PossibleScriptsDataName[i]}");
                    continue;
                }
                var newItem = GameUtility.FindCustomScriptData(PossibleScriptsDataName[i]) ?? ScriptableObject.CreateInstance<CustomScriptData>();
                data.possibleScriptsData = data.possibleScriptsData.Append(newItem).ToArray();
                customScriptJson.Assign(newItem);
            }
        }

        public void Serialize(string targetFolder, bool overrides = false)
        {
            var folder = Directory.CreateDirectory(targetFolder);
            var filePath = Path.Combine(folder.FullName, $"{Name}.json");
            if (!overrides && File.Exists(filePath))
            {
                return;
            }
            if (!UniversalUtility.SerializeJson(filePath, this))
            {
                CustomScenario.Logger.Error($"Failed to serialize {nameof(AscensionsData)}[{Name}]");
            }
        }

        public void SerializeCustomScriptData(string targetFolder, bool overrides = false)
        {
            var folder = Directory.CreateDirectory(targetFolder);
            foreach (var data in PossibleScriptsData)
            {
                var filePath = Path.Combine(folder.FullName, $"{data.Name}.json");
                if (!overrides && File.Exists(filePath))
                {
                    continue;
                }
                if (!UniversalUtility.SerializeJson(filePath, data))
                {
                    CustomScenario.Logger.Error($"Failed to serialize {nameof(CustomScriptData)}[{data.Name}]");
                }
            }
        }
    }
    public class CharacterAmount_Json
    {
        public int AllCharactersAmount { get; set; } = 9;
        public int TownsfolkAmount { get; set; } = 5;
        public int OutcastAmount { get; set; } = 2;
        public int MinionAmount { get; set; } = 1;
        public int DemonAmount { get; set; } = 1;
        public int TownsfolkDeckAmount { get; set; } = 5;
        public int OutcastDeckAmount { get; set; } = 2;
        public int MinionDeckAmount { get; set; } = 1;
        public int DemonDeckAmount { get; set; } = 1;

        public void Initialize(CharactersCount charactersCount)
        {
            TownsfolkAmount = charactersCount.GetAmount(ECharacterType.Villager, false);
            OutcastAmount = charactersCount.GetAmount(ECharacterType.Outcast, false);
            MinionAmount = charactersCount.GetAmount(ECharacterType.Minion, false);
            DemonAmount = charactersCount.GetAmount(ECharacterType.Demon, false);
            TownsfolkDeckAmount = charactersCount.GetAmount(ECharacterType.Villager, true);
            OutcastDeckAmount = charactersCount.GetAmount(ECharacterType.Outcast, true);
            MinionDeckAmount = charactersCount.GetAmount(ECharacterType.Minion, true);
            DemonDeckAmount = charactersCount.GetAmount(ECharacterType.Demon, true);
            AllCharactersAmount = TownsfolkAmount + OutcastAmount + MinionAmount + DemonAmount;
        }
        public void Assign(CharactersCount charactersCount)
        {
            charactersCount.town = TownsfolkAmount;
            charactersCount.outs = OutcastAmount;
            charactersCount.minion = MinionAmount;
            charactersCount.demon = DemonAmount;

            charactersCount.dTown = TownsfolkDeckAmount;
            charactersCount.dOuts = OutcastDeckAmount;
            charactersCount.dMinion = MinionDeckAmount;
            charactersCount.dDemon = DemonDeckAmount;
        }
    }
    public class ScriptInfo_Json
    {
        public System.Collections.Generic.List<string> Villagers { get; set; } = new();
        public System.Collections.Generic.List<string> Outcasts { get; set; } = new();
        public System.Collections.Generic.List<string> Minions { get; set; } = new();
        public System.Collections.Generic.List<string> Demons { get; set; } = new();
        public System.Collections.Generic.List<CharacterAmount_Json> CharactersAmount { get; set; } = new();
        public System.Collections.Generic.List<string> MustInclude { get; set; } = new();

        public void Initialize(ScriptInfo scriptInfo)
        {
            for (int i = 0; i < scriptInfo.startingTownsfolks.Count; i++)
            {
                Villagers.Add(scriptInfo.startingTownsfolks[i].characterId);
            }
            for (int i = 0; i < scriptInfo.startingOutsiders.Count; i++)
            {
                Outcasts.Add(scriptInfo.startingOutsiders[i].characterId);
            }
            for (int i = 0; i < scriptInfo.startingMinions.Count; i++)
            {
                Minions.Add(scriptInfo.startingMinions[i].characterId);
            }
            for (int i = 0; i < scriptInfo.startingDemons.Count; i++)
            {
                Demons.Add(scriptInfo.startingDemons[i].characterId);
            }

            for (int i = 0; i < scriptInfo.characterCounts.Count; i++)
            {
                var amtData = new CharacterAmount_Json();
                amtData.Initialize(scriptInfo.characterCounts[i]);
                CharactersAmount.Add(amtData);
            }
            if (scriptInfo.mustInclude != null)
            {
                for (int i = 0; i < scriptInfo.mustInclude.Count; i++)
                {
                    MustInclude.Add(scriptInfo.mustInclude[i].characterId);
                }
            }
        }

        public void Assign(ScriptInfo scriptInfo)
        {
            scriptInfo.startingTownsfolks = new List<CharacterData>(Villagers.Count);
            scriptInfo.startingOutsiders = new List<CharacterData>(Outcasts.Count);
            scriptInfo.startingMinions = new List<CharacterData>(Minions.Count);
            scriptInfo.startingDemons = new List<CharacterData>(Demons.Count);
            scriptInfo.characterCounts = new List<CharactersCount>(CharactersAmount.Count);
            scriptInfo.mustInclude = new List<CharacterData>(MustInclude.Count);

            for (int i = 0; i < Villagers.Count; i++)
            {
                var characterData = GameUtility.FindCharacterById(Villagers[i]);
                if (characterData == null)
                {
                    CustomScenario.Logger.Warning($"Cannot find character by id {Villagers[i]}");
                    continue;
                }
                scriptInfo.startingTownsfolks.Add(GameUtility.FindCharacterById(Villagers[i]));
            }
            for (int i = 0; i < Outcasts.Count; i++)
            {
                var characterData = GameUtility.FindCharacterById(Outcasts[i]);
                if (characterData == null)
                {
                    CustomScenario.Logger.Warning($"Cannot find character by id {Outcasts[i]}");
                    continue;
                }
                scriptInfo.startingOutsiders.Add(GameUtility.FindCharacterById(Outcasts[i]));
            }
            for (int i = 0; i < Minions.Count; i++)
            {
                var characterData = GameUtility.FindCharacterById(Minions[i]);
                if (characterData == null)
                {
                    CustomScenario.Logger.Warning($"Cannot find character by id {Minions[i]}");
                    continue;
                }
                scriptInfo.startingMinions.Add(GameUtility.FindCharacterById(Minions[i]));
            }
            for (int i = 0; i < Demons.Count; i++)
            {
                var characterData = GameUtility.FindCharacterById(Demons[i]);
                if (characterData == null)
                {
                    CustomScenario.Logger.Warning($"Cannot find character by id {Demons[i]}");
                    continue;
                }
                scriptInfo.startingDemons.Add(GameUtility.FindCharacterById(Demons[i]));
            }

            for (int i = 0; i < CharactersAmount.Count; i++)
            {
                var newItem = GameUtility.CreateDefaultCharactersCount();
                CharactersAmount[i].Assign(newItem);
                scriptInfo.characterCounts.Add(newItem);
            }

            for (int i = 0; i < MustInclude.Count; i++)
            {
                var characterData = GameUtility.FindCharacterById(MustInclude[i]);
                if (characterData == null)
                {
                    CustomScenario.Logger.Warning($"Cannot find character by id {MustInclude[i]}");
                    continue;
                }
                scriptInfo.mustInclude.Add(characterData);
            }
        }
    }

    public class CustomScriptData_Json
    {
        public string Name { get; set; } = "";
        public ScriptInfo_Json ScriptInfo { get; set; } = new();

        public void Initialize(CustomScriptData customScriptData)
        {
            Name = customScriptData.name;
            ScriptInfo.Initialize(customScriptData.scriptInfo);
        }

        public void Assign(CustomScriptData customScriptData)
        {
            customScriptData.name = Name;
            customScriptData.scriptInfo = new ScriptInfo();
            ScriptInfo.Assign(customScriptData.scriptInfo);
        }
    }
}