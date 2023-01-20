using CommonUtils;
using UnityEngine;

namespace Liquids2D.GridLines {
	public class GridLine : MonoBehaviour {
		public void Set(Color color, Vector2 position, Vector2 size) {
			transform.position = position;
			transform.localScale = size;
			this.GetCachedComponent<SpriteRenderer>().color = color;
		}
	}
}
