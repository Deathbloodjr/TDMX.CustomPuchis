using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

namespace CustomPuchis.Patches
{
    class CustomPuchiHandler : MonoBehaviour
    {
#if IL2CPP
        static CustomPuchiHandler() => ClassInjector.RegisterTypeInIl2Cpp<CustomPuchiHandler>();
#endif

        int currentIndex = 0;
        List<Sprite> sprites = new List<Sprite>();

        public Sprite previousSprite;

        private Image image;
        void Start()
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                ModLogger.Log("No image found", LogType.Error);
            }

            previousSprite = image.sprite;
        }

        public void Initialize(PuchiMetaData data = null)
        {
            currentIndex = 0;
            sprites.Clear();

            if (data == null)
            {
                return;
            }
            for (int i = 0; i < data.Files.Count; i++)
            {
                sprites.Add(SpriteUtility.LoadSprite(Path.Combine(data.FolderPath, data.Files[i])));
            }
        }

        void LateUpdate()
        {
            if (sprites.Count > 0)
            {
                // The Animator just forced the game's original sprite here
                Sprite activeGameSprite = image.sprite;

                if (sprites.Contains(activeGameSprite))
                {
                    return;
                }

                // Intercept and swap it increase index
                if (activeGameSprite != previousSprite)
                {
                    previousSprite = activeGameSprite;
                    currentIndex = (currentIndex + 1) % sprites.Count;
                }
                image.sprite = sprites[currentIndex];
            }
        }
    }
}
