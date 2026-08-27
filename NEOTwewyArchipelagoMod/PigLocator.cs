using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEOTwewyArchipelagoMod
{
    public static class PigLocator
    {
        private static bool __pigInit = false;

        public static Dictionary<ArchipelagoLocationID, GameLocationID> archiToPig = new();
        public static Dictionary<GameLocationID, ArchipelagoLocationID> pigToArchi = new();

        public static bool TryGetPigDataId(ArchipelagoLocationID id, out GameLocationID gameID)
        {
            return archiToPig.TryGetValue(id, out gameID);
        }

        public static bool TryGetArchiId(GameLocationID id, out ArchipelagoLocationID archiID)
        {
            return pigToArchi.TryGetValue(id, out archiID);
        }

        public static void AddPairing(ArchipelagoLocationID id, GameLocationID gameID)
        {
            archiToPig.Add(id, gameID);
            pigToArchi.Add(gameID, id);
        }

        public static void Initialize()
        {
            if (!__pigInit)
            {
                foreach (KeyValuePair<long, long> entry in PIG_DATA_ARCHI_MAP)
                {
                    GameLocationID gameID = new(entry.Key);
                    ArchipelagoLocationID archiID = new(entry.Value);
                    AddPairing(archiID, gameID);
                }
                __pigInit = true;
            }

        }
        //Game ID -> Archi ID
        private static Dictionary<long, long> PIG_DATA_ARCHI_MAP = new()
        {
            { 1203900, 410001 }, // W1D2 - Pig Samba (Dogenzaka)
            { 1302900, 410002 }, // W1D3 - Pig Carol (104 Building)
            { 1303900, 410003 }, // W1D3 - Pig Samba (Center Street)
            { 1404900, 410004 }, // W1D4 - Pig Carol(Tower Records)
            { 1408900, 410005 }, // W1D4 - Pig Bolero/Mambo (Shibuya Hikarie)
        };

    }
}
