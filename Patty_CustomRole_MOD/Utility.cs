using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace Patty_CustomRole_MOD
{
    public static class Utility
    {
        public static Sprite FindSprite(string fileNameWithoutExt)
        {
            foreach (var (file, sprite) in CustomRole.allLoadedTextures)
            {
                if (Path.GetFileNameWithoutExtension(file.Name).Equals(fileNameWithoutExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    return sprite;
                }
            }
            return null!;
        }

        /// <summary>
        /// Guarantees to find the character data by name, including custom characters.
        /// Unless the character does not exist at all.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CharacterData FindCharacter(string name)
        {
            Func<CharacterData, bool> findFunc = c => c.name == name;
            var charData = ProjectContext.Instance.gameData.allCharacterData.Find(findFunc);
            if (charData == null)
            {
                foreach (var characterData in CustomRole.allCharacterData.Value)
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

        public static SkinData FindSkin(string name)
        {
            foreach (var data in CustomRole.allSkinData.Value)
            {
                if (data.name == name)
                    return data;
            }
            return null!;
        }

        public static Type FindType(string assemblyName, string typeName)
        {
            var result = typeof(CharacterData).Assembly.GetType($"Il2Cpp.{typeName}");
            if (result != null)
            {
                return result;
            }
            foreach (MelonBase melon in MelonBase.RegisteredMelons)
            {
                if (Path.GetFileName(melon.MelonAssembly.Location).Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = melon.MelonAssembly.Assembly.GetType(typeName);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static (Texture2D texture, bool isOriginal) GetReadableImage(this Texture2D texture2D)
        {
            if (texture2D.isReadable)
            {
                return (texture2D, true);
            }

            var tempRT = RenderTexture.GetTemporary(texture2D.width,
                                                    texture2D.height,
                                                    depthBuffer: 0,
                                                    RenderTextureFormat.ARGB32);

            var activeRT = RenderTexture.active;
            Texture2D newTexture = null;

            try
            {
                RenderTexture.active = tempRT;
                Graphics.Blit(texture2D, tempRT);

                newTexture = new Texture2D(texture2D.width,
                                           texture2D.height,
                                           TextureFormat.RGBA32,
                                           mipChain: false);

                newTexture.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                newTexture.Apply();
            }
            finally
            {
                RenderTexture.active = activeRT;
                RenderTexture.ReleaseTemporary(tempRT);
            }

            return (newTexture, false);
        }

        public static void ExtractImage(this Sprite sprite, string folder, bool overrides = false)
        {
            if (sprite == null)
                return;
            var fileName = Path.Combine(folder, $"{sprite.name}.png");
            if (!overrides && File.Exists(fileName))
                return;

            var (texture2d, isOriginal) = sprite.texture.GetReadableImage();
            var texture = new Texture2D(Mathf.CeilToInt(sprite.rect.width), Mathf.CeilToInt(sprite.rect.height), TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            int srcX = Mathf.CeilToInt(sprite.rect.x);
            int srcY = Mathf.CeilToInt(sprite.rect.y);
            int width = Mathf.CeilToInt(sprite.rect.width);
            int height = Mathf.CeilToInt(sprite.rect.height);

            Graphics.CopyTexture(texture2d,
                                 srcElement: 0,
                                 srcMip: 0,
                                 srcX, srcY, width, height,
                                 texture,
                                 dstElement: 0,
                                 dstMip: 0,
                                 dstX: 0, dstY: 0);

            texture.Apply();
            File.WriteAllBytes(fileName, ImageConversion.EncodeToPNG(texture));
            if (!isOriginal)
            {
                UnityEngine.Object.Destroy(texture2d);
            }
        }

        public static Sprite? CreateSprite(FileInfo file)
        {
            var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true);
            texture.filterMode = FilterMode.Trilinear;
            texture.mipMapBias = -1f;
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file.FullName), true))
            {
                MelonLogger.Error(file.FullName + " is not a valid image file.");
                return null;
            }

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.Tight);
            sprite.name = Path.GetFileNameWithoutExtension(file.Name);
            return sprite;
        }
    }
}
