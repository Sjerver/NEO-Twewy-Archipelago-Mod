using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppInterop.Runtime;
using Il2CppMaster;
using Il2CppUI.MainMenu.Badge;
using Il2CppUI.Panel;
using Il2CppUI.Utility;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppUI.Battle.BattleResultBadgeController;
using static MelonLoader.MelonLogger;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(UISpriteBank), nameof(UISpriteBank.GetSprite))]
    public static class UISpriteBankPatch
    {
        public static UIImageLoaderPersonal CharacterIconLoader = new UIImageLoaderPersonal();

        public static void Postfix(ref string atlasPath, ref string spriteName, ref Sprite __result)
        {
            if (spriteName == "bad_99_00_00")
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

    //----------------------------------------------------------------------------------------------------------------
    // Generic Icon Replacement logic
    //----------------------------------------------------------------------------------------------------------------
    public static class IconReplacement
    {
        public static void SetCustomSprite(ArchipelagoItem item, Action<Sprite> setSprite)
        {
            if (!item.IsItemFromOurSlot() || item.itemType == ItemType.AllItems)
            {
                
                AllItems itemObject = MasterDataBase<AllItems>.Get((int)item.id);

                string atlasPath = UIUtility.GetItemAtlasPath(itemObject);
                string spriteName = UIUtility.GetItemImageName(itemObject);

                //MelonLogger.Msg($"{atlasPath} atlas -> spriteName {spriteName}");

                setSprite(UISpriteBank.Instance.GetSprite(atlasPath, spriteName));
            }
            else if (item.itemType == ItemType.SocialNode)
            {
                string iconTexture = "Graphic_resource/UI/MainMenu/Sphere/icon/" + FieldCommonData.m_FieldSkillTreeData.GetIconTexture((SkillTree.ELabel)item.id);
                if (UISpriteBankPatch.CharacterIconLoader.mDataList.ContainsKey("SphereChara" + item.id))
                {
                    Sprite sprite = UISpriteBankPatch.CharacterIconLoader.GetSprite("SphereChara" + item.id, Vector2.zero);
                    setSprite(sprite);
                }
                else
                {
                    UISpriteBankPatch.CharacterIconLoader.AddLoader("SphereChara" + item.id, iconTexture);
                    UISpriteBankPatch.CharacterIconLoader.Run(
                        DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(() => {
                            Sprite sprite = UISpriteBankPatch.CharacterIconLoader.GetSprite("SphereChara" + item.id, Vector2.zero);

                            setSprite(sprite);
                        })
                    );
                }
            }
            else if (item.itemType == ItemType.VIP)
            {
                string brandAtlasPath = BadgeEquipUI.BrandAtlasPath;
                //string brandSpriteName =  UIUtility.GetSpriteAtlasFileName((Brand.ELabel)item.id);
                string brandString = item.id.ToString("D2");
                string brandSpriteName = $"brand_{brandString}";
                Sprite sprite = UISpriteBank.Instance.GetSprite(brandAtlasPath, brandSpriteName);
                setSprite(sprite);
            } else if (item.itemType == ItemType.Skill)
            {
                byte[] archi_data = File.ReadAllBytes("Mods/NeoTwewyArchipelago/Resources/skill_book.png");

                Texture2D archi_tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(archi_tex, archi_data);

                Sprite sprite = Sprite.Create(
                    archi_tex,
                    new Rect(0, 0, archi_tex.width, archi_tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                setSprite(sprite);
            }
        }
    }

}
