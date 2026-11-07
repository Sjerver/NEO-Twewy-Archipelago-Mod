using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppUI;
using Il2CppUI.Shop;
using Il2CppUI.Think;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2CppCustomComponents.LinedRectGraphic.LinedRect;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(SaveDataSkill))]
    public static class SaveDataSkillPatch2
    {

        //----------------------------------------------------------------------------------------------------------------
        //Points
        //----------------------------------------------------------------------------------------------------------------
        [HarmonyPrefix]
        [HarmonyPatch("AddSkillPoint")]
        public static void PreAddSkillPoint(ref int inAddPoint)
        {
            if (inAddPoint < 0)
            {
                //MelonLogger.Msg("Prevent FP reduction");
                inAddPoint = 0;
            }
        }

        [HarmonyPatch(typeof(UISphere))]
        public static class UISpherePointsPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Initialize")]
            public static void PreInitialize()
            {
                Initialization.InitializeSNSPoints();
            }
        }

        //----------------------------------------------------------------------------------------------------------------
        //Skills
        //----------------------------------------------------------------------------------------------------------------

        [HarmonyPrefix]
        [HarmonyPatch("SetIsSkill")]
        public static void PreSetIsSkill(Skill.ELabel inName, bool inIsSet)
        {
            if (inIsSet)
            {
                //TODO: Here we can write is skill set logic! But maybe in another file for skills themselves...
                //Also maybe this one? SetIsSkillData
            }
        }
    }


    
}
