using System;

[Serializable]
public class Map
{
    public MapCoord Size;

    // not a 2d array because FullSerializer does not support it
    public MapCell[] Cells;

    public static int Distance(MapCoord start, MapCoord end)
    {
        return Math.Abs(end.x - start.x) + Math.Abs(end.z - start.z);
    }

    public static int Distance(MapCell start, MapCell end)
    {
        return Math.Abs(end.Position.x - start.Position.x) + Math.Abs(end.Position.z - start.Position.z);
    }


    public void Resize(MapCoord newSize)
    {
        Size.x = newSize.x;
        Size.z = newSize.z;
        Cells = new MapCell[newSize.z * newSize.x];
        // create cells in array
        for (int z = 0; z < newSize.z; z++)
        {
            for (int x = 0; x < newSize.x; x++)
            {
                Cells[z * Size.x + x] = new MapCell(new MapCoord(x, z));
            }
        }
    }

    public bool OnMap(MapCoord p)
    {
        if (p.x > 0 && p.x < Size.x && p.z > 0 && p.z < Size.z)
            return true;
        else
            return false;
    }

    public bool OnMap(int x, int z)
    {
        if (x >= 0 && x < Size.x && z >= 0 && z < Size.z)
            return true;
        else
            return false;
    }

    // getting cell with check if it's on the map
    public MapCell GetCell(MapCoord pos)
    {
        return GetCell(pos.x, pos.z);
    }

    public MapCell GetCell(int x, int z)
    {
        if (OnMap(x, z))
            return Cells[z * Size.x + x];
        else
            return null;
    }

    public void SetCell(int x, int z, MapCell cell)
    {
        Cells[z * Size.x + x] = cell;
    }
}