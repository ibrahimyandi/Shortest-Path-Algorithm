using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public class AStar
{
    public class PathResults
    {
        public List<HexCell> ShortestPath { get; set; }
        public List<HexCell> ExtendedPath { get; set; }
    }
    static public PathResults FindPathAStar(HexCell fromCell, HexCell toCell, int heuristic, int slopeCost)
    {
        if (fromCell == null || toCell == null)
        {
            return null;
        }

        List<HexCell> openList = new List<HexCell>();
        List<HexCell> closeList = new List<HexCell>();
        List<HexCell> path = new List<HexCell>();

        openList.Add(fromCell);
        
        int cellsChecked = 0;  // Keep track of the number of cells checked

        while (openList.Count > 0)
        {
            HexCell current = openList.OrderBy(node => node.totalCostFunc).First();
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
                    float distance = PathFinding.cube_distance(neighbor, toCell);
                    float totalCost = distance * heuristic + moveCost;
                    cellsChecked++;

                    if (!openList.Contains(neighbor))
                    {
                        neighbor.parent = current;
                        neighbor.totalCostFunc = totalCost;
                        openList.Add(neighbor);
                    }
                    else if (totalCost < neighbor.totalCostFunc)
                    {
                        neighbor.parent = current;
                        neighbor.totalCostFunc = totalCost;
                    }
                }
            }
        }

        if (!closeList.Exists(x => x.Position == toCell.Position))
        {
            return null;
        }

        HexCell temp = closeList.First(cell => cell.Position == toCell.Position);

        while (temp != fromCell && temp != null)
        {
            path.Add(temp);
            temp = temp.parent;
        }

        path.Add(fromCell);
        path.Reverse();
        return  new PathResults { ShortestPath = path, ExtendedPath = closeList };
    }
}
