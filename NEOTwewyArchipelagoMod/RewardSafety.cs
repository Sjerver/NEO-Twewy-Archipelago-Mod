using Il2Cpp;
using MelonLoader;

namespace NEOTwewyArchipelagoMod
{
    public class RewardSafety
    { //TODO: Menus should not be save [misclicks are annoying]
        public bool isScanMode = false;
        public bool isPromptStatus = false;
        public bool IsPreMapChangeBattle = false;
        public bool isBattleStart = false;
        public bool errorFree = true;

        public bool assumeBattle = false;

        public bool mainMenuOpen = false;
        public bool shopMenuOpen = false;
        public bool optionMenuOpen = false;

        public bool getValue()
        {
            return errorFree && !assumeBattle && !isScanMode && isPromptStatus && !IsPreMapChangeBattle && !isBattleStart && !mainMenuOpen && !shopMenuOpen && !optionMenuOpen;
        }

        public bool safeStatusCheck()
        {
            if (assumeBattle) { return false; }
            try
            {
                FieldManager fieldManager = FieldManager.Instance;

                if (fieldManager != null) {

                    bool newBattleStart = fieldManager.IsBattleStart;
                    if (newBattleStart){
                        //MelonLogger.Msg("Assuming battle due to IsBattleStart being true");
                        assumeBattle = true;}
                    //Return early if we are assuming a battle, as the others arent safe to check during battle
                    if (assumeBattle){return false; }

                    bool newScanMode = fieldManager.IsScanMode;
                    bool newPromptStatus = fieldManager.IsPromptStatus();
                    bool newPreMapChangeBattle = fieldManager.IsPreMapChangeBattle();
                    

                    //if (isScanMode != newScanMode) { MelonLogger.Msg($"isScanMode {isScanMode} -> {newScanMode}"); }
                    isScanMode = newScanMode;

                    //if (isPromptStatus != newPromptStatus) { MelonLogger.Msg($"isPromptStatus {isPromptStatus} -> {newPromptStatus}"); }
                    isPromptStatus = newPromptStatus;

                    //if (IsPreMapChangeBattle != newPreMapChangeBattle) { MelonLogger.Msg($"IsPreMapChangeBattle {IsPreMapChangeBattle} -> {newPreMapChangeBattle}"); }
                    IsPreMapChangeBattle = newPreMapChangeBattle;

                    //if (isBattleStart != newBattleStart) { MelonLogger.Msg($"isBattleStart {isBattleStart} -> {newBattleStart}"); }
                    isBattleStart = newBattleStart;
                }
                errorFree = true;
            }
            catch(Exception e)
            {
                errorFree = false;
                //MelonLogger.Error($"RewardSafety error: {e}");
            }
            return getValue();
        }

        public static implicit operator bool(RewardSafety obj)
        {
            bool value = obj.getValue();
            if (Core.lastRewardSafety != value)
            {
                //MelonLogger.Msg($"RewardSafety Update {Core.lastRewardSafety} -> {value} ");
                Core.lastRewardSafety = value;
            }
            return value;
        }
        public override string ToString()
        {
            bool value = getValue();
            return value.ToString();
        }
    }
}