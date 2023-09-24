using System.Collections;
using System.Collections.Generic;

public class Histogram<TKey> : SortedDictionary<TKey, uint>
{
    public Histogram(IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
        {
            if (!ContainsKey(key))
                Add(key, 1);
            else
                this[key] += 1;
        }
    }
}