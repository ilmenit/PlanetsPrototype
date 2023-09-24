public class BattleCell
{
    public MapCoord Position;
    public Player Owner;
    public Unit Unit;
    public BattleCell CellInFront;

    public BattleCell(MapCoord pos)
    {
        Position = pos;
        Unit = null;
        CellInFront = null;
    }
}