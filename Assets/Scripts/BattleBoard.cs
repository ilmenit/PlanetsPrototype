public class BattleBoard
{
    public static MapCoord Size = new MapCoord(3, 4);
    public BattleCell[] Cells;

    public void Init()
    {
        Cells = new BattleCell[Size.z * Size.x];

        // create cells in array
        for (int z = 0; z < Size.z; z++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Cells[z * Size.x + x] = new BattleCell(new MapCoord(x, z));
            }
        }

        for (int x = 0; x < Size.x; x++)
        {
            GetCell(x, 0).CellInFront = GetCell(x, 1);
            GetCell(x, 3).CellInFront = GetCell(x, 2);
        }
    }

    public BattleBoard()
    {
        Init();
    }

    public bool OnBoard(int x, int z)
    {
        if (x >= 0 && x < Size.x && z >= 0 && z < Size.z)
            return true;
        else
            return false;
    }

    // getting cell with check if it's on the map
    public BattleCell GetCell(MapCoord pos)
    {
        return GetCell(pos.x, pos.z);
    }

    public BattleCell GetCell(int x, int z)
    {
        if (OnBoard(x, z))
            return Cells[z * Size.x + x];
        else
            return null;
    }
}