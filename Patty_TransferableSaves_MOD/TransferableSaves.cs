using MelonLoader;
using Microsoft.Win32;
using Patty_TransferableSaves_MOD;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
[assembly: MelonInfo(typeof(TransferableSaves), nameof(Patty_TransferableSaves_MOD), "1.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: HarmonyDontPatchAll]
namespace Patty_TransferableSaves_MOD
{
    public class TransferableSaves : MelonMod
    {
        public readonly static string[] registryToIgnores = new string[]
        {
            "unity.player_session_count",
            "unity.player_sessionid"
        };

        public readonly static string[] unitySpecificRegistry = new string[]
        {
            "Screenmanager Fullscreen mode Default",
            "Screenmanager Fullscreen mode",
            "Screenmanager Native RefreshRate Denominator",
            "Screenmanager Native RefreshRate Numerator",
            "Screenmanager Resolution Height Default",
            "Screenmanager Resolution Height",
            "Screenmanager Resolution Use Native Default",
            "Screenmanager Resolution Use Native",
            "Screenmanager Resolution Width Default",
            "Screenmanager Resolution Width",
            "Screenmanager Resolution Window Height",
            "Screenmanager Resolution Window Width",
            "Screenmanager Stereo 3D",
            "Screenmanager Window Position X",
            "Screenmanager Window Position Y",
            "unity.player_session_count",
            "unity.player_sessionid",
            "UnitySelectMonitor",
        };

        public readonly static Lazy<JsonSerializerOptions> UnsafeEnumSerializerOptions = new(() => new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter(), new PlayerPrefs_Json_Converter() },
        }, isThreadSafe: false);

        public static PlayerPrefs_Json GameSaves = new PlayerPrefs_Json();
        public static PlayerPrefs_Json UnitySaves = new PlayerPrefs_Json();
        public static string GameSaveLocation { get; } = Path.Combine(Application.persistentDataPath, "GamePlayerPrefs.json");
        public static string UnitySaveLocation { get; } = Path.Combine(Application.persistentDataPath, "UnityPlayerPrefs.json");
        public static MelonLogger.Instance Logger { get; internal set; }

        public override void OnEarlyInitializeMelon()
        {
            Logger = LoggerInstance;
            ReloadSaveFromRegistry();
            if (File.Exists(GameSaveLocation))
            {
                GameSaves = JsonSerializer.Deserialize<PlayerPrefs_Json>(File.ReadAllText(GameSaveLocation), UnsafeEnumSerializerOptions.Value);
            }

            if (File.Exists(GameSaveLocation))
            {
                UnitySaves = JsonSerializer.Deserialize<PlayerPrefs_Json>(File.ReadAllText(UnitySaveLocation), UnsafeEnumSerializerOptions.Value);
            }
            if (GameSaves != null)
            {
                for (var i = 0; i < GameSaves.Datas.Count; i++)
                {
                    PlayerPrefs_Data data = GameSaves.Datas[i];
                    data.SetValue();
                }
            }
            if (UnitySaves != null)
            {
                for (var i = 0; i < UnitySaves.Datas.Count; i++)
                {
                    PlayerPrefs_Data data = UnitySaves.Datas[i];
                    data.SetValue();
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            PlayerPrefs_Save();
        }

        public override void OnApplicationQuit()
        {
            PlayerPrefs_Save();
        }

        [SupportedOSPlatform("windows")]
        public static void ReloadSaveFromRegistry()
        {
            GameSaves.Datas.Clear();
            UnitySaves.Datas.Clear();
            RegistryKey gameRegistry = null!;
            try
            {
                gameRegistry = Registry.CurrentUser.OpenSubKey($"Software\\{Application.companyName}\\{Application.productName}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            if (gameRegistry == null)
            {
                return;
            }
            string[] valueNames = gameRegistry.GetValueNames();
            for (var i = 0; i < valueNames.Length; i++)
            {
                var keyName = valueNames[i];
                var key = keyName;
                var index = key.LastIndexOf("_");
                if (index <= -1)
                {
                    continue;
                }
                key = key.Remove(index, key.Length - index);
                if (Array.Exists(registryToIgnores, x => x == key))
                    continue;
                var saves = Array.Exists(unitySpecificRegistry, x => x == key) ? UnitySaves : GameSaves;
                switch (gameRegistry.GetValueKind(keyName))
                {
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                    case RegistryValueKind.MultiString:
                    case RegistryValueKind.Binary:
                        saves.Datas.Add(new PlayerPrefs_Data(key, PlayerPrefs.GetString(key), PlayerPrefs_Data.Type.String));
                        break;
                    case RegistryValueKind.DWord:
                    case RegistryValueKind.QWord:
                        if (PlayerPrefs.GetInt(key, -1) == -1 && PlayerPrefs.GetInt(key, 0) == 0)
                        {
                            saves.Datas.Add(new PlayerPrefs_Data(key, PlayerPrefs.GetFloat(key), PlayerPrefs_Data.Type.Float));
                        }
                        else
                        {
                            saves.Datas.Add(new PlayerPrefs_Data(key, PlayerPrefs.GetInt(key), PlayerPrefs_Data.Type.Int));
                        }
                        break;
                    default:
                        saves.Datas.Add(new PlayerPrefs_Data(key, gameRegistry.GetValue(keyName), PlayerPrefs_Data.Type.Unknown));
                        break;
                }
            }
            gameRegistry.Close();
        }

        private static string GetMacOSPrefsPath()
        {
            var initialName = "com";
            if (Application.unityVersionVer <= 2020)
            {
                if (Application.unityVersionVer <= 2020 && Application.unityVersionMaj <= 2)
                    initialName = "unity";
                else if (Application.unityVersionVer <= 2020 && Application.unityVersionMaj >= 3)
                    initialName = "com";
                else
                    initialName = "unity";
            }
            string fileName = $"{initialName}.{Application.companyName}.{Application.productName}.plist";
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var prefPaths = Path.Combine(homeFolder, "Library", "Preferences", fileName);
            return prefPaths;
        }

        private static string GetLinuxPrefsFolder()
        {
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string prefsFolder = Path.Combine(homeFolder, ".config", "unity3d", Application.companyName, Application.productName);
            return prefsFolder;
        }

        public static void PlayerPrefs_Save()
        {
            ReloadSaveFromRegistry();
            string json = JsonSerializer.Serialize(GameSaves, UnsafeEnumSerializerOptions.Value);
            File.WriteAllText(GameSaveLocation, json);
            json = JsonSerializer.Serialize(UnitySaves, UnsafeEnumSerializerOptions.Value);
            File.WriteAllText(UnitySaveLocation, json);
        }
    }
}
