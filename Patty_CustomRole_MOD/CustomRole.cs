using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Utils;
using Patty_CustomRole_MOD;
using Patty_CustomRole_MOD.Json;
using Patty_CustomRole_MOD.Patch;
using Patty_CustomRole_MOD.QoL;
using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomRole), "Patty_CustomRole_MOD", "2.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonPriority(99)]
[assembly: HarmonyDontPatchAll]

namespace Patty_CustomRole_MOD
{
    public class CustomRole : MelonMod
    {

        public string BasePath => Path.Combine(Path.GetDirectoryName(MelonAssembly.Location), nameof(CustomRole));

        public static MelonLogger.Instance Logger { get; private set; }

        internal readonly static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<SkinData>> allSkinData = new(() => Resources.FindObjectsOfTypeAll<SkinData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<AchievementData>> allAchievementData = new(() => Resources.FindObjectsOfTypeAll<AchievementData>(), isThreadSafe: false);

        public readonly static Lazy<JsonSerializerOptions> jsonSerializerOptions = new(() => new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        }, isThreadSafe: false);

        public readonly static Dictionary<FileInfo, Sprite> allLoadedTextures = new();
        public readonly static Dictionary<FileInfo, Role_Json> allRolesDataJson = new();
        public readonly static Dictionary<FileInfo, CharacterSkin_Json> allCharacterSkinDataJson = new();
        internal static MelonPreferences_Category configCategory;
        internal static HashSet<SkinData> extractedSkins = new HashSet<SkinData>();

        public override void OnEarlyInitializeMelon()
        {
            Logger = LoggerInstance;
            configCategory = MelonPreferences.CreateCategory(Configuration.MAIN_CATEGORY);
            configCategory.CreateEntry(Configuration.EXTRACT_GAME_FILES, true, description: "When enabled, extract all of the game character if its not extracted yet");
            configCategory.CreateEntry(Configuration.EXTRACT_HIDDEN_SKINS, false, description: "When enabled, extract all of the game hidden skin if its not extracted yet");
            configCategory.CreateEntry(Configuration.EXTRACT_HIDDEN_ACHIEVEMENT, false, description: "When enabled, extract all of the game hidden achievement if its not extracted yet");
            configCategory.CreateEntry(Configuration.WAIT_FRAMES, 2, description: "The amount of frames this mod has to wait before executing. Increase this if some mod is activating later than this mod." +
                                                                    "\nWhich can make changes implemented by this mod replaced by that mod" +
                                                                    "\nMinimum: 2 Frame");
            configCategory.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, "CustomRole.cfg"));
            configCategory.SaveToFile();
        }

        public override void OnInitializeMelon()
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<TestRole>();
            }
            catch (Exception ex)
            {
                Logger.Error((ex.InnerException ?? ex).Message);
            }
        }

        public override void OnLateInitializeMelon()
        {
            try
            {
                HarmonyInstance.PatchAll(typeof(PatchList));
            }
            catch (HarmonyException ex)
            {
                LoggerInstance.BigError(ex.ToString());
            }
            MelonCoroutines.Start(InitializeAfterCertainFrame());
        }

        private IEnumerator InitializeAfterCertainFrame()
        {
            var waitAmount = Mathf.Max(2, configCategory.GetEntry<int>(Configuration.WAIT_FRAMES).Value);
            for (var i = 0; i < waitAmount; i++)
            {
                yield return YieldCache.WaitForEndOfFrame.Value;
            }

            if (configCategory.GetEntry<bool>(Configuration.EXTRACT_GAME_FILES).Value)
            {
                ExtractCharacters();
            }

            yield return YieldCache.WaitForEndOfFrame.Value;
            if (configCategory.GetEntry<bool>(Configuration.EXTRACT_GAME_FILES).Value &&
                configCategory.GetEntry<bool>(Configuration.EXTRACT_HIDDEN_SKINS).Value)
            {
                foreach (var skinData in allSkinData.Value)
                {
                    ExtractSkin(skinData);
                }
            }

            yield return YieldCache.WaitForEndOfFrame.Value;
            if (configCategory.GetEntry<bool>(Configuration.EXTRACT_GAME_FILES).Value)
            {
                foreach (var achievement in allAchievementData.Value)
                {
                    var isRegistered = ProjectContext.Instance.gameData.allAchievements.Contains(achievement);
                    if (!isRegistered && !configCategory.GetEntry<bool>(Configuration.EXTRACT_HIDDEN_ACHIEVEMENT).Value)
                    {
                        continue;
                    }
                    DirectoryInfo achievementFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Achievement"));
                    var achievementJson = new AchievementData_Json(achievement);
                    var achievementPath = Path.Combine(achievementFolder.FullName, $"{achievement.id}.json");
                    if (!File.Exists(achievementPath))
                        File.WriteAllText(achievementPath, JsonSerializer.Serialize(achievementJson, jsonSerializerOptions.Value));
                }
            }

            yield return YieldCache.WaitForEndOfFrame.Value;
            LoadAllTextures();

            yield return YieldCache.WaitForEndOfFrame.Value;
            LoadAllSkins();

            yield return YieldCache.WaitForEndOfFrame.Value;
            LoadAllAchievement();

            yield return YieldCache.WaitForEndOfFrame.Value;
            LoadAllCharacters();
        }

        private void ExtractCharacters()
        {
            DirectoryInfo characterFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Role"));
            foreach (var character in allCharacterData.Value)
            {
                try
                {
                    string folderName;
                    if (character.role?.GetIl2CppType().Assembly.GetName().name == "Assembly-CSharp")
                    {
                        folderName = "Vanilla";
                    }
                    else
                    {
                        folderName = Utility.GetMelonBaseFromRole(character.role)?.Info?.Name ?? "Modded";
                    }
                    DirectoryInfo characterSubFolder = Directory.CreateDirectory(Path.Combine(characterFolder.FullName, folderName));
                    var characterPath = Path.Combine(characterSubFolder.FullName, $"{character.characterId}.json");
                    ExtractRole(character, characterPath, folderName);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to extract {character.name}: {e.Message}");
                }
            }
        }

        private void LoadAllTextures()
        {
            DirectoryInfo textureFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Texture"));
            foreach (FileInfo png in textureFolder.GetFiles("*.png", SearchOption.AllDirectories))
            {
                var sprite = Utility.CreateSprite(png);
                if (sprite != null)
                    allLoadedTextures[png] = sprite;
            }
        }
        private void LoadAllAchievement()
        {
            DirectoryInfo achievementFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Achievement"));
            foreach (FileInfo json in achievementFolder.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (LoadJsonFromFile(json.FullName, out AchievementData_Json achievementJson))
                {
                    ModdedAchievementCompendium.allAchievementDataJson[json] = achievementJson;
                    var achievement = Utility.FindAchievementById(achievementJson.Id);
                    if (achievement == null)
                    {
                        achievement = ScriptableObject.CreateInstance<AchievementData>();
                        achievement.name = Path.GetFileNameWithoutExtension(json.Name);
                    }
                    if (achievementJson.AutoUnlocked)
                    {
                        if (!SavesGame.UnlockedAchievements.ids.Contains(achievement.id))
                        {
                            SavesGame.UnlockedAchievements.ids.Add(achievement.id);
                        }
                        if (!ProjectContext.Instance.gameData.allAchievements.Contains(achievement))
                        {
                            ProjectContext.Instance.gameData.allAchievements = ProjectContext.Instance.gameData.allAchievements.Append(achievement).ToArray();
                        }
                    }
                    achievementJson.AssignData(achievement);
                    ModdedAchievementCompendium.dataToJson[achievement] = achievementJson;
                }
            }
        }

        private void LoadAllSkins()
        {
            DirectoryInfo skinFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Skin"));
            foreach (FileInfo json in skinFolder.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (LoadJsonFromFile(json.FullName, out CharacterSkin_Json skinJson))
                {
                    allCharacterSkinDataJson[json] = skinJson;
                    var skin = Utility.FindSkinById(skinJson.SkinId);
                    if (skin == null)
                    {
                        skin = ScriptableObject.CreateInstance<SkinData>();
                        skin.name = Path.GetFileNameWithoutExtension(json.Name);
                        skin.unlockWith = new AutoUnlocked();
                    }
                    if (skinJson.AutoUnlocked)
                    {
                        skin.UnlockSkin();
                        if (!SavesGame.UnlockedSkins.ids.Contains(skin.skinId))
                            SavesGame.UnlockedSkins.ids.Add(skin.skinId);
                    }
                    skinJson.AssignData(skin);
                }
            }
        }

        private void LoadAllCharacters()
        {
            DirectoryInfo characterFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Role"));
            foreach (FileInfo json in characterFolder.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (LoadJsonFromFile(json.FullName, out Role_Json roleJson))
                {
                    allRolesDataJson[json] = roleJson;
                    CharacterData character = Utility.FindCharacterById(roleJson.CharacterId);
                    if (character == null)
                    {
                        character = ScriptableObject.CreateInstance<CharacterData>();
                        ProjectContext.Instance.gameData.allCharacterData.Add(character);
                    }
                    roleJson.AssignData(character);
                    foreach (var skinJson in allCharacterSkinDataJson.Values.Where(x => x.SkinFor == character.characterId))
                    {
                        var skin = Utility.FindSkinById(skinJson.SkinId);
                        if (skin == null)
                        {
                            skin = ScriptableObject.CreateInstance<SkinData>();
                            skin.name = Path.GetFileNameWithoutExtension(json.Name);
                        }
                        if (skinJson.AutoUnlocked)
                        {
                            skin.UnlockSkin();
                            if (!SavesGame.UnlockedSkins.ids.Contains(skin.skinId))
                                SavesGame.UnlockedSkins.ids.Add(skin.skinId);
                        }
                        if (character.skins.Find(new Func<SkinData, bool>(x => x.skinId == skin.skinId)) == null)
                        {
                            character.skins.Add(skin);
                        }
                        skinJson.AssignData(skin);
                        skinJson.AssignSkin(skin, character);
                        var characterPreference = SavesGame.CharacterPreferences.prefs.Find(new Func<CharacterPreference, bool>(x => x.chId == character.characterId && x.prefSkinId == skin.skinId));
                        if (characterPreference != null)
                        {
                            character.ChangeSkin(skin);
                        }
                    }
                }
            }
        }

        public bool LoadJsonFromFile<T>(string path, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), jsonSerializerOptions.Value);
            }
            catch (Exception ex)
            {
                result = default!;
                Logger.Error($"Cannot load file {Path.GetFileName(path)}, reason: {(ex.InnerException ?? ex).Message}");
            }
            return result != null;
        }


        public Role_Json ExtractRole(CharacterData character, string path, string skinSubFolderName = "")
        {
            var roleJson = new Role_Json(character);
            if (!File.Exists(path))
                File.WriteAllText(path, JsonSerializer.Serialize(roleJson, jsonSerializerOptions.Value));

            DirectoryInfo textureFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Texture"));

            if (character.skins != null && character.skins.Count > 0)
            {
                foreach (var skin in character.skins)
                {
                    if (skin == null)
                        continue;
                    ExtractSkin(skin, skinSubFolderName);
                }
            }

            character.art.ExtractImage(textureFolder.FullName);
            character.art_cute.ExtractImage(textureFolder.FullName);
            character.art_nice.ExtractImage(textureFolder.FullName);
            character.art_animated.ExtractImage(textureFolder.FullName);
            character.randomArt.ExtractImage(textureFolder.FullName);
            character.backgroundArt.ExtractImage(textureFolder.FullName);

            return roleJson;
        }

        public void ExtractSkin(SkinData skin, string skinSubFolderName = "")
        {
            if (skin == null)
            {
                return;
            }
            if (extractedSkins.Contains(skin))
            {
                return;
            }
            extractedSkins.Add(skin);
            DirectoryInfo skinFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Skin"));
            if (!string.IsNullOrWhiteSpace(skinSubFolderName))
            {
                skinFolder = Directory.CreateDirectory(Path.Combine(skinFolder.FullName, skinSubFolderName));
            }
            DirectoryInfo textureFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Texture"));
            var skinJson = new CharacterSkin_Json(skin);
            var skinPath = Path.Combine(skinFolder.FullName, $"{skin.skinId}.json");
            if (!File.Exists(skinPath))
                File.WriteAllText(skinPath, JsonSerializer.Serialize(skinJson, jsonSerializerOptions.Value));

            skin.art.ExtractImage(textureFolder.FullName);
            skin.animated_art.ExtractImage(textureFolder.FullName);
            skin.lockedArt.ExtractImage(textureFolder.FullName);
        }
    }
}
