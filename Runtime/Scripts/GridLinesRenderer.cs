using CommonUtils.UnityComponents;
using UnityEngine;

namespace Liquids2D {
	public interface IGridLineRenderer : IUnityComponent {
		GridLine[] HorizontalLines { get; }
		GridLine[] VerticalLines { get; }
		Color Color { get; }

		void CreateGridLines(Vector2 offset, float CellSize, float LineWidth);
		void RenderGridLines(Vector2 offset, float CellSize, float LineWidth);
	}

	public class GridLinesRenderer : MonoBehaviour, IGridLineRenderer {
		[SerializeField] private GridLine gridLinePrefab;
		[SerializeField] private Color color = Color.black;
		[SerializeField] private bool enable = true;

		public GridLine[] HorizontalLines { get; private set; }
		public GridLine[] VerticalLines { get; private set; }
		public Color Color => color;

		public void CreateGridLines(Vector2 offset, float CellSize, float LineWidth) {
			var gridLineContainer = new GameObject ("GridLines");
			gridLineContainer.transform.parent = this.transform;

			// vertical grid lines
			VerticalLines = new GridLine[Grid.Width + 1];
			for (int x = 0; x < Grid.Width + 1; x++) {
				var line = Instantiate(gridLinePrefab);
				line.name = $"Vertical {x}";
				float xpos = offset.x + (CellSize * x) + (LineWidth * x);
				line.Set (color,
					new Vector2 (xpos, offset.y),
					new Vector2 (LineWidth, (Grid.Height*CellSize) + LineWidth * Grid.Height + LineWidth));
				line.transform.parent = gridLineContainer.transform;
				VerticalLines [x] = line;
			}

			// horizontal grid lines
			HorizontalLines = new GridLine[Grid.Height + 1];
			for (int y = 0; y < Grid.Height + 1; y++) {
				var line = Instantiate(gridLinePrefab);
				line.name = $"Horizontal {y}";
				float ypos = offset.y - (CellSize * y) - (LineWidth * y);
				line.Set (color, new Vector2 (offset.x, ypos), new Vector2 ((Grid.Width*CellSize) + LineWidth * Grid.Width + LineWidth, LineWidth));
				line.transform.parent = gridLineContainer.transform;
				HorizontalLines [y] = line;
			}
		}

		public void RenderGridLines(Vector2 offset, float CellSize, float LineWidth) {
			if (!enable) return;
			// vertical grid lines
			for (int x = 0; x < Grid.Width + 1; x++) {
				float xpos = offset.x + (CellSize * x) + (LineWidth * x);
				VerticalLines [x].Set (color, new Vector2 (xpos, offset.y), new Vector2 (LineWidth, (Grid.Height*CellSize) + LineWidth * Grid.Height + LineWidth));
			}

			// horizontal grid lines
			for (int y = 0; y < Grid.Height + 1; y++) {
				float ypos = offset.y - (CellSize * y) - (LineWidth * y);
				HorizontalLines [y] .Set (color, new Vector2 (offset.x, ypos), new Vector2 ((Grid.Width*CellSize) + LineWidth * Grid.Width + LineWidth, LineWidth));
			}
		}
	}
}