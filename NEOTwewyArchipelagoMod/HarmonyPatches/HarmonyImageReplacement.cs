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
        public static void Postfix(ref string atlasPath, ref string spriteName,ref Sprite __result)
        {
            if(spriteName == "bad_99_00_00")
            { //Originally "bad_99_00_00" is the 1 Yen Pin
                byte[] archi_data = File.ReadAllBytes("Mods/NeoTwewyArchipelago/Resources/archi_pin.png");

                Texture2D archi_tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(archi_tex, archi_data);

                Sprite archi_sprite = Sprite.Create(
                    archi_tex,
                    new Rect(0, 0, archi_tex.width, archi_tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                __result = archi_sprite;
            }
            
        }
    }
}
