using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class CellObject
{
    public string Id;
    public MapCell OwnerCell;

    public CellObject()
    {

    }

    public CellObject(string id, MapCell owner)
    {
        Id = id;
        OwnerCell = owner;
    }
}

[Serializable]
public class Terrain : CellObject
{
    public Terrain()
    {

    }

    public Terrain(string id, MapCell owner) : base(id, owner)
    {
    }
}

    [Serializable]
public class Building : CellObject
{
    public List <Building> InputBuildings;
    public List <Building> OutputBuildings;

    public Building()
    {

    }

    public Building(string id, MapCell owner) : base(id, owner)
    {
        InputBuildings = new List<Building>();
        OutputBuildings = new List<Building>();
    }

    public Goods GetProduction()
    {
        Goods production = new Goods();
        production.Add(GetProducedType(), GetProducedCount());
        return production;
    }

    public Goods GetDemandAsGoods()
    {
        Goods demand = new Goods();

        // we do not consider now multiple suppliers for the same building
        var BuildingDefinition = Game.Instance.Definitions.Buildings[Id];
        demand.Add(GetDemandedType(), GetDemandCount());
        return demand;
    }

    public string GetProducedType()
    {
        return Game.Instance.Definitions.Buildings[Id].ProducesId;
    }

    public string GetDemandedType()
    {
        return Game.Instance.Definitions.Buildings[Id].DemandsId;
    }


    public int GetProducedCount()
    {
        var BuildingDefinition = Game.Instance.Definitions.Buildings[Id];
        return BuildingDefinition.ProductionMultiplier * (1 + InputBuildings.Count) - OutputBuildings.Count;
    }

    public int GetDemandCount()
    {
        var BuildingDefinition = Game.Instance.Definitions.Buildings[Id];
        return 1 - InputBuildings.Count;
    }


}
