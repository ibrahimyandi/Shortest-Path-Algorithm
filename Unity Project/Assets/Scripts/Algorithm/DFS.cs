using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DFS : MonoBehaviour
{
    public class PathResults
    {
        public List<HexCell> ShortestPath { get; set; }
        public List<HexCell> ExtendedPath { get; set; }
    }

static public PathResults FindPathDFS(HexCell fromCell, HexCell toCell)
    {
        if (fromCell == null || toCell == null)
        {
            return null;
        }

        HashSet<HexCell> visitedCells = new HashSet<HexCell>();
        Stack<HexCell> stack = new Stack<HexCell>();
        Dictionary<HexCell, HexCell> parentMap = new Dictionary<HexCell, HexCell>();

        stack.Push(fromCell);
        visitedCells.Add(fromCell);

        while (stack.Count > 0)
        {
            HexCell current = stack.Pop();

            if (current.Equals(toCell))
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
                    stack.Push(neighbor);
                }
            }
        }

        if (!visitedCells.Contains(toCell))
        {
            return null;
        }

        HexCell temp = toCell;
        List<HexCell> path = new List<HexCell>();

        while (temp != fromCell && temp != null)
        {
            path.Add(temp);
            temp = parentMap[temp];
        }

        path.Add(fromCell);
        path.Reverse();

        return new PathResults { ShortestPath = path, ExtendedPath = visitedCells.ToList() };
    }
}
