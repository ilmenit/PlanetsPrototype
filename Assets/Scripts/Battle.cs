using System.Collections.Generic;

/* Battle rules
 * - Units from the second row can attack only if there is no friendly unit in front
 * - Units in the back row can be attacked only if there is not friendly unit
 */

public enum BattlePhase
{
    Start,
    Deployment,
    Attack,
    Reinforcement,
    End
}

public class Battle : Singleton<Battle>
{
    public MapCell MapCellWhereBattleIs;
    public BattleBoard Board;
    public Player Attacker;
    public Player Defender;
    public Unit SelectedUnit;
    public Player ActivePlayer = null;
    public BattlePhase Phase;
    public int DeploymentsEnded;
    public string SelectedUnitName = "";

    private BattleCell viewClickedBattleBoardCell = null;
    public BattleCell SelectedBattleBoardCell = null;

    public void Init(MapCell mapCellWhereBattleIs, Player attacker, Player defender)
    {
        MapCellWhereBattleIs = mapCellWhereBattleIs;
        Attacker = attacker;
        Defender = defender;
        Board = new BattleBoard();
        ActivePlayer = attacker;
        Phase = BattlePhase.Deployment;

        Player topPlayer;
        Player bottomPlayer;
        if (attacker == Model.Instance.GuiPlayer)
        {
            bottomPlayer = Model.Instance.GuiPlayer;
            topPlayer = defender;
        }
        else if (defender == Model.Instance.GuiPlayer)
        {
            bottomPlayer = Model.Instance.GuiPlayer;
            topPlayer = attacker;
        }
        else
        {
            topPlayer = defender;
            bottomPlayer = attacker;
        }

        BattleCell cell;
        for (int x = 0; x < BattleBoard.Size.x; ++x)
        {
            cell = Board.GetCell(x, 0);
            cell.Owner = bottomPlayer;
            cell = Board.GetCell(x, 1);
            cell.Owner = bottomPlayer;
            cell = Board.GetCell(x, 2);
            cell.Owner = topPlayer;
            cell = Board.GetCell(x, 3);
            cell.Owner = topPlayer;
        }
    }

    public bool CanDeployUnit(Player player, BattleCell cell)
    {
        if (cell.Unit != null || cell.Owner != player)
            return false;
        return true;
    }

    public bool DeployUnit(string unitName, Player player, BattleCell cell)
    {
        Unit unitTemplate = Unit.FindDefinition(unitName);
        Unit unit = unitTemplate.Clone();
        unit.Player = player;
        cell.Unit = unit;
        unit.Cell = cell;
        player.Fleet.Remove(unitName);
        Frontend.Instance.SetDirty(Dirty.ClearAll);
        return true;
    }

    public void BattleDeploymentEnds()
    {
        Phase = BattlePhase.Attack;
        Frontend.Instance.SetDirty(Dirty.ClearAll);
    }


    public void BattleEnds()
    {
        // add units from the BattleField to player
        foreach(var cell in Board.Cells)
        {
            if (cell.Unit == null)
                continue;
            if (!cell.Unit.IsAlive())
                continue;

            cell.Unit.Player.FleetInBattle.Add(cell.Unit.Name);
            RemoveFromCell(cell.Unit);
        }

        Frontend.Instance.SetDirty(Dirty.ClearAll);

        Phase = BattlePhase.End;
        Engine.Instance.GameState.Fire(GameStateTrigger.BattleEnds);
    }

    public void NextPlayer()
    {
        SelectedUnitName = "";

        if (ActivePlayer == Attacker)
            ActivePlayer = Defender;
        else if (ActivePlayer == Defender)
            ActivePlayer = Attacker;

        if (ActivePlayer.ControledByAI)
            Engine.Instance.ActionController = this.GetAIAction;
        else
            Engine.Instance.ActionController = this.GetHumanAction;

        /*
        // for hot-seat game
        if (!ActivePlayer.ControledByAI)
        {
            Log.TurnEnd();
            model.GuiPlayer = model.ActivePlayer;
        }
        */
    }

    public void UntapAllUnits(Player player)
    {
        foreach (var cell in Board.Cells)
        {
            if (cell.Unit == null)
                continue;

            if (cell.Owner == player)
            {
                cell.Unit.Tapped = false;
                cell.Unit.Engaged = 0;
            }
        }
    }

    public bool AreAllUnitsTapped(Player player)
    {
        foreach (var cell in Board.Cells)
        {
            if (cell.Unit == null)
                continue;

            if (cell.Owner == player && !cell.Unit.Tapped)
                return false;
        }
        return true;
    }

    public void PlayerTurnStarts()
    {
        if (AreAllUnitsTapped(ActivePlayer))
            UntapAllUnits(ActivePlayer);
    }

    public bool IsUnitInFrontOf(Unit unit)
    {
        BattleCell unitCell = unit.Cell;
        if (unitCell.CellInFront == null)
            return false;

        if (unitCell.CellInFront.Unit != null)
            return true;

        return false;
    }

    public bool CanAttack(Unit attacker, Unit defender)
    {
        if (attacker == null ||
            defender == null ||
            defender.Cell.Owner == attacker.Cell.Owner)
            return false;

        // if there are units in the enemy front row then then you cannot attack the back row until Ranged

        if (IsUnitInFrontOf(attacker) || IsUnitInFrontOf(defender))
        {
            if (attacker.Traits.Contains("Ranged"))
                return true;
            else
                return false;
        }
        return true;
    }

    public void RemoveFromCell(Unit unit)
    {
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);
        BattleCell cell = unit.Cell;
        unit.Cell = null;
        cell.Unit = null;
    }

    public int CalculateDamage(Unit attacker, Unit defender)
    {
        int damage = attacker.HPProportionalDamage();

        if (attacker.AttackSize != UnitSize.None)
        {
            if (attacker.AttackSize == defender.Size)
                damage += damage / 2;
            else
                damage -= damage / 2;
        }

        damage += defender.Engaged;

        if (damage <= 0)
            damage = 1; // attack is always at least 1 to prevent infinite battles

        return damage;
    }

    public List<Unit> GetUnitsInRow(int row)
    {
        List<Unit> units = new List<Unit>();
        for (int x=0;x<BattleBoard.Size.x;++x)
        {
            var cell = Board.GetCell(x, row);
            if (cell.Unit != null)
                units.Add(cell.Unit);
        }
        return units;
    }

    public void Attack(Unit attacker, Unit defender, bool isFirstAttack)
    {
        Unit firstToAttack = attacker;
        Unit secondToAttack = defender;

        // always tap attacker on attack
        if (isFirstAttack)
        {
            attacker.Tapped = true;
            DeselectUnit();
        }


        // handle first strike
        if (defender.Traits.Contains("FirstStrike") && isFirstAttack)
        {
            if (!(attacker.Traits.Contains("FirstStrike") || attacker.Traits.Contains("SideAttack")))
            {
                firstToAttack = defender;
                secondToAttack = attacker;
            }
        }

        Loom.Instance.CallCoroutineOnMainThread(Frontend.Instance.ShowAttack(attacker, defender));

        int damage = CalculateDamage(firstToAttack, secondToAttack);
        secondToAttack.ReceiveDamage(damage);

        Loom.Instance.CallCoroutineOnMainThread(Frontend.Instance.ShowDamage(secondToAttack, damage));

        // handle SpreadAttack
        if (isFirstAttack && defender.IsAlive())
        {
            if (attacker.Traits.Contains("SpreadAttack"))
            {
                var units = GetUnitsInRow(defender.Cell.Position.z);
                units.Remove(defender);
                foreach (var unit in units)
                    Attack(attacker, unit, false);
            }
        }


        // handling return fire
        if (isFirstAttack && secondToAttack.IsAlive())
        {
            if (!attacker.Traits.Contains("SideAttack"))
            {
                if (CanAttack(secondToAttack, firstToAttack)) // return fire
                {
                    Attack(secondToAttack, firstToAttack, false);
                }
            }
        }

        // modify engage of defender 
        if (isFirstAttack && secondToAttack.IsAlive())
        {
            if (attacker.Traits.Contains("NoEngage"))
                defender.Engaged += 0;
            else if (attacker.Traits.Contains("DoubleEngage"))
                defender.Engaged += 2;
            else if (attacker.Traits.Contains("TrippleEngage"))
                defender.Engaged += 3;
            else
                defender.Engaged += 1;
        }

        // if attacker has "berserk" and enemy is dead, it gets untapped
        if (isFirstAttack)
        {
            if (attacker.Traits.Contains("Berserk") && !defender.IsAlive())                
                attacker.Tapped = false;
        }
    }

    public void SelectUnit(Unit unit)
    {
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);

        if (unit == null)
        {
            DeselectUnit();
            return;
        }
        SelectedUnit = unit;
    }

    public void DeselectUnit()
    {
        if (SelectedUnit == null)
            return;

        SelectedUnit = null;
        Frontend.Instance.SetDirty(Dirty.Map | Dirty.RightPanel);
    }

    public void SkipTurn()
    {
        if (Phase == BattlePhase.Deployment)
        {
            if (CheckWinningPlayer() != null)
            {
                BattleEnds();
                return;
            }
            DeploymentsEnded++;
            if (DeploymentsEnded == 2) // both players ended
            {
                BattleDeploymentEnds();
            }
            BattleTurnEnds();
        }
        else if (Phase == BattlePhase.Attack)
        {
            UntapAllUnits(ActivePlayer);
            BattleTurnEnds();
        }
    }

    public void BattleTurnEnds()
    {
        DeselectUnit();
        if (Phase == BattlePhase.Attack && CheckWinningPlayer() != null)
        {
            BattleEnds();
            return;
        }
        NextPlayer();
        PlayerTurnStarts();
        Frontend.Instance.SetDirty(Dirty.ClearAll);
    }

    public int CountUnitsOfPlayer(Player player)
    {
        if (player == null)
            return 0;

        int count = 0;
        foreach (var cell in Board.Cells)
        {
            if (cell.Unit == null)
                continue;

            if (cell.Unit.Player == player)
                ++count;
        }
        return count;
    }

    public Player CheckWinningPlayer()
    {
        if (CountUnitsOfPlayer(Attacker) == 0)
            return Defender;
        if (CountUnitsOfPlayer(Defender) == 0)
            return Attacker;
        return null;
    }

    public void GetAIAction()
    {
        BattleTurnEnds();
    }

    public void BattleCellClicked(BattleCell cell)
    {
        Log.WhoAmI();
        Frontend.Instance.SetDirty(Dirty.RightPanel | Dirty.Map); // also map to show selected cell

        if (Phase == BattlePhase.Deployment)
        {
            if (!string.IsNullOrEmpty(SelectedUnitName))
            {
                if (CanDeployUnit(ActivePlayer, cell))
                {
                    DeploymentsEnded = 0;
                    DeployUnit(SelectedUnitName, ActivePlayer, cell);
                    SelectedUnitName = "";
                    BattleTurnEnds();
                    return;
                }
            }
            if (cell.Unit == null)
                return;
            SelectUnit(cell.Unit);
        }
        else if (Phase == BattlePhase.Attack)
        {
            if (cell.Unit == null)
            {
                DeselectUnit();
                return;
            }

            if (SelectedUnit != null)
            {
                if (!SelectedUnit.Tapped &&
                    SelectedUnit.Cell.Owner == ActivePlayer &&
                    CanAttack(SelectedUnit, cell.Unit))
                {
                    Attack(SelectedUnit, cell.Unit, true);
                    BattleTurnEnds();
                    return;
                }
            }
            SelectUnit(cell.Unit);
        }
    }

    public void GetHumanAction()
    {
        BattleCell clickedBoardCell;
        lock (Engine.Instance.threadLock)
        {
            clickedBoardCell = viewClickedBattleBoardCell;
            viewClickedBattleBoardCell = null;
        }

        if (Phase == BattlePhase.Deployment)
        {
            if (GetUnitsToDeploy(ActivePlayer).Count == 0)
            {
                SkipTurn();
                return;
            }
        }
        // we can click cells only if we are active player and player that is looking at the gui
        if (clickedBoardCell != null)
        {
            BattleCellClicked(clickedBoardCell);
        }
    }

    public List<string> GetUnitsToDeploy(Player player)
    {
        return new List<string>(player.Fleet);
    }

    public void ViewBattleBoardClicked(BattleCell cell)
    {
        lock (Engine.Instance.threadLock)
        {
            viewClickedBattleBoardCell = cell;
        }
    }
}