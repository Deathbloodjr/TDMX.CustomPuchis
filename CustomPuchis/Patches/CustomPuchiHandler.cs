using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPuchis.Patches
{
    class CustomPuchiHandler : MonoBehaviour
    {
        bool isSprite1;
        public Sprite sprite1;
        public Sprite sprite2;

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
            isSprite1 = false;
            LoadSprites();
        }

        void LoadSprites()
        {
            sprite1 = SpriteUtility.LoadSprite(Path.Combine("BepInEx", "data", "TestingMods", "CustomPuchi", "Puchi1.png"));
            sprite2 = SpriteUtility.LoadSprite(Path.Combine("BepInEx", "data", "TestingMods", "CustomPuchi", "Puchi2.png"));
        }

        void LateUpdate()
        {
            // The Animator just forced the game's original sprite here
            Sprite activeGameSprite = image.sprite;

            // Intercept and swap it based on its identity
            if (activeGameSprite != previousSprite)
            {
                previousSprite = activeGameSprite;
                isSprite1 = !isSprite1;
                //ModLogger.Log("Changing sprite to Sprite" + (isSprite1 ? 1 : 2));
                // This log was happening way more often than the sprite was changing, so that's odd
            }

            // You always need to set the sprite
            if (isSprite1)
            {
                image.sprite = sprite1;
            }
            else
            {
                image.sprite = sprite2;
            }
        }
    }
}
