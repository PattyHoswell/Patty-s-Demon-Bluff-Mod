using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
namespace Patty_CustomScenario_MOD
{
    public class AscensionScenario_Data
    {
        public ScriptInfo_Data Characters { get; set; } = new();
        public System.Collections.Generic.List<CharacterAmount_Data> CharactersAmount { get; set; } = new();

        public void InitializeFromScriptInfo(ScriptInfo scriptInfo, List<CharactersCount> charactersCount)
        {
            for (int i = 0; i < scriptInfo.startingTownsfolks.Count; i++)
            {
                this.Characters.Villagers.Add(scriptInfo.startingTownsfolks[i].name);
            }
            for (int i = 0; i < scriptInfo.startingOutsiders.Count; i++)
            {
                this.Characters.Outcasts.Add(scriptInfo.startingOutsiders[i].name);
            }
            for (int i = 0; i < scriptInfo.startingMinions.Count; i++)
            {
                this.Characters.Minions.Add(scriptInfo.startingMinions[i].name);
            }
            for (int i = 0; i < scriptInfo.startingDemons.Count; i++)
            {
                this.Characters.Demons.Add(scriptInfo.startingDemons[i].name);
            }

            InitializeCharacterAmount(charactersCount);
        }

        public void InitializeFromAscension(AscensionsData ascension, List<CharactersCount> charactersCount)
        {
            for (int i = 0; i < ascension.townsfolks.Count; i++)
            {
                this.Characters.Villagers.Add(ascension.townsfolks[i].name);
            }
            for (int i = 0; i < ascension.outsiders.Count; i++)
            {
                this.Characters.Outcasts.Add(ascension.outsiders[i].name);
            }
            for (int i = 0; i < ascension.minions.Count; i++)
            {
                this.Characters.Minions.Add(ascension.minions[i].name);
            }
            for (int i = 0; i < ascension.demons.Count; i++)
            {
                this.Characters.Demons.Add(ascension.demons[i].name);
            }

            InitializeCharacterAmount(charactersCount);
        }

        public void InitializeCharacterAmount(List<CharactersCount> charactersCount)
        {
            const int ALL_CHAR_COUNT = 9;
            const int TOWN_COUNT = 5;
            const int OUTS_COUNT = 2;
            const int MINION_COUNT = 1;
            const int DEMON_COUNT = 1;

            CharactersAmount.Clear();
            for (var i = 0; i < charactersCount.Count; i++)
            {
                var toCopy = charactersCount[i];
                if (toCopy == null)
                    toCopy = new CharactersCount(ALL_CHAR_COUNT, TOWN_COUNT, DEMON_COUNT, OUTS_COUNT, MINION_COUNT);
                var charAmountData = new CharacterAmount_Data();
                charAmountData.AllCharactersAmount = toCopy.allCharCount;
                charAmountData.TownsfolkAmount = toCopy.town;
                charAmountData.OutcastAmount = toCopy.outs;
                charAmountData.MinionAmount = toCopy.minion;
                charAmountData.DemonAmount = toCopy.demon;
                CharactersAmount.Add(charAmountData);
            }
        }

        public void AssignData(AscensionsData data)
        {
            var scriptInfo = data.currentPickedScript;
            var list = new List<ScriptInfo>(1);
            list.Add(scriptInfo);
            data.possibleScripts = (Il2CppReferenceArray<ScriptInfo>)list.ToArray();
            data.possibleScriptsData = new Il2CppReferenceArray<CustomScriptData>(0);
            AssignData_Internal(scriptInfo);
        }

        public void AssignData(CustomScriptData data)
        {
            var scriptInfo = data.scriptInfo;
            AssignData_Internal(scriptInfo);
        }

        public void AssignData(ScriptInfo data)
        {
            AssignData_Internal(data);
        }

        private void AssignData_Internal(ScriptInfo scriptInfo)
        {
            scriptInfo.startingTownsfolks = new List<CharacterData>(this.Characters.Villagers.Count);
            for (var i = this.Characters.Villagers.Count - 1; i >= 0; i--)
            {
                var charName = this.Characters.Villagers[i];
                var charData = CustomScenario.FindCharacter(charName);
                if (charData == null)
                {
                    CustomScenario.Logger.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                scriptInfo.startingTownsfolks.Add(charData);
            }

            scriptInfo.startingOutsiders = new List<CharacterData>(this.Characters.Outcasts.Count);
            for (var i = this.Characters.Outcasts.Count - 1; i >= 0; i--)
            {
                var charName = this.Characters.Outcasts[i];
                var charData = CustomScenario.FindCharacter(charName);
                if (charData == null)
                {
                    CustomScenario.Logger.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                scriptInfo.startingOutsiders.Add(charData);
            }

            scriptInfo.startingMinions = new List<CharacterData>(this.Characters.Minions.Count);
            for (var i = this.Characters.Minions.Count - 1; i >= 0; i--)
            {
                var charName = this.Characters.Minions[i];
                var charData = CustomScenario.FindCharacter(charName);
                if (charData == null)
                {
                    CustomScenario.Logger.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                scriptInfo.startingMinions.Add(charData);
            }

            scriptInfo.startingDemons = new List<CharacterData>(this.Characters.Demons.Count);
            for (var i = this.Characters.Demons.Count - 1; i >= 0; i--)
            {
                var charName = this.Characters.Demons[i];
                var charData = CustomScenario.FindCharacter(charName);
                if (charData == null)
                {
                    CustomScenario.Logger.Error($"Character '{charName}' not found in game data. Skipping.");
                    continue;
                }
                scriptInfo.startingDemons.Add(charData);
            }

            scriptInfo.characterCounts = new List<CharactersCount>();
            for (var i = 0; i < CharactersAmount.Count; i++)
            {
                scriptInfo.characterCounts.Add(new CharactersCount(CharactersAmount[i].AllCharactersAmount,
                                                                   CharactersAmount[i].TownsfolkAmount,
                                                                   CharactersAmount[i].DemonAmount,
                                                                   CharactersAmount[i].OutcastAmount,
                                                                   CharactersAmount[i].MinionAmount));
            }
        }

        public class ScriptInfo_Data
        {
            public System.Collections.Generic.List<string> Villagers { get; set; } = new();
            public System.Collections.Generic.List<string> Outcasts { get; set; } = new();
            public System.Collections.Generic.List<string> Minions { get; set; } = new();
            public System.Collections.Generic.List<string> Demons { get; set; } = new();
        }

        public class CharacterAmount_Data
        {
            public int AllCharactersAmount { get; set; } = 9;
            public int TownsfolkAmount { get; set; } = 5;
            public int OutcastAmount { get; set; } = 2;
            public int MinionAmount { get; set; } = 1;
            public int DemonAmount { get; set; } = 1;
        }
    }
}