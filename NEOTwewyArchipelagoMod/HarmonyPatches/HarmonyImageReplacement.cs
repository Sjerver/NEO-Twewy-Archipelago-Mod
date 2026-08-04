using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(UISpriteBank), nameof(UISpriteBank.GetSprite))]
    public static class UISpriteBankPatch
    {
        //TODO: Potentially update to replace the sprite one time
        public static void Postfix(ref string atlasPath, ref string spriteName,ref Sprite __result)
        {
            if(spriteName == "bad_99_00_00")
            { //Originally "bad_99_00_00" is the 1 Yen Pin
                byte[] data = File.ReadAllBytes("Mods/NeoTwewyArchipelago/Resources/archi_pin.png");

                Texture2D tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(tex, data);

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );

                __result = sprite;
            }
            
        }
    }
}
