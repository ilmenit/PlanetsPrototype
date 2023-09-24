using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextBattleView : MonoBehaviour
{
    private const int viewCellSize = 5;
    public TerminalRect ClickableBattleBoard;
    public TerminalList FleetList;
    public TerminalText ClickableTurnButton;

    public Terminal Terminal;

    public string GetUnitSizeIcon(UnitSize size)
    {
        switch (size)
        {
            case UnitSize.Small:
                return "S";
            case UnitSize.Medium:
                return "M";
            case UnitSize.Large:
                return "L";
            default:
                return ":";
        }
    }

    public void ShowBattleBoardUnit(Unit unit, int x, int y)
    {
        Terminal.PrintAt(x, y, unit.Icon);
        Terminal.PrintAt(x, y + 1, "A" + GetUnitSizeIcon(unit.AttackSize) + unit.HPProportionalDamage());
        Terminal.PrintAt(x, y + 2, "H" + GetUnitSizeIcon(unit.Size) + unit.HitPoints.ToString("D2"));

        if (unit.Engaged > 0)
            Terminal.PrintAt(x, y + 3, unit.Engaged.ToString(), false, 2);

        if (Battle.Instance.CanAttack(Battle.Instance.SelectedUnit, unit))
        {
            int hpAfterAttack = unit.HitPoints - Battle.Instance.CalculateDamage(Battle.Instance.SelectedUnit, unit);
            if (hpAfterAttack < 0)
                hpAfterAttack = 0;
            Terminal.PrintAt(x+2, y + 3, hpAfterAttack.ToString("D2"));
        }

    }

    public void ShowBattleBoardCell(BattleCell cell)
    {
        Color selectedHighlight = new Color(.3f, .3f, .3f, 1.0f);
        Terminal.SetColor(Color.white);
        Color backgroundColor = cell.Owner.GetPlayerColor();
        if (cell.Unit != null && cell.Unit == Battle.Instance.SelectedUnit)
            backgroundColor += selectedHighlight;

        // check if cell is target
        if (Battle.Instance.Phase == BattlePhase.Attack)
        {
            if (cell.Unit != null && cell.Unit.Tapped)
                backgroundColor /= 2;
            if (Battle.Instance.SelectedUnit != null &&
                cell.Unit != null)
            {
                if (Battle.Instance.CanAttack(Battle.Instance.SelectedUnit, cell.Unit))
                    backgroundColor = new Color(.5f, .2f, .2f, 1.0f);
            }
        }
        int x = ClickableBattleBoard.Position.x + cell.Position.x * viewCellSize;
        int y = ClickableBattleBoard.Position.y + ((BattleBoard.Size.z - 1) - cell.Position.z) * viewCellSize;

        Terminal.SetColor(Color.white, backgroundColor);
        Terminal.PrintAt(x, y + 0, "    ");
        Terminal.PrintAt(x, y + 1, "    ");
        Terminal.PrintAt(x, y + 2, "    ");
        Terminal.PrintAt(x, y + 3, "    ");

        if (cell.Unit != null)
            ShowBattleBoardUnit(cell.Unit, x, y);
    }

    public void ShowBattleBoard()
    {
        ClickableBattleBoard.gameObject.SetActive(true);
        for (int z = 0; z < BattleBoard.Size.z; ++z)
        {
            for (int x = 0; x < BattleBoard.Size.x; ++x)
            {
                ShowBattleBoardCell(Battle.Instance.Board.GetCell(x, z));
            }
        }
    }

    public void BattleBoardClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        // Log.Print("Position " + position.ToString());
        // check if click is on the map
        MapCoord pos = new MapCoord(position.x / viewCellSize, ((consoleObject.Size.y - 1) - position.y) / viewCellSize);
        BattleCell cell = Battle.Instance.Board.GetCell(pos);
        if (cell != null)
        {
            Log.Print("Cell " + cell.Position);
            Battle.Instance.ViewBattleBoardClicked(cell);
        }
    }

    public void ShowPlayerData()
    {
        Terminal.SetColor(Color.white);
        Terminal.SetBackground(Battle.Instance.ActivePlayer.GetPlayerColor());
        string toPrint = Battle.Instance.ActivePlayer.Name;
        toPrint += ", Phase ";
        switch (Battle.Instance.Phase)
        {
            case BattlePhase.Start:
                toPrint += "Start";
                break;
            case BattlePhase.Deployment:
                toPrint += "Deployment";
                break;
            case BattlePhase.Attack:
                toPrint += "Attack";
                break;
            case BattlePhase.Reinforcement:
                toPrint += "Reinforcement";
                break;
            case BattlePhase.End:
                toPrint += "Battle Ends";
                break;
            default:
                break;
        }
        Terminal.PrintAt(0, 0, toPrint, true, 40);
    }

    public void ShowOptions()
    {
        // clear
        // ClickableTurnButton.Hide();
        Terminal.SetColor(Color.white, Color.grey);

        string turnText = "Turn";
        int unitsOfPlayer = Battle.Instance.CountUnitsOfPlayer(Battle.Instance.ActivePlayer);

        ClickableTurnButton.gameObject.SetActive(false);

        switch (Battle.Instance.Phase)
        {
            case BattlePhase.Deployment:
                if (Battle.Instance.CountUnitsOfPlayer(Battle.Instance.ActivePlayer) == 0)
                {
                    if (Battle.Instance.ActivePlayer == Battle.Instance.Attacker)
                        turnText = "Withdraw";
                    else if (Battle.Instance.ActivePlayer == Battle.Instance.Defender)
                        turnText = "Surrender";
                }
                else
                    turnText = "End Deploy";
                break;
            case BattlePhase.Attack:
                turnText = "Regroup";
                break;
            case BattlePhase.Reinforcement:
                turnText = "Cancel";
                break;
            case BattlePhase.End:
                turnText = "End Battle";
                break;
            default:
                break;
        }
        ClickableTurnButton.Text = turnText;
        ClickableTurnButton.gameObject.SetActive(true);
    }

    public List <string> GetAvailableUnits(Player player)
    {
        return new List<string>(player.Fleet);
    }

    public Histogram<string> GetAvailableUnitsHistogram(Player player)
    {
        return new Histogram<string>(Battle.Instance.GetUnitsToDeploy(player));
    }


    public void ShowUnitsList()
    {
        if (!(Battle.Instance.Phase == BattlePhase.Deployment || Battle.Instance.Phase == BattlePhase.Reinforcement))
            return;

        var unitsHistogram = GetAvailableUnitsHistogram(Battle.Instance.ActivePlayer);

        FleetList.Clear();
        foreach (var pair in unitsHistogram)
        {
            var line = TerminalText.Instantiate();

            if (pair.Key == Battle.Instance.SelectedUnitName)
                line.Color = Color.yellow;
            else
                line.Color = Color.white;

            line.Text = pair.Value + "x" + pair.Key;

            FleetList.Add(line);
        }
        FleetList.gameObject.SetActive(true);
        Terminal.Refresh();

    }

    public void ShowBattleView()
    {
        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.Map))
            ShowBattleBoard();

        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.PlayerData))
            ShowPlayerData();

        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.Options))
            ShowOptions();

        if (FlagsHelper.IsSet(Frontend.Instance.DirtyBits, Dirty.RightPanel))
            ShowUnitsList();

        if (Battle.Instance.Phase == BattlePhase.End)
        {
            Player winner = Battle.Instance.CheckWinningPlayer();
            Terminal.SetColor(Color.white, winner.GetPlayerColor());
            Terminal.PrintAt(10, 10, "Battle Won by " + winner.Name);
        }
    }

    public void UnitListClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        //Log.Print("Research list click position: " + position.ToString());
        var unitsHistogram = GetAvailableUnitsHistogram(Battle.Instance.ActivePlayer);

        if (position.y < unitsHistogram.Count)
        {
            Battle.Instance.DeselectUnit();
            Battle.Instance.SelectedUnitName = unitsHistogram.ElementAt(position.y).Key;
            Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);
        }
    }

    public void EnableListeners()
    {

    }

    public void DisableListeners()
    {

    }


    public void TurnButtonClickHandler(TerminalRect consoleObject, Vector2Int position)
    {
        Log.WhoAmI();
        Log.Print("TURN BUTTON CLICKED");
        Battle.Instance.SkipTurn();
    }

    public void Init()
    {
        Log.WhoAmI();
        ClickableBattleBoard = TerminalRect.Instantiate().SetPosition(1, 2).SetSize(viewCellSize * BattleBoard.Size.x, viewCellSize * BattleBoard.Size.z).SetOnClick(BattleBoardClickHandler);
        ClickableBattleBoard.gameObject.SetActive(false);
        ClickableTurnButton = TerminalText.Instantiate().SetPosition(28, 22).SetText("Turn").SetOnClick(TurnButtonClickHandler);
        ClickableTurnButton.gameObject.SetActive(false);
        FleetList = TerminalList.Instantiate().SetPosition(22, 2).SetOnClick(UnitListClickHandler);
        FleetList.gameObject.SetActive(false);
    }
}