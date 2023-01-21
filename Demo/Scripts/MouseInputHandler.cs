using CommonUtils;
using UnityEngine;

namespace Liquids2D.Demo {
	public class MouseInputHandler : EnhancedMonoBehaviour {
		[SerializeField] private LiquidContainer liquidContainer;
		[SerializeField] private Camera2D camera2D;

		[ShowInInspector] public bool? DrawingWalls { get; private set; }

		private void Update() {
			Vector2 pos = camera2D.Camera.ScreenToWorldPoint (Input.mousePosition);
			var gridPosition = liquidContainer.WorldPointToGridCoordinate(pos);

			// Check if we are filling or erasing walls
			if (Input.GetMouseButtonDown(0)) {
				switch (liquidContainer.GetCellType(gridPosition)) {
					case CellType.Blank:
						DrawingWalls = true;
						break;
					case CellType.Solid:
						DrawingWalls = false;
						break;
					case CellType.Invalid:
						DrawingWalls = null;
						break;
				}
				return;
			}

			if (Input.GetMouseButtonUp(0)) DrawingWalls = null;

			// Left click draws/erases walls
			if (DrawingWalls.HasValue && liquidContainer.GridPositionIsValid(gridPosition)) {
				liquidContainer.SetCellType(gridPosition.x, gridPosition.y, DrawingWalls.Value ? CellType.Solid : CellType.Blank);
			}

			if (Input.GetMouseButton(1) && liquidContainer.GridPositionIsValid(gridPosition)) {
				liquidContainer.AddLiquidAt(gridPosition.x, gridPosition.y, 5);
			}
		}
	}
}
