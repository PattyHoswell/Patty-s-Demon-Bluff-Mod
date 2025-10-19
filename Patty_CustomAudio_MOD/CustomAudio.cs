using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Patty_CustomAudio_MOD;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
[assembly: MelonInfo(typeof(CustomAudio), "Patty_CustomAudio_MOD", "1.0.0", "PattyHoswell")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonPriority(99)]
[assembly: HarmonyDontPatchAll]

namespace Patty_CustomAudio_MOD
{
    public class CustomAudio : MelonMod
    {
        internal readonly static Lazy<Il2CppArrayBase<AudioClip>> allAudioClip = new(() => Resources.FindObjectsOfTypeAll<AudioClip>(), isThreadSafe: false);

        public string BasePath => Path.GetDirectoryName(MelonAssembly.Location);

        public readonly static Dictionary<FileInfo, AudioClip> allLoadedAudioClips = new();

        public override void OnLateInitializeMelon()
        {
            try
            {
                base.HarmonyInstance.PatchAll(typeof(PatchList));
            }
            catch (HarmonyException ex)
            {
                MelonLogger.Error((ex.InnerException ?? ex).Message);
            }
            ExtractAudioNames();
            MelonCoroutines.Start(LoadAndOverridesClip());
        }

        public void ExtractAudioNames()
        {
            if (File.Exists(Path.Combine(BasePath, "AudioNames.txt")))
            {
                return;
            }
            var audioClipNameBuilder = new StringBuilder();
            audioClipNameBuilder.AppendLine("SFX/Music:");
            foreach (var audioClip in allAudioClip.Value.OrderBy(x => x.name))
            {
                audioClipNameBuilder.Append("- ").AppendLine(audioClip.name);
            }

            audioClipNameBuilder.AppendLine().AppendLine("MenuButton SFX:");
            var sfxController = GameObject.FindObjectOfType<SfxController>(true);
            if (sfxController != null)
            {
                for (int i = 0; i < sfxController.clickSfxs.Count; i++)
                {
                    var clickSfx = sfxController.clickSfxs[i];
                    audioClipNameBuilder.Append("- ").AppendLine($"MenuButton{i}");
                }
            }
            File.WriteAllText(Path.Combine(BasePath, "AudioNames.txt"), audioClipNameBuilder.ToString());
        }

        public IEnumerator LoadAndOverridesClip()
        {
            var allowedExtensions = new string[]
            {
                ".MP3",
                ".OGG",
                ".WAV"
            };

            DirectoryInfo audioFolder = Directory.CreateDirectory(Path.Combine(BasePath, "Audio"));
            foreach (var audioFile in Directory.EnumerateFiles(audioFolder.FullName, "*.*", SearchOption.AllDirectories)
                                               .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToUpperInvariant())))
            {
                yield return Utility.LoadAudioClip(audioFile, delegate (AudioClip? clip)
                {
                    if (clip != null)
                    {
                        allLoadedAudioClips[new FileInfo(audioFile)] = clip;
                    }
                });
            }
            ReplaceClip();
        }

        public void ReplaceClip()
        {
            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.TryGetComponent(out MusicController musicController))
                {
                    if (Utility.TryGetLoadedClip(musicController.contentCreatorMusic.name, out AudioClip clip))
                    {
                        musicController.contentCreatorMusic = clip;
                    }
                    if (Utility.TryGetLoadedClip(musicController.source.clip.name, out clip))
                    {
                        musicController.source.clip = clip;
                    }
                    if (Utility.TryGetLoadedClip(musicController.menuSource.clip.name, out clip))
                    {
                        musicController.menuSource.clip = clip;
                    }
                    if (Utility.TryGetLoadedClip(musicController.killingSource.clip.name, out clip))
                    {
                        musicController.killingSource.clip = clip;
                    }
                    musicController.currentSource?.Play();
                }
                if (rootGameObject.TryGetComponent(out SfxController sfxController))
                {
                    for (int i = 0; i < sfxController.clickSfxs.Count; i++)
                    {
                        var sfx = sfxController.clickSfxs[i];
                        if (Utility.TryGetLoadedClip($"MenuButton{i}", out AudioClip clip))
                            sfxController.clickSfxs[i] = clip;
                    }
                }
            }
        }
    }
}
