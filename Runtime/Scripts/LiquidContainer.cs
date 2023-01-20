using CommonUtils;
using CommonUtils.UnityComponents;
using UnityEngine;

namespace Liquids2D {
	public interface ILiquidContainer {
		Vector2Int GridSize { get; }
		float CellSize { get; }
		float LineWidth { get; }
		Vector2 HorizontalLinesScale { get; }
		Vector2 VerticalLinesScale { get; }
	}

	public class LiquidContainer : EnhancedMonoBehaviour, ILiquidContainer {
		[SerializeField] private Vector2Int gridSize = new(80, 40);
		[SerializeField] private Camera2D camera2D;
		[SerializeField] private Cell cellPrefab;
		[SerializeField] private Sprite[] liquidFlowSprites;

		[SerializeField]
		[Range(0.1f, 1f)]
		private float cellSize = 1;

		public float CellSize => cellSize;

		private float PreviousCellSize = 1;

		[SerializeField]
		[Range(0f, 0.1f)]
		private float lineWidth = 0;

		public float LineWidth => lineWidth;
		public float Height => (GridSize.y * CellSize) + LineWidth * GridSize.y + LineWidth;
		public float Width => (GridSize.x * CellSize) + LineWidth * GridSize.x + LineWidth;

		private float PreviousLineWidth = 0;

		private Color LineColor => gridLineRenderer.Color;

		private Color PreviousLineColor = Color.black;

		[SerializeField]
		private bool ShowFlow = true;

		[SerializeField] private bool RenderDownFlowingLiquid = false;
		[SerializeField] private bool RenderFloatingLiquid = false;

		public Vector2Int GridSize => gridSize;

		private Cell[,] Cells;
		private GridLine[] HorizontalLines => gridLineRenderer.HorizontalLines;
		private GridLine[] VerticalLines => gridLineRenderer.VerticalLines;

		[ShowInInspector] public Vector2 HorizontalLinesScale => new(Width, LineWidth);
		[ShowInInspector] public Vector2 VerticalLinesScale => new(LineWidth, Height);

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

			Cells = new Cell[gridSize.x, gridSize.y];
			Vector2 offset = transform.position;

			// Organize the grid objects
			var cellContainer = new GameObject ("Cells").transform;
			cellContainer.parent = transform;

			if(gridLineRenderer.IsValid()) gridLineRenderer.CreateGridLines(offset, this);

			// Cells
			for (int x = 0; x < gridSize.x; x++) {
				for (int y = 0; y < gridSize.y; y++) {
					Cell cell = Instantiate(cellPrefab);
					float xpos = offset.x + (x * cellSize) + (lineWidth * x) + lineWidth;
					float ypos = offset.y - (y * cellSize) - (lineWidth * y) - lineWidth;
					cell.Set (x, y, new Vector2 (xpos, ypos), cellSize, liquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);

					// add a border
					if (x == 0 || y == 0 || x == gridSize.x - 1 || y == gridSize.y - 1) {
						cell.SetType ( CellType.Solid );
					}

					cell.transform.parent = cellContainer;
					Cells [x, y] = cell;
				}
			}
			UpdateNeighbors ();
		}

		// Live update the grid properties
		private void RefreshGrid() {

			Vector2 offset = this.transform.position;

			if(gridLineRenderer.IsValid()) gridLineRenderer.RenderGridLines(offset, this);

			// Cells
			for (int x = 0; x < gridSize.x; x++) {
				for (int y = 0; y < gridSize.y; y++) {
					float xpos = offset.x + (x * cellSize) + (lineWidth * x) + lineWidth;
					float ypos = offset.y - (y * cellSize) - (lineWidth * y) - lineWidth;
					Cells [x, y].Set (x, y, new Vector2 (xpos, ypos), cellSize, liquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);
				}
			}

			// Fit camera to grid
			camera2D.Target.position = transform.position + new Vector3(HorizontalLines [0].transform.localScale.x/2f, -VerticalLines [0].transform.localScale.y/2f);
			camera2D.Target.localScale = new Vector2 (HorizontalLines [0].transform.localScale.x, VerticalLines [0].transform.localScale.y);
			camera2D.Set();
		}

		// Sets neighboring cell references
		private void UpdateNeighbors() {
			for (int x = 0; x < gridSize.x; x++) {
				for (int y = 0; y < gridSize.y; y++) {
					if (x > 0) {
						Cells[x, y].Left = Cells [x - 1, y];
					}
					if (x < gridSize.x - 1) {
						Cells[x, y].Right = Cells [x + 1, y];
					}
					if (y > 0) {
						Cells[x, y].Top = Cells [x, y - 1];
					}
					if (y < gridSize.y - 1) {
						Cells[x, y].Bottom = Cells [x, y + 1];
					}
				}
			}
		}

		private void Update () {

			// Update grid lines and cell size
			if (PreviousCellSize != cellSize || PreviousLineColor != LineColor || PreviousLineWidth != lineWidth) {
				RefreshGrid ();
			}

			// Convert mouse position to Grid Coordinates
			Vector2 pos = camera2D.Camera.ScreenToWorldPoint (Input.mousePosition);
			int x = (int)((pos.x - this.transform.position.x) / (cellSize + lineWidth));
			int y = -(int)((pos.y - this.transform.position.y) / (cellSize + lineWidth));

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
				if (x != 0 && y != 0 && x != gridSize.x - 1 && y != gridSize.y - 1) {
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
