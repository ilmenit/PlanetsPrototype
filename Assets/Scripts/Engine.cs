// need of    Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER","yes");
// foreach enum         foreach (InventionType type in (InventionType[])Enum.GetValues(typeof(InventionType)))

using System;
using System.Collections.Generic;
using System.Threading;

public enum GameStateType
{
    NotStarted,
    GameStarting,
    Map,
    InResearchWindow,
    InResearchConfirmation,
    InFleetWindow,
    InCellInfo,
    InBuildingOnTerrain,
    InSelectingSupportForBuilding,
    Battle,
    BattleSwitchingPlayer,
    End,
    Quiting
}

public enum GameStateTrigger
{
    StartGame,
    ShowMap,
    ShowCell,
    ShowWhatCanBeBuilt,
    ShowSupplyingBuildings,
    ShowResearch,
    ShowFleet,
    ItemSelected,
    StartBattle,
    BattleNextPlayer,
    BattleEnds,
    Confirm,
    Cancel,
    EndGame,
    Save,
    Load,
    LoadBattle,
    LoadMap,
    Quit
}


public partial class Engine : Singleton<Engine>
{
    protected Engine()
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    public Object threadLock = new Object();
    public Model model; // shortcut for Model.instance

    private MapCell viewClickedMapCell;
    public MapCell SelectedMapCell;
    public Building SelectedBuilding;
    public Terrain SelectedTerrain;
    public CommandAcquireInvention SelectedInventionCommand;
    public Command AvailableActions;

    public delegate void ActionControllerHandler();
    public ActionControllerHandler ActionController;

    public FSM<GameStateType, GameStateTrigger> GameState;

    public void MainMultiThread()
    {
        while (true)
        {
            MainSingleThread();
        }
    }

    public void MainSingleThread()
    {
        // now we have two tests settings for multithreading
        // 1. threadLock and all actions are passed from UI to this thread
        // 2. testing, if dirty, do not add dirt. If dirty, clean the dirt
        bool acquiredLock = false;

        try
        {
            Monitor.TryEnter(threadLock, 0, ref acquiredLock);
            if (acquiredLock)
            {
                // if there is nothing to process, lets get new action
                if (Frontend.Instance.DirtyBits == 0)
                {

                    if (ActionController != null)
                        ActionController();
                    else
                        Log.Error("Action Controller is not set!");
                }
            }
        }
        finally
        {
            if (acquiredLock)
            {
                Monitor.Exit(threadLock);
            }
        }

        System.Threading.Thread.Sleep(1);
    }

    public void GetAIAction()
    {
        var commands = new List<Command>();
        AvailableActions.GetAllCommands(commands);
        var bestCommand = GetBestCommand(commands);
        bestCommand.Execute();
    }

    public void GetHumanAction()
    {
        MapCell clickedMapCell;
        clickedMapCell = viewClickedMapCell;
        viewClickedMapCell = null;

        // we can click cells only if we are active player and player that is looking at the gui
        if (clickedMapCell != null && model.ActivePlayer == model.GuiPlayer)
        {
            MapCellClicked(clickedMapCell);
        }
    }

    // contextual option
    public void MapCellClicked(MapCell cell)
    {
        Log.WhoAmI();
        Frontend.Instance.SetDirty(Dirty.RightPanel | Dirty.Map); // also map to show selected cell

        if (cell == SelectedMapCell)
        {
            new CommandAcquireCell(cell, model.ActivePlayer).Execute();
        }
        SelectMapCell(cell);
        Loom.Instance.QueueOnMainThread(() =>
        {
            GameState.Fire(GameStateTrigger.ShowCell);
        });
    }

    private void SetPlayerController()
    {
        if (model.ActivePlayer.ControledByAI)
            ActionController = GetAIAction;
        else
            ActionController = GetHumanAction;
    }

    public void GiveControlToNextPlayer()
    {
        Log.WhoAmI();

        if (model.Players.Count == 0)
        {
            Log.Error("No players in NextPlayer!");
            return;
        }

        int playerIndex = model.Players.IndexOf(model.ActivePlayer);
        if (playerIndex == -1)
        {
            model.ActivePlayer = model.Players[0];
            SetPlayerController();
            return;
        }
        ++playerIndex;

        // clip index
        playerIndex %= model.Players.Count;
        if (playerIndex == 0)
            ++model.PlayerTurn;

        model.ActivePlayer = model.Players[playerIndex];

        // change controller
        SetPlayerController();

        // for hot-seat game
        if (!model.ActivePlayer.ControledByAI)
        {
            Log.TurnEnd();
            model.GuiPlayer = model.ActivePlayer;
        }
    }

    private void PlayerTurnStarts()
    {
        Log.WhoAmI();
        OutpostsAndPlanetsSpreadInfluence();
        CollectIncome();
        FillAvailableCommands();
    }

    public void BattleStarts(MapCell cell)
    {
        Log.WhoAmI();
        model.Battle.Init(cell, model.ActivePlayer, cell.Owner);
        Frontend.Instance.SetDirty(Dirty.ClearAll);
        GameState.Fire(GameStateTrigger.StartBattle);
    }

    private void CollectIncome()
    {
        // Three options to collect income:
        // 1. Civ like of infinite growth
        //  --- you can wait to get something better (anticipation
        // 2. Worker placement-like that you get each turn max of what's placed. Can by justified that you are not able to store more.
        //  --- may prevent players growing too strong or making some resources useles at some point because of too high number
        // 3. Finite resources
        // --- Roguelike-like - when used, does not get replaced. But with war and front it may be bad.

        // 4. Slipways - generate some always but more with synergies
        Goods income = CalculateIncome(model.ActivePlayer);
        model.ActivePlayer.AvailableGoods.Add(income);
    }

    private void OutpostsAndPlanetsSpreadInfluence()
    {
        foreach (MapCell cell in model.Map.Cells)
        {
            if (cell.Owner != model.ActivePlayer)
                continue;
            if (cell.Type == CellType.Planet)
            {
                TakeCellOwnership(cell, model.ActivePlayer);
            }
        }
    }

    public Goods CalculateIncome(Player player)
    {
        Goods income = new Goods();
        foreach (MapCell cell in model.Map.Cells)
        {
            if (cell.Owner != player)
                continue;

            // calcualte income from buildings
            income.Add(CalculateCellProduction(cell));
        }
        return income;
    }

    public void PlayerTurnEnds()
    {
        Log.WhoAmI();
        DeselectCell();
        GiveControlToNextPlayer();
        PlayerTurnStarts();
        Frontend.Instance.SetDirty(Dirty.ClearAll);
    }

    public void SelectMapCell(MapCell cell)
    {
        SelectedMapCell = cell;
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);
    }

    public void DeselectCell()
    {
        if (SelectedMapCell == null)
            return;

        SelectedMapCell = null;
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);
    }

    public void Invent(string invention, Player player)
    {
        if (player.ResearchState.DiscoveredInventions.Contains(invention))
            return;

        player.ResearchState.MarkAsInvented(invention);
    }

    public Goods GetInventionCost(string invention)
    {
        var inventionDef = Game.Instance.Definitions.Inventions[invention];
        return new Goods(inventionDef.Type, inventionDef.Cost);
    }

    public Goods GetSupplyLineCost(Building supplier, Building toBuilding, Player player)
    {
        int distance = Map.Distance(toBuilding.OwnerCell, supplier.OwnerCell);
        return new Goods("Credits", distance * player.SupplyLineCost);
    }


    public void AddUnitToFleet(string unitName, Player player)
    {
        player.Fleet.Add(unitName);
    }

    public void Init()
    {
        // make local shortcut
        model = Model.Instance;

        // initialize game states

        GameState = new FSM<GameStateType, GameStateTrigger>(GameStateType.NotStarted);

        GameState.Configure(GameStateType.NotStarted)
            .Permit(GameStateTrigger.StartGame, GameStateType.GameStarting);

        GameState.Configure(GameStateType.GameStarting)
            .Permit(GameStateTrigger.ShowMap, GameStateType.Map)
            .OnEntry(NewGame);

        GameState.Configure(GameStateType.Map)
            .Permit(GameStateTrigger.StartBattle, GameStateType.Battle)
            .Permit(GameStateTrigger.ShowResearch, GameStateType.InResearchWindow)
            .Permit(GameStateTrigger.ShowFleet, GameStateType.InFleetWindow)
            .Permit(GameStateTrigger.ShowCell, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.EndGame, GameStateType.End)
            .OnEntry( Frontend.Instance.EnteringMap )
            .OnExit( Frontend.Instance.ExitingMap );


        GameState.Configure(GameStateType.InCellInfo)
            .Permit(GameStateTrigger.StartBattle, GameStateType.Battle)
            .Permit(GameStateTrigger.ShowResearch, GameStateType.InResearchWindow)
            .Permit(GameStateTrigger.ShowFleet, GameStateType.InFleetWindow)
            .Permit(GameStateTrigger.ShowMap, GameStateType.Map)
            .Permit(GameStateTrigger.ShowCell, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.ShowWhatCanBeBuilt, GameStateType.InBuildingOnTerrain)
            .Permit(GameStateTrigger.ShowSupplyingBuildings, GameStateType.InSelectingSupportForBuilding)
            .Permit(GameStateTrigger.Cancel, GameStateType.Map)
            .Permit(GameStateTrigger.EndGame, GameStateType.End)
            .OnEntry(Frontend.Instance.EnteringCellInfo)
            .OnExit(Frontend.Instance.ExitingCellInfo);

        GameState.Configure(GameStateType.InBuildingOnTerrain)
            .Permit(GameStateTrigger.Cancel, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.ShowCell, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.ItemSelected, GameStateType.InCellInfo)
            .OnEntry(Frontend.Instance.EnteringBuildingOnTerrain)
            .OnExit(Frontend.Instance.ExitingBuildingOnTerrain);


        GameState.Configure(GameStateType.InSelectingSupportForBuilding)
            .Permit(GameStateTrigger.Cancel, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.ShowCell, GameStateType.InCellInfo)
            .Permit(GameStateTrigger.ItemSelected, GameStateType.InCellInfo)
            .OnEntry(Frontend.Instance.EnteringSelectingSupportForBuilding)
            .OnExit(Frontend.Instance.ExitingSelectingSupportForBuilding);


        GameState.Configure(GameStateType.InResearchWindow)
            .Permit(GameStateTrigger.ItemSelected, GameStateType.InResearchConfirmation)
            .Permit(GameStateTrigger.Cancel, GameStateType.Map)
            .OnEntry( Frontend.Instance.EnteringResearch )
            .OnExit( Frontend.Instance.ExitingResearch );

        GameState.Configure(GameStateType.InResearchConfirmation)
            .Permit(GameStateTrigger.Confirm, GameStateType.InResearchWindow)
            .Permit(GameStateTrigger.Cancel, GameStateType.Map)
            .OnEntry(Frontend.Instance.EnteringResearchConfirmation)
            .OnExit(Frontend.Instance.ExitingResearchConfirmation);


        GameState.Configure(GameStateType.InFleetWindow)
            .Permit(GameStateTrigger.ItemSelected, GameStateType.InFleetWindow)
            .Permit(GameStateTrigger.Cancel, GameStateType.Map)
            .OnEntry( Frontend.Instance.EnteringFleet )
            .OnExit( Frontend.Instance.ExitingFleet );

        GameState.Configure(GameStateType.Battle)
            .Permit(GameStateTrigger.BattleEnds, GameStateType.Map)
            .OnEntry( Frontend.Instance.EnteringBattle )
            .OnExit(Frontend.Instance.ExitingBattle );

        GameState.Fire(GameStateTrigger.StartGame);
    }

    public void EnteringMap()
    {

    }

    public void ExitingMap()
    {

    }

    public void TESTBATTLE()
    {
        MapCell cell = model.Map.GetCell(0, 0);
        cell.Owner = model.Players[1];
        BattleStarts(cell);
    }

    public void NewGame()
    {
        Log.WhoAmI();
        model.Clear();
        CreatePlayers();
        new MapCreator().Create();
        PlayerTurnEnds();
        GameState.Fire(GameStateTrigger.ShowMap);
    }

    private void CreatePlayers()
    {
        Player mainPlayer = new Player("Player1", 0);
        Model.Instance.Players.Add(mainPlayer);
        Model.Instance.GuiPlayer = mainPlayer;
        Model.Instance.Players.Add(new Player("Player2", 1, true));
    }

    public void Explore(MapCell cell, Player player)
    {
        cell.ExploredBy.Add(player);
        Frontend.Instance.SetDirty(Dirty.Map);
    }

    public List<MapCell> GetCellsAround(MapCell cell, bool includeCentral, bool diagonals=true)
    {
        List<MapCell> toReturn = new List<MapCell>();

        if (includeCentral)
            toReturn.Add(cell);

        MapCell newCell;
        newCell = model.Map.GetCell(cell.Position.x - 1, cell.Position.z);
        if (newCell != null)
            toReturn.Add(newCell);

        newCell = model.Map.GetCell(cell.Position.x + 1, cell.Position.z);
        if (newCell != null)
            toReturn.Add(newCell);

        newCell = model.Map.GetCell(cell.Position.x, cell.Position.z - 1);
        if (newCell != null)
            toReturn.Add(newCell);

        newCell = model.Map.GetCell(cell.Position.x, cell.Position.z + 1);
        if (newCell != null)
            toReturn.Add(newCell);

        if (diagonals)
        {
            // Diagonals
            newCell = model.Map.GetCell(cell.Position.x - 1, cell.Position.z - 1);
            if (newCell != null)
                toReturn.Add(newCell);

            newCell = model.Map.GetCell(cell.Position.x + 1, cell.Position.z - 1);
            if (newCell != null)
                toReturn.Add(newCell);

            newCell = model.Map.GetCell(cell.Position.x - 1, cell.Position.z + 1);
            if (newCell != null)
                toReturn.Add(newCell);

            newCell = model.Map.GetCell(cell.Position.x + 1, cell.Position.z + 1);
            if (newCell != null)
                toReturn.Add(newCell);

        }

        return toReturn;
    }

    public Goods GetAcquiringCellCost(MapCell cell, Player player)
    {
        if (cell == null || cell.Type != CellType.Planet)
            return new Goods("Energy", player.CellAcquisitionCost);

        // if planet, then cost *5
        return new Goods("Energy", player.CellAcquisitionCost * 5);
    }

    public Goods GetTransformationCost(Player player)
    {
        return new Goods("Technology", player.TransformationCost);
    }

    public Goods GetBuildingCost(string buildingName, Player player)
    {
        var cost = Game.Instance.Definitions.Buildings[buildingName].Cost / player.BuildingCostDivider;
        return new Goods("Credits", cost);
    }

    public Goods GetUnitCost(string unitName, Player player)
    {
        return new Goods("Technology", Game.Instance.Definitions.Units[unitName].Cost);
    }

    public List<CommandBuildOnTerrain> GetAvailableBuildOnTerrainCommands(Terrain terrain, bool affordable)
    {
        List<CommandBuildOnTerrain> buildingCommands = new List<CommandBuildOnTerrain>();
        // find cell
        foreach (var cellContainer in AvailableActions["Cells"])
        {
            foreach (var command in cellContainer.Value)
            {
                CommandBuildOnTerrain cmd = command.Value as CommandBuildOnTerrain;
                if (cmd == null)
                    continue;
                if (cmd.terrain != terrain)
                    continue;

                if (affordable == cmd.IsAffordable())
                    buildingCommands.Add(cmd);
            }
        }        
        return buildingCommands;
    }

    public List<CommandTransformTerrain> GetAvailableTransformTerrainCommands(Terrain terrain, bool affordable)
    {
        List<CommandTransformTerrain> transformCommand = new List<CommandTransformTerrain>();
        // find cell
        foreach (var cellContainer in AvailableActions["Cells"])
        {
            foreach (var command in cellContainer.Value)
            {
                CommandTransformTerrain cmd = command.Value as CommandTransformTerrain;
                if (cmd == null)
                    continue;
                if (cmd.terrain != terrain)
                    continue;

                if (affordable == cmd.IsAffordable())
                    transformCommand.Add(cmd);
            }
        }
        return transformCommand;
    }


    public List<CommandCreateSupplyLine> GetAvailableCreateSupplyLineCommands(Building toBuilding, bool affordable)
    {
        List<CommandCreateSupplyLine> supplyLineCommands = new List<CommandCreateSupplyLine>();

        // find cell
        foreach (var cellContainer in AvailableActions["Cells"])
        {
            foreach (var command in cellContainer.Value)
            {
                CommandCreateSupplyLine cmd = command.Value as CommandCreateSupplyLine;
                if (cmd == null)
                    continue;
                if (cmd.toBuilding != toBuilding)
                    continue;

                if (affordable == cmd.IsAffordable())
                    supplyLineCommands.Add(cmd);
            }
        }
        return supplyLineCommands;
    }

    public List<CommandBuildUnit> GetAvailableBuildUnitCommands(Player player, bool affordable)
    {
        List<CommandBuildUnit> availableBuildCommands = new List<CommandBuildUnit>();
        foreach (var buildUnitCommand in AvailableActions["Fleet"])
        {
            CommandBuildUnit cmd = buildUnitCommand.Value as CommandBuildUnit;
            if (cmd == null)
                continue;

            if (affordable == cmd.IsAffordable())
                availableBuildCommands.Add(cmd);
        }

        return availableBuildCommands;
    }

    public bool IsCellExploredBy(MapCell cell, Player player)
    {
        return cell.ExploredBy.Contains(player);
    }


    public List<CommandAcquireInvention> GetAvailableAcquireInventionCommands(bool affordable)
    {
        List<CommandAcquireInvention> acquireInventionCommands = new List<CommandAcquireInvention>();

        foreach (var acquireInventionCommand in AvailableActions["Research"])
        {
            CommandAcquireInvention cai = acquireInventionCommand.Value as CommandAcquireInvention;
            if (cai == null)
                continue;

            if (affordable == cai.IsAffordable())
                acquireInventionCommands.Add(cai);
        }

        return acquireInventionCommands;
    }

    public bool TakeCellOwnership(MapCell cell, Player player, bool mustConnect = true)
    {
        //Log.WhoAmI();
        var cellsAround = GetCellsAround(cell, false, false);

        if (mustConnect)
        {
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
        }

        cell.Owner = player;
        Explore(cell, player);

        foreach (var around in cellsAround)
        {
            Explore(around, player);

            // if we take ownership of a planet, planet controls all the cells around

            if (cell.Type == CellType.Planet)
            {
                if (around.Owner != player &&
                    (around.Type != CellType.Planet)
                    )
                {
                    TakeCellOwnership(around, player);
                }
            }
        }
        return true;
    }

    public void ViewClickedCell(MapCell cell)
    {
        lock (threadLock)
        {
            Log.WhoAmI();
            viewClickedMapCell = cell;
        }
    }

    public Goods CalculateCellProduction(MapCell cell)
    {
        Goods toReturn = new Goods();
        foreach (var building in cell.Buildings)
        {
            toReturn.Add(building.GetProduction());
        }
        return toReturn;
    }

    public Goods CalculateCellDemand(MapCell cell)
    {
        Goods toReturn = new Goods();
        foreach (var building in cell.Buildings)
        {
            toReturn.Add(building.GetDemandAsGoods());
        }
        return toReturn;
    }
}