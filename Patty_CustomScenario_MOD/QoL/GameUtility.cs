using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.IO;
using UnityEngine;

namespace Patty_CustomScenario_MOD.QoL
{
    public static class GameUtility
    {
        internal readonly static Lazy<Il2CppArrayBase<AscensionsData>> allAscensionData = new(() => Resources.FindObjectsOfTypeAll<AscensionsData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<CustomScriptData>> allCustomScriptData = new(() => Resources.FindObjectsOfTypeAll<CustomScriptData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<SkinData>> allSkinData = new(() => Resources.FindObjectsOfTypeAll<SkinData>(), isThreadSafe: false);

        /// <summary>
        /// Guarantees to find the character data by name, including custom characters.
        /// Unless the character does not exist at all.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CharacterData FindCharacter(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<CharacterData, bool> findFunc = c => c.name.Equals(name, comparison);
            var charData = ProjectContext.Instance.gameData.allCharacterData.Find(findFunc);
            if (charData == null)
            {
                foreach (var characterData in allCharacterData.Value)
                {
                    if (findFunc.Invoke(characterData))
                    {
                        ProjectContext.Instance.gameData.allCharacterData.Add(characterData);
                        return characterData;
                    }
                }
            }
            if (charData == null)
            {
                // In case the character is not in the game data yet (e.g., custom characters not yet added)
                // This is not actually needed if we ensure custom characters are added this mod is loaded.
                // But just in case, we do this fallback.
                foreach (var characterData in Resources.FindObjectsOfTypeAll<CharacterData>())
                {
                    if (findFunc.Invoke(characterData))
                    {
                        ProjectContext.Instance.gameData.allCharacterData.Add(characterData);
                        return characterData;
                    }
                }
            }
            return charData!;
        }
        /// <summary>
        /// Guarantees to find the character data by id, including custom characters.
        /// Unless the character does not exist at all.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CharacterData FindCharacterById(string id, bool searchByNameIfNull = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<CharacterData, bool> findFunc = c => c.characterId.Equals(id, comparison);
            var charData = ProjectContext.Instance.gameData.allCharacterData.Find(findFunc);
            if (charData == null)
            {
                foreach (var characterData in allCharacterData.Value)
                {
                    if (findFunc.Invoke(characterData))
                    {
                        ProjectContext.Instance.gameData.allCharacterData.Add(characterData);
                        return characterData;
                    }
                }
            }
            if (charData == null)
            {
                // In case the character is not in the game data yet (e.g., custom characters not yet added)
                // This is not actually needed if we ensure custom characters are added this mod is loaded.
                // But just in case, we do this fallback.
                foreach (var characterData in Resources.FindObjectsOfTypeAll<CharacterData>())
                {
                    if (findFunc.Invoke(characterData))
                    {
                        ProjectContext.Instance.gameData.allCharacterData.Add(characterData);
                        return characterData;
                    }
                }
            }
            if (charData == null && searchByNameIfNull)
            {
                charData = FindCharacter(id);
            }
            return charData!;
        }

        public static CustomScriptData_Json FindCustomScenarioData_Json(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<string, bool> findFunc = item => Path.GetFileNameWithoutExtension(item).Equals(name, comparison);
            foreach (var (filePath, data) in CustomScenario.customScripts)
            {
                if (findFunc.Invoke(filePath))
                {
                    return data;
                }
            }
            return null!;
        }

        public static AscensionsData_Json FindAscensionData_Json(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<string, bool> findFunc = item => Path.GetFileNameWithoutExtension(item).Equals(name, comparison);
            foreach (var (filePath, data) in CustomScenario.customAscensions)
            {
                if (findFunc.Invoke(filePath))
                {
                    return data;
                }
            }
            return null!;
        }

        public static CustomScriptData FindCustomScriptData(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<CustomScriptData, bool> findFunc = c => c.name.Equals(name, comparison);

            foreach (var customScriptData in allCustomScriptData.Value)
            {
                if (findFunc.Invoke(customScriptData))
                {
                    return customScriptData;
                }
            }
            // In case the character is not in the game data yet
            foreach (var customScriptData in Resources.FindObjectsOfTypeAll<CustomScriptData>())
            {
                if (findFunc.Invoke(customScriptData))
                {
                    return customScriptData;
                }
            }
            return null!;
        }

        public static AscensionsData FindAscensionData(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            Func<AscensionsData, bool> findFunc = c => c.name.Equals(name, comparison);

            foreach (var ascensionData in allAscensionData.Value)
            {
                if (findFunc.Invoke(ascensionData))
                {
                    return ascensionData;
                }
            }
            // In case the character is not in the game data yet
            foreach (var ascensionData in Resources.FindObjectsOfTypeAll<AscensionsData>())
            {
                if (findFunc.Invoke(ascensionData))
                {
                    return ascensionData;
                }
            }
            return null!;
        }

        public static CharactersCount CreateDefaultCharactersCount()
        {
            const int ALL_CHAR_COUNT = 6;
            const int TOWN_COUNT = 3;
            const int OUTS_COUNT = 1;
            const int MINION_COUNT = 1;
            const int DEMON_COUNT = 1;

            var result = new CharactersCount(ALL_CHAR_COUNT, TOWN_COUNT, DEMON_COUNT, OUTS_COUNT, MINION_COUNT);
            result.dTown = TOWN_COUNT;
            result.dOuts = OUTS_COUNT;
            result.dMinion = MINION_COUNT;
            result.dDemon = DEMON_COUNT;
            return result;
        }

        public static ScriptInfo CreateDefaultScriptInfo()
        {
            var result = new ScriptInfo();
            result.characterCounts = new Il2CppSystem.Collections.Generic.List<CharactersCount>();
            result.characterCounts.Add(CreateDefaultCharactersCount());
            result.startingTownsfolks = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            result.startingTownsfolks.Add(FindCharacterById("Baker_22847064"));
            result.startingTownsfolks.Add(FindCharacterById("Baker_22847064"));
            result.startingTownsfolks.Add(FindCharacterById("Baker_22847064"));
            result.startingTownsfolks.Add(FindCharacterById("Confessor_18741708"));
            result.startingTownsfolks.Add(FindCharacterById("Confessor_18741708"));
            result.startingTownsfolks.Add(FindCharacterById("Confessor_18741708"));

            result.startingOutsiders = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            result.startingOutsiders.Add(FindCharacterById("Bombardier_79093372"));

            result.startingMinions = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            result.startingMinions.Add(FindCharacterById("Minion_71804875"));

            result.startingDemons = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            result.startingDemons.Add(FindCharacterById("Pooka_13445289"));
            return result;
        }

        public static CustomScriptData CreateDefaultCustomScriptData()
        {
            var result = ScriptableObject.CreateInstance<CustomScriptData>();
            result.name = "CustomScriptData";
            result.scriptInfo = CreateDefaultScriptInfo();
            return result;
        }

        public static int GetAmount(this CharactersCount charactersCount, ECharacterType type, bool isDeck)
        {
            if (type == ECharacterType.Villager)
            {
                return isDeck ? charactersCount.dTown : charactersCount.town;
            }
            if (type == ECharacterType.Outcast)
            {
                return isDeck ? charactersCount.dOuts : charactersCount.outs;
            }
            if (type == ECharacterType.Minion)
            {
                return isDeck ? charactersCount.dMinion : charactersCount.minion;
            }
            if (type == ECharacterType.Demon)
            {
                return isDeck ? charactersCount.dDemon : charactersCount.demon;
            }
            return -1;
        }
    }
}
