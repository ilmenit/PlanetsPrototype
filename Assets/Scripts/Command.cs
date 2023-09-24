using System;
using System.Collections.Generic;

public class Command : Dictionary<string, Command>
{
    // AI evaluation of this move
    public virtual int Evaluate()
    {
        return int.MinValue;
    }

    public virtual bool Execute()
    {
        Engine.Instance.FillAvailableCommands();
        return true;
    }

    // this one is to validate existing command (that could be received e.g. with online game)
    public virtual bool IsValid()
    {
        return true;
    }

    public virtual bool IsAffordable()
    {
        return true;
    }

    // command can be available but plannot cannot afford execution
    public virtual bool IsValidAndAffordable()
    {
        return IsValid() && IsAffordable();
    }

    public void GetAllCommands(List <Command> outList)
    {
        outList.Add(this);
        foreach (var cmd in this.Values)
            cmd.GetAllCommands(outList);
    }

}

public class CommandContainer : Command
{
    public override bool IsValidAndAffordable()
    {
        // if any child is valid and affordable then true
        var items = this.Values;
        foreach (var child in items)
        {
            if (child.IsValidAndAffordable())
                return true;
        }
        return false;
    }
}


public class CommandCellContainer : CommandContainer
{
    public MapCell Cell;

    public CommandCellContainer(MapCell cell) 
    {
        Cell = cell;
    }
}


public partial class Engine : Singleton<Engine>
{
    private void AddAcquiringCommand(MapCell cell, CommandCellContainer cellContainer)
    {
        var player = model.ActivePlayer;
        // check if cell can be acquired
        if (cell.Owner != player)
        {
            if (CommandAcquireCell.IsValid(cell, player))
            {
                cellContainer.Add(cellContainer.Count.ToString(), new CommandAcquireCell(cell, player));
            }
        }
    }

    private void AddTerrainCommands(MapCell cell, CommandCellContainer cellContainer)
    {
        var player = model.ActivePlayer;

        if (cell.Owner != model.ActivePlayer)
            return;

        foreach (var terrain in cell.Terrains)
        {
            // add buildings

            foreach (var entry in Game.Instance.Definitions.Buildings)
            {
                var terrainName = entry.Value.TerrainId;
                if (terrainName != terrain.Id)
                    continue;

                var buildingName = entry.Key;

                if (CommandBuildOnTerrain.IsValid(cell, terrain, buildingName, player))
                {
                    cellContainer.Add(cellContainer.Count.ToString(), new CommandBuildOnTerrain(cell, terrain, buildingName, player));
                }
            }

            // Add transformations
            foreach (var entry in Game.Instance.Definitions.Transformations)
            {
                var terrainName = entry.Value.From;
                if (terrainName != terrain.Id)
                    continue;

                var transformNAme = entry.Key;

                if (CommandTransformTerrain.IsValid(cell, terrain, transformNAme, player))
                {
                    cellContainer.Add(cellContainer.Count.ToString(), new CommandTransformTerrain(cell, terrain, transformNAme, player));
                }
            }
        }
    }

    private void AddBuildingCommands(MapCell cell, CommandCellContainer cellContainer)
    {
        var player = model.ActivePlayer;
        if (cell.Owner != player)
            return;

        foreach (var building in cell.Buildings)
        {
            var currentBuildingDef = Game.Instance.Definitions.Buildings[building.Id];
            for (int z = -player.SupplyLineRange; z <= player.SupplyLineRange; ++z)
            {
                for (int x = -player.SupplyLineRange; x <= player.SupplyLineRange; ++x)
                {
                    MapCell tempCell = model.Map.GetCell(x + cell.Position.x, z + cell.Position.z);
                    if (tempCell == null) // out of map
                        continue;

                    foreach (var supplier in tempCell.Buildings)
                    {
                        if (CommandCreateSupplyLine.IsValid(supplier, building, player))
                        {
                            cellContainer.Add(cellContainer.Count.ToString(), new CommandCreateSupplyLine(supplier, building, player));
                        }
                    }
                }
            }
        }
    }


    private void AddCellsCommands()
    {
        var cellCommands = AvailableActions["Cells"];

        foreach (var cell in model.Map.Cells)
        {
            var cellContainer = new CommandCellContainer(cell);

            AddAcquiringCommand(cell, cellContainer);
            AddTerrainCommands(cell, cellContainer);
            AddBuildingCommands(cell, cellContainer);

            if (cellContainer.Count > 0)
                cellCommands.Add(cellCommands.Count.ToString(), cellContainer);
        }
    }

    private void AddResearchCommands()
    {
        var player = model.ActivePlayer;

        var researchCommands = AvailableActions["Research"];

        foreach (var inventionDef in Game.Instance.Definitions.Inventions)
        {
            var invention = inventionDef.Key;
            if (CommandAcquireInvention.IsValid(invention, player))
            {
                researchCommands.Add(researchCommands.Count.ToString(), new CommandAcquireInvention(invention, player));
            }
        }
    }

    private void AddFleetCommands()
    {
        var player = model.ActivePlayer;

        var fleetCommands = AvailableActions["Fleet"];

        foreach (var unitDef in Game.Instance.Definitions.Units)
        {
            var unitName = unitDef.Key;

            if (CommandBuildUnit.IsValid(unitName, player))
            {
                fleetCommands.Add(fleetCommands.Count.ToString(), new CommandBuildUnit(unitName, player));
            }
        }

    }

    public void FillAvailableCommands()
    {
        AvailableActions = new CommandAvailableActions();

        // this one is always available
        AvailableActions["End Turn"] = new CommandEndTurn();

        AddCellsCommands();
        AddResearchCommands();
        AddFleetCommands();
    }


    public Command GetBestCommand(List <Command> commands)
    {
        Command bestCommand = null;
        int bestScore = int.MinValue;
        foreach (var cmd in commands)
        {
            var eval = cmd.Evaluate();
            if (eval>bestScore)
            {
                bestScore = eval;
                bestCommand = cmd;
            }
        }
        return bestCommand;
    }
}
