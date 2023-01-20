using CommonUtils;
using UnityEngine;

namespace Liquids2D {
	public class GridLine : MonoBehaviour {
		public void Set(Color color, Vector2 position, Vector2 size) {
			transform.localPosition = position;
			transform.localScale = size;
			this.GetCachedComponent<SpriteRenderer>().color = color;
		}
	}
}
