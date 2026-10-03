using System;

namespace Machi.Core
{
    [Serializable]
    public class StageEntry
    {
        public string id;
        public int stage;
    }

    /// <summary>Everything that persists between sessions. Plain fields for JsonUtility.</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string[] board;
        public int coins;
        public int materials;
        public int energy;
        public long energyUpdatedAt;     // unix seconds of the last energy regen tick
        public int revival;
        public StageEntry[] buildingStages;
        public string[] completedOrders;
        public string[] unlockedRegions;
    }
}
