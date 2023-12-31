using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
	const float rotationSpeed = 180f;
	const float travelSpeed = 4f;
	public float movementSpeed = 0f;

	public static Unit unitPrefab;
	public HexCell cellPosition;
	public HexGrid hexGrid;

	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.unit = null;
			}
			location = value;
			value.unit = this;
			transform.localPosition = value.Position;
		}
	}

	private HexCell location;
	public float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}
	float orientation;
    bool moving = false;
	public List<HexCell> pathToTravel;
    LineRenderer line;
    void Start ()
    {
        line = GetComponent<LineRenderer>(); 
    }
	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidDestination (HexCell cell) {
		return !cell.unit;
	}
    public List<Vector3> points;
	public void TravelDraw (List<HexCell> path) {
        pathToTravel = path;
        points = new List<Vector3>();
        for (int j = 0; j < pathToTravel.Count; j++)
        {
            points.Add(new Vector3(pathToTravel[j].Position.x, 4f, pathToTravel[j].Position.z));
        }
        points.Reverse();
        line.positionCount = path.Count;
		line.SetPositions(points.ToArray());
	}
	public void Travel(List<HexCell> path)
	{
		hexGrid.DisableAllButton();
		Location = path[path.Count - 1];
		pathToTravel = path;
		points = new List<Vector3>();
		for (int j = 0; j < pathToTravel.Count; j++)
		{
			points.Add(new Vector3(pathToTravel[j].Position.x, 4f, pathToTravel[j].Position.z));
		}
		points.Reverse();
		line.positionCount = path.Count;
		line.SetPositions(points.ToArray());

		StopAllCoroutines();
		StartCoroutine(TravelPath());
	}

	IEnumerator TravelPath()
	{
		Vector3 a, b, c = pathToTravel[0].Position;

		for (int i = 1; i < pathToTravel.Count; i++)
		{
			movementSpeed = travelSpeed / ((pathToTravel[i].movementCost) / 30);
			line.SetPositions(points.ToArray());
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;

			float t = 0f;
			while (t < 1f)
			{
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				LookAt(pathToTravel[i].Position);
				t += Time.deltaTime * travelSpeed;
				yield return null;
			}

			pathToTravel[i - 1].unit = null;
			pathToTravel[i - 1].DisableHighlight();
			pathToTravel[i].unit = this;
			if (i != pathToTravel.Count - 1)
				pathToTravel[i].EnableHighlight(Color.red);
			line.positionCount--;
		}

		transform.localPosition = location.Position;
		hexGrid.EnableAllButton();
		hexGrid.AStarAlgorithm();
	}

	void LookAt(Vector3 point)
	{
		point.y = transform.localPosition.y;
		
		Vector3 lookAtPoint = point;

		Quaternion toRotation = Quaternion.LookRotation(lookAtPoint - transform.localPosition);

		float speed = rotationSpeed * Time.deltaTime;
		transform.localRotation = Quaternion.Slerp(transform.localRotation, toRotation, speed);

		orientation = transform.localRotation.eulerAngles.y;
	}

	public void Die () {
		location.unit = null;
		Destroy(gameObject);
	}

	public void OnEnable () {
		if (location) {
			transform.localPosition = location.Position;
		}
	}
}

public static class Bezier {
	public static Vector3 GetPoint (Vector3 a, Vector3 b, Vector3 c, float t) {
		float r = 1f - t;
		return r * r * a + 2f * r * t * b + t * t * c;
	}
}