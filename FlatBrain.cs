using UnityEngine;
using UnityEngine.UI;

public class FlatBrain {

	// Separar y controlar foco (x,y,z)

	public Texture2D texture;
	private IntVector dim;
	private Color[,] color;
	private int[,,] numeros;
	private int[] offset;
	private static int barH = 20;
	private int x_, h, w;
	public int x0;
	public IntVector centro;
	private float prop;

	private void setPixel(int x, int y, int valor) {
		texture.SetPixel (x, y, color [valor < 0 ? 1 : 0, Mathf.Abs(valor)]);
	}

	public FlatBrain(PointCloud pc, GameObject img) {
		dim = new IntVector (pc.dimension);
		numeros = pc.numeros;
		offset = new int[5];
		offset [0] = dim.y;
		offset [1] = barH + offset[0];
		offset [2] = dim.z + offset[1];
		offset [3] = barH + offset[2];
		offset [4] = dim.y + offset[3];
		h = barH + offset[4];
		w = dim.max ();
		int w2 = Mathf.NextPowerOfTwo (w);
		int h2 = Mathf.NextPowerOfTwo (h);

		color = new Color[2, pc.prof_color + 1];
		color [0, 0] = Camera.main.backgroundColor;
		float n, d = pc.prof_color - 1;
		for (int i = 1; i <= pc.prof_color; i++) {
			n = (i - 1) / d;
			color [0, i] = new Color (n, n, n);
			color [1, i] = Color.Lerp (Camera.main.backgroundColor, color [0, i], .5f);
		}

		texture = new Texture2D (Mathf.NextPowerOfTwo (w), Mathf.NextPowerOfTwo (h), TextureFormat.RGB24, false, false);
		for (int i = 0; i < w2; i++)
			for (int j = 0; j < h2; j++)
				texture.SetPixel (i, j, color [0, 0]);
		texture.Apply ();
		centro = dim / 2;
		img.GetComponent<RawImage> ().texture = texture;
		img.GetComponent<RawImage> ().uvRect = new Rect (0, 0, w/(float)w2, h/(float)h2);
		prop = img.GetComponent<RectTransform> ().rect.height / (float)h;
		img.GetComponent<RawImage> ().SetNativeSize ();
		img.GetComponent<RawImage> ().transform.localScale = new Vector3 (prop, prop, 1f);
		x_ = Mathf.RoundToInt(img.GetComponent<RectTransform> ().rect.width);
		x0 = Screen.width - 20 - Mathf.RoundToInt(x_ * prop);
	}

	public bool clicked(Vector3 normal) {
		int i, j, k;
		i = Mathf.RoundToInt ((Input.mousePosition.x - Screen.width + 20) / prop) + x_;
		j = Mathf.RoundToInt (Input.mousePosition.y / prop);
		if (i <= 0 || i >= w - 1)
			return false;
		else {
			if (j > offset [0] && j < offset [1]) {
				k = dim.z * (i - 1) / (w - 2);
				if (k == centro.z)
					return false;
				centro.z = k;
			} else if (j > offset [2] && j < offset [3]) {
				k = dim.y * (i - 1) / (w - 2);
				if (k == centro.y)
					return false;
				centro.y = k;
			} else if (j > offset [4] && j < h) {
				k = dim.x * (i - 1) / (w - 2);
				if (k == centro.x)
					return false;
				centro.x = k;
			} else
				return false;
		}
		setTexture (normal);
		return true;
	}

	public void resetCenter () {
		centro = dim / 2;
	}

	public void setTexture(Vector3 normal) {
		int i, j;
		for (i = 0; i < dim.x; i++) {
			for (j = 0; j < dim.y; j++)
				setPixel (i, j, numeros [i, j, centro.z]);
			texture.SetPixel (i, centro.y, Color.green);
			for (j = 0; j < dim.z; j++)
				setPixel (i, offset[1] + j, numeros [i, centro.y, j]);
			texture.SetPixel (i, offset[1] + centro.z, Color.blue);
		}
		for (i = 0; i < dim.y; i++) {
			for (j = 0; j < dim.z; j++)
				setPixel (j, offset[3] + i, numeros [centro.x, i, j]);
			texture.SetPixel (centro.x, i, Color.red);
			texture.SetPixel (centro.z, offset [3] + i, Color.blue);
		}
		for (i = 0; i < dim.z; i++) {
			texture.SetPixel (centro.x, offset[1] + i, Color.red);
			texture.SetPixel (i, offset[3] + centro.y, Color.green);
		}
		texture.SetPixel (centro.x, centro.y, Color.blue);
		texture.SetPixel (centro.x, offset[1] + centro.z, Color.green);
		texture.SetPixel (centro.z, offset[3] + centro.y, Color.red);
		for (i = 0; i < w; i++) {
			for (j = offset [0]; j < offset [1]; j++)
				texture.SetPixel (i, j, color[0,0]);
			for (j = offset [2]; j < offset [3]; j++)
				texture.SetPixel (i, j, color[0,0]);
			for (j = offset [4]; j < h; j++)				
				texture.SetPixel (i, j, color[0,0]);
			texture.SetPixel (i, offset[0] + 9, Color.blue);
			texture.SetPixel (i, offset[0] + 10, Color.cyan);
			texture.SetPixel (i, offset[0] + 11, Color.blue);
			texture.SetPixel (i, offset[2] + 9, Color.green);
			texture.SetPixel (i, offset[2] + 10, Color.yellow);
			texture.SetPixel (i, offset[2] + 11, Color.green);
			texture.SetPixel (i, offset[4] + 9, Color.red);
			texture.SetPixel (i, offset[4] + 10, Color.magenta);
			texture.SetPixel (i, offset[4] + 11, Color.red);
		}
		for (j = 0; j < barH; j++) {
			for (i = -1; i <= 1; i += 2) {
				texture.SetPixel (i + centro.z * w / dim.z, offset [0] + j, Color.blue);
				texture.SetPixel (i + centro.y * w / dim.y, offset [2] + j, Color.green);
				texture.SetPixel (i + centro.x * w / dim.x, offset [4] + j, Color.red);
			}
			texture.SetPixel (centro.z * w / dim.z, offset [0] + j, Color.cyan);
			texture.SetPixel (centro.y * w / dim.y, offset [2] + j, Color.yellow);
			texture.SetPixel (centro.x * w / dim.x, offset [4] + j, Color.magenta);
		}
		// xy
		float x, y, d, k;
		int I, J;
		for (int n = 0; n < 3; n++) {
			I = n == 2 ? 2 : 0;
			J = n == 1 ? 2 : 1;
			x = normal[I];
			y = normal[J];
			d = Mathf.Max (Mathf.Abs (x), Mathf.Abs (y));
			if (d != 0f) {
				x /= d;
				y /= d;
				k = 1f;
				i = centro[I] - Mathf.RoundToInt (k * x);
				j = centro[J] - Mathf.RoundToInt (k * y);
				while (i >= 0 && j >= 0 && i < dim[I] && j < dim[J]) {
					texture.SetPixel (i, n == 0 ? j : j + offset [n == 1 ? 1 : 3], Color.yellow);
					k += 1f;
					i = centro[I] - Mathf.RoundToInt (k * x);
					j = centro[J] - Mathf.RoundToInt (k * y);
				}
				k = 1f;
				i = centro[I] + Mathf.RoundToInt (k * y);
				j = centro[J] - Mathf.RoundToInt (k * x);
				while (i >= 0 && j >= 0 && i < dim[I] && j < dim[J]) {
					texture.SetPixel (i, n == 0 ? j : j + offset [n == 1 ? 1 : 3], Color.magenta);
					k += 1f;
					i = centro[I] + Mathf.RoundToInt (k * y);
					j = centro[J] - Mathf.RoundToInt (k * x);
				}
				k = 1f;
				i = centro[I] - Mathf.RoundToInt (k * y);
				j = centro[J] + Mathf.RoundToInt (k * x);
				while (i >= 0 && j >= 0 && i < dim[I] && j < dim[J]) {
					texture.SetPixel (i, n == 0 ? j : j + offset [n == 1 ? 1 : 3], Color.magenta);
					k += 1f;
					i = centro[I] - Mathf.RoundToInt (k * y);
					j = centro[J] + Mathf.RoundToInt (k * x);
				}
			}
		}
		texture.Apply ();
	}
}
