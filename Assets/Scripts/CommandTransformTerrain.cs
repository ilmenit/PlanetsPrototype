using System;
using System.Collections.Generic;

public class CommandTransformTerrain : Command
{
    public MapCell cell;
    public Player player;
    public Terrain terrain;
    public string transformName;

    public CommandTransformTerrain(MapCell cell, Terrain terrain, string transformName, Player player)
    {
        this.cell = cell;
        this.player = player;
        this.terrain = terrain;
        this.transformName = transformName;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        // + if after transformation on the terrain we can build something useful (how to evaluate it?)
        // City to Industrial to Build Factory if we are missing Technology?
        // Ice to Water to Build Algae Farm if we need food?
        return int.MinValue;
    }

    // command can be available but plannot cannot afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetTransformationCost(player);
        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(cell, terrain, transformName, player);
    }

    public static bool IsValid(MapCell cell, Terrain terrain, string transformNAme, Player player)
    {
        if (cell.Owner != player)
            return false;

        if (!cell.Terrains.Contains(terrain))
            return false;

        if (!player.ResearchState.AllowedTransformations.Contains(transformNAme))
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        var transformDef = Game.Instance.Definitions.Transformations[transformName];
        cell.Terrains.Remove(terrain);
        cell.Terrains.Add(new Terrain(transformDef.To, cell));

        var cost = Engine.Instance.GetTransformationCost(player);
        player.AvailableGoods.Sub(cost);

        if (player == Model.Instance.GuiPlayer)
        {
            Engine.Instance.SelectedTerrain = null;
            Frontend.Instance.SetDirty(Dirty.ClearAll);
        }
        Log.TurnAction("Transformed to " + transformName);
        base.Execute();
        return true;
    }
}
