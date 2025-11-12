using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Patty_ModdedCompendium_MOD.Patch
{
    internal static class PatchList
    {
        public static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new Lazy<Il2CppArrayBase<CharacterData>>(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);


        [HarmonyPrefix, HarmonyPatch(typeof(Compendium), nameof(Compendium.LoadUnlockedCharacters))]
        public static void LoadUnlockedCharacters(Compendium __instance)
        {
            if (__instance.unlockedCharacters == null)
            {
                __instance.unlockedCharacters = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            }
            bool hasAddedAnything = false;
            Dictionary<ECharacterType, List<CharacterData>> newHiddenChars = new Dictionary<ECharacterType, List<CharacterData>>();
            List<CharacterData> newUnknownChars = new List<CharacterData>();
            Dictionary<MelonBase, List<CharacterData>> newMelonBases = new Dictionary<MelonBase, List<CharacterData>>();
            Dictionary<ECharacterType, List<CharacterData>> newCharTypes = new Dictionary<ECharacterType, List<CharacterData>>();
            foreach (var characterData in allCharacterData.Value)
            {
                if (characterData == null)
                {
                    continue;
                }
                if (ModdedCompendium.moddedCharacterDatas.HasValue(characterData))
                {
                    continue;
                }
                hasAddedAnything = true;
                if (Path.GetFileName(characterData.role?.GetType().Assembly.Location) == "Assembly-CSharp.dll")
                {
                    if (ModdedCompendium.configCategory.GetEntry<bool>(ModdedCompendium.SHOW_UNCATEGORIZED_VANILLA_CHARS).Value == true &&
                        __instance.pages.All(x => x.characterDatas.All(y => y != characterData)))
                    {
                        newHiddenChars.AddToList(characterData.type, characterData);
                        ModdedCompendium.hiddenCharacterDatas.Add(characterData);
                    }
                    continue;
                }
                var melonBase = ModdedCompendium.GetMelonBaseFromRole(characterData.role);
                if (melonBase != null)
                {
                    newMelonBases.AddToList(melonBase, characterData);
                    ModdedCompendium.moddedCharacterDatasCategorized.AddToList(melonBase, characterData);
                }
                else
                {
                    newUnknownChars.Add(characterData);
                    ModdedCompendium.unknownCharacterDatas.Add(characterData);
                }
                ModdedCompendium.moddedCharacterDatas.AddToList(characterData.type, characterData);
                newCharTypes.AddToList(characterData.type, characterData);
            }
            if (!hasAddedAnything)
            {
                return;
            }
            if (ModdedCompendium.configCategory.GetEntry<bool>(ModdedCompendium.SHOW_UNCATEGORIZED_VANILLA_CHARS).Value == true)
            {
                foreach (var (characterType, characterDatas) in newHiddenChars)
                {
                    if (characterDatas.Count > 0)
                    {
                        ModdedCompendium.CreateNewPage(__instance, characterType, characterDatas);
                    }
                }
            }
            if (ModdedCompendium.configCategory.GetEntry<ECategorySortOption>(ModdedCompendium.SORT_OPTION).Value == ECategorySortOption.ByMod)
            {
                foreach (var (melonBase, characterDatas) in newMelonBases)
                {
                    if (characterDatas.Count > 0)
                    {
                        ModdedCompendium.CreateNewPageCategorized(__instance, melonBase, characterDatas);
                    }
                }
                if (newUnknownChars.Count > 0)
                {
                    ModdedCompendium.CreateNewPageCategorized(__instance, null, newUnknownChars);
                }
            }
            else
            {
                foreach (var (characterType, characterDatas) in newCharTypes)
                {
                    if (characterDatas.Count > 0)
                    {
                        ModdedCompendium.CreateNewPage(__instance, characterType, characterDatas);
                    }
                }
            }
            ModdedCompendium.SortCompendium(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Compendium), nameof(Compendium.LoadUnlockedCharacters))]
        public static void LoadUnlockedCharacters_Postfix(Compendium __instance)
        {
            if (__instance.unlockedCharacters == null)
            {
                __instance.unlockedCharacters = new Il2CppSystem.Collections.Generic.List<CharacterData>();
            }
            foreach (var characterData in ModdedCompendium.hiddenCharacterDatas)
            {
                if (!__instance.unlockedCharacters.Contains(characterData))
                    __instance.unlockedCharacters.Add(characterData);
            }
            foreach (var characterList in ModdedCompendium.moddedCharacterDatas.Values)
            {
                foreach (var characterData in characterList)
                    if (!__instance.unlockedCharacters.Contains(characterData))
                        __instance.unlockedCharacters.Add(characterData);
            }
            __instance.allCharacters = __instance.unlockedCharacters.Count;
            __instance.UpdateCompletion();
        }
    }
}
