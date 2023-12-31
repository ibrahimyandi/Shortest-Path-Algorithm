using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
	private int radius = 3;
	public HexCell cellPrefab;
	public HexCell[] cells;
	public Text[] labels;
	public List<GameObject> listItem;
	private Color32[] terrainColor = {
		new Color32(90, 235, 27, 255), // Plains
		new Color32(18, 74, 9, 255), // Forest
		new Color32(13, 189, 130, 255), // Marsh
		new Color32(176, 129, 21, 255), // Highlands
		new Color32(105, 24, 4, 255)}; // Mountain
	HexMesh hexMesh;
	Canvas gridCanvas;
	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;
	public GameObject highlight;
	public Transform[] featurePrefab;
	int[,] cellCoordinates;
	public HexCell selectedCell;
	public Unit startUnit;
	public Unit finishUnit;
	public Text TotalMovementCost;
	public Text TotalCheckedCellCount;
	private int[] terrainMoveCost = new int[5]{ 2, 8, 14, 18, 28 };
	//Plains, Forest, Marsh, Highlands, Mountain
	public Text cellLabelPrefab;
	List<HexCell> lastPath;
	public ToggleGroup toggleGroup;
	public Text pathText;
	List<HexCell> extendedPath = new List<HexCell>();
	public Canvas canvas;
	public GameObject itemTemplate;
	public GameObject content;
	public Slider radiusSlider;
	public Text radiusText;
	static public bool animProcess = false;
	public Toggle toggleCameraControl;
	private int heuristic = 10;
	private int slopeCost = 8;

	void Awake()
	{
		Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, 20 + 29*radius, Camera.main.transform.position.z);
		int totalCellCount = 1; 
		radiusText.text = radius.ToString();
		radiusSlider.value = radius;
		for (int i = 1; i <= radius; i++)
		{
			totalCellCount += 6 * i; 
		}
		int minCoordinate = -radius;
		int maxCoordinate = radius;

		int arraySize = maxCoordinate - minCoordinate + 1; 
		cellCoordinates = new int[arraySize, arraySize];		Vector3 currentPoint = transform.position;
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		cells = new HexCell[totalCellCount];
		labels = new Text[totalCellCount];
		int[] xz = new int[2] { 0, 0 };
		int[,] deltas = new int[,] { { 1, -1 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 0 } };
		CreateCell(0, 0, 0);
		for (int i = 0, index = 1; i < radius+1; i++)
		{
			int x = xz[0];
			int z = xz[1] + i;
			for (int j = 0; j < 6; j++)
			{
				for (int k = 0; k < i; k++)
				{
					x = x + deltas[j, 0];
					z = z + deltas[j, 1];
					CreateCell(x, -x - z, index++);
				}
			}
		}

		cells[0].unit = GameObject.Find("Start").GetComponent<Unit>();
		cells[0].unit.cellPosition = cells[0];
		cells[0].EnableHighlight(Color.red);

		cells[15].unit = GameObject.Find("Finish").GetComponent<Unit>();
		cells[15].unit.cellPosition = cells[15];
		cells[15].EnableHighlight(Color.blue);
	}
	void Start()
	{
		toggleCameraControl.isOn = Camera.main.GetComponent<CameraController>().cameraControl;
		foreach (HexCell cell in cells)
		{
			int[,] deltas = new int[,] { { 1, -1 }, { 1, 0 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { 0, -1 } };
			int i = 0;
			foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
			{
				int nX = cell.coordinates.X + deltas[i, 0];
				int nZ = cell.coordinates.Z + deltas[i, 1];
				if ((nX <= radius && nX >= -radius) && (-nX - nZ <= radius && -nX - nZ >= -radius) && (nZ <= radius && nZ >= -radius))
				{
					int arrayX = nX + radius;
					int arrayZ = nZ + radius;
					int nIndex = cellCoordinates[arrayX, arrayZ];

					cell.SetNeighbor(direction, cells[nIndex]);
				}
				i++;
			}
		}
		CreateTerrain(cells);
		hexMesh.Triangulate(cells);
		ShowCost();
		toggleGroup.GetComponent<ToggleGroup>();
		Slider[] sliders = canvas.GetComponentsInChildren<Slider>();
		foreach (Slider slider in sliders)
		{
			if (slider.name == "Slider_Radius")
			{
				slider.value = radius;
			}
			else if (slider.name == "Slider_Heuristic")
			{
				slider.value = heuristic;
			}
			else if (slider.name == "Slider_Plains")
			{
				slider.value = terrainMoveCost[0];
			}
			else if (slider.name == "Slider_Forest")
			{
				slider.value = terrainMoveCost[1];
			}
			else if (slider.name == "Slider_Marsh")
			{
				slider.value = terrainMoveCost[2];
			}
			else if (slider.name == "Slider_Highlands")
			{
				slider.value = terrainMoveCost[3];
			}
			else if (slider.name == "Slider_Mountain")
			{
				slider.value = terrainMoveCost[4];
			}
			else if (slider.name == "Slider_Slope")
			{
				slider.value = slopeCost;
			}
		}
	}
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleInput(0);
		}
		if (Input.GetMouseButtonDown(1))
		{
			HandleInput(1);
		}
	}
	public void ToggleCamera(){
		Camera.main.GetComponent<CameraController>().cameraControl = !Camera.main.GetComponent<CameraController>().cameraControl;
	}
	public void CameraReset(){
		Camera.main.transform.position = new Vector3(0f, 20 + 29*radius, 0f);
		Camera.main.transform.eulerAngles = new Vector3(90f, 0f, 0f);
	}
	public void QuitGame(){
		#if UNITY_EDITOR
        	UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	public void SliderUpdate(Slider slider){
		if (slider.name == "Slider_Radius")
		{
			radius = (int)radiusSlider.value;
			radiusText.text = radius.ToString();
			RandomMap();
			startUnit.cellPosition = cells[0];
			finishUnit.cellPosition = cells[1];
			startUnit.TravelDraw(lastPath);
		}
		else if (slider.name == "Slider_Heuristic")
		{
			heuristic = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = heuristic.ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Plains")
		{
			terrainMoveCost[0] = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = terrainMoveCost[0].ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Forest")
		{
			terrainMoveCost[1] = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = terrainMoveCost[1].ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Marsh")
		{
			terrainMoveCost[2] = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = terrainMoveCost[2].ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Highlands")
		{
			terrainMoveCost[3] = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = terrainMoveCost[3].ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Mountain")
		{
			terrainMoveCost[4] = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = terrainMoveCost[4].ToString();
			CellMovementCostUpdate(cells);
		}
		else if (slider.name == "Slider_Slope")
		{
			slopeCost = (int)slider.value;
			slider.GetComponentInChildren<Text>().text = slopeCost.ToString();
			CellMovementCostUpdate(cells);
		}
	}
	public void RandomMap()
	{
		for (int i = 0; i < cells.Length; i++)
		{
			Destroy(cells[i].gameObject);
			Destroy(labels[i].gameObject);
		}
		toggleGroup.transform.GetChild(1).GetComponent<Toggle>().isOn = true;
		Awake();
		Start();
		ShowCost();
		AStarAlgorithm();
	}
	public void AddFeature(HexCell cell, Vector3 position, int terrain)
	{
		Transform instance = Instantiate(featurePrefab[terrain]);
		instance.localPosition = new Vector3(0f, 0f, 3f);
		instance.transform.SetParent(cell.transform, false);
		instance.name = "Feature " + featurePrefab[terrain].name;
	}
	public void DisableAllButton()
	{
		animProcess = true;
		Button[] buttons = canvas.GetComponentsInChildren<Button>();
		Slider[] sliders = canvas.GetComponentsInChildren<Slider>();
		foreach (Button button in buttons)
		{
			if (button.name != "Button_QuitGame" && button.name != "Button_ResetCamera")
			{
				button.interactable = false;
			}
		}
		foreach (Slider slider in sliders)
		{
			slider.interactable = false;
		}
	}
	public void EnableAllButton()
	{
		animProcess = false;
		Button[] buttons = canvas.GetComponentsInChildren<Button>();
		Slider[] sliders = canvas.GetComponentsInChildren<Slider>();

		foreach (Button button in buttons)
		{
			button.interactable = true;
		}

		foreach (Slider slider in sliders)
		{
			slider.interactable = true;
		}
	}
	void HandleInput(int clickNumber)
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit) && animProcess == false)
		{
			if (clickNumber == 0)
			{
				TouchCellLeftClick(hit.point);
			}
			if (clickNumber == 1)
			{
				TouchCellRightClick(hit.point);
			}
		}
	}

	public void PathTextUpdate(string algorithm)
	{
		pathText.text = algorithm + " = ";
		for (int i = 0; i < lastPath.Count; i++)
		{
			if (i != lastPath.Count - 1)
			{
				pathText.text += lastPath[i].index.ToString() + ", ";
			}
			else
			{
				pathText.text += lastPath[i].index.ToString();
			}
		}
	}

	void TouchCellRightClick(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = cellCoordinates[coordinates.X + radius, coordinates.Z + radius];
		HexCell cell = cells[index];

		foreach (var item in cells)
		{
			item.DisableHighlight();
		}

		cell.EnableHighlight(Color.blue);
		startUnit.cellPosition.EnableHighlight(Color.red);

		finishUnit.cellPosition.unit = null;
		cell.unit = finishUnit;
		finishUnit.cellPosition = cell;
		finishUnit.transform.position = new Vector3(cell.transform.position.x, cell.transform.position.y, cell.transform.position.z);
		AStarAlgorithm();
	}

	void TouchCellLeftClick(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = cellCoordinates[coordinates.X + radius, coordinates.Z + radius];
		HexCell cell = cells[index];

		foreach (var item in cells)
		{
			item.DisableHighlight();
		}

		cell.EnableHighlight(Color.red);
		finishUnit.cellPosition.EnableHighlight(Color.blue);

		startUnit.cellPosition.unit = null;
		cell.unit = startUnit;
		startUnit.cellPosition = cell;
		startUnit.transform.position = new Vector3(cell.transform.position.x, cell.transform.position.y, cell.transform.position.z);
		AStarAlgorithm();
	}


	public void StartAnimUnit()
	{
		if (lastPath != null)
		{
			DisableAllButton();
			startUnit.Travel(lastPath);
			startUnit.cellPosition = lastPath[lastPath.Count - 1];
			lastPath = null;
			extendedPath = null;
		}
	}
	public void StartAnimAlgorithm()
	{
		if (extendedPath != null)
		{
			DisableAllButton();
			foreach (HexCell cell in cells)
			{
				cell.DisableHighlight();
			}
			StartCoroutine(AnimatePath());
		}

	}
	IEnumerator AnimatePath()
	{
		foreach (HexCell cell in extendedPath)
		{
			cell.EnableHighlight(Color.black);
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(3f);

		foreach (HexCell cell in extendedPath)
		{
			cell.DisableHighlight();
		}
		EnableAllButton();
	}

	public void ToggleEvent(Toggle toggle)
	{
		if (toggle.name == "Toggle_Index")
		{
			ShowIndex();
		}
		else if (toggle.name == "Toggle_MovementCost")
		{
			ShowCost();
		}
		else if (toggle.name == "Toggle_Hide")
		{
			HideLabel();
		}
		else if (toggle.name == "Toggle_CameraControl")
		{
			ToggleCamera();
		}
	}
	public void ShowIndex()
	{
		int index = 0;
		foreach (var item in labels)
		{
			item.GetComponent<Text>().text = index.ToString();
			index++;
		}
	}

	public void ShowCost()
	{
		int index = 0;
		foreach (var item in labels)
		{
			item.GetComponent<Text>().text = cells[index].movementCost.ToString();
			index++;
		}
	}

	public void HideLabel()
	{
		foreach (var item in labels)
		{
			item.GetComponent<Text>().text = "";
		}
	}

	public void movementCost(List<HexCell> path)
	{
		int totalCost = 0;
		for (int i = 0; i < path.Count - 1; i++)
		{
			totalCost += PathFinding.GetMoveCost(path[i], path[i + 1], slopeCost);
		}
		TotalMovementCost.text = "Total Cost " + totalCost.ToString();
		TotalCheckedCellCount.text = "Visited Cell Count " + extendedPath.Count.ToString();
	}

	public void BFSAlgorithm()
	{
		var pathResults = BFS.FindPathBFS(startUnit.cellPosition, finishUnit.cellPosition);
		lastPath = pathResults.ShortestPath;
		startUnit.TravelDraw(pathResults.ShortestPath);
		PathTextUpdate("BFS");
		extendedPath = pathResults.ExtendedPath;
		movementCost(pathResults.ShortestPath);
	}
	public void DFSAlgorithm()
	{
		var pathResults = DFS.FindPathDFS(startUnit.cellPosition, finishUnit.cellPosition);
		lastPath = pathResults.ShortestPath;
		startUnit.TravelDraw(pathResults.ShortestPath);
		PathTextUpdate("DFS");
		extendedPath = pathResults.ExtendedPath;
		movementCost(pathResults.ShortestPath);
	}
	public void DijkstraAlgorithm()
	{
		var pathResults = Djikstra.FindPathDjikstra(startUnit.cellPosition, finishUnit.cellPosition, slopeCost);
		lastPath = pathResults.ShortestPath;
		startUnit.TravelDraw(pathResults.ShortestPath);
		PathTextUpdate("Dijkstra's");
		extendedPath = pathResults.ExtendedPath;
		movementCost(pathResults.ShortestPath);
	}

	public void AStarAlgorithm()
	{
		var pathResults = AStar.FindPathAStar(startUnit.cellPosition, finishUnit.cellPosition, heuristic, slopeCost);
		lastPath = pathResults.ShortestPath;
		startUnit.TravelDraw(pathResults.ShortestPath);
		PathTextUpdate("A*");
		extendedPath = pathResults.ExtendedPath;
		movementCost(pathResults.ShortestPath);
	}

	public void GreddyAlgorithm()
	{
		var pathResults = Greddy.FindPathGreedy(startUnit.cellPosition, finishUnit.cellPosition, heuristic, slopeCost);
		lastPath = pathResults.ShortestPath;
		startUnit.TravelDraw(pathResults.ShortestPath);
		PathTextUpdate("Greddy");
		extendedPath = pathResults.ExtendedPath;
		movementCost(pathResults.ShortestPath);
	}

	public void LineRenderer(Button button)
	{
		Color colorRed = new Color32(231, 76, 60, 255);
		Color colorGreen = new Color32(46, 204, 113, 255);
		
		if (startUnit.GetComponent<LineRenderer>().enabled)
		{
			startUnit.GetComponent<LineRenderer>().enabled = false;
			
			button.GetComponent<Image>().color = colorRed;
		}
		else
		{
			startUnit.GetComponent<LineRenderer>().enabled = true;
			button.GetComponent<Image>().color = colorGreen;
		}
	}

	void CreateCell(int x, int z, int index)
	{
		Vector3 position;
		position.x = (x + z * 0.5f) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = -z * (HexMetrics.outerRadius * 1.5f);

		Text label = labels[index] = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = "0";

		HexCell cell = cells[index] = Instantiate<HexCell>(cellPrefab);
		cell.name = index + ". Cell[" + x + ", " + (-x - z) + ", " + z + "]";
		cellCoordinates[x + radius, z + radius] = index;
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.index = index;
		cell.label = label;

		GameObject highlightTemp = Instantiate<GameObject>(highlight);
		highlightTemp.transform.SetParent(cell.transform, false);
		highlightTemp.name = "Highlight";

		CellVisualEdit(cell, 0, 0, defaultColor);
	}
	public void CellMovementCostUpdate(HexCell[] cells){
		foreach (HexCell cell in cells)
		{
			cell.movementCost = terrainMoveCost[cell.terrain];
			cell.label.text = cell.movementCost.ToString();
		}
	}
	public void CreateTerrain(HexCell[] cells)
	{
		System.Random rnd = new System.Random();
		int mountainCount = rnd.Next(1, 5) * radius-3;
		for (int i = 0; i < mountainCount; i++)
		{
			int cellIndex = rnd.Next(0, cells.Length);
			CellVisualEdit(cells[cellIndex], 4, 2, terrainColor[4]);
			foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
			{
				HexCell neighbor = cells[cellIndex].GetNeighbor(direction);
				if (neighbor == null)
				{
					continue;
				}
				if (rnd.Next(0, 10) < 4)
				{
					CellVisualEdit(neighbor, 4, 2, terrainColor[4]);
				}
			}
		}
		foreach (HexCell cell in cells)
		{
			if (!(cell.terrain == 4 || cell.terrain == 3))
			{
				int terrainChance = rnd.Next(0, 75);
				if (0 <= terrainChance && terrainChance < 30)
				{
					CellVisualEdit(cell, 0, 0, terrainColor[0]);
				}
				else if (30 <= terrainChance && terrainChance < 55)
				{
					CellVisualEdit(cell, 1, 0, terrainColor[1]);
				}
				else if (55 <= terrainChance && terrainChance < 75)
				{
					CellVisualEdit(cell, 2, 0, terrainColor[2]);
				}
			}
		}
		foreach (HexCell cell in cells)
		{
			foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
			{
				HexCell neighbor = cell.GetNeighbor(direction);
				if (neighbor == null)
				{
					continue;
				}
				if (neighbor.terrain == 4 && cell.terrain != 4)
				{
					CellVisualEdit(cell, 3, 1, terrainColor[3]);
				}
			}
		}
	}
	public void CellVisualEdit(HexCell cell, int terrain, int elevation, Color color)
	{
		cell.color = color;
		cell.terrain = terrain;
		cell.Elevation = elevation;
		cell.movementCost = terrainMoveCost[terrain];
	}
	public HexCell GetCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = cellCoordinates[coordinates.X + radius, coordinates.Z + radius];
		return cells[index];
	}
	public void Refresh()
	{
		hexMesh.Triangulate(cells);
	}
}