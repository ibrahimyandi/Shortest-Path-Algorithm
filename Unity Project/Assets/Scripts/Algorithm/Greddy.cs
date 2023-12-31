using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

static public class Greddy
{
    public class PathResults
    {
        public List<HexCell> ShortestPath { get; set; }
        public List<HexCell> ExtendedPath { get; set; }
    }
    static public PathResults FindPathGreedy(HexCell fromCell, HexCell toCell, int heuristic, int slopeCost)
    {
        if (fromCell == null || toCell == null)
        {
            return null;
        }

        List<HexCell> openList = new List<HexCell>();
        List<HexCell> closeList = new List<HexCell>();
        List<HexCell> path = new List<HexCell>();

        if (fromCell == toCell)
        {
            return new PathResults
            {
                ShortestPath = new List<HexCell> { fromCell },
                ExtendedPath = new List<HexCell>()
            };
        }

        HexCell current = fromCell;
        openList.Add(fromCell);

        while (openList.Count > 0 && !closeList.Exists(x => x.Position == toCell.Position))
        {
            openList = openList.OrderBy(node => PathFinding.cube_distance(node, toCell)).ToList();
            current = openList[0];
            openList.Remove(current);
            closeList.Add(current);

            if (current.Position == toCell.Position)
            {
                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor != null && !closeList.Contains(neighbor))
                {
                    closeList.Add(neighbor);
                    float moveCost = PathFinding.GetMoveCost(current, neighbor, slopeCost);
                    if (!openList.Contains(neighbor))
                    {
                        neighbor.parent = current;
                        neighbor.distance = PathFinding.cube_distance(neighbor, toCell);
                        neighbor.totalCostFunc = neighbor.distance * heuristic;
                        openList.Add(neighbor);
                    }                
                }
            }
        }

        if (!closeList.Exists(x => x.Position == toCell.Position))
        {
            return null;
        }

        HexCell temp = closeList[closeList.IndexOf(toCell)];
        if (temp == null)
        {
            return null;
        }

        do
        {
            path.Add(temp);
            temp = temp.parent;
        } while (temp != fromCell && temp != null);

        path.Add(fromCell);
        path.Reverse();
        return new PathResults { ShortestPath = path, ExtendedPath = closeList };
    }
}
