using System;

[Serializable]
public struct MapCoord
{
    public int x, z;

    public MapCoord(int pX, int pZ)
    {
        x = pX;
        z = pZ;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MapCoord))
            return false;

        return this == (MapCoord)obj;
    }

    public static bool operator ==(MapCoord c1, MapCoord c2)
    {
        return c1.x == c2.x && c1.z == c2.z;
    }

    public static bool operator !=(MapCoord c1, MapCoord c2)
    {
        return !(c1 == c2);
    }

    public override int GetHashCode()
    {
        return x ^ z;
    }

    public override string ToString()
    {
        return x.ToString() + "," + z.ToString();
    }
}