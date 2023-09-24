using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MyExtensions
{
    private static readonly Color DefaultPlayerColor = new Color(.0f, .0f, .0f, 1.0f);

    public static Color GetPlayerColor(this Player player)
    {
        if (player == null)
            return DefaultPlayerColor;

        int colorID = player.Index % Game.Instance.Definitions.PlayerColors.Count;
        return Game.Instance.Definitions.PlayerColors[colorID.ToString()].Color;
    }

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = MyRandom.Instance.Next(n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string CapTo(this string str, int count)
    {
        if (str.Length < count)
            return str.PadRight(count,'.');
        else
            return str.Substring(0, count-1) + '.';
    }


    public static T RandomElement<T>(this List<T> list)
    {
        int n = list.Count;
        int r = MyRandom.Instance.Next(n);
        return list[r];
    }


    public static void AddToFront<T>(this List<T> list, T item)
    {
        // omits validation, etc.
        list.Insert(0, item);
    }

    public static T GetOfType<T>(this IList list)
    {
        foreach (var obj in list)
        {
            if (obj is T)
                return (T)obj;
        }
        return default(T);
    }

    public static T RemoveOfType<T>(this IList list)
    {
        T obj = list.GetOfType<T>();
        if (!EqualityComparer<T>.Default.Equals(obj, default(T)))
            list.Remove(obj);
        return obj;
    }

    public static IList<T> ShallowCopy<T>(this IList<T> listToClone)
    {
        return new List<T>(listToClone);
    }
}