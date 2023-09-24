// unity free data to make it processed in external thread
using System;
using System.Collections.Generic;

[Serializable]
public class Model : Singleton<Model>
{
    protected Model()
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    public Map Map;
    public List<Player> Players;
    public Player ActivePlayer;
    public Player GuiPlayer; // player that sees GUI
    public Dictionary<Player, string> Inventions;

    public int PlayerTurn;
    public Battle Battle;

    public void Clear()
    {
        Map = new Map();
        Players = new List<Player>();
        Inventions = new Dictionary<Player, string>();
        ActivePlayer = null;
        GuiPlayer = null;
        PlayerTurn = 1;
        Battle = Battle.Instance;
    }
}