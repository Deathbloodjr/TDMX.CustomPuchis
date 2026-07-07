using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPuchis.Patches
{
    internal class SpriteUtility
    {
        private static Dictionary<string, Sprite> LoadedSprites = new Dictionary<string, Sprite>();

        public static Sprite LoadSprite(string filePath)
        {
            if (LoadedSprites.ContainsKey(filePath))
            {
                var sprite = LoadedSprites[filePath];
                if (sprite == null)
                {
                    LoadedSprites.Remove(filePath);
                    return LoadSprite(filePath);
                }
                else
                {
                    return sprite;
                }
            }
            if (!File.Exists(filePath))
            {
                ModLogger.Log($"File path not found: {filePath}", LogType.Error);
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(fileData))
            {
                ModLogger.Log($"Failed to load texture data from bytes: {filePath}", LogType.Error);
                return null;
            }

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float pixelsPerUnit = 100f;

            Sprite customSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
            LoadedSprites.Add(filePath, customSprite);
            return customSprite;
        }
    }
}
