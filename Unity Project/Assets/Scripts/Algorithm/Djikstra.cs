using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Djikstra
{
    public class PathResults
    {
        public List<HexCell> ShortestPath { get; set; }
        public List<HexCell> ExtendedPath { get; set; }
    }
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> heap = new List<T>();

        public int Count => heap.Count;

        public void Enqueue(T item)
        {
            heap.Add(item);
            int i = heap.Count - 1;

            while (i > 0)
            {
                int parent = (i - 1) / 2;

                if (heap[i].CompareTo(heap[parent]) >= 0)
                    break;

                Swap(i, parent);
                i = parent;
            }
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Queue is empty.");

            T root = heap[0];
            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);

            int i = 0;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < heap.Count && heap[left].CompareTo(heap[smallest]) < 0)
                    smallest = left;

                if (right < heap.Count && heap[right].CompareTo(heap[smallest]) < 0)
                    smallest = right;

                if (smallest == i)
                    break;

                Swap(i, smallest);
                i = smallest;
            }

            return root;
        }

        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }

    static public PathResults FindPathDjikstra(HexCell startCell, HexCell endCell, int slopeCost)
    {
        List<HexCell> extendedPath = new List<HexCell>();

        List<HexCell> shortestPaths = new List<HexCell>();
        
        PriorityQueue<Path> priorityQueue = new PriorityQueue<Path>();
        HashSet<HexCell> visitedCells = new HashSet<HexCell>();

        priorityQueue.Enqueue(new Path(startCell));

        while (priorityQueue.Count > 0)
        {
            Path currentPath = priorityQueue.Dequeue();
            HexCell currentCell = currentPath.LastCell;

            if (currentCell == endCell)
            {
                shortestPaths = currentPath.ToList();
            }

            visitedCells.Add(currentCell);

            foreach (var neighbor in currentCell.neighbors)
            {
                if (neighbor != null && !visitedCells.Contains(neighbor))
                {
                    visitedCells.Add(neighbor);
                    extendedPath.Add(neighbor);
                    priorityQueue.Enqueue(currentPath.Extend(neighbor, slopeCost));
                }
            }
        }

        return new PathResults {ShortestPath = shortestPaths, ExtendedPath = extendedPath} ;
    }


    public class Path : IComparable<Path>
    {
        public List<HexCell> Cells { get; private set; }
        public HexCell LastCell => Cells[Cells.Count - 1];
        public int Cost { get; private set; }

        public Path(HexCell startCell)
        {
            Cells = new List<HexCell> { startCell };
            Cost = 0;
        }

        private Path(List<HexCell> cells, int cost)
        {
            Cells = cells;
            Cost = cost;
        }

        public Path Extend(HexCell newCell, int slopeCost)
        {
            List<HexCell> newCells = new List<HexCell>(Cells);
            newCells.Add(newCell);

            int newCost = Cost + PathFinding.GetMoveCost(LastCell, newCell, slopeCost);

            return new Path(newCells, newCost);
        }

        public int CompareTo(Path other)
        {
            return Cost.CompareTo(other.Cost);
        }

        public List<HexCell> ToList()
        {
            return new List<HexCell>(Cells);
        }
    }
}
