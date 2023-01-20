using CommonUtils;
using UnityEngine;

namespace Liquids2D {
	public class Camera2D : EnhancedMonoBehaviour {
		public Transform Area;

		[ShowInInspector] public Vector3 CurrentPosition { get; private set; }
		[ShowInInspector] public Vector3 CurrentScale { get; private set; }

		public void Set() {
			var height = Area.localScale.y * 100;
			var width = Area.localScale.x * 100;

			var w = Screen.width / width;
			var h = Screen.height / height;

			var ratio = w / h;
			var size = (height / 2) / 100f;

			if (w < h)
				size /= ratio;

			Camera.main.orthographicSize = size;

			Vector2 position = Area.transform.position;

			Vector3 camPosition = position;
			Vector3 point = Camera.main.WorldToViewportPoint(camPosition);
			Vector3 delta = camPosition - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
			Vector3 destination = transform.position + delta;

			transform.position = destination;
		}

		public void LateUpdate() {
			if (CurrentPosition != Area.transform.position || CurrentScale != Area.transform.localScale) {
				CurrentPosition = Area.transform.position;
				CurrentScale = Area.transform.localScale;
				Set();
			}

		}
	}
}