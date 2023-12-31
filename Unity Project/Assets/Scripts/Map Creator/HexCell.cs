using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public Color color;
	public int terrain;
    public int index;
	public Unit unit;
	public Text label;
	public HexCell[] neighbors = new HexCell[6];
	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}
	public int Elevation {
		get {
			return elevation;
		}
		set {
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			transform.localPosition = position;
		}
	}
	int elevation;
	public HexCell parent;
	public float distance;
	public int movementCost;
	public HexCell PathFrom { get; set; }
	public float totalCostFunc;

	void Start(){
		DisableHighlight();
	}
    public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}
	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}
   	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

   	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
	}
	public void DisableHighlight () {
		SpriteRenderer highlight = transform.GetChild(0).GetComponent<SpriteRenderer>();
		highlight.enabled = false;
	}
	public void EnableHighlight (Color color) {
		SpriteRenderer highlight = transform.GetChild(0).GetComponent<SpriteRenderer>();
		highlight.color = color;
		highlight.enabled = true;
	}
}
