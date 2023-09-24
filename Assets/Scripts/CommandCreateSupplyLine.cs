using System;
using System.Collections.Generic;

public class CommandCreateSupplyLine : Command
{
    public Player player;
    public Building supplier;
    public Building toBuilding;

    public CommandCreateSupplyLine(Building supplier, Building toBuilding, Player player)
    {
        this.player = player;
        this.supplier = supplier;
        this.toBuilding = toBuilding;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        // +2 if it leads to Advanced one

        return int.MinValue;
    }

    // command can be available but plannot cannot afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetSupplyLineCost(supplier, toBuilding,player);
        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(supplier, toBuilding, player);
    }

    public static bool IsValid(Building supplier, Building toBuilding, Player player)
    {
        if (supplier.OwnerCell.Owner != player)
            return false;

        if (toBuilding.OwnerCell.Owner != player)
            return false;

        if (supplier.GetProducedType() != toBuilding.GetDemandedType())
            return false;

        if (supplier.GetProducedCount() <= 0)
            return false;

        if (toBuilding.GetDemandCount() <= 0)
            return false;

        if (Map.Distance(supplier.OwnerCell, toBuilding.OwnerCell) > player.SupplyLineRange)
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        var player = toBuilding.OwnerCell.Owner;
        var cost = Engine.Instance.GetSupplyLineCost(supplier, toBuilding, player);
        player.AvailableGoods.Sub(cost);

        toBuilding.InputBuildings.Add(supplier);
        supplier.OutputBuildings.Add(toBuilding);

        if (player == Model.Instance.GuiPlayer)
        {
            Frontend.Instance.SetDirty(Dirty.ClearAll);
        }
        Log.TurnAction("Created Supply Line");
        base.Execute();
        return true;
    }
}
