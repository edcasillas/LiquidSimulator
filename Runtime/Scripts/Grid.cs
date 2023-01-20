using CommonUtils.UnityComponents;
using UnityEngine;

namespace Liquids2D {
	public class Grid : MonoBehaviour {
		public const int Width = 80;
		public const int Height = 40;

		[SerializeField] private GameObject View; // Camera view
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

		private Color LineColor => gridLineRenderer.Color;

		private Color PreviousLineColor = Color.black;

		[SerializeField]
		private bool ShowFlow = true;

		[SerializeField] private bool RenderDownFlowingLiquid = false;
		[SerializeField] private bool RenderFloatingLiquid = false;

		private Cell[,] Cells;
		private GridLine[] HorizontalLines => gridLineRenderer.HorizontalLines;
		private GridLine[] VerticalLines => gridLineRenderer.VerticalLines;

		private LiquidSimulator LiquidSimulator;

		private bool Fill;

		private IGridLineRenderer gridLineRenderer;

		private void Awake() {
			gridLineRenderer = GetComponent<IGridLineRenderer>();

			// Generate our viewable grid GameObjects
			CreateGrid ();

			// Initialize the liquid simulator
			LiquidSimulator = new LiquidSimulator ();
			LiquidSimulator.Initialize(Cells);
		}

		private void CreateGrid() {

			Cells = new Cell[Width, Height];
			Vector2 offset = transform.position;

			// Organize the grid objects
			GameObject cellContainer = new GameObject ("Cells");
			cellContainer.transform.parent = transform;

			if(gridLineRenderer.IsValid()) gridLineRenderer.CreateGridLines(offset, CellSize, LineWidth);

			// Cells
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					Cell cell = Instantiate(cellPrefab);
					float xpos = offset.x + (x * CellSize) + (LineWidth * x) + LineWidth;
					float ypos = offset.y - (y * CellSize) - (LineWidth * y) - LineWidth;
					cell.Set (x, y, new Vector2 (xpos, ypos), CellSize, liquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);

					// add a border
					if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1) {
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

			if(gridLineRenderer.IsValid()) gridLineRenderer.RenderGridLines(offset, CellSize, LineWidth);

			// Cells
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
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
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					if (x > 0) {
						Cells[x, y].Left = Cells [x - 1, y];
					}
					if (x < Width - 1) {
						Cells[x, y].Right = Cells [x + 1, y];
					}
					if (y > 0) {
						Cells[x, y].Top = Cells [x, y - 1];
					}
					if (y < Height - 1) {
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
				if (x != 0 && y != 0 && x != Width - 1 && y != Height - 1) {
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
