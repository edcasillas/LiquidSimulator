using System;
using UnityEngine;

namespace Liquids2D {
	public class Grid : MonoBehaviour {
		private const int width = 80;
		private const int height = 40;

		[SerializeField] private GameObject View; // Camera view
		[SerializeField] private GridLine gridLinePrefab;
		[SerializeField] private Cell cellPrefab;
		[SerializeField] private Sprite[] liquidFlowSprites;

		[SerializeField]
		[Range(0.1f, 1f)]
		private float CellSize = 1;

		private float PreviousCellSize = 1;

		[SerializeField]
		[Range(0f, 0.1f)]
		private float LineWidth = 0;

		private float PreviousLineWidth = 0;

		[SerializeField]
		private Color LineColor = Color.black;

		private Color PreviousLineColor = Color.black;

		[SerializeField]
		private bool ShowFlow = true;

		[SerializeField] private bool RenderDownFlowingLiquid = false;
		[SerializeField] private bool RenderFloatingLiquid = false;

		private Cell[,] Cells;
		private GridLine[] HorizontalLines;
		private GridLine[] VerticalLines;

		private LiquidSimulator LiquidSimulator;

		private bool Fill;

		private void Awake() {
			// Generate our viewable grid GameObjects
			CreateGrid ();

			// Initialize the liquid simulator
			LiquidSimulator = new LiquidSimulator ();
			LiquidSimulator.Initialize(Cells);
		}

		private void CreateGrid() {

			Cells = new Cell[width, height];
			Vector2 offset = this.transform.position;

			// Organize the grid objects
			GameObject gridLineContainer = new GameObject ("GridLines");
			GameObject cellContainer = new GameObject ("Cells");
			gridLineContainer.transform.parent = this.transform;
			cellContainer.transform.parent = this.transform;

			// vertical grid lines
			VerticalLines = new GridLine[width + 1];
			for (int x = 0; x < width + 1; x++) {
				var line = Instantiate(gridLinePrefab);
				float xpos = offset.x + (CellSize * x) + (LineWidth * x);
				line.Set (LineColor, new Vector2 (xpos, offset.y), new Vector2 (LineWidth, (height*CellSize) + LineWidth * height + LineWidth));
				line.transform.parent = gridLineContainer.transform;
				VerticalLines [x] = line;
			}

			// horizontal grid lines
			HorizontalLines = new GridLine[height + 1];
			for (int y = 0; y < height + 1; y++) {
				var line = Instantiate(gridLinePrefab);
				float ypos = offset.y - (CellSize * y) - (LineWidth * y);
				line.Set (LineColor, new Vector2 (offset.x, ypos), new Vector2 ((width*CellSize) + LineWidth * width + LineWidth, LineWidth));
				line.transform.parent = gridLineContainer.transform;
				HorizontalLines [y] = line;
			}

			// Cells
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Cell cell = Instantiate(cellPrefab);
					float xpos = offset.x + (x * CellSize) + (LineWidth * x) + LineWidth;
					float ypos = offset.y - (y * CellSize) - (LineWidth * y) - LineWidth;
					cell.Set (x, y, new Vector2 (xpos, ypos), CellSize, liquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);

					// add a border
					if (x == 0 || y == 0 || x == width - 1 || y == height - 1) {
						cell.SetType ( CellType.Solid );
					}

					cell.transform.parent = cellContainer.transform;
					Cells [x, y] = cell;
				}
			}
			UpdateNeighbors ();
		}

		// Live update the grid properties
		private void RefreshGrid() {

			Vector2 offset = this.transform.position;

			// vertical grid lines
			for (int x = 0; x < width + 1; x++) {
				float xpos = offset.x + (CellSize * x) + (LineWidth * x);
				VerticalLines [x].Set (LineColor, new Vector2 (xpos, offset.y), new Vector2 (LineWidth, (height*CellSize) + LineWidth * height + LineWidth));
			}

			// horizontal grid lines
			for (int y = 0; y < height + 1; y++) {
				float ypos = offset.y - (CellSize * y) - (LineWidth * y);
				HorizontalLines [y] .Set (LineColor, new Vector2 (offset.x, ypos), new Vector2 ((width*CellSize) + LineWidth * width + LineWidth, LineWidth));
			}

			// Cells
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					float xpos = offset.x + (x * CellSize) + (LineWidth * x) + LineWidth;
					float ypos = offset.y - (y * CellSize) - (LineWidth * y) - LineWidth;
					Cells [x, y].Set (x, y, new Vector2 (xpos, ypos), CellSize, liquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);
				}
			}

			// Fit camera to grid
			View.transform.position = this.transform.position + new Vector3(HorizontalLines [0].transform.localScale.x/2f, -VerticalLines [0].transform.localScale.y/2f);
			View.transform.localScale = new Vector2 (HorizontalLines [0].transform.localScale.x, VerticalLines [0].transform.localScale.y);
			Camera.main.GetComponent<Camera2D> ().Set ();
		}

		// Sets neighboring cell references
		private void UpdateNeighbors() {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (x > 0) {
						Cells[x, y].Left = Cells [x - 1, y];
					}
					if (x < width - 1) {
						Cells[x, y].Right = Cells [x + 1, y];
					}
					if (y > 0) {
						Cells[x, y].Top = Cells [x, y - 1];
					}
					if (y < height - 1) {
						Cells[x, y].Bottom = Cells [x, y + 1];
					}
				}
			}
		}

		private void Update () {

			// Update grid lines and cell size
			if (PreviousCellSize != CellSize || PreviousLineColor != LineColor || PreviousLineWidth != LineWidth) {
				RefreshGrid ();
			}

			// Convert mouse position to Grid Coordinates
			Vector2 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			int x = (int)((pos.x - this.transform.position.x) / (CellSize + LineWidth));
			int y = -(int)((pos.y - this.transform.position.y) / (CellSize + LineWidth));

			// Check if we are filling or erasing walls
			if (Input.GetMouseButtonDown (0)) {
				if ((x > 0 && x < Cells.GetLength (0)) && (y > 0 && y < Cells.GetLength (1))) {
					if (Cells [x, y].Type == CellType.Blank) {
						Fill = true;
					} else {
						Fill = false;
					}
				}
			}

			// Left click draws/erases walls
			if (Input.GetMouseButton (0)) {
				if (x != 0 && y != 0 && x != width - 1 && y != height - 1) {
					if ((x > 0 && x < Cells.GetLength (0)) && (y > 0 && y < Cells.GetLength (1))) {
						if (Fill) {
							Cells [x, y].SetType(CellType.Solid);
						} else {
							Cells [x, y].SetType(CellType.Blank);
						}
					}
				}
			}

			// Right click places liquid
			if (Input.GetMouseButton(1)) {
				if ((x > 0 && x < Cells.GetLength (0)) && (y > 0 && y < Cells.GetLength (1))) {
					Cells [x, y].AddLiquid (5);
				}
			}

			// Run our liquid simulation
			LiquidSimulator.Simulate (ref Cells);
		}

		// Load some sprites to show the liquid flow directions
		private void Reset() => liquidFlowSprites = Resources.LoadAll<Sprite>("LiquidFlowSprites");
	}
}
