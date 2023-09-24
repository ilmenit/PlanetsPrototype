using System;
using System.Collections.Generic;

public class CommandAvailableActions : CommandContainer
{
    public CommandAvailableActions() : base()
    {        
        this["Fleet"] = new CommandContainer();
        this["Research"] = new CommandContainer();
        this["Cells"] = new CommandContainer();
    }
}
