using System;
using System.Collections.Generic;

public class CommandBuildOnTerrain : Command
{
    public MapCell cell;
    public Player player;
    public Terrain terrain;
    public string buildingName;

    public CommandBuildOnTerrain(MapCell cell, Terrain terrain, string buildingName, Player player)
    {
        this.cell = cell;
        this.player = player;
        this.terrain = terrain;
        this.buildingName = buildingName;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        // +2 if generated resource IsAdvanced (tech/money/energy)
        // * building output multiplier (1/2)
        // +1 for each demand of generated resource in achievable distance
        // -1 if generated resource is not needed in achievable distance
        return int.MinValue;
    }

    // command can be available but plannot cannot afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetBuildingCost(buildingName,player);
        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(cell, terrain, buildingName, player);
    }

    public static bool IsValid(MapCell cell, Terrain terrain, string buildingName, Player player)
    {
        if (cell.Owner != player)
            return false;

        if (!cell.Terrains.Contains(terrain))
            return false;

        if (!player.ResearchState.AllowedBuildings.Contains(buildingName))
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        cell.Terrains.Remove(terrain);
        cell.Buildings.Add(new Building(buildingName, cell));

        // substract the price of acquisition
        var cost = Engine.Instance.GetBuildingCost(buildingName,player);
        player.AvailableGoods.Sub(cost);

        if (player == Model.Instance.GuiPlayer)
        {
            Frontend.Instance.SetDirty(Dirty.ClearAll);
        }
        Log.TurnAction("Built on cell " + cell.Name);
        base.Execute();
        return true;
    }
}
