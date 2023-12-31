using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PathFinding : MonoBehaviour
{

    static public Vector3 CellCoordinates(HexCell cell){
        return new Vector3(cell.coordinates.X, cell.coordinates.Y, cell.coordinates.Z);
    }

    static public Vector3 cube_subtract(HexCell a, HexCell b){
        Vector3 firstCell = CellCoordinates(a);
        Vector3 secondCell = CellCoordinates(b);
        return new Vector3(firstCell.x - secondCell.x, firstCell.y - secondCell.y, firstCell.z - secondCell.z);
    }

    static public float cube_distance(HexCell a, HexCell b){
        Vector3 vec = cube_subtract(a, b);
        return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 2;
    }

    static public int GetMoveCost(HexCell fromCell, HexCell toCell, int slopeCost){
        int moveCost = 0;
        HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
        moveCost = edgeType == HexEdgeType.Slope ? slopeCost : 0;
        moveCost += toCell.movementCost;
        return moveCost;
    }


}
