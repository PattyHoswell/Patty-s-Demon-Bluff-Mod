using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Patty_ModdedCompendium_MOD;
using Patty_ModdedCompendium_MOD.Patch;
using Patty_ModdedCompendium_MOD.QoL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;

[assembly: MelonInfo(typeof(ModdedCompendium), "Patty_ModdedCompendium_MOD", "1.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: HarmonyDontPatchAll]
namespace Patty_ModdedCompendium_MOD
{
    public class ModdedCompendium : MelonMod
    {
        public static readonly string MODDED_COMPENDIUM = "Modded_Compendium";
        public static readonly string SORT_OPTION = "Sort_Option";
        public static readonly string SHOW_UNCATEGORIZED_VANILLA_CHARS = "Show_Uncategorized_Vanilla_Characters";
        public static MelonLogger.Instance Logger { get; internal set; }
        public static readonly string COLOR_TAGS_REGEX = @"<color=.*?>";
        public static readonly List<CharacterData> hiddenCharacterDatas = new List<CharacterData>();
        public static readonly List<CharacterData> unknownCharacterDatas = new List<CharacterData>();
        public static readonly Dictionary<MelonBase, List<CharacterData>> moddedCharacterDatasCategorized = new Dictionary<MelonBase, List<CharacterData>>();
        public static readonly Dictionary<ECharacterType, List<CharacterData>> moddedCharacterDatas = new Dictionary<ECharacterType, List<CharacterData>>();
        public static readonly Dictionary<ECharacterType, string> colorTags = new Dictionary<ECharacterType, string>
        {
            { ECharacterType.None, "<color=green>" },
            { ECharacterType.Villager, "<color=lightblue>" },
            { ECharacterType.Outcast, "<color=yellow>" },
            { ECharacterType.Minion, "<color=orange>" },
            { ECharacterType.Demon, "<color=red>" },
        };
        internal static MelonPreferences_Category configCategory;
        public override void OnLateInitializeMelon()
        {
            Logger = LoggerInstance;
            moddedCharacterDatas.Populate();

            configCategory = MelonPreferences.CreateCategory(MODDED_COMPENDIUM);
            configCategory.CreateEntry(SORT_OPTION, ECategorySortOption.Regular, description: "Define the sort options. Valid Options:" +
                                                                        $"\n{string.Join(", ", Enum.GetNames(typeof(ECategorySortOption)))}");
            configCategory.CreateEntry(SHOW_UNCATEGORIZED_VANILLA_CHARS, true, description: "When enabled, show characters that are in-game but are hidden");
            configCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "ModdedCompendium.cfg"));
            configCategory.SaveToFile();
            try
            {
                HarmonyInstance.PatchAll(typeof(PatchList));
            }
            catch (HarmonyException ex)
            {
                LoggerInstance.BigError(ex.ToString());
            }
        }

        public static MelonBase GetMelonBaseFromRole(Role role)
        {
            if (role == null)
            {
                return null;
            }
            return MelonBase.RegisteredMelons.FirstOrDefault(x => x.MelonAssembly.Location == role.GetType().Assembly.Location);
        }

        internal static IEnumerable<CharactersCompendiumPage> GetCompendiumCharacters(Compendium compendium, ECharacterType characterType)
        {
            Func<CharactersCompendiumPage, bool> findSimilarPage = x => x.pageName.GetLocalizedString().Contains(characterType.GetName(), StringComparison.OrdinalIgnoreCase);
            return compendium.pages.Where(findSimilarPage);
        }

        internal static void CreateNewPage(Compendium compendium, ECharacterType characterType, List<CharacterData> characterDatas)
        {
            if (characterDatas.Count <= 0)
            {
                return;
            }
            Func<CharactersCompendiumPage, bool> findSimilarPage = x => x.pageName.GetLocalizedString().Contains(characterType.GetName(), StringComparison.OrdinalIgnoreCase);
            var charactersPages = GetCompendiumCharacters(compendium, characterType);
            var pagesCount = charactersPages.Count();
            var page = charactersPages.FirstOrDefault(x => x.characterDatas.Length < compendium.cards.Length) ?? new CharactersCompendiumPage();
            if (page.characterDatas == null)
            {
                page.characterDatas = new CharacterData[0];
            }
            var availableSpace = compendium.cards.Length - page.characterDatas.Length;
            var sliceCount = Mathf.Min(availableSpace, characterDatas.Count);
            var slicedCharacterDatas = characterDatas.GetRange(0, Mathf.Max(sliceCount, 0));
            characterDatas.RemoveRange(0, slicedCharacterDatas.Count);
            page.characterDatas = page.characterDatas.Union(slicedCharacterDatas).ToArray();

            var colorTag = "";
            var firstItem = charactersPages.FirstOrDefault();
            if (firstItem != null)
            {
                var match = Regex.Match(charactersPages.First().pageName.GetLocalizedString(), COLOR_TAGS_REGEX);
                if (match.Success)
                {
                    colorTag = match.Groups[0].Value;
                }
            }
            else if ((firstItem == null || string.IsNullOrWhiteSpace(colorTag)) && !colorTags.TryGetValue(characterType, out colorTag))
            {
                Logger.Warning($"Unable to get color tag for {characterType.GetName()}");
            }
            if (!compendium.pages.Contains(page))
            {
                var lastIndex = compendium.pages.FindLastIndex(findSimilarPage);
                compendium.pages = lastIndex >= 0 ? compendium.pages.Insert(lastIndex + 1, page) : compendium.pages.Append(page).ToArray();
                var entry = LocalizedStringUtil.AddString($"COMPENDIUM_PAGE_{characterType}_{compendium.pages.IndexOf(page)}", $"{colorTag}{characterType.GetName()}s");
                page.pageName = entry;
                if (pagesCount >= 1)
                {
                    page.page = $"{pagesCount}/{pagesCount + 1}";

                }
            }

            if (characterDatas.Count > 0)
            {
                CreateNewPage(compendium, characterType, characterDatas);
            }
        }

        internal static void CreateNewPageCategorized(Compendium compendium, MelonBase melonBase, List<CharacterData> characterDatas)
        {
            if (characterDatas.Count <= 0)
            {
                return;
            }
            var nameSearch = melonBase != null ?
                                LocalizedStringUtil.AddString($"COMPENDIUM_TITLE_{melonBase.Info.Name}", melonBase.Info.Name) :
                                LocalizedStringUtil.AddString($"COMPENDIUM_TITLE_UNKNOWN", "Unknown");
            Func<CharactersCompendiumPage, bool> findSimilarPage = x => x.pageName.GetLocalizedString().Contains(nameSearch.GetLocalizedString(), StringComparison.OrdinalIgnoreCase);
            var modPages = compendium.pages.Where(findSimilarPage);
            var page = modPages.FirstOrDefault(x => x.characterDatas.Length < compendium.cards.Length) ?? new CharactersCompendiumPage();
            if (page.characterDatas == null)
            {
                page.characterDatas = new CharacterData[0];
            }
            var availableSpace = compendium.cards.Length - page.characterDatas.Length;
            var sliceCount = Mathf.Min(availableSpace, characterDatas.Count);
            var slicedCharacterDatas = characterDatas.GetRange(0, Mathf.Max(sliceCount, 0));
            characterDatas.RemoveRange(0, slicedCharacterDatas.Count);

            page.characterDatas = page.characterDatas.Union(slicedCharacterDatas).ToArray();

            var colorTag = "";
            if (!colorTags.TryGetValue(ECharacterType.None, out colorTag))
            {
                Logger.Warning($"Unable to get color tag for {ECharacterType.None.GetName()}");
            }
            if (!compendium.pages.Contains(page))
            {
                compendium.pages = compendium.pages.Append(page).ToArray();
            }
            page.pageName = nameSearch;

            if (characterDatas.Count > 0)
            {
                CreateNewPageCategorized(compendium, melonBase, characterDatas);
            }
        }

        internal static void SortCompendium(Compendium compendium, bool rename = true)
        {
            var villagersCategory = GetCompendiumCharacters(compendium, ECharacterType.Villager).ToArray();
            var outcastsCategory = GetCompendiumCharacters(compendium, ECharacterType.Outcast).ToArray();
            var minionsCategory = GetCompendiumCharacters(compendium, ECharacterType.Minion).ToArray();
            var demonsCategory = GetCompendiumCharacters(compendium, ECharacterType.Demon).ToArray();
            var unknownCategory = compendium.pages.Except(demonsCategory).Except(outcastsCategory).Except(minionsCategory).Except(demonsCategory).ToArray();
            Sort(villagersCategory, ECharacterType.Villager, rename);
            Sort(outcastsCategory, ECharacterType.Outcast, rename);
            Sort(minionsCategory, ECharacterType.Minion, rename);
            Sort(demonsCategory, ECharacterType.Demon, rename);
            if (unknownCategory != null && unknownCategory.Length > 0)
            {
                SortCategorized(compendium, unknownCategory, rename);
            }
        }
        static void Sort(CharactersCompendiumPage[] category, ECharacterType characterType, bool rename = true)
        {
            for (var i = 0; i < category.Length; i++)
            {
                var item = category[i];
                if (!string.IsNullOrWhiteSpace(item.pageName.GetLocalizedString()))
                {
                    var colorTag = "";
                    var match = Regex.Match(item.pageName.GetLocalizedString(), COLOR_TAGS_REGEX);
                    if (match.Success)
                    {
                        colorTag = match.Groups[0].Value;
                    }
                    else if (!colorTags.TryGetValue(characterType, out colorTag))
                    {
                        Logger.Warning($"Unable to get color tag for {characterType.GetName()}");
                    }
                    if (rename)
                    {
                        LocalizedString entry;
                        if (item.pageName == null || item.pageName.TableEntryReference.Key.Length == 0)
                        {
                            entry = LocalizedStringUtil.AddString($"COMPENDIUM_PAGE_{characterType}_{i + 1}", $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        else
                        {
                            entry = LocalizedStringUtil.ChangeString(item.pageName.TableEntryReference.Key, $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        item.pageName = entry;
                        if (category.Length > 1)
                        {
                            item.page = $"{i + 1}/{category.Length}";
                        }
                    }
                }
                else
                {
                    var colorTag = "";
                    if (!colorTags.TryGetValue(characterType, out colorTag))
                    {
                        Logger.Warning($"Unable to get color tag for {characterType.GetName()}");
                    }
                    if (rename)
                    {
                        LocalizedString entry;
                        if (item.pageName == null || item.pageName.TableEntryReference.Key.Length == 0)
                        {
                            entry = LocalizedStringUtil.AddString($"COMPENDIUM_PAGE_{characterType}_{i + 1}", $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        else
                        {
                            entry = LocalizedStringUtil.ChangeString(item.pageName.TableEntryReference.Key, $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        item.pageName = entry;
                        if (category.Length > 1)
                        {
                            item.page = $"{i + 1}/{category.Length}";
                        }
                    }
                }
                if (item.characterDatas != null)
                {
                    item.characterDatas = item.characterDatas.OrderBy(x => x.type).ThenBy(x => x.startingAlignment).ThenBy(x => x.name).ToArray();
                }
            }
        }

        static void SortCategorized(Compendium compendium, CharactersCompendiumPage[] category, bool rename = true)
        {
            for (var i = 0; i < category.Length; i++)
            {
                var item = category[i];
                Func<CharactersCompendiumPage, bool> findSimilarPage = x => x.pageName.GetLocalizedString().Equals(item.pageName.GetLocalizedString(), StringComparison.OrdinalIgnoreCase);
                var modPages = compendium.pages.Where(findSimilarPage);
                if (!string.IsNullOrWhiteSpace(item.pageName.GetLocalizedString()))
                {
                    var colorTag = "";
                    var match = Regex.Match(item.pageName.GetLocalizedString(), COLOR_TAGS_REGEX);
                    if (match.Success)
                    {
                        colorTag = match.Groups[0].Value;
                    }
                    else if (!colorTags.TryGetValue(ECharacterType.None, out colorTag))
                    {
                        Logger.Warning($"Unable to get color tag for {ECharacterType.None.GetName()}");
                    }
                    if (rename)
                    {
                        LocalizedString entry;
                        if (item.pageName == null || item.pageName.TableEntryReference.Key.Length == 0)
                        {
                            entry = LocalizedStringUtil.AddString($"COMPENDIUM_TITLE_{item.pageName.GetLocalizedString()}", $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        else
                        {
                            entry = LocalizedStringUtil.ChangeString(item.pageName.TableEntryReference.Key, $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        item.pageName = entry;
                        if (modPages.Count() > 1)
                        {
                            item.page = $"{modPages.FindIndex(item) + 1}/{modPages.Count()}";
                        }
                    }
                }
                else
                {
                    var colorTag = "";
                    if (!colorTags.TryGetValue(ECharacterType.None, out colorTag))
                    {
                        Logger.Warning($"Unable to get color tag for {ECharacterType.None.GetName()}");
                    }
                    if (rename)
                    {
                        LocalizedString entry;
                        if (item.pageName == null || item.pageName.TableEntryReference.Key.Length == 0)
                        {
                            entry = LocalizedStringUtil.AddString($"COMPENDIUM_TITLE_{item.pageName.GetLocalizedString()}", $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        else
                        {
                            entry = LocalizedStringUtil.ChangeString(item.pageName.TableEntryReference.Key, $"{colorTag}{item.pageName.GetLocalizedString()}");
                        }
                        item.pageName = entry;
                        if (modPages.Count() > 1)
                        {
                            item.page = $"{modPages.FindIndex(item) + 1}/{modPages.Count()}";
                        }
                    }
                }
                if (item.characterDatas != null)
                {
                    item.characterDatas = item.characterDatas.OrderBy(x => x.type).ThenBy(x => x.startingAlignment).ThenBy(x => x.name).ToArray();
                }
            }
        }
    }
}
