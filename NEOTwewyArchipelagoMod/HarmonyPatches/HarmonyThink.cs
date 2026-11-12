using HarmonyLib;
using Il2Cpp;
using Il2CppMaster;
using Il2CppUI.Think;

namespace NEOTwewyArchipelagoMod.HarmonyPatches
{
    [HarmonyPatch(typeof(ThinkingBoardValueObject))]
    public static class PatchThinkingBoardValueObject
    {
        public static bool __thinking_initialized = false;

        public static List<int> first_boards = new List<int>
        {
            1001010, //Day 0 start board
            1101010, //Day 1 start board
            1201010, //Day 2 start board
            1301010,
            1401010,
            1501010,
            1601010,
            1701010
        };

        [HarmonyPostfix]
        [HarmonyPatch("UpdateBoard")]
        public static void UpdateBoard(ThinkingBoardValueObject __instance)
        {
            if (!__thinking_initialized)
            {
                var thinkArray = MasterDataBase<BoardData>.Array;
                foreach (BoardData boardData in thinkArray)
                {
                    if (first_boards.Contains((int)boardData.Id))
                    { 
                        //MelonLogger.Msg("Day 0 start board flag change");
                        boardData.mOpenScenarioFlag[0] = Scenario.EName.System_EnableMenu_Badge;
                        boardData.mCheckScenarioFlag[0] = Scenario.EName.System_EnableMenu_Badge;
                        boardData.mGrayoutScenarioFlag[0] = Scenario.EName.System_EnableMenu_Badge;
                    }
                }
                __thinking_initialized = true;
            }

            //MelonLogger.Msg($"Thinking board updated: {__instance.mBoardData.Id} - {__instance.mText.text}");
            if (first_boards.Contains((int)__instance.mBoardData.Id))
            {
                //string finalString = "Unchecked locations: \n";
                //List<ArchipelagoItem> missingLocations = Core.save.GetMissingLocations();
                //foreach (ArchipelagoItem item in missingLocations)
                //{
                //    finalString += $"- {item.locationName} \n";
                //}

                //__instance.mText.text = finalString;

                string finalString = "Obtained Skills: \n";
                foreach (Skill skill in MasterDataBase<Skill>.Array)
                {
                    SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();
                    if (saveData.GetIsSkill((Skill.ELabel)skill.Id))
                    {
                        if(skill.Name == "Sphere_Reward_Item")
                        {
                            AllItems allItem = MasterDataBase<AllItems>.Get((int)skill.ShopReward);
                            finalString += $"- {TextManager.GetText(skill.Name)} {TextManager.GetText(allItem.Name)} \n";
                        } else if (skill.Name == "SKL_Name_0033")
                        { //For "Bon Appetit" we should get the shops name
                            foreach (Shop shop in MasterDataBase<Shop>.Array)
                            {
                                if(shop.Skill == (Skill.ELabel)skill.Id){
                                    finalString += $"- {TextManager.GetText(skill.Name)} ({TextManager.GetText(shop.Name)}) \n";
                                    break;
                                }
                            }

                        } else
                        {
                            finalString += $"- {TextManager.GetText(skill.Name)} \n";
                        }
                            
                    }
                    
                }
                __instance.mText.text = finalString;
            }
        }
    }
}
