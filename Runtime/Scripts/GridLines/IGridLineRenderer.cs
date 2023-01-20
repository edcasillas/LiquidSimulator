using CommonUtils.UnityComponents;
using UnityEngine;

namespace Liquids2D.GridLines {
	public interface IGridLineRenderer : IUnityComponent {
		Color Color { get; }

		void CreateGridLines(Vector2 offset, ILiquidContainer container);
		void RenderGridLines(Vector2 offset, ILiquidContainer container);
	}
}