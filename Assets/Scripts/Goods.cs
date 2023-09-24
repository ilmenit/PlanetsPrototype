using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Goods
{
    private Dictionary<string, int> values = new Dictionary<string, int>();

    public Goods(string s1, int value)
    {
        Set(s1, value);
    }

    public Goods()
    {
    }


    // Stock1 < Stock2 when all values are smaller. We don't use operator< to avoid issues with >, >= and !=
    public bool FitsIn(Goods other)
    {
        // If other is not a valid object reference, this instance is greater.
        if (other == null)
            return false;

        foreach (var key in values.Keys)
        {
            if (Get(key) > other.Get(key))
                return false;
        }
        return true;
    }

    public bool Contains(string resource, int value=1)
    {
        if (Get(resource) >= value)
            return true;        

        return false;
    }


    public void Add(Goods other)
    {
        foreach (var key in other.values.Keys)
        {
            if (values.ContainsKey(key))
                values[key] += other.Get(key);
            else
                values[key] = other.Get(key);
        }
    }

    public void Sub(Goods other)
    {
        foreach (var key in other.values.Keys)
        {
            if (values.ContainsKey(key))
                values[key] -= other.Get(key);
            else
                values[key] = other.Get(key);
        }
    }

    public int Get(string type)
    {
        if (values == null) { throw new ArgumentNullException(nameof(values)); } // using C# 6
        if (type == null) { throw new ArgumentNullException(nameof(type)); } //  using C# 6

        int value;
        return values.TryGetValue(type, out value) ? value : 0;
    }

    public int Set(string type, int value)
    {
        values[type] = value;
        return values[type];
    }

    public int Add(string type, int value)
    {
        if (values.ContainsKey(type))
            values[type] += value;
        else
            values[type] = value;
        return values[type];
    }

    public int Sub(string type, int value)
    {
        if (values.ContainsKey(type))
            values[type] -= value;
        else
            values[type] = value;
        return values[type];
    }

    public void Set(Goods other)
    {
        values = new Dictionary<string, int>(other.values);
    }

    public static string RandomType()
    {
        var len = Game.Instance.Definitions.Goods.Count;
        var r = MyRandom.Instance.Next(len);
        return Game.Instance.Definitions.Goods.ElementAt(r).Key;
    }
}