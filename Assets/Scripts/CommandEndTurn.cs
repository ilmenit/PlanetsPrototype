using System;
using System.Collections.Generic;

public class CommandEndTurn : Command
{
    // AI evaluation of this move
    public override int Evaluate()
    {
        return int.MinValue+1;
    }

    public override bool Execute()
    {
        Engine.Instance.PlayerTurnEnds();
        return true;
    }
}
