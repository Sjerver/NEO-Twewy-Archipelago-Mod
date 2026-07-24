using Il2Cpp;
using Il2CppMaster;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Il2CppMaster.ItemConst;

namespace NEOTwewyArchipelagoMod
{
    public class MyScenarioReward
    {
        public int mID { get; set; }
        public int mReward1st { get; set; }
        public int mReward1stCount { get; set; }
        public int mReward2nd {  get; set; }
        public int mReward2ndCount { get; set; }

        public int mSaveIndex { get; set; }

        public MyScenarioReward(int id, int itemId, int itemCount, int item2Id = -1, int itemCount2 = 0, int saveIndex = 251 /* From the TestReward*/) {
            mID = id; mReward1st = itemId; mReward1stCount = itemCount; mReward2nd = item2Id; mReward2ndCount = itemCount2; mSaveIndex =  saveIndex;          
        }


        public JObject getJSON()
        {
            return new JObject
            {
                ["mId"] = mID,
                ["mReward1st"] = mReward1st,
                ["mReward1stCount"] = mReward1stCount,
                ["mReward2nd"] = mReward2nd,
                ["mReward2ndCount"] = mReward2ndCount,
                ["mSaveIndex"] = mSaveIndex 
            };
        }

    }

    public class MyBadge
    {
        //mId mItemId mBrand  mNameChance mInfoChance mPsychic mPsychicKey mAttack mAddAttack  mMaxValue mAddMaxValue    mHoldBasicCost mChargeTime mComboCount mRebootTime mRebootTimeDec mBootTime   mBootTimeDec mAutoRecoverTime    mAutoRecoverTimeDec mMaxLevel   mLevelUpType mLevelUpRate    mAbility mPairAbility    mComboDamage mAddSellPrice   mSellPrice mRarity mSortIndex mSortPsychic    mBadgeSpriteName mBadgeSpriteAtlas   mBadgeClass mBadgeCategory  mBadgePsychicType mEvolutionLevel mEcolutionCommon mEvolutionBadge mChancetimeType mMashupElement  mInfoMovie

        public int mID { get; set; }
        public int mItemId { get; set; }
        public int mBrand {  get; set; }
        public string mNameChance { get; set; }
        public string mInfoChance {  get; set; }
        public int mPsychic {  get; set; }
        public int mPsychicKey { get; set; }
        public int mAttack {  get; set; }
        public int mAddAttack { get; set; }
        public int mMaxValue { get; set;}
        public int mAddMaxValue { get; set;}
        public int mHoldBasicCost { get; set; }
        public int mChargeTime { get; set; }
        public int mComboCount { get; set; }
        public float mRebootTime { get; set; }
        public int mRebootTimeDec {  get; set; }
        public int mBootTime { get; set; }
        public int mBootTimeDec { get;set; }
        public float mAutoRecoverTime { get; set; }
        public int mAutoRecoverTimeDec { get; set; }
        public int mMaxLevel { get; set; }
        public int mLevelUpType { get; set; }
        public float mLevelUpRate { get; set; }
        public Array mAbility {  get; set; }
        public int mPairAbility { get; set; }
        public int mComboDamage { get; set; }
        public int mAddSellPrice { get; set; }
        public int mSellPrice { get; set; }
        public int mRarity { get; set; }
        public int mSortIndex { get; set; }
        public int mSortPsychic { get; set; }
        public string mBadgeSpriteName { get; set; }
        public int mBadgeSpriteAtlas { get; set; }
        public int mBadgeClass { get; set; }
        public int mBadgeCategory { get; set; }
        public int mBadgePsychicType { get; set; }
        public int mEvolutionlevel { get; set; }
        public int mEvolutionCommon {  get; set; }
        public Array mEvolutionBadge { get; set; }
        public int mChancetimeType {  get; set; }
        public int mMashupElement { get; set; }
        public int mInfoMovie { get; set; }


        public MyBadge()
        // Currently only used to make an Archipelago badge
        {
            mID = 99;
            mItemId = 999;
            mBrand = 0;
            mNameChance = "Chance_Name_0000";
            mInfoChance = "";
        }

    }

    public class MySkill
    {
        public int mID { get; set; }
        public string mName { get; set; } = "Sphere_Reward_Item";
        public string mInfo { get; set; } = "Com_ItemName";
        public int mPoint { get; set; }
        public float[] mParameter { get; set; }
        public int mShopReward { get; set; }
        public string mDialogImage { get; set; } = "";
        public int mSaveIndex { get; set; } = 100;


        public MySkill(int id, int point, int shopReward, int saveIndex)
        {
            mID = id; mPoint = point; mShopReward = shopReward; mSaveIndex = saveIndex;
            mParameter = [0.0f, 0.0f];

        }

        public JObject getJSON()
        {


            return new JObject
            {
                ["mId"] = mID,
                ["mName"] = mName,
                ["mInfo"] = mInfo,
                ["mPoint"] = mPoint,
                ["mParameter"] = JArray.FromObject(mParameter),
                ["mShopReward"] = mShopReward,
                ["mDialogImage"] = mDialogImage,
                ["mSaveIndex"] = mSaveIndex,
            };
        }
    }

}
