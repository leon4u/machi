using System;

namespace Machi.Core
{
    // Config data. Plain public fields so Unity's JsonUtility and System.Text.Json can both read it.

    [Serializable]
    public class ItemDef
    {
        public string id;
        public string name;
        public string chain;
        public int level;
        public string next;   // item produced when two of these merge; empty = max level
    }

    [Serializable]
    public class WeightedItem
    {
        public string item;
        public int weight;
    }

    [Serializable]
    public class GeneratorDef
    {
        public string id;
        public string name;
        public int energyCost;
        public WeightedItem[] outputs;
    }

    [Serializable]
    public class ItemCount
    {
        public string item;
        public int count;
    }

    [Serializable]
    public class OrderDef
    {
        public string id;
        public string npc;
        public string title;
        public ItemCount[] requires;
        public int rewardCoins;
        public int rewardMaterials;
        // Order becomes available once this building reaches this stage ("" = always).
        public string prereqBuilding;
        public int prereqStage;
        // Order becomes available once this order is done ("" = no requirement).
        public string prereqOrder;
    }

    [Serializable]
    public class BuildingStageDef
    {
        public int cost;     // restoration materials to reach this stage
        public int revival;  // town revival points gained
        public string note;
    }

    [Serializable]
    public class BuildingDef
    {
        public string id;
        public string name;
        public string region;
        // stages[0] is the cost of going 0 -> 1, stages[1] is 1 -> 2, ...
        public BuildingStageDef[] stages;
    }

    [Serializable]
    public class RegionDef
    {
        public string id;
        public string name;
        public string chapter;
        public bool unlockedAtStart;
    }

    [Serializable]
    public class BoardCell
    {
        public int x;
        public int y;
        public string item;   // item id or generator id
    }

    [Serializable]
    public class GameConfig
    {
        public int boardWidth = 7;
        public int boardHeight = 9;
        public int startCoins;
        public int startMaterials;
        public int startEnergy = 100;
        public int maxEnergy = 100;
        public int energyRegenSeconds = 120;
        public ItemDef[] items;
        public GeneratorDef[] generators;
        public OrderDef[] orders;
        public BuildingDef[] buildings;
        public RegionDef[] regions;
        public BoardCell[] startingBoard;
    }
}
