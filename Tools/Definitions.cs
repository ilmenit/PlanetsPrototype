using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class TerrainsDefinition
{
    public string Id;
    public string Name;
    public string Icon;
    public int Popularity;
}

[System.Serializable]
public class GoodsDefinition
{
    public string Id;
    public string Name;
    public string Icon;
    public Color Color;
    public bool IsAdvanced;
}

[System.Serializable]
public class RacesDefinition
{
    public string Id;
    public string Name;
}

[System.Serializable]
public class PlayerColorsDefinition
{
    public string Id;
    public Color Color;
}

[System.Serializable]
public class SpaceObjectsDefinition
{
    public string Id;
    public string Name;
    public string TerrainType;
    public string Icon;
}

[System.Serializable]
public class UnitsDefinition
{
    public string Id;
    public string Name;
    public int Cost;
    public int Attack;
    public int HP;
    public string Size;
    public string WeaponSize;
    public List <string> Traits;
}

[System.Serializable]
public class BuildingsDefinition
{
    public string Id;
    public string Name;
    public string TerrainId;
    public string BuildingType;
    public string DemandsId;
    public string ProducesId;
    public int ProductionMultiplier;
    public int Cost;
}

[System.Serializable]
public class ImprovementsDefinition
{
    public string Id;
    public string Name;
    public string Type;
    public string Description;
}

[System.Serializable]
public class TransformationsDefinition
{
    public string Id;
    public string Name;
    public string From;
    public string To;
}

[System.Serializable]
public class InventionsDefinition
{
    public string Id;
    public string Type;
    public int Cost;
    public int X;
    public int Y;
}

[System.Serializable]
public class TechnologiesDefinition
{
    public string Id;
    public string Invention;
    public string Type;
}

[System.Serializable]
public class Definitions
{
    public Dictionary<string, TerrainsDefinition > Terrains;
    public Dictionary<string, GoodsDefinition > Goods;
    public Dictionary<string, RacesDefinition > Races;
    public Dictionary<string, PlayerColorsDefinition > PlayerColors;
    public Dictionary<string, SpaceObjectsDefinition > SpaceObjects;
    public Dictionary<string, UnitsDefinition > Units;
    public Dictionary<string, BuildingsDefinition > Buildings;
    public Dictionary<string, ImprovementsDefinition > Improvements;
    public Dictionary<string, TransformationsDefinition > Transformations;
    public Dictionary<string, InventionsDefinition > Inventions;
    public Dictionary<string, TechnologiesDefinition > Technologies;
}

