using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Patty_CustomAudio_MOD
{
    public static class Utility
    {
        public static bool TryGetLoadedClip(string fileName, out AudioClip result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }
            foreach (var (fileInfo, audioClip) in CustomAudio.allLoadedAudioClips)
            {
                if (Path.GetFileNameWithoutExtension(fileInfo.Name).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    result = audioClip;
                    return true;
                }
            }
            return false;
        }

        public static AudioType GetAudioType(string filePath)
        {
            AudioType result;
            switch (Path.GetExtension(filePath)?.ToUpperInvariant())
            {
                case ".MP3":
                    result = AudioType.MPEG;
                    break;
                case ".OGG":
                    result = AudioType.OGGVORBIS;
                    break;
                case ".WAV":
                    result = AudioType.WAV;
                    break;
                default:
                    result = AudioType.UNKNOWN;
                    break;
            }
            return result;
        }

        public static IEnumerator LoadAudioClip(string filePath, Action<AudioClip?>? onComplete = null)
        {
            var webRequest = UnityWebRequest.Get($"file://{filePath}");
            yield return webRequest.SendWebRequest();
            var clip = WebRequestWWW.InternalCreateAudioClipUsingDH(webRequest.downloadHandler,
                                                                    webRequest.url,
                                                                    stream: false,
                                                                    compressed: false,
                                                                    GetAudioType(filePath));
            if (clip != null)
            {
                clip.name = Path.GetFileNameWithoutExtension(filePath);
                onComplete?.Invoke(clip);
            }
            else
            {
                MelonLogger.Error($"Failed to load audio clip from path: {filePath}");
                onComplete?.Invoke(null);
            }
        }
    }
}