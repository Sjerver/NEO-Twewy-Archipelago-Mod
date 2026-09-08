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
    [HarmonyPatch(typeof(SaveDataSkill))]
    public static class SaveDataSkillPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetIsSkill")]
        public static void PreSetIsSkill(Skill.ELabel inName, bool inIsSet)
        {
            if (inIsSet)
            {
                //TODO: Here we can write is skill set logic!
                //Also maybe this one? SetIsSkillData
            }
        }
    }

    //FieldSkillTreeData.GetSkillStatus will likely need to be patched to check locations to know if the status of a node
}
