using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppInterop.Runtime;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppSystem;
using Il2CppUI;
using Il2CppUI.Dialog;
using Il2CppUI.Shop;
using Il2CppUI.Think;
using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Il2Cpp.FieldSkillTreeData;
using static Il2CppCustomComponents.LinedRectGraphic.LinedRect;
using static MelonLoader.MelonLogger;

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
        //Skill Save data
        //----------------------------------------------------------------------------------------------------------------

        //Only called by OnRewardGet according to Ghidra
        [HarmonyPrefix]
        [HarmonyPatch("SetIsSkill")]
        public static bool PreSetIsSkill(SaveDataSkill __instance, Skill.ELabel inName, bool inIsSet)
        {
            if(inName == Skill.ELabel.Invalid)
            {
                return false;
            }
            int mSaveIndex = MasterDataBase<SkillTree>.Get((int)inName).mSaveIndex;
            __instance.SetIsSkillData(mSaveIndex, inIsSet);
            return false;
        }

        public static bool easyDifficultyException = false;
        [HarmonyPrefix]
        [HarmonyPatch("SetIsSkillData")]
        public static bool PreSetIsSkillData(ref int inIndex, bool inIsSet)
        {
            bool normalCodeExecution = true;
            if(inIndex >= 0 && !easyDifficultyException)
            {
                GameLocationID gameLocationID = new GameLocationID(inIndex);
                ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);
                if (!archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) { return true; }

                //MelonLogger.Msg($"SetIsSkillData Archipelago {item.locationName} {inIndex} ");
                Core.save.enqueueLocation(archipelagoLocationID);
                normalCodeExecution = false;
            }
            else
            {
                inIndex = inIndex * -1;
                //MelonLogger.Msg($"Normal call; proceed with setting skill for {inIndex}!");
                normalCodeExecution = true;
            }
            Skill skill = MasterDataBase<Skill>.Get(inIndex);
            if(skill.ShopReward > 0)
            {
                //MelonLogger.Msg($"Container skill {(Skill.ELabel)skill.Id} executing normally");
                normalCodeExecution = true;
            }
            easyDifficultyException = false;



            return normalCodeExecution;


        }

        public static void SetEasyDifficultySkill(SaveDataSkill saveData)
        {
            easyDifficultyException = true;
            saveData.SetIsSkillData(0, true);
        }

        /**
            Incoming References - SaveDataSkill$$GetIsSkill
            TrophyManager$$CheckAllSkillReleased        -> Checks for Items?
            SkillUtility$$GetBadgeDeckNum               -> Checks for Item?
            SkillUtility$$GetBadgeGodEquipNum           -> Checks for Item?
            SkillUtility$$GetBadgeSameEquipNum          -> Checks for Item?
            SkillUtility$$GetLevelPullNoiseSymbol       -> Checks for Item?
            SkillUtility$$GetMoneyMax                   -> Checks for Item?
            SkillUtility$$GetSkillParameter             -> Used in several other methods in SkillUtility -> Intuition? Checks for item! Is Correct
            SkillUtility$$IsAreaPigNoise                -> Checks for Item?
            SkillUtility$$IsBadgeBulkSale               -> Checks for Item?
            SkillUtility$$IsBadgeEvolution              -> Checks for Item?
            SkillUtility$$IsBlowCanceler                -> Checks for Item?
            SkillUtility$$IsChangeBadgeRetry            -> Checks for Item?
            SkillUtility$$IsChapterSelectPigNoise       -> Checks for Item?
            SkillUtility$$IsDifficultyEasy              -> Checks for Item?
            SkillUtility$$IsDifficultyHard              -> Checks for Item?
            SkillUtility$$IsDifficultyUltimate          -> Checks for Item?
            SkillUtility$$IsDispEnemyHP                 -> Checks for Item?
            SkillUtility$$IsDodgeRollUp                 -> Checks for Item?
            SkillUtility$$IsEnableBossNoise             -> Checks for Item?
            SkillUtility$$IsFashionLeaveCoordinate      -> Checks for Item?
            SkillUtility$$IsMoneyAutoCharge             -> Checks for Item?
            SkillUtility$$IsOneHandBold                 -> Checks for Item?
            SkillUtility$$IsShopItemRelease             -> Checks for Item?
            SkillUtility$$IsSynchroBonusArea            -> Checks for Item?
            FieldSkillTreeData$$GetSkillStatus          -> Used for other methods?
            SaveDataSkill$$BrandQuestClear              -> Usually checks if skill is gotten to skip other checks -> Incorporate into relationship patch
            SaveDataSkill$$GetCompRate                  -> Checks for Item?
            SaveDataSkill$$PurchaseQuestClear           -> Usually checks if skill is gotten to skip other checks -> Incorporate into relationship patch

        ******************************************
        Incoming References - FieldSkillTreeData$$GetSkillStatus
        UI.Common.CollectionRateUI$$GetCompRate             -> Should check for item?
        UI.UISphere.CharaInfo$$SetFrame                     -> Usually checks if skill is gotten to skip other checks -> Incorporate into relationship patch
        UI.UISphere.Cursor$$ButtonGuideUpdate               -> Needs to check for location instead of item?
        UI.UIMap.EventInfoData$$SetIcon                     -> Needs to check for location instead of item?
        UI.UISphere$$UpdateInfoPanel                        -> Usually checks if skill is gotten to skip other checks? -> Incorporate into relationship patch
        UI.Report.ChapterScrollObject$$SetParam             -> Needs to check for location instead of item?
        UI.Report.UIChapter.SubMissionInfo$$UpdateParam     -> Needs to check for location instead of item?
        *************************************
        Additional things to take a look at
        UI.UISphere$$SequenceRewardItemGet                  -> Handles items from skills -> Prefix to trigger our item workflow if archipelago item?
        //UI.UISphere$$SequenceSkillGetEffect Changes state to  UI.UISphere$$SequenceRewardItemGet and also can call DialogGetUI$$OpenRewardInfoDialog
        DialogGetUI$$OpenRewardInfoDialog                   -> Prevent the "original" pop-up here?
        -> But also call it later in our custom skill stuff!
         * **/
    }

    //----------------------------------------------------------------------------------------------------------------
    //Reward handling
    //---------------------------------------------------------------------------------------------------------------
    [HarmonyPatch(typeof(UISphere))]
    public static class SphereRewardGet
    {

        public static void CustomRewardGet(QueuedReward reward )
        {
            //MelonLogger.Msg($"CustomRewardGet call for {reward.itemName}");
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();
            Skill.ELabel inSkill = (Skill.ELabel)reward.GameID();
            if (NEOTwewyDataManager.item_data.TryGetValue(reward.name, out NEOTwewyItemData itemData))
            {
                int i = 0;
                while (saveData.GetIsSkill(inSkill) && i < itemData.sameSkill.Length)
                {
                    inSkill = (Skill.ELabel)itemData.sameSkill[i];
                }
            }


            Skill skill = MasterDataBase<Skill>.Get((int)inSkill);
            //MelonLogger.Msg($"Received {TextManager.GetText(skill.Name)} {inSkill}!");

            if(inSkill == 0)
            {
                SaveDataSkillPatch2.SetEasyDifficultySkill(saveData);
            } else
            {
                saveData.SetIsSkillData(-1 * (int)inSkill, true);
            }
                
            TrophyManager.Instance.CheckAllSkillReleased();


            Core.archiItemDisplayQueue.Enqueue(reward);

        }

        

        [HarmonyPostfix]
        [HarmonyPatch("UpdateInfoPanel")]
        public static void PostUpdateInfoPanel(UISphere __instance)
        {
            UISphere.CharaInfo selectedCharacter = __instance.mSelectPanel.Chara;
            SkillTree skillTree = MasterDataBase<SkillTree>.Get((int)selectedCharacter.ID);
            Skill skill = MasterDataBase<Skill>.Get((int)skillTree.Skill);

            GameLocationID gameLocationID = new GameLocationID((int)skill.Id);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);

            if (!archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) { return; }
            else
            {
                FieldSkillTreeData.ESkillStatus skillStatus = FieldCommonData.m_FieldSkillTreeData.GetSkillStatus(selectedCharacter.ID);
                //foreach(Il2CppSystem.Collections.Generic.KeyValuePair<string, Il2CppUI.Utility.UIText> pair in __instance.mUITextList)
                //{
                //    MelonLogger.Msg($"[{selectedCharacter.ID}] {pair.Key}: {pair.Value.Text.CurrentText}");
                //}

                if (ModOptions.skill_hints)
                {
                    archipelagoLocationID.CreateHint();
                }

                if (item.itemType == ItemType.Skill)
                {
                    Skill rewardSkill = MasterDataBase<Skill>.Get((int)item.id);
                    if (rewardSkill.Name == "SKL_Name_0033")
                    {//For "Bon Appetit" we should get the shops name
                        foreach (Shop shop in MasterDataBase<Shop>.Array)
                        {
                            if (shop.Skill == (Skill.ELabel)skill.Id)
                            {
                                __instance.SetText("SkillName", $"{TextManager.GetText(skill.Name)} ({TextManager.GetText(shop.Name)})");
                                __instance.SetText("SkillDetails", TextManager.GetText(rewardSkill.Info));
                                break;
                            }
                        }
                    } else
                    {
                        __instance.SetText("SkillName", item.name);
                        __instance.SetText("SkillDetails", TextManager.GetText(rewardSkill.Info));
                    }
                } else if(!item.IsItemFromOurSlot())
                {
                    if(item.IsProgression())
                    {
                        __instance.SetText("SkillDetails", $"'{item.name}' from {item.player}'s {item.itemGame} (Important)");
                    } else
                    {
                        __instance.SetText("SkillDetails", $"'{item.name}' from {item.player}'s {item.itemGame}");
                    }
                        __instance.SetText("SkillName", "Item");
                    
                } else
                { //Bon Appetit Disctinctions
                    __instance.SetText("SkillName", "Item");
                    __instance.SetText("SkillDetails", item.name);
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("SequenceRewardItemGet")]
        public static void PreSequenceRewardItemGetl(UISphere __instance)
        {
            UISphere.CharaInfo selectedCharacter = __instance.mSelectPanel.Chara;
            SkillTree skillTree = MasterDataBase<SkillTree>.Get((int)selectedCharacter.ID);
            Skill skill = MasterDataBase<Skill>.Get((int)skillTree.Skill);

            GameLocationID gameLocationID = new GameLocationID((int)skill.Id);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();
            if (archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) {
                if(item.itemType == ItemType.OtherGame || item.count > 0)
                {
                    PatchDialogGetUI.currentItem = item;
                } 
            }
            
        }
    }
    
    [HarmonyPatch(typeof(DialogUI))]
    public static class SpendFPTextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetMainTextID")]
        public static void PostSetMainTextID(DialogUI __instance, ref string textID)
        {
            if (textID == "SysMsg_SphereSkill")
            {
                __instance.SetText("Main", "Spend FP to unlock this location?" );
            }
        }
    }

    [HarmonyPatch(typeof(DialogGetUI))]
    public static class SkillDialogGet
    {
        private static ArchipelagoItem currentSkill = null;
        public static bool showLocation = false;

        public static void CustomOpenRewardInfoDialog(ArchipelagoItem skill, Il2CppSystem.Action onUIEndEvent, Il2CppSystem.Action<DialogGetUI> onEnded)
        {
            currentSkill = skill; 
            showLocation = false;
            DialogGetUI.OpenRewardInfoDialog(
                    (Skill.ELabel)skill.id,
                    onUIEndEvent,
                    onEnded
            );
        }

        [HarmonyPrefix]
        [HarmonyPatch("OpenRewardInfoDialog")]
        public static void PreOpenRewardInfoDialog(DialogGetUI __instance, Skill.ELabel skillID, Il2CppSystem.Action onUIEndEvent, Il2CppSystem.Action<DialogGetUI> onEnded)
        { 
            if (currentSkill == null)
            {//This if the case when done from "Menu"
                //MelonLogger.Msg("PreOpenRewardInfoDialog Archipelago");
                GameLocationID gameLocationID = new GameLocationID((long)skillID);
                ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);
                if (archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) {
                    currentSkill = item;
                    showLocation = true;
                }
                
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        public static void PostInitialize(DialogGetUI __instance)
        {
            //MelonLogger.Msg($"Initializing for {(long)__instance.SkillId}"); 
            if (currentSkill != null && showLocation)
            {
                //MelonLogger.Msg($"Changing text for {currentSkill.locationName}"); 
                __instance.SetMainText(currentSkill.locationName);
                if (currentSkill.IsItemFromOurSlot())
                {
                    __instance.SetSubText($"Found {currentSkill.name}!");
                } else
                {
                    __instance.SetSubText($"Sent {currentSkill.player}'s {currentSkill.name}!");
                }
                    
                __instance.mItemImage.gameObject.SetActive(false);
            } else if(currentSkill != null)
            {
                Skill skill = MasterDataBase<Skill>.Get((int)currentSkill.id);
                if (skill.Name == "SKL_Name_0033")
                {//For "Bon Appetit" we should get the shops name
                    foreach (Shop shop in MasterDataBase<Shop>.Array)
                    {
                        if (shop.Skill == (Skill.ELabel)skill.Id)
                        {
                            __instance.SetText("Main", $"{TextManager.GetText(skill.Name)} ({TextManager.GetText(shop.Name)})");
                            break;
                        }
                    }
                } else
                {
                    __instance.SetMainTextID(skill.Name);
                }
                __instance.SetSubTextID(skill.Info);
            }

            //MelonLogger.Msg($"No current item detected");

            showLocation = false;
            currentSkill = null;
        }

    }

    [HarmonyPatch(typeof(FieldSkillTreeData))]
    public static class GetSkillStatusPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetSkillStatus")]
        public static bool PreGetSkillStatus(FieldSkillTreeData __instance, SkillTree.ELabel inSkillTreeID, ref FieldSkillTreeData.ESkillStatus __result)
        {
            //MelonLogger.Msg($"Called PreGetSkillStatus for {inSkillTreeID}");

            SkillTree skillTree = MasterDataBase<SkillTree>.Get((int)inSkillTreeID);
            Skill skill = MasterDataBase<Skill>.Get((int)skillTree.Skill);
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();

            //Get Skill Location
            GameLocationID gameLocationID = new GameLocationID(skill.Id);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.Skill);
            //Skip custom code if location is not relevant
            if (!archipelagoLocationID.TryGetItem(out ArchipelagoItem item)) {
                //MelonLogger.Msg($"Cannot find location {archipelagoLocationID}");
                return true; }

            if (Core.save.IsLocationChecked(archipelagoLocationID))
            {//If location is checked we don't need to be able to buy it again!
                __result = FieldSkillTreeData.ESkillStatus.Enable;
                return false;
            } else if (saveData.GetIsSkill((Skill.ELabel)skill.Id))
            { //If we have the skill, we still don't want Enable if location is not done!
                //MelonLogger.Msg($"Called PreGetSkillStatus for {inSkillTreeID} SkilLGotten");
                FieldSkillTreeData.ELineStatus lineStatus = __instance.GetLineStatus(inSkillTreeID,false);
                //Mirroring real behaviour of this function after "ignoring" the GetIsSkill check
                if(lineStatus == FieldSkillTreeData.ELineStatus.SkillOpen)
                {
                    __result = FieldSkillTreeData.ESkillStatus.NotRelease;
                } else
                {
                    __result = FieldSkillTreeData.ESkillStatus.Disable;
                }
                return false;
            }
            
            return true;

        }
    }

}
