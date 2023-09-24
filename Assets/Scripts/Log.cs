using System.Collections.Generic;
using System.Diagnostics;

public static class Log
{
    private static bool LogWhoIAM = false;

    private static List<string> actionsDoneThisTurn = new List<string>();

    public static void Print(string toPrint)
    {
        UnityEngine.Debug.Log(toPrint);
    }

    public static void Warning(string toPrint)
    {
        UnityEngine.Debug.LogWarning(toPrint);
    }

    public static void Error(string toPrint)
    {
        UnityEngine.Debug.LogError(toPrint);
    }

    public static void TurnAction(string action)
    {
        actionsDoneThisTurn.Add(action);
    }

    public static void TurnEnd()
    {
        Print("---- Actions performed this turn: " + actionsDoneThisTurn.Count.ToString() + " -----");
        foreach (string action in actionsDoneThisTurn)
        {
            Print(action);
        }
        actionsDoneThisTurn.Clear();
    }

    public static void WhoAmI(string toPrint = "")
    {
        if (!LogWhoIAM)
            return;

        StackTrace callStack = new StackTrace();
        StackFrame frame = callStack.GetFrame(1);
        var method = frame.GetMethod();
        if (toPrint.Length > 0)
            toPrint = " - " + toPrint;
        UnityEngine.Debug.Log("In " + method.DeclaringType.ToString() + ":" + method.Name + toPrint);
    }
}