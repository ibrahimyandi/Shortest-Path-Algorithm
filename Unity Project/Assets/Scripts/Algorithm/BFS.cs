using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFS : MonoBehaviour
{
    public class PathResults
    {
        public List<HexCell> ShortestPath { get; set; }
        public List<HexCell> ExtendedPath { get; set; }
    }

    static public PathResults FindPathBFS(HexCell fromCell, HexCell toCell)
    {
        if (fromCell == null || toCell == null)
        {
            return null;
        }

        List<HexCell> visitedCells = new List<HexCell>();
        Queue<HexCell> queue = new Queue<HexCell>();
        Dictionary<HexCell, HexCell> parentMap = new Dictionary<HexCell, HexCell>();

        queue.Enqueue(fromCell);
        visitedCells.Add(fromCell);

        while (queue.Count > 0)
        {
            HexCell current = queue.Dequeue();

            if (current.Position == toCell.Position)
            {
                break;
            }

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor != null && !visitedCells.Contains(neighbor))
                {
                    visitedCells.Add(neighbor);
                    parentMap[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!visitedCells.Exists(x => x.Position == toCell.Position))
        {
            return null;
        }

        HexCell temp = toCell;
        List<HexCell> shortestPath = new List<HexCell>();

        while (temp != fromCell && temp != null)
        {
            shortestPath.Add(temp);
            temp = parentMap[temp];
        }

        shortestPath.Add(fromCell);
        shortestPath.Reverse();

        return new PathResults { ShortestPath = shortestPath, ExtendedPath = visitedCells };
    }
}
