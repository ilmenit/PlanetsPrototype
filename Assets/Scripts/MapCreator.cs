using System.Collections.Generic;

public class MapCreator
{
    public List<MapCell> CellsToProcess = new List<MapCell>();
    public List<MapCell> ProcessedCells = new List<MapCell>();
    public List<MapCell> PlanetsCells = new List<MapCell>();
    public List<MapCell> SpaceObjectsCells = new List<MapCell>();
    public List<MapCell> LifeCells = new List<MapCell>();
    public List<string> ListOfTerrains = new List<string>();

    private void CreateLife(MapCell cell)
    {
        // do not create intelligent too close to the others
        int minLifeDistance = 4 + MyRandom.Instance.Next(2);
        foreach (var lifeCell in LifeCells)
        {
            int distance = Map.Distance(cell, lifeCell);
            if (distance < minLifeDistance)
                return;
        }

        var index = Model.Instance.Players.Count;
        var name = "Player" + index.ToString();
        bool knowsWarp = true;

        /*
        Race name generator:
        Origin - Earth-lings
        Race type - Reptil-(i)ans
        Type/Shape - Blob-oids
        */

        var newPlayer = new Player(name, index, true, knowsWarp);

        for (int i=0;i<8;++i)
        {
            string resource;
            switch(MyRandom.Instance.Next(3))
            {
                case 0:
                    resource = "Energy";
                    break;
                case 1:
                    resource = "Technology";
                    break;
                default:
                    resource = "Credits";
                    break;
            }
            newPlayer.AvailableGoods.Add(resource, MyRandom.Instance.Next(3));
        }
        Model.Instance.Players.Add(newPlayer);
        Engine.Instance.TakeCellOwnership(cell, newPlayer, false);
        LifeCells.Add(cell);
    }

    private void FillNewListOfTerrains()
    {
        foreach (var key in Game.Instance.Definitions.Terrains.Keys)
        {
            for (int i = 0; i < Game.Instance.Definitions.Terrains[key].Popularity; ++i)
                ListOfTerrains.Add(key);
        }
        ListOfTerrains.Shuffle();
    }

    private string GetNextTerrainFromTheList()
    {
        if (ListOfTerrains.Count == 0)
            FillNewListOfTerrains();

        var terrainName = ListOfTerrains.RandomElement();
        ListOfTerrains.Remove(terrainName);
        return terrainName;
    }

    private void CreatePlanet(MapCell cell)
    {
        Player owner = null;
        if (PlanetsCells.Count == 0)
            owner = Model.Instance.Players[0];

        PlanetsCells.Add(cell);
        cell.Type = CellType.Planet;
        cell.Name = "Planet " + PlanetNames.GetRandom();
        cell.Icon = "O";
        Engine.Instance.TakeCellOwnership(cell, owner, false);

        if (owner != null)
        {
            cell.Buildings.Add(new Building("Capital",cell));
            cell.Buildings.Add(new Building("Factories",cell));
            cell.Buildings.Add(new Building("Power Plant",cell));
            LifeCells.Add(cell);
        }
        else
        {
            bool hasLife = false;
            for (int i = 0; i < 3; ++i)
            {
                var terrainName = GetNextTerrainFromTheList();
                if (terrainName == "City" || terrainName == "Industrial")
                    hasLife = true;

                cell.Terrains.Add(new Terrain(terrainName, cell));
            }
            if (hasLife)
            {
                CreateLife(cell);
            }
        }
    }


    private void CreateSpaceOBject(MapCell cell)
    {
        SpaceObjectsCells.Add(cell);
        cell.Type = CellType.Mine;

        var terrainName = GetNextTerrainFromTheList();

        foreach (var item in Game.Instance.Definitions.SpaceObjects)
        {
            if (item.Value.TerrainType == terrainName)
            {
                cell.Name = item.Value.Name;
                cell.Icon = item.Value.Icon;
                cell.Terrains.Add(new Terrain(terrainName, cell));

                if (terrainName == "City" || terrainName == "Industrial")
                    CreateLife(cell);

                break;
            }
        }
    }


    private void ProcessCell(MapCell cellToProcess)
    {
        // if no planets around, start new planet
        int minPlanetDistance = 3 + MyRandom.Instance.Next(2);
        bool tooClose = false;

        foreach (var planet in PlanetsCells)
        {
            int distance = Map.Distance(cellToProcess, planet);
            if (distance < minPlanetDistance)
            {
                tooClose = true;
                break;
            }
        }
        if (!tooClose)
        {
            //if (PlanetsCells.Count == 0)
            {
                CreatePlanet(cellToProcess);
                return;
            }
        }

        // if no resources around, start new resource
        int minResourceDistance = 2;
        tooClose = false;

        foreach (var obj in SpaceObjectsCells)
        {
            int distance = Map.Distance(cellToProcess, obj);
            if (distance < minResourceDistance)
            {
                tooClose = true;
                break;
            }
        }
        if (!tooClose)
        {
            CreateSpaceOBject(cellToProcess);
            return;
        }
    }

    public void Create()
    {
        int mapSizeX = 16;
        int mapSizeZ = 16;

        Model.Instance.Map.Resize(new MapCoord(mapSizeX, mapSizeZ));

        Player owner = null;
        CellsToProcess.Add(Model.Instance.Map.GetCell(1, 1));
        while (CellsToProcess.Count != 0)
        {
            // select random cell
            MapCell cellToProcess = CellsToProcess.RandomElement();
            //MapCell cellToProcess = CellsToProcess[0];

            CellsToProcess.RemoveAll(x => x == cellToProcess);

            if (ProcessedCells.Contains(cellToProcess))
                continue;

            ProcessCell(cellToProcess);

            ProcessedCells.Add(cellToProcess);

            // extend list
            foreach (var cell in Engine.Instance.GetCellsAround(cellToProcess, false, true))
            {
                CellsToProcess.Add(cell);
            }
        }

    }
}
