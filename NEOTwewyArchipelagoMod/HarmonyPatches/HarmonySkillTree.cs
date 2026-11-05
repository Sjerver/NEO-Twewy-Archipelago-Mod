using HarmonyLib;
using Il2Cpp;
using Il2CppHnLib;
using Il2CppMaster;
using Il2CppScenario;
using Il2CppUI;
using Il2CppUI.Shop;
using Il2CppUI.Think;
using Il2CppUI.Utility;
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
    public static class SaveDataSkillPatch
    {
        //----------------------------------------------------------------------------------------------------------------
        //Quests (Brand/Purchase)
        //----------------------------------------------------------------------------------------------------------------

        [HarmonyPrefix]
        [HarmonyPatch("BrandQuestClear")]
        public static bool PreBrandQuestClear(SaveDataSkill __instance, Brand.ELabel inBrand, int inLevel, out SkillTree outSkillTree)
        {
            bool allowNormalCodeExecution = true;
            outSkillTree = null;
            SaveDataShop saveData = SaveLoadController.Get<SaveDataShop>();

            foreach (SkillTree skillTree in MasterDataBase<SkillTree>.Array) {

                if(skillTree.mReleaseValue == 0) { continue; }
                
                GameLocationID gameLocationID = new GameLocationID(skillTree.Id + 1000);
                ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);

                if (!archipelagoLocationID.IsInSeed()) { continue; }

                bool relationshipFound = __instance.GetIsRelationshiptData((int)skillTree.Id);
                
                Shop shop = skillTree.GetShop();
                SaveDataShop.IBrandInfo brandInfo = saveData.GetBrandInfo(inBrand);

                if (shop.mBrand != Brand.ELabel.NoBrand && skillTree.mReleaseValue <= brandInfo.VipLevel)
                {
                    if (!Core.save.IsLocationChecked(archipelagoLocationID))
                    {
                        //Has the corresponding node been unlocked (relationship)?
                        if (relationshipFound)
                        {
                            //Node has been found already, so we need to manually unlock the location since the relationshipMet will not be set again
                            //MelonLogger.Msg("Node has been found already, so we need to manually unlock the location since the relationshipMet will not be set again");
                            __instance.SetIsRelationshipData((int)skillTree.Id, true);
                            allowNormalCodeExecution = false;
                        }
                        else
                        {
                            //Else: Node has not been found yet, so we can continue normal code, since next time location is found instead
                            //MelonLogger.Msg("Node has not been found yet, so we can continue normal code, since next time location is found instead");
                        }
                    }
                    else
                    {
                        //Location is already done, so we cannot continue normal code
                        //MelonLogger.Msg("Location is already done, so we cannot continue normal code");
                        allowNormalCodeExecution = false;
                    }
                }
            }
            return allowNormalCodeExecution;
        }

        [HarmonyPrefix]
        [HarmonyPatch("PurchaseQuestClear")]
        public static bool PrePurchaseQuestClear(SaveDataSkill __instance, Shop.ELabel inShop, int inPurchaseNum, out SkillTree outSkillTree)
        {
            //MelonLogger.Msg($"PurchaseQuestClear called with inShop: {(int)inShop}, inPurchaseNum: {inPurchaseNum}");
            Shop shop = MasterDataBase<Shop>.Get((int)inShop);
            outSkillTree = null;
            bool allowNormalCodeExecution = true;
            //Get Node(s) from Shop custom list

            if (NEOTwewyDataManager.SHOP_TO_NODES.TryGetValue((int)inShop, out List<int> nodeIndices))
            {
                foreach (int node in nodeIndices)
                {
                    //+1000 cause we want to look for the relationship status, not the met status
                    GameLocationID gameLocationID = new GameLocationID(node + 1000);
                    ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);


                    //Quest stuff
                    outSkillTree = MasterDataBase<SkillTree>.Get(node);
                    bool relationshipFound = __instance.GetIsRelationshiptData(node);

                    //We dont actually need to execute the code if the location is not relevant
                    if (!archipelagoLocationID.IsInSeed()) { continue; }

                    //Quest done?
                    if (shop.mBrand == Brand.ELabel.NoBrand && outSkillTree.mReleaseValue <= inPurchaseNum)
                    {
                        if (!Core.save.IsLocationChecked(archipelagoLocationID))
                        {
                            //Has the corresponding node been unlocked (relationship)?
                            if (relationshipFound)
                            {
                                //Node has been found already, so we need to manually unlock the location since the relationshipMet will not be set again
                                //MelonLogger.Msg("Node has been found already, so we need to manually unlock the location since the relationshipMet will not be set again");
                                __instance.SetIsRelationshipData(node, true);
                                allowNormalCodeExecution = false;
                            }
                            else
                            {
                                //Else: Node has not been found yet, so we can continue normal code, since next time location is found instead
                                //MelonLogger.Msg("Node has not been found yet, so we can continue normal code, since next time location is found instead");
                            }
                        }
                        else
                        {
                            //Location is already done, so we cannot continue normal code
                            //MelonLogger.Msg("Location is already done, so we cannot continue normal code");
                            allowNormalCodeExecution = false;
                        }
                    }
                }
            } else
            {
                //MelonLogger.Msg("No nodes found for this shop in SHOP_TO_NODES mapping, proceeding with normal code execution.");
            }
            return allowNormalCodeExecution;
        }

        //----------------------------------------------------------------------------------------------------------------
        //Social Network and Social Network Nodes
        //----------------------------------------------------------------------------------------------------------------
        [HarmonyPrefix]
        [HarmonyPatch("SetIsHaveMet")]
        public static void PreSetIsHaveMet(SkillTree.ELabel inName, bool inIsSet)
        {
            //MelonLogger.Msg($"SetIsHaveMet called with inName: {inName}, inIsSet: {inIsSet}");
            //Calls SetIsHaveMetData
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetIsHaveMetData")]
        public static bool PreSetIsHaveMetData(int inIndex, bool inIsSet, SkillTree inSkillTree)
        {
            //We want to execute real code if we cant find location in seed
            GameLocationID gameLocationID = new GameLocationID(inIndex);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);
            if (!archipelagoLocationID.IsInSeed()) { return true; }

            if (FieldManagerSkillPatch.currentItem >= 0)
            { //An Item currently stored, that we want to give without triggering archipelago
                FieldManagerSkillPatch.currentItem = -1;
                //MelonLogger.Msg($"SetIsHaveMetData proceeding for {inSkillTree.mCharaName}");
                return true;
            } else
            {
                //This is a real call by the game we need to intercept and use as "location"
                
                //MelonLogger.Msg($"SetIsHaveMetData called with inIndex {inIndex} for name: {archipelagoLocationID.GetName()} ({(SkillTree.ELabel)inIndex}), inIsSet: {inIsSet}");
                Core.save.enqueueLocation(archipelagoLocationID);
            }
            return false;
        }


        //These ones are responsible for the "connected"/blue status of nodes
        [HarmonyPrefix]
        [HarmonyPatch("SetIsRelationship")]
        public static bool PreSetIsRelationship(SaveDataSkill __instance, SkillTree.ELabel inName, bool inIsSet)
        {
            int mSaveIndex = MasterDataBase<SkillTree>.Get((int)inName).mSaveIndex;
            __instance.SetIsRelationshipData(mSaveIndex, inIsSet);
            //MelonLogger.Msg($"SetIsRelationship called with inName: {inName}, inIsSet: {inIsSet}");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetIsRelationshipData")]
        public static bool SetIsRelationshipData(int inIndex, bool inIsSet)
        {
            //We want to execute real code if we cant find location in seed
            GameLocationID gameLocationID = new GameLocationID(inIndex + 1000);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);
            if (!archipelagoLocationID.IsInSeed()) { return true; }

            if (FieldManagerSkillPatch.currentItem >= 0)
            { //An Item currently stored, that we want to give without triggering archipelago
                FieldManagerSkillPatch.currentItem = -1;

                SkillTree sk = MasterDataBase<SkillTree>.Get(inIndex);
                //MelonLogger.Msg($"SetIsHaveMetData proceeding for {sk.mCharaName}");
                return true;
            }
            else
            {
                //MelonLogger.Msg($"SetIsRelationshipData called with inIndex {inIndex} for name: {archipelagoLocationID.GetName()} ({(SkillTree.ELabel)inIndex}), inIsSet: {inIsSet}");
                Core.save.enqueueLocation(archipelagoLocationID);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FieldManager))]
    public static class FieldManagerSkillPatch
    {

        //Item of the currently to open skill tree
        public static int currentItem = -1;

        public static void ReserveSkillTreeCustom(int item)
        {
            //MelonLogger.Msg($"Set current item for skill tree to {item}");
            currentItem = item;
            FieldManager.Instance.GetReserveSkillTreeDict((SkillTree.ELabel)item);

            if(currentItem < 0)
            {
                //MelonLogger.Msg($"ReserveSkillTreeCustom: currentItem is not set, no custom saveData modification needed.");
                return;
            }
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();
            SkillTree.ELabel inSkillTree = (SkillTree.ELabel)item;

            //bool isMet = saveData.GetIsHaveMet(inSkillTree);
            //if (isMet)
            //{
            //    saveData.SetIsRelationship(inSkillTree, true);
            //} else
            //{
            //    saveData.SetIsHaveMet(inSkillTree, true);
            //}

            saveData.SetIsRelationship(inSkillTree, true);

            currentItem = -1;

        }

        [HarmonyPrefix]
        [HarmonyPatch("GetReserveSkillTreeDict")]
        public static bool GetReserveSkillTreeDictPrefix(SkillTree.ELabel inSkillTree)
        {
            //MelonLogger.Msg($"GetReserveSkillTreeDictPrefix called with inSkillTree: {inSkillTree}");
            if (currentItem >= 0)
            { //An Item currently stored, that we want to give without triggering archipelago
                //MelonLogger.Msg("GetReserveSkillTreeDictPrefix: currentItem is set, skipping archipelago location enqueue.");
                return true;
            }
            return false;
            
        }
    }
    //----------------------------------------------------------------------------------------------------------------
    //Special conditions for Met/Relationship activation / handling
    //----------------------------------------------------------------------------------------------------------------

    [HarmonyPatch(typeof(ShopStaffController))]
    public static class ShopMeetPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnSceneCreate")]
        public static void OnSceneCreate(ShopStaffController __instance)
        {

            if (NEOTwewyDataManager.SHOP_TO_NODES.TryGetValue((int)__instance.mMasterShop.mId, out List<int> nodeIndices))
            {
                foreach (int node in nodeIndices)
                {
                    //Get the archipelago id for the Met location
                    GameLocationID gameLocationID = new GameLocationID(node);
                    ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);

                    //Skip custom code if location is not relevant
                    if (!archipelagoLocationID.IsInSeed()) { continue; }

                    if (!Core.save.IsLocationChecked(archipelagoLocationID))
                    {//If the location hasn't already been checked check it manually
                        SaveLoadController.Get<SaveDataSkill>().SetIsHaveMet((SkillTree.ELabel)node, true);
                        //MelonLogger.Msg("MANUAL SHOP CHECK!");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ScenarioIndexKindExtensions))]
    public static class AcceptNodeChangePatch
    //Both of these always trigger even if Node is already met / relationship established, which is what we need
    {
        [HarmonyPrefix]
        [HarmonyPatch("Accept_HaveMet")]
        public static bool PreAccept_HaveMet(int index, bool evaluation)
        {
            if (!evaluation) { return true; } //Otherwise they can trigger early

            //MelonLogger.Msg($"Prefix for Accept_HaveMet {index} ({evaluation})");

            GameLocationID gameLocationID = new GameLocationID(index);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);
            
            //Skip custom code if location is not relevant
            if (!archipelagoLocationID.IsInSeed()) { return true; }

            Core.save.enqueueLocation(archipelagoLocationID);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Accept_Relationship")]
        public static bool PreAccept_Relationship(int index, bool evaluation)
        {
            if (!evaluation) { return true; } //Otherwise they can trigger early

            //MelonLogger.Msg($"Prefix for Accept_Relationship {index} ({evaluation})");
            
            GameLocationID gameLocationID = new GameLocationID(index + 1000);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);

            //Skip custom code if location is not relevant
            if (!archipelagoLocationID.IsInSeed()) { return true; }

            Core.save.enqueueLocation(archipelagoLocationID);

            return false;
        }
    }

    //----------------------------------------------------------------------------------------------------------------
    //UI Stuff
    //----------------------------------------------------------------------------------------------------------------

    [HarmonyPatch(typeof(FieldSkillTreeData))]
    public static class FieldSkillTreeDataPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetLineStatus")]
        public static bool PreGetLineStatus(SkillTree.ELabel inSkillTreeID,ref FieldSkillTreeData.ELineStatus __result, bool inCurrentDayStatus = false)
        {
            //Get Met Location
            GameLocationID gameLocationID = new GameLocationID((int)inSkillTreeID + 1000);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);

            //Skip custom code if location is not relevant
            if (!archipelagoLocationID.IsInSeed()) { return true; }

            SkillTree skillTree = MasterDataBase<SkillTree>.Get((int)inSkillTreeID);
            
            if((int)skillTree.ConnectDay > Core.furthestDayReached)
            { //Should not be connected, therfore we set it custom so it ignores the relationship that may be established
                __result = FieldSkillTreeData.ELineStatus.NotConnect;
                return false;
            } else
            {
                return true;
            }     
        }
    }



    [HarmonyPatch(typeof(UISphere.CharaInfo))]
    public static class UISphereCharaInfoPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetFrame")]
        public static bool PreSetFrame(UISphere.CharaInfo __instance)
        {
            FieldSkillTreeData.ELineStatus lineStatus = FieldCommonData.m_FieldSkillTreeData.GetLineStatus(__instance.ID, true);
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();

            if (lineStatus == FieldSkillTreeData.ELineStatus.NotConnect && saveData.GetIsRelationship(__instance.ID))
            {//Not connected but relationship unlocked
                __instance.mFrameAnim.Play(UISphere.CharaInfo.AnimFrameGrayGlow);
                __instance.mSkillPointTxt.text = "";
                return false;
            }
            return true;
        }

    }

    [HarmonyPatch(typeof(UISphere))]
    public static class UISpherePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateInfoPanel")]
        public static void PostUpdateInfoPanel(UISphere __instance)
        {
            UISphere.CharaInfo selectedCharacter = __instance.mSelectPanel.Chara;
            GameLocationID gameLocationID = new GameLocationID((int)selectedCharacter.ID + 1000);
            ArchipelagoLocationID archipelagoLocationID = gameLocationID.ToArchipelagoLocation(LocationType.SocialNode);
            SaveDataSkill saveData = SaveLoadController.Get<SaveDataSkill>();
            
            if (!archipelagoLocationID.IsInSeed()) { return; }

            string eventType = __instance.GetUIText("Events").Text.CurrentText;
            if (!Core.save.IsLocationChecked(archipelagoLocationID)){
                //Relationship is unlocked but location or "Quest" not yet checked, therefore the current conditions status should be displayed
                if (eventType == "QUEST")
                {
                    //Show condition if not yet completed
                    __instance.GetUIText("EventNowNum").SetTextActive(true);
                } else if (eventType == "TIMEFRAME")
                {
                    __instance.SetText("EventNowNum","Unchecked");
                    __instance.GetUIText("EventNowNum").SetTextActive(true);
                } else
                {
                    MelonLogger.Error($"Unknown eventType: {eventType} for character {selectedCharacter.ID}");
                }
            } else if (Core.save.IsLocationChecked(archipelagoLocationID))
            { //Cleared the location but relationship not unlocked
                __instance.SetText("EventNowNum", "Checked");
                __instance.GetUIText("EventNowNum").SetTextActive(true);
            }
            
        }

    }
}
