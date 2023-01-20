using CommonUtils.UnityComponents;
using UnityEngine;

namespace Liquids2D {
	public interface IGridLineRenderer : IUnityComponent {
		GridLine[] HorizontalLines { get; }
		GridLine[] VerticalLines { get; }
		Color Color { get; }

		void CreateGridLines(Vector2 offset, ILiquidContainer container);
		void RenderGridLines(Vector2 offset, ILiquidContainer container);
	}

	public class GridLinesRenderer : MonoBehaviour, IGridLineRenderer {
		[SerializeField] private GridLine gridLinePrefab;
		[SerializeField] private Color color = Color.black;
		[SerializeField] private bool enable = true;

		public GridLine[] HorizontalLines { get; private set; }
		public GridLine[] VerticalLines { get; private set; }
		public Color Color => color;

		public void CreateGridLines(Vector2 offset, ILiquidContainer container) {
			if(!enable) return;
			var gridLinesParent = new GameObject ("GridLines").transform;
			gridLinesParent.parent = transform;

			// vertical grid lines
			VerticalLines = new GridLine[container.GridSize.x + 1];
			for (var x = 0; x < container.GridSize.x + 1; x++) {
				var line = Instantiate(gridLinePrefab, gridLinesParent);
				line.name = $"Vertical {x}";
				var xpos = offset.x + (container.CellSize * x) + (container.LineWidth * x);
				line.Set (color,
					new Vector2 (xpos, offset.y),
					new Vector2 (container.LineWidth, (container.GridSize.y*container.CellSize) + container.LineWidth * container.GridSize.y + container.LineWidth));
				VerticalLines [x] = line;
			}

			// horizontal grid lines
			HorizontalLines = new GridLine[container.GridSize.y + 1];
			for (int y = 0; y < container.GridSize.y + 1; y++) {
				var line = Instantiate(gridLinePrefab, gridLinesParent);
				line.name = $"Horizontal {y}";
				var ypos = offset.y - (container.CellSize * y) - (container.LineWidth * y);
				line.Set (color, new Vector2 (offset.x, ypos), new Vector2 ((container.GridSize.x*container.CellSize) + container.LineWidth * container.GridSize.x + container.LineWidth, container.LineWidth));
				HorizontalLines [y] = line;
			}
		}

		public void RenderGridLines(Vector2 offset, ILiquidContainer container) {
			if (!enable) return;
			// vertical grid lines
			for (int x = 0; x < container.GridSize.x + 1; x++) {
				float xpos = offset.x + (container.CellSize * x) + (container.LineWidth * x);
				VerticalLines [x].Set (color, new Vector2 (xpos, offset.y), new Vector2 (container.LineWidth, (container.GridSize.y*container.CellSize) + container.LineWidth * container.GridSize.y + container.LineWidth));
			}

			// horizontal grid lines
			for (int y = 0; y < container.GridSize.y + 1; y++) {
				float ypos = offset.y - (container.CellSize * y) - (container.LineWidth * y);
				HorizontalLines [y] .Set (color, new Vector2 (offset.x, ypos), new Vector2 ((container.GridSize.x*container.CellSize) + container.LineWidth * container.GridSize.x + container.LineWidth, container.LineWidth));
			}
		}
	}
}