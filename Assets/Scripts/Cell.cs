using System;
using System.Collections.Generic;

/* Mines
 * Ore - asteroids (small damage each turn) in battle
 * Energy - no missiles
 * Water - gravity - only big units?
 * Plants - ???
 */

public enum CellType
{
    Empty,
    Mine, // different mines can do different things?
    Planet,
}

[Serializable]
public class MapCell
{
    public string Name;
    public string Icon = ".";
    public MapCoord Position;
    public List<Terrain> Terrains = new List<Terrain>();
    public List<Building> Buildings = new List<Building>();
    public HashSet<Player> ExploredBy = new HashSet<Player>();
    public List<Building> SupplyLines = new List<Building>();
    public Player Owner = null;
    public CellType Type = CellType.Empty;

    public MapCell()
    {
    }

    public MapCell(MapCoord aPosition) : this()
    {
        Position = aPosition;
        Name = "Space Sector";
    }
}