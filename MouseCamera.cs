using UnityEngine;

public class MouseCamera {
	public float delta_angulo, delta_posicion, dist0;
	private bool rotando, huboClick;
	private Vector3 centro, eje, delta, inicial, /*cprevio,*/ forw0;

	private class Tcopy {
		public Vector3 position, localPosition, up, forward, right;
		public Quaternion rotation, localRotation;
		public void set(Transform t) {
			position = t.position;
			rotation = t.rotation;
			localPosition = t.localPosition;
			localRotation = t.localRotation;
			up = t.up;
			forward = t.forward;
			right = t.right;
		}
		public void reset() {
			Camera.main.transform.position = position;
			Camera.main.transform.rotation = rotation;
			Camera.main.transform.localPosition = localPosition;
			Camera.main.transform.localRotation = localRotation;
		}
	}
	private Tcopy tprevia;

	private void resetUp() {
		Transform t = Camera.main.transform;
		if (t.forward.y < .99f && t.forward.y > -.99f) {
			float coseno = Vector3.Dot (Vector3.up, t.up);
			float previo = coseno, angulo = Mathf.Acos (coseno);
			while (coseno < 1f && Mathf.Abs (angulo) > .01f) {			
				Camera.main.transform.RotateAround (t.position, t.forward, angulo);
				coseno = Vector3.Dot (Vector3.up, t.up);
				if (coseno < previo)
					angulo *= -.5f;
				previo = coseno;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MouseCamera"/> class.
	/// </summary>
	/// <param name="delta_angle">Delta angle.</param>
	/// <param name="delta_position">Delta position.</param>
	/// <param name="init_distance">Init distance.</param>
	public MouseCamera (float delta_angle = .2f, float delta_position = .1f) {
		delta_angulo = delta_angle;
		delta_posicion = delta_position;
		forw0 = Vector3.forward;
		dist0 = 2f;
		resetCamera ();
		tprevia = new Tcopy ();
		huboClick = false;
	}

	public MouseCamera (float delta_angle, float delta_position, Vector3 forward, float dist = 2f) {
		delta_angulo = delta_angle;
		delta_posicion = delta_position;
		forw0 = forward;
		dist0 = dist;
		resetCamera ();
		tprevia = new Tcopy ();
		huboClick = false;
	}

	private void resetCamera() {
		centro = Vector3.zero;
		Camera.main.transform.position = -dist0 * forw0;
		Camera.main.transform.LookAt (centro);
	}

	public void setCenter(Vector3 center) {
		Camera.main.transform.Translate (center - centro);
		Camera.main.transform.LookAt (center);
		centro = center;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	public bool update () {
		if (Input.GetMouseButton (0)) {
			if (!huboClick) {
				//rotando = !Input.GetKey (KeyCode.LeftShift);			
				inicial = Input.mousePosition;
				//cprevio = centro;
				tprevia.set (Camera.main.transform);
				huboClick = true;
			} else
				tprevia.reset ();
			delta = inicial - Input.mousePosition;
			if (delta.magnitude > 0) {
				//if (rotando) {
					eje = delta.y * tprevia.right - delta.x * tprevia.up;
					Camera.main.transform.RotateAround (centro, eje.normalized, delta_angulo * delta.magnitude);
				    //Camera.main.transform.LookAt (centro);
				/*} else {
					centro = cprevio + delta * delta_posicion;
					Camera.main.transform.Translate (delta * delta_posicion);
				}*/
			}
		} else {
			huboClick = false;
			/*if (Input.mouseScrollDelta.y != 0f) {
				Vector3 forward = centro - Camera.main.transform.position;
				forward = (Input.mouseScrollDelta.y > 0f) ? forward.normalized : -forward.normalized;
				Camera.main.transform.position += 0.1f * dist0 * forward;
				//centro += 0.1f * dist0 * forward;
			} else if (Input.GetKey (KeyCode.Q)) {
				resetCamera ();
				return true;
			} else if (Input.GetKey (KeyCode.Z))
				resetUp ();*/
		}
		return false;
	}

	public Vector3 getNormal() { return (centro - Camera.main.transform.position).normalized; }
	public Vector3 getCamera() { return Camera.main.transform.position; }
	public Vector3 getFocus() { return centro; }
}
