using System;
using System.Collections.Generic;

public class CommandAcquireCell : Command
{
    public MapCell cell;
    public Player player;

    public CommandAcquireCell(MapCell cell, Player player) 
    {
        this.cell = cell;
        this.player = player;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        // +1 for each explored area by acquisition
        // +5 for planet
        // +1 for each Advanced terrain that produces advanced good (e.g. Industrial gives Tech)
        // 
        return int.MinValue;
    }

    // command can be available but plannot cannot afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetAcquiringCellCost(cell, player);

        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(cell, player);
    }

    public static bool IsValid(MapCell cell, Player player)
    {
        // already owned
        if (cell.Owner == player)
            return false;

        if (!player.ResearchState.Improvements.Contains("Interstellar Travel"))
            return false;

        // cells to be acquired must connect to existing empire
        var cellsAround = Engine.Instance.GetCellsAround(cell, false, false);

        bool connectedToExisting = false;
        foreach (var around in cellsAround)
        {
            if (around.Owner == player)
            {
                connectedToExisting = true;
                break;
            }
        }
        if (!connectedToExisting)
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        // take ownership
        Engine.Instance.TakeCellOwnership(cell, player);

        // substract the price of acquisition
        var cost = Engine.Instance.GetAcquiringCellCost(cell, player);
        player.AvailableGoods.Sub(cost);

        if (player == Model.Instance.GuiPlayer)
        {
            Frontend.Instance.SetDirty(Dirty.PlayerData | Dirty.Map);
        }
        Log.TurnAction("Acquired cell " + cell.Name);

        base.Execute();
        return true;
    }
}
