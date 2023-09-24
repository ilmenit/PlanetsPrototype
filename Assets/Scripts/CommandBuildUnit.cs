using System;
using System.Collections.Generic;

public class CommandBuildUnit : Command
{
    public string unitName;
    public Player player;

    public CommandBuildUnit(string unitName, Player player)
    {
        this.unitName = unitName;
        this.player = player;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        return int.MinValue;
    }

    // command can be available but player may not afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetUnitCost(unitName, player);
        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(unitName, player);
    }

    public static bool IsValid(string unitName, Player player)
    {
        // already owned
        if (!player.ResearchState.AllowedUnits.Contains(unitName))
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        Engine.Instance.AddUnitToFleet(unitName, player);

        // substract the price of acquisition
        var cost = Engine.Instance.GetUnitCost(unitName, player);
        player.AvailableGoods.Sub(cost);

        if (player == Model.Instance.GuiPlayer)
        {
            Frontend.Instance.SetDirty(Dirty.ClearAll);
        }
        Log.TurnAction("Built unit " + unitName);

        base.Execute();
        return true;
    }
}
