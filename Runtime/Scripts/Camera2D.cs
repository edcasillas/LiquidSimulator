using CommonUtils;
using UnityEngine;

namespace Liquids2D {
	[RequireComponent(typeof(Camera))]
	public class Camera2D : EnhancedMonoBehaviour {
		[SerializeField] private Transform target;

		[ShowInInspector] public Vector3 CurrentPosition { get; private set; }
		[ShowInInspector] public Vector3 CurrentScale { get; private set; }

		private new Camera camera;
		public Camera Camera {
			get {
				if (!camera) camera = GetComponent<Camera>();
				return camera;
			}
		}

		public Transform Target => target;

		public void Set() {
			var height = target.localScale.y * 100;
			var width = target.localScale.x * 100;

			var w = Screen.width / width;
			var h = Screen.height / height;

			var ratio = w / h;
			var size = (height / 2) / 100f;

			if (w < h)
				size /= ratio;

			Camera.orthographicSize = size;

			Vector2 position = target.transform.position;

			Vector3 camPosition = position;
			Vector3 point = Camera.WorldToViewportPoint(camPosition);
			Vector3 delta = camPosition - Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
			Vector3 destination = transform.position + delta;

			transform.position = destination;
		}

		public void LateUpdate() {
			if (CurrentPosition != target.transform.position || CurrentScale != target.transform.localScale) {
				CurrentPosition = target.transform.position;
				CurrentScale = target.transform.localScale;
				Set();
			}
		}
	}
}