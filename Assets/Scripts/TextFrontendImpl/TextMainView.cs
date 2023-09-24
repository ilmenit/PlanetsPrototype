using System.Collections.Generic;
using UnityEngine;

public class TextMainView : MonoBehaviour
{
    public Terminal Terminal;

    public TerminalRect ClickableMap;
    public TerminalText ClickableFleetButton;
    public TerminalText ClickableResearchButton;
    public TerminalText ClickableTurnButton;
    public TerminalText ClickableBattleButton;

    public TerminalRect RightPanel;
    public TerminalText RightPanelHeader;
    public TerminalList PanelList;
    public TerminalText ClickableClosePanelButton;

    public void MapClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        // Log.Print("Position " + position.ToString());
        // check if click is on the map
        MapCoord pos = new MapCoord(position.x, (consoleObject.Size.y - 1) - position.y);
        MapCell cell = Model.Instance.Map.GetCell(pos);
        if (cell != null)
            Engine.Instance.ViewClickedCell(cell);
    }

    public void FleetButtonClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        Engine.Instance.DeselectCell();
        Frontend.Instance.SetDirty(Dirty.RightPanel);
    }

    public void ShowMapView()
    {
        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.Map))
        {
            ShowMap();
        }


        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.PlayerData))
            ShowPlayerData();

        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.RightPanel))
        {
        }

        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.Options))
            ShowOptions();
    }

    public void ShowFleetPanel()
    {
        Log.WhoAmI();
        PanelList.Clear();

        Player player = Model.Instance.GuiPlayer;
        var availableBuildUnitCommands = Engine.Instance.GetAvailableBuildUnitCommands(player, true);
        var unavailableBuildUnitCommands = Engine.Instance.GetAvailableBuildUnitCommands(player, false);

        // display research according to types
        RightPanelHeader.SetText("[Fleet]");

        PanelList.Clear();
        foreach (var cmd in availableBuildUnitCommands)
        {
            var line = GetUnitTextLine(cmd.unitName, player);
            line.Color = Color.yellow;
            PanelList.Add(line);
        }
        foreach (var cmd in unavailableBuildUnitCommands)
        {
            var line = GetUnitTextLine(cmd.unitName, player);
            line.Color = Color.grey;
            PanelList.Add(line);
        }
        Terminal.Refresh();
    }


    private int countInList(string name, List<string> list)
    {
        int count = 0;
        foreach (var str in list)
        {
            if (str.Equals(name))
                count++;
        }
        return count;
    }

    public TerminalText GetUnitTextLine(string unitName, Player player)
    {
        var line = TerminalText.Instantiate();

        string toReturn = countInList(unitName,Model.Instance.ActivePlayer.Fleet).ToString() + '*' + unitName;

        var cost = Engine.Instance.GetUnitCost(unitName, player);
        string costString = GetGoodsAsString(cost);

        line.SetText(toReturn.CapTo(18) + "[" + costString + "]");
        return line;
    }

    private TerminalText GetInventionTextLine(string inventionName, Player player)
    {
        var line = TerminalText.Instantiate();
        var invDef = Game.Instance.Definitions.Inventions[inventionName];
        var icon = Game.Instance.Definitions.Goods[invDef.Type].Icon;
        line.Text = inventionName.CapTo(18) + "[" + icon + invDef.Cost + "] ";
        return line;
    }

    public void ShowResearchPanel()
    {
        Log.WhoAmI();
        PanelList.Clear();

        Player player = Model.Instance.GuiPlayer;
        var availableInventions = Engine.Instance.GetAvailableAcquireInventionCommands(true);
        var unavailableInventions = Engine.Instance.GetAvailableAcquireInventionCommands(false);

        Terminal.SetColor(Color.white);

        // display research according to types
        string toPrint = "";
        toPrint = "[RESEARCH]";
        RightPanelHeader.SetText(toPrint);

        PanelList.Clear();

        foreach (var invCommand in availableInventions)
        {
            var line = GetInventionTextLine(invCommand.invention, player);
            line.Color = Color.yellow;
            PanelList.Add(line);
        }
        foreach (var invCommand in unavailableInventions)
        {
            var line = GetInventionTextLine(invCommand.invention, player);
            line.Color = Color.grey;
            PanelList.Add(line);
        }
        Terminal.Refresh();
    }

    public void ShowResearchConfirmationPanel()
    {
        Log.WhoAmI();
        PanelList.Clear();

        Player player = Model.Instance.GuiPlayer;
        Terminal.SetColor(Color.white);

        // display research according to types
        string toPrint = "";

        var inventionName = Engine.Instance.SelectedInventionCommand.invention;

        var invDef = Game.Instance.Definitions.Inventions[inventionName];
        var icon = Game.Instance.Definitions.Goods[invDef.Type].Icon;
        toPrint = "[" + inventionName.CapTo(18) + icon + invDef.Cost + "]";

        RightPanelHeader.SetText(toPrint);
        PanelList.Clear();

        int y = 2;
        foreach (var tech in Game.Instance.Definitions.Technologies)
        {
            if (tech.Value.Invention == Engine.Instance.SelectedInventionCommand.invention)
            {
                var tech_line = tech.Value.Type[0] + ":" + tech.Key;
                Terminal.PrintAt(RightPanel.Position.x, RightPanel.Position.y + y, tech_line, false);
                y++;
            }
        }

        var line1 = TerminalText.Instantiate().SetText("<invent>").SetColor(Color.yellow);
        PanelList.Add(line1);
        PanelList.SetPosition(0, y+1);

        Terminal.Refresh();
    }


    public void TurnButtonClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        var endTurn = Engine.Instance.AvailableActions["End Turn"];
        endTurn.Execute();
        //Engine.Instance.PlayerTurnEnds();

    }

    public void ResearchButtonClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        Engine.Instance.GameState.Fire(GameStateTrigger.ShowResearch);
    }


    public void BattleButtonClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();

        // find enemy cell
        MapCell battleCell = null;
        foreach (var cell in Model.Instance.Map.Cells)
        {
            if (cell.Owner != null && cell.Owner != Model.Instance.ActivePlayer)
            {
                battleCell = cell;
                break;
            }
        }
        if (battleCell != null)
        {
            Engine.Instance.BattleStarts(battleCell);
        }
    }

    public void SupplyListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        //Log.Print("Supply list click position: " + position.ToString());
        Player player = Model.Instance.GuiPlayer;
        var affordableSupplyLineCommands = Engine.Instance.GetAvailableCreateSupplyLineCommands(Engine.Instance.SelectedBuilding, true);
        if (position.y < affordableSupplyLineCommands.Count)
        {
            var cmd = affordableSupplyLineCommands[position.y];
            if (cmd.Execute())
            {
                Engine.Instance.SelectedTerrain = null;
                Engine.Instance.GameState.Fire(GameStateTrigger.ItemSelected);
            }
        }
    }


    public void BuildingOnTerrainListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();

        Log.Print("Buildings list click position: " + position.ToString());
        var affordableBuildingCommands = Engine.Instance.GetAvailableBuildOnTerrainCommands(Engine.Instance.SelectedTerrain,true);
        var availableTransformationsCommands = Engine.Instance.GetAvailableTransformTerrainCommands(Engine.Instance.SelectedTerrain, true);

        if (position.y < affordableBuildingCommands.Count)
        {
            var cmd = affordableBuildingCommands[position.y];            
            if (cmd.Execute())
            {
                Engine.Instance.SelectedTerrain = null;
                Engine.Instance.GameState.Fire(GameStateTrigger.ItemSelected);
            }
        }
        else if (position.y < (affordableBuildingCommands.Count + availableTransformationsCommands.Count) )
        {
            var cmd = availableTransformationsCommands[position.y - affordableBuildingCommands.Count];
            if (cmd.Execute())
            {
                Engine.Instance.SelectedTerrain = null;
                Engine.Instance.GameState.Fire(GameStateTrigger.ItemSelected);
            }
        }

    }


    public void CellInfoListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        Log.Print("CellInfo list click position: " + position.ToString());
        if (Engine.Instance.SelectedMapCell.Owner != Model.Instance.ActivePlayer)
            return;

        var availableTerrains = Engine.Instance.SelectedMapCell.Terrains;
        var availableBuildings = Engine.Instance.SelectedMapCell.Buildings;

        if (position.y < availableTerrains.Count)
        {
            Engine.Instance.SelectedTerrain = availableTerrains[position.y];
            Engine.Instance.GameState.Fire(GameStateTrigger.ShowWhatCanBeBuilt);
        }
        else if (position.y < (availableTerrains.Count + availableBuildings.Count) )
        {
            var buildingIndex = position.y - availableTerrains.Count;
            Engine.Instance.SelectedBuilding = availableBuildings[buildingIndex];
            Engine.Instance.GameState.Fire(GameStateTrigger.ShowSupplyingBuildings);
        }
    }

    public void ResearchListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        //Log.Print("Research list click position: " + position.ToString());
        var availableResearch = Engine.Instance.GetAvailableAcquireInventionCommands(true);
        if (position.y < availableResearch.Count)
        {
            Engine.Instance.SelectedInventionCommand = availableResearch[position.y];
            Engine.Instance.GameState.Fire(GameStateTrigger.ItemSelected);
        }
    }


    public void ResearchConfirmationClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        if (position.y == 0)
        {
            var cmd = Engine.Instance.SelectedInventionCommand;
            cmd.Execute();

            var commands = Engine.Instance.GetAvailableAcquireInventionCommands(true);

            if (commands.Count > 0)
                Engine.Instance.GameState.Fire(GameStateTrigger.Confirm);
            else
                Engine.Instance.GameState.Fire(GameStateTrigger.Cancel);
        }
    }


    public void FleetListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        //Log.Print("Fleet List click position: " + position.ToString());
        var availableShips = Engine.Instance.GetAvailableBuildUnitCommands(Model.Instance.ActivePlayer, true);
        if (position.y < availableShips.Count)
        {
            var cmd = availableShips[position.y];
            if (cmd.Execute())
            {
                Engine.Instance.GameState.Fire(GameStateTrigger.ItemSelected);
            }
        }
    }

    public void Init()
    {
        int mapSizeX = 16;
        int mapSizeZ = 16;

        ClickableMap = TerminalRect.Instantiate().SetPosition(1, 3).SetSize(mapSizeX, mapSizeZ).SetOnClick(MapClickHandler);
        ClickableMap.gameObject.name = "ClickableMap";
        ClickableMap.CleanOnDisable = false;
        ClickableMap.gameObject.SetActive(false);

        ClickableFleetButton = TerminalText.Instantiate().SetPosition(18, 22).SetText("Fleet").SetOnClick( (TerminalRect a, Vector2Int b) => Engine.Instance.GameState.Fire(GameStateTrigger.ShowFleet) );
        ClickableFleetButton.gameObject.name = "ClickableFleetButton";
        ClickableFleetButton.gameObject.SetActive(false);

        ClickableResearchButton = TerminalText.Instantiate().SetPosition(25, 22).SetText("Research").SetOnClick( ResearchButtonClickHandler );
        ClickableResearchButton.gameObject.name = "ClickableResearchButton";
        ClickableResearchButton.gameObject.SetActive(false);

        ClickableTurnButton = TerminalText.Instantiate().SetPosition(35, 22).SetText("Turn").SetOnClick(TurnButtonClickHandler);
        ClickableTurnButton.gameObject.name = "ClickableTurnButton";
        ClickableTurnButton.gameObject.SetActive(false);

        ClickableBattleButton = TerminalText.Instantiate().SetPosition(1, 22).SetText("Battle").SetOnClick(BattleButtonClickHandler);
        ClickableBattleButton.gameObject.name = "ClickableBattleButton";
        ClickableBattleButton.gameObject.SetActive(false);

        RightPanel = TerminalRect.Instantiate().SetPosition(18, 2).SetSize(22, 22);
        RightPanel.gameObject.name = "RightPanel";

        RightPanelHeader = TerminalText.Instantiate().SetPosition(0, 0).SetText("PanelHeader");
        RightPanelHeader.gameObject.name = "RightPanelHeader";
        RightPanelHeader.transform.SetParent(RightPanel.transform);

        PanelList = TerminalList.Instantiate().SetPosition(0, 2);
        PanelList.gameObject.name = "PanelList";
        PanelList.transform.SetParent(RightPanel.transform);

        ClickableClosePanelButton = TerminalText.Instantiate().SetPosition(18, 21).SetText("[X]").SetOnClick((TerminalRect a, Vector2Int b) => Engine.Instance.GameState.Fire(GameStateTrigger.Cancel));
        ClickableClosePanelButton.gameObject.name = "ClickableClosePanelButton";
        ClickableClosePanelButton.transform.SetParent(RightPanel.transform);

        RightPanel.gameObject.SetActive(false);
    }

    private void ShowMapCell(MapCell cell)
    {
        int x = ClickableMap.Position.x + cell.Position.x;
        int y = ClickableMap.Position.y + (Model.Instance.Map.Size.z - 1) - cell.Position.z;
        Color selectedHighlight = new Color(.3f, .3f, .3f, 1.0f);


        if (Engine.Instance.IsCellExploredBy(cell, Model.Instance.GuiPlayer))
        //if (Engine.Instance.IsCellExploredBy(cell, Model.Instance.GuiPlayer) || true)
        {
            // Terminal.SetColor(GetPlayerColor(cell.Owner));
            Terminal.SetColor(Color.white);
            Color backgroundColor = cell.Owner.GetPlayerColor();

            // check if action can be performed on this cell
            var character = cell.Icon;

            var cellCommands = Engine.Instance.AvailableActions["Cells"];

            foreach (var command in cellCommands.Values)
            {
                var cellCommand = command as CommandCellContainer;
                if (cellCommand == null)
                    continue;
                if (cellCommand.Cell == cell)
                {
                    if (cellCommand.IsValidAndAffordable())
                        Terminal.SetColor(Color.yellow);
                }
            }

            if (cell == Engine.Instance.SelectedMapCell)
                backgroundColor += selectedHighlight;

            Terminal.SetBackground(backgroundColor);
            Terminal.PrintAt(x, y, character);
        }
        else
        {
            Terminal.SetColor(Color.gray);

            if (cell == Engine.Instance.SelectedMapCell)
                Terminal.SetBackground(selectedHighlight);
            else
                Terminal.SetBackground(Terminal.DefaultBackgroundColor);
            Terminal.PrintAt(x, y, "#");
        }
    }

    public void ShowOptions()
    {
        Terminal.SetColor(Color.white, Terminal.DefaultBackgroundColor);
        // clear
        var availableProduction = Engine.Instance.GetAvailableBuildUnitCommands(Model.Instance.ActivePlayer, true);
        var unvavailableProduction = Engine.Instance.GetAvailableBuildUnitCommands(Model.Instance.ActivePlayer, false);

        if (availableProduction.Count > 0) 
            ClickableFleetButton.Color = Color.yellow; 
        else 
            ClickableFleetButton.Color = Color.grey;

        var availableInventions = Engine.Instance.GetAvailableAcquireInventionCommands(true);
        if (availableInventions.Count > 0)
            ClickableResearchButton.Color = Color.yellow;
        else
            ClickableResearchButton.Color = Color.grey;
    }

    public void ShowMap()
    {
        Log.WhoAmI();
        var Map = Model.Instance.Map;
        for (int z = 0; z < Map.Size.z; ++z)
        {
            for (int x = 0; x < Map.Size.x; ++x)
            {
                var cell = Map.GetCell(x, z);
                ShowMapCell(cell);
            }
        }
    }

    private string GetGoodsAsString(Goods goods)
    {
        string toReturn = "";
        bool added = false;
        foreach (var i in Game.Instance.Definitions.Goods.Keys)
        {
            int value = goods.Get(i);
            if (value > 0)
            {
                if (added)
                    toReturn += " ";
                toReturn += Game.Instance.Definitions.Goods[i].Icon + "" + goods.Get(i);
                added = true;
            }
        }
        return toReturn;
    }

    private string InventionNamesAsIcons(List<string> inventionNames)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var name in inventionNames)
        {
            if (counts.ContainsKey(name))
                counts[name]++;
            else
                counts[name] = 1;
        }

        string toReturn = "";
        bool added = false;
        foreach (var pair in counts)
        {
            if (added)
                toReturn += " ";
            toReturn += "NAME"; // !!!
            added = true;
        }
        return toReturn;
    }

    private TerminalText GetBuildingTextLine(string buildingName, Player player)
    {
        var line = TerminalText.Instantiate();
        var buildingDef = Game.Instance.Definitions.Buildings[buildingName];
        Goods demand = new Goods(buildingDef.DemandsId, 1);
        Goods production = new Goods(buildingDef.ProducesId, buildingDef.ProductionMultiplier);
        string demandSupply = GetGoodsAsString(demand) + "->" + GetGoodsAsString(production) + " ";
        var cost = Engine.Instance.GetBuildingCost(buildingName,player);
        line.Text = (demandSupply + buildingName).CapTo(18) + "[" + GetGoodsAsString(cost) + "]";
        return line;
    }


    private TerminalText GetTransformationTextLine(string transformationName, Player player)
    {
        var line = TerminalText.Instantiate();
        var transformDef = Game.Instance.Definitions.Transformations[transformationName];
        var cost = Engine.Instance.GetTransformationCost(player);
        line.Text = (">" + transformDef.To + "< " + transformDef.Name).CapTo(18) + "[" + GetGoodsAsString(cost) + "]";
        return line;
    }


    public void ShowBuildingOnTerrainList()
    {
        Log.WhoAmI();
        PanelList.Clear();

        Player player = Model.Instance.GuiPlayer;

        var affordableBuildingCommands = Engine.Instance.GetAvailableBuildOnTerrainCommands(Engine.Instance.SelectedTerrain, true);
        var unaffordableBuildingCommands = Engine.Instance.GetAvailableBuildOnTerrainCommands(Engine.Instance.SelectedTerrain, false);
        var availableTransformations = Engine.Instance.GetAvailableTransformTerrainCommands(Engine.Instance.SelectedTerrain, true);
        var unavailableTransformations = Engine.Instance.GetAvailableTransformTerrainCommands(Engine.Instance.SelectedTerrain, false);

        // display research according to types
        RightPanelHeader.SetText("[BUILD]");

        PanelList.Clear();

        foreach (var buildingCommand in affordableBuildingCommands)
        {
            var line = GetBuildingTextLine(buildingCommand.buildingName, player);
            line.Color = Color.yellow;
            PanelList.Add(line);
        }
        foreach (var transformationCommand in availableTransformations)
        {
            var line = GetTransformationTextLine(transformationCommand.transformName, player);
            line.Color = Color.yellow;
            PanelList.Add(line);
        }
        foreach (var buildingCommand in unaffordableBuildingCommands)
        {
            var line = GetBuildingTextLine(buildingCommand.buildingName, player);
            line.Color = Color.grey;
            PanelList.Add(line);
        }
        foreach (var transformationCommand in unavailableTransformations)
        {
            var line = GetTransformationTextLine(transformationCommand.transformName, player);
            line.Color = Color.grey;
            PanelList.Add(line);
        }
        Terminal.Refresh();
    }


    private TerminalText GetSupplyTextLine(Building building, Player player)
    {
        var line = TerminalText.Instantiate();
        Goods cost = Engine.Instance.GetSupplyLineCost(building, Engine.Instance.SelectedBuilding, player);
        line.Text = building.Id.CapTo(18) + "[" + GetGoodsAsString(cost) + "]";
        return line;
    }

    public void ShowSupplyForBuilding()
    {
        Log.WhoAmI();
        PanelList.Clear();

        Player player = Model.Instance.GuiPlayer;
        var availableSupplyCommands = Engine.Instance.GetAvailableCreateSupplyLineCommands(Engine.Instance.SelectedBuilding, true);
        var unavailableSupplyCommands = Engine.Instance.GetAvailableCreateSupplyLineCommands(Engine.Instance.SelectedBuilding, false);

        // display research according to types
        RightPanelHeader.SetText("[SELECT SUPPLY]");

        PanelList.Clear();

        foreach (var cmd in availableSupplyCommands)
        {
            var line = GetSupplyTextLine(cmd.supplier, player);
            line.Color = Color.yellow;
            PanelList.Add(line);
        }

        foreach (var cmd in unavailableSupplyCommands)
        {
            var line = GetSupplyTextLine(cmd.supplier, player);
            line.Color = Color.grey;
            PanelList.Add(line);
        }

        Terminal.Refresh();
    }


    public void ShowCellInfo()
    {
        PanelList.Clear();

        var cell = Engine.Instance.SelectedMapCell;

        if (cell == null)
            return;

        if (!cell.ExploredBy.Contains(Model.Instance.GuiPlayer))
        {
            // print icon and name
            RightPanelHeader.SetText("[Unexplored]");
            return;
        }

        // print icon and name
        string toPrint = "[" + cell.Name + "]";
        RightPanelHeader.Text = toPrint;
        RightPanelHeader.BackgroundColor = cell.Owner.GetPlayerColor();
        Terminal.SetColor(Color.white, Terminal.DefaultBackgroundColor);

        // print product on the cell
        toPrint = "Produces: " + GetGoodsAsString(Engine.Instance.CalculateCellProduction(cell));
        Terminal.PrintAt(RightPanel.Position.x, RightPanel.Position.y + 1, toPrint, false);

        // print what it demands

        toPrint = "Demands: " + GetGoodsAsString(Engine.Instance.CalculateCellDemand(cell));
        Terminal.PrintAt(RightPanel.Position.x, RightPanel.Position.y + 2, toPrint, false);

        // print interactable objects
        toPrint = "[Interactables]";
        
        Terminal.PrintAt(RightPanel.Position.x, RightPanel.Position.y + 4, toPrint, false);

        PanelList.Clear();

        // terrains
        foreach (var terrain in cell.Terrains)
        {
            var line = TerminalText.Instantiate();
            line.Text = "<" + Game.Instance.Definitions.Terrains[terrain.Id].Name + ">";

            var affordableBuildingCommands = Engine.Instance.GetAvailableBuildOnTerrainCommands(terrain, true);
            var transformations = Engine.Instance.GetAvailableTransformTerrainCommands(terrain, true);

            if (cell.Owner != Model.Instance.GuiPlayer)
                line.Color = Color.grey;
            else if (affordableBuildingCommands.Count > 0 || transformations.Count > 0)
                line.Color = Color.yellow;
            else
                line.Color = Color.white;

            PanelList.Add(line);
        }

        // buildings
        foreach (var building in cell.Buildings)
        {
            var line = TerminalText.Instantiate();
            string demandSupply = GetGoodsAsString(building.GetDemandAsGoods()) + "->" + GetGoodsAsString(building.GetProduction()) +" ";
            line.Text = demandSupply + Game.Instance.Definitions.Buildings[building.Id].Name;

            var availableSupplyCommands = Engine.Instance.GetAvailableCreateSupplyLineCommands(building, true);

            if (cell.Owner != Model.Instance.GuiPlayer)
                line.Color = Color.grey;
            else if (building.InputBuildings.Count==0 && availableSupplyCommands.Count>0 )
                line.Color = Color.yellow;
            else
                line.Color = Color.white;

            PanelList.Add(line);
        }

        Terminal.Refresh();
    }

    public void ShowPlayerData()
    {
        Log.WhoAmI();
        Terminal.SetColor(Color.white, Model.Instance.GuiPlayer.GetPlayerColor());

        int playerInfoX = 0;
        int playerInfoY = 0;

        // show basic data
        Terminal.PrintAt(playerInfoX, playerInfoY, Model.Instance.GuiPlayer.Name + ", Turn: " + Model.Instance.PlayerTurn.ToString(), false, 40);
        Terminal.PrintAt(playerInfoX, playerInfoY + 1, "", false, 40);

        // show stock and income
        string toPrint = "";
        Goods income = Engine.Instance.CalculateIncome(Model.Instance.GuiPlayer);
        Goods playerGoods = Model.Instance.GuiPlayer.AvailableGoods;
        toPrint += Game.Instance.Definitions.Goods["Technology"].Icon + ":" + playerGoods.Get("Technology") + "(+" + income.Get("Technology") + ") ";
        toPrint += Game.Instance.Definitions.Goods["Energy"].Icon + ":" + playerGoods.Get("Energy") + "(+" + income.Get("Energy") + ") ";
        toPrint += Game.Instance.Definitions.Goods["Credits"].Icon + ":" + playerGoods.Get("Credits") + "(+" + income.Get("Credits") + ") ";
        Terminal.PrintAt(playerInfoX, playerInfoY + 1, toPrint, false, Terminal.Size.x);        
    }
}