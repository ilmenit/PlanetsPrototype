// Race names:
// Origin - Earth-lings
// Race type - Reptil-ians
// Shape - Tharg-oids

using System.Collections.Generic;

public class Player
{
    // player definition
    public int Index;
    public string Name;
    public bool ControledByAI = false;

    // gathered during the game
    public Goods AvailableGoods = new Goods();
    public List<string> Fleet = new List<string>();
    public List<string> FleetInBattle = new List<string>();
    public List<string> Tactics = new List<string>();
    public ResearchState ResearchState = new ResearchState();

    // costs
    public int CellAcquisitionCost;
    public int BuildingCostDivider;
    public int TransformationCost;
    public int SupplyLineCost;
    public int SupplyLineRange;
    public int ExploreRange;
    // public int ColonizationCost;

    public Player()
    {

    }

    public Player(string Name, int Index, bool ControledByAI = false, bool KnowsWarp = true)
    {
        this.Name = Name;
        this.Index = Index;
        this.ControledByAI = ControledByAI;

        CellAcquisitionCost = 1;
        BuildingCostDivider = 1;
        SupplyLineCost = 2;
        SupplyLineRange = 2;
        ExploreRange = 1;
        TransformationCost = 4;

        Engine.Instance.Invent("Initial", this);
        Engine.Instance.Invent("Exohabitation", this);

        if (KnowsWarp)
            Engine.Instance.Invent("Warp Portals", this);

        //Tactics.Add("Attack Again");
        //Tactics.Add("Quick Fix");
        // Fleet.Add("Fighter");
        
        /*
        foreach (var unit in Unit.Definitions)
        {
            Fleet.Add(unit.Name);
            Fleet.Add(unit.Name);
        }
        */
    }


}