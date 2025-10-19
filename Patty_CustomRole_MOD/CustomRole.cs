using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Patty_CustomRole_MOD;
using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomRole), "Patty_CustomRole_MOD", "1.0.2", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonPriority(99)]

namespace Patty_CustomRole_MOD
{
    public class CustomRole : MelonMod
    {
        public string BasePath => Path.GetDirectoryName(MelonAssembly.Location);

        public static MelonLogger.Instance Logger;

        internal readonly static Lazy<Il2CppArrayBase<CharacterData>> allCharacterData = new(() => Resources.FindObjectsOfTypeAll<CharacterData>(), isThreadSafe: false);
        internal readonly static Lazy<Il2CppArrayBase<SkinData>> allSkinData = new(() => Resources.FindObjectsOfTypeAll<SkinData>(), isThreadSafe: false);

        public readonly static Lazy<JsonSerializerOptions> jsonSerializerOptions = new(() => new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        }, isThreadSafe: false);

        public readonly static Dictionary<FileInfo, Sprite> allLoadedTextures = new();
        public readonly static Dictionary<FileInfo, Role_Data> allRolesData = new();
        public readonly static Dictionary<FileInfo, CharacterSkin_Data> allCharacterSkinData = new();

        public override void OnEarlyInitializeMelon()
        {
            Logger = LoggerInstance;
        }

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<TestRole>();
        }

        public override void OnLateInitializeMelon()
        {
            MelonCoroutines.Start(InitializeAfter2Frame());
        }

        private IEnumerator InitializeAfter2Frame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            ExtractFile();
            LoadAllTextures();
            LoadAllSkins();
            LoadAllCharacters();
        }

        private void ExtractFile()
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
                        folderName = Path.GetFileNameWithoutExtension(character.role?.GetType().Assembly.Location) ?? "Modded";
                    }
                    DirectoryInfo characterSubFolder = Directory.CreateDirectory(Path.Combine(characterFolder.FullName, folderName));
                    var characterPath = Path.Combine(characterSubFolder.FullName, $"{character.name}.json");
                    var duplicateAmt = Utility.CheckDuplicatedAmountCharName(character.name);
                    if (duplicateAmt > 1)
                    {
                        MelonLogger.Warning($"There are duplicate character name. {character.name}. extracting by character id instead...");
                        if (duplicateAmt > 2)
                        {
                            LoggerInstance.BigError($"There are {duplicateAmt} characters with the name {character.name}, make sure to check the correct one by id!");
                        }
                        characterPath = Path.Combine(characterSubFolder.FullName, $"{character.characterId}.json");
                    }
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

        private void LoadAllSkins()
        {
            DirectoryInfo skinFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Skin"));
            foreach (FileInfo json in skinFolder.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (LoadCustomRoleFile(json.FullName, out CharacterSkin_Data result))
                {
                    allCharacterSkinData[json] = result;
                    var skin = Utility.FindSkin(json.Name);
                    if (skin == null)
                    {
                        skin = ScriptableObject.CreateInstance<SkinData>();
                        skin.name = Path.GetFileNameWithoutExtension(json.Name);
                        skin.UnlockSkin();
                    }
                    result.AssignData(skin);
                }
            }
        }

        private void LoadAllCharacters()
        {
            DirectoryInfo characterFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Role"));
            foreach (FileInfo json in characterFolder.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (LoadCustomRoleFile(json.FullName, out Role_Data result))
                {
                    allRolesData[json] = result;
                    var character = Utility.FindCharacter(Path.GetFileNameWithoutExtension(json.Name));
                    if (character == null)
                    {
                        LoggerInstance.Warning(json.Name + " not found by file name, trying by id...");
                        character = Utility.FindCharacterById(Path.GetFileNameWithoutExtension(json.Name));
                    }
                    if (character == null)
                    {
                        LoggerInstance.Warning(json.Name + " not found by file name, trying by the characterId inside the json...");
                        character = Utility.FindCharacterById(Path.GetFileNameWithoutExtension(result.CharacterId));
                    }
                    if (character == null)
                    {
                        LoggerInstance.Warning(json.Name + " doesn't seem to exist. Creating new CharacterData instead");
                        character = ScriptableObject.CreateInstance<CharacterData>();
                        ProjectContext.Instance.gameData.allCharacterData.Add(character);
                    }
                    result.AssignData(character);

                    foreach (var skinData in allCharacterSkinData.Values)
                    {
                        var targetCharacterSkin = Utility.FindCharacter(skinData.SkinFor);
                        if (targetCharacterSkin == character)
                        {
                            var skin = Utility.FindSkin(json.Name);
                            if (skin == null)
                            {
                                skin = ScriptableObject.CreateInstance<SkinData>();
                                skin.name = Path.GetFileNameWithoutExtension(json.Name);
                                skin.UnlockSkin();
                                character.skins.Add(skin);
                            }
                            skinData.AssignData(skin, true);
                        }
                    }
                }
            }
        }

        public bool LoadCustomRoleFile<T>(string path, out T result)
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


        public Role_Data ExtractRole(CharacterData character, string path, string skinSubFolderName = "")
        {
            var roleData = new Role_Data(character);
            if (!File.Exists(path))
                File.WriteAllText(path, JsonSerializer.Serialize(roleData, jsonSerializerOptions.Value));

            DirectoryInfo textureFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Texture"));

            if (character.skins != null && character.skins.Count > 0)
            {
                DirectoryInfo skinFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Skin"));
                if (!string.IsNullOrWhiteSpace(skinSubFolderName))
                {
                    skinFolder = Directory.CreateDirectory(Path.Combine(skinFolder.FullName, skinSubFolderName));
                }

                foreach (var skin in character.skins)
                {
                    if (skin == null)
                        continue;

                    var skinData = new CharacterSkin_Data(skin);
                    var skinPath = Path.Combine(skinFolder.FullName, $"{skin.name}.json");
                    if (!File.Exists(skinPath))
                        File.WriteAllText(skinPath, JsonSerializer.Serialize(skinData, jsonSerializerOptions.Value));

                    skin.art.ExtractImage(textureFolder.FullName);
                    skin.animated_art.ExtractImage(textureFolder.FullName);
                    skin.lockedArt.ExtractImage(textureFolder.FullName);
                }
            }

            character.art.ExtractImage(textureFolder.FullName);
            character.art_cute.ExtractImage(textureFolder.FullName);
            character.art_nice.ExtractImage(textureFolder.FullName);
            character.art_animated.ExtractImage(textureFolder.FullName);
            character.randomArt.ExtractImage(textureFolder.FullName);
            character.backgroundArt.ExtractImage(textureFolder.FullName);

            return roleData;
        }
    }
}
