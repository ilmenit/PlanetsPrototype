using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ResearchState 
{
    public List<string> DiscoveredInventions = new List<string>();
    public List<string> AllowedBuildings = new List<string>();
    public List<string> AllowedUnits = new List<string>();
    public List<string> AllowedTransformations = new List<string>();
    public List<string> Improvements = new List<string>();
    public Vector2Int Size = new Vector2Int();
    public bool [] ResearchMatrix;

    public ResearchState()
    {
        Resize(new Vector2Int(5, 7)); // !!! magic value
    }

    public void Resize(Vector2Int newSize)
    {
        Size = newSize;
        ResearchMatrix = new bool[newSize.y * newSize.x];
    }

    private bool IsInvented(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Size.x || pos.y < 0 || pos.y >= Size.y)
            return false;

        return ResearchMatrix[pos.y * Size.x + pos.x];
    }

    private void MarkAsInvented(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Size.x || pos.y < 0 || pos.y >= Size.y)
            return;

        ResearchMatrix[pos.y * Size.x + pos.x] = true;
    }

    public Vector2Int GetInventionPosition(string inventionName)
    {
        if (!Game.Instance.Definitions.Inventions.ContainsKey(inventionName))
        {
            Log.Warning("No invention " + inventionName);
            return new Vector2Int(-1, -1);
        }
        var inv = Game.Instance.Definitions.Inventions[inventionName];
        return new Vector2Int(inv.X, inv.Y);
    }

    public void MarkAsInvented(string inventionName)
    {
        if (DiscoveredInventions.Contains(inventionName))
            return;
        DiscoveredInventions.Add(inventionName);
        MarkAsInvented(GetInventionPosition(inventionName));

        foreach (var technology in Game.Instance.Definitions.Technologies)
        {
            var techName = technology.Key;
            if (technology.Value.Invention == inventionName)
            {
                // Log.Print("Allowing " + technology.Key);

                switch (technology.Value.Type)
                {
                    case "Building":
                        AllowedBuildings.Add(techName);
                        break;
                    case "Transformation":
                        AllowedTransformations.Add(techName);
                        break;
                    case "Unit":
                        AllowedUnits.Add(techName);
                        break;
                    case "Improvement":
                        Improvements.Add(techName);
                        break;
                    default:
                        throw new Exception("Unexpected Case: " + technology.Value.Type);
                }
            }
        }

    }

    private bool CanBeInvented(Vector2Int pos)
    {
        if (IsInvented(pos))
            return false;

        if (IsInvented(new Vector2Int(pos.x-1, pos.y)))
            return true;
        if (IsInvented(new Vector2Int(pos.x+1, pos.y)))
            return true;
        if (IsInvented(new Vector2Int(pos.x, pos.y-1)))
            return true;
        if (IsInvented(new Vector2Int(pos.x, pos.y+1)))
            return true;

        return false;
    }

    public bool CanBeInvented(string inventionName)
    {
        return CanBeInvented(GetInventionPosition(inventionName));
    }
}
