using CommonUtils;
using CommonUtils.UnityComponents;
using Liquids2D.GridLines;
using System;
using UnityEngine;

namespace Liquids2D {
	public interface ILiquidContainer {
		Vector2Int GridSize { get; }
		float CellSize { get; }
		float LineWidth { get; }
		Vector2 HorizontalLinesScale { get; }
		Vector2 VerticalLinesScale { get; }

		Vector2Int WorldPointToGridCoordinate(Vector2 worldPoint);
	}

	public class LiquidContainer : EnhancedMonoBehaviour, ILiquidContainer {
		[SerializeField] private Vector2Int gridSize = new(80, 40);
		[SerializeField] private bool matchAspectRatio;
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

		[ShowInInspector] public Vector2 HorizontalLinesScale => new(Width, LineWidth);
		[ShowInInspector] public Vector2 VerticalLinesScale => new(LineWidth, Height);

		private LiquidSimulator LiquidSimulator;

		private IGridLineRenderer gridLineRenderer;

		private void Awake() {
			if (matchAspectRatio) {
				var ar = (float)Screen.height / (float)Screen.width;
				gridSize.y = Mathf.RoundToInt(gridSize.x * ar);
			}

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
			updateNeighbors ();
		}

		// Live update the grid properties
		private void refreshGrid() {

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
			camera2D.Target.position = transform.position + new Vector3(Width / 2f, -Height / 2f);
			camera2D.Target.localScale = new Vector2(Width, Height);
			camera2D.Set();
		}

		// Sets neighboring cell references
		private void updateNeighbors() {
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
				refreshGrid ();
			}

			// Run our liquid simulation
			LiquidSimulator.Simulate (ref Cells);
		}

		public void SetCellType(int x, int y, CellType cellType) {
			if (x != 0 && y != 0 && x != gridSize.x - 1 && y != gridSize.y - 1) { // Check the location is not an edge
				Cells[x, y].SetType(cellType);
			}
		}

		public void AddLiquidAt(int x, int y, int amount) => Cells[x, y].AddLiquid(amount);

		public Vector2Int WorldPointToGridCoordinate(Vector2 worldPoint) => new(
			(int)((worldPoint.x - transform.position.x) / (cellSize + lineWidth)),
			-(int)((worldPoint.y - transform.position.y) / (cellSize + lineWidth)));

		public CellType GetCellType(Vector2Int gridPosition) {
			if (!GridPositionIsValid(gridPosition)) return CellType.Invalid;
			return Cells[gridPosition.x, gridPosition.y].Type;
		}

		public bool GridPositionIsValid(Vector2Int gridPosition)
			=> ((gridPosition.x > 0 && gridPosition.x < Cells.GetLength(0)) && (gridPosition.y > 0 && gridPosition.y < Cells.GetLength(1)));

		// Load some sprites to show the liquid flow directions
		private void Reset() => liquidFlowSprites = Resources.LoadAll<Sprite>("LiquidFlowSprites");
	}
}
