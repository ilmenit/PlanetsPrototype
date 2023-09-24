using System;
using System.Collections.Generic;

public class CommandAcquireInvention : Command
{
    public string invention;
    public Player player;

    public CommandAcquireInvention(string invention, Player player)
    {
        this.invention = invention;
        this.player = player;
    }

    // AI evaluation of this move
    public override int Evaluate()
    {
        // ??? how to evaluate what is needed?
        // +1 for building we need?
        // +1 for ship we need?

        return int.MinValue;
    }

    // command can be available but plannot cannot afford execution
    public override bool IsAffordable()
    {
        var cost = Engine.Instance.GetInventionCost(invention);
        if (cost.FitsIn(player.AvailableGoods))
            return true;
        return false;
    }

    public override bool IsValid()
    {
        return IsValid(invention, player);
    }

    public static bool IsValid(string invention, Player player)
    {
        // if already invented, then not valid
        if (player.ResearchState.DiscoveredInventions.Contains(invention))
            return false;

        if (!player.ResearchState.CanBeInvented(invention))
            return false;

        return true;
    }

    public override bool Execute()
    {
        // if we have resources to acquire
        if (!IsValidAndAffordable())
            return false;

        // substract the cost of invention
        var cost = Engine.Instance.GetInventionCost(invention);
        player.AvailableGoods.Sub(cost);

        // take ownership
        Engine.Instance.Invent(invention, player);

        if (player == Model.Instance.GuiPlayer)
        {
            Frontend.Instance.SetDirty(Dirty.PlayerData | Dirty.RightPanel | Dirty.Options);
        }
        Log.TurnAction("Acquired invention " + invention);
        base.Execute();
        return true;
    }
}
