using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class PointCloud {

	public static int CNT_MAX = 1000;
	public const int DEFAULT_DEPTH = 256;
	private string [] texto;
	public int prof_color, lado_sector;
	public IntVector dimension;
	public Vector3 offset, centro;
	public int[,,] numeros;
	public bool[,,] visible;
	public IntVector desfase;
	private Vector3[] posicion;
	private Thread loader;
	private int[,,] limites;
	private int cantidad_leida, maximo, downsample;
	private bool vivo;
	private int[,,] plano;
	private int nearThresh, farThresh;

	public PointCloud (int radio, int depth = DEFAULT_DEPTH) {
		maximo = 2 * radio + 1;
		offset = Vector3.one;
		centro = Vector3.one * radio;
		dimension = new IntVector (maximo + 2);
		numeros = dimension.newArray<int> ();
		visible = dimension.newArray<bool> ();
		posicion = new Vector3[dimension.max()];
		texto = null;
		prof_color = depth;
		loader = new Thread (read);
		loader.Start ();
	}

	public PointCloud (TextAsset pointCloudFile, int downsampling = 0,
		int depth = DEFAULT_DEPTH) {
		texto = pointCloudFile.text.Split (new char[] { ' ', '\n' },
			System.StringSplitOptions.RemoveEmptyEntries);
		maximo = int.Parse (texto [0]);
		offset = new Vector3 (
			float.Parse (texto [1]),
			float.Parse (texto [2]),
			float.Parse (texto [3]));
		dimension = new IntVector (
			int.Parse (texto [5]),
			int.Parse (texto [4]),
			int.Parse (texto [6]));
		numeros = dimension.newArray<int> ();
		prof_color = Mathf.RoundToInt (Mathf.Pow (2f,
			Mathf.Ceil (Mathf.Log (Mathf.Sqrt (depth)) / Mathf.Log (2f))));
		prof_color *= prof_color;
		downsample = downsampling + 1;
		limites = new int[1,2,3];
		for (int i = 0; i < 3; i++) {
			limites [0, 0, i] = dimension [i];
			limites [0, 1, i] = 0;
		}
		loader = new Thread (read);
		loader.Start ();
	}

	public void cleanse() {
		vivo = false;
	}

	private void read () {
		vivo = true;
		cantidad_leida = 0;
		int i, j, k, t, T;
		if (texto == null) {
			t = 0;
			T = (dimension.x - 2) * (dimension.y - 2) * (dimension.z - 2);
			int radio = maximo / 2;
			for (i = 0; i < dimension.x; i++)
				posicion [i].x = i - radio - 1;
			for (j = 0; j < dimension.y; j++)
				posicion [j].y = j - radio - 1;
			for (k = 0; k < dimension.z; k++)
				posicion [k].z = k - radio - 1;
			for (i = 0; i < dimension.x; i++)
				for (j = 0; j < dimension.y; j++)
					for (k = 0; k < dimension.z; k++)
						numeros [i, j, k] = -1;
			int rr = radio * radio;
			float rf = .9f * rr;
			Vector3 aux = new Vector3 ();
			for (i = 1; i < dimension.x - 1; i++) {
				aux.x = posicion [i].x * posicion [i].x;
				for (j = 1; j < dimension.y - 1; j++) {
					aux.y = posicion [j].y * posicion [j].y + aux.x;
					for (k = 1; k < dimension.z - 1 && vivo; k++) {
						cantidad_leida = Mathf.FloorToInt (CNT_MAX * (float)t++ / T);
						aux.z = posicion [k].z * posicion [k].z + aux.y;
						if (visible [i, j, k] = (aux.z <= rr && i <= dimension.x / 2))
							numeros [i, j, k] = Mathf.RoundToInt (
								(prof_color - 1) * Mathf.Abs (aux.z >= rf ?
									Mathf.Sin (Mathf.PI * j / maximo) : 
									Mathf.Cos (1.5f * Mathf.PI * aux.z / rf)));
					}
				}
			}
		} else {
			t = 7;
			T = dimension.prod () + 7;
			for (k = dimension.z - 1; k >= 0; k--)
				for (i = dimension.x - 1; i >= 0; i--)
					for (j = 0; j < dimension.y && vivo; j++) {
						cantidad_leida = Mathf.FloorToInt (CNT_MAX * (float)t / T);
						if ((numeros [i, j, k] = int.Parse (texto [t++]) - 1) >= 0) {
							numeros [i, j, k] = prof_color * numeros [i, j, k] / maximo;
							if (i < limites [0, 0, 0])
								limites [0, 0, 0] = i;
							if (j < limites [0, 0, 1])
								limites [0, 0, 1] = j;
							if (k < limites [0, 0, 2])
								limites [0, 0, 2] = k;
							if (i > limites [0, 1, 0])
								limites [0, 1, 0] = i;
							if (j > limites [0, 1, 1])
								limites [0, 1, 1] = j;
							if (k > limites [0, 1, 2])
								limites [0, 1, 2] = k;
						}
					}
		}
		cantidad_leida = CNT_MAX;
	}

	public float done() {
		if (cantidad_leida == CNT_MAX) {
			int i, j, k;
			if (texto != null) {
				texto = null;
				loader = null;
				desfase = new IntVector (dimension);
				dimension.x = 2 + (limites [0, 1, 0] - limites [0, 0, 0] + 1) / downsample;
				dimension.y = 2 + (limites [0, 1, 1] - limites [0, 0, 1] + 1) / downsample;
				dimension.z = 2 + (limites [0, 1, 2] - limites [0, 0, 2] + 1) / downsample;
				int[,,] old_num = numeros;
				numeros = dimension.newArray<int> ();
				for (i = 0; i < dimension.x; i++) {
					for (j = 0; j < dimension.y; j++)
						numeros [i, j, 0] = numeros [i, j, dimension.z - 1] = -1;
					for (k = 0; k < dimension.z; k++)
						numeros [i, 0, k] = numeros [i, dimension.y - 1, k] = -1;
				}
				for (j = 0; j < dimension.y; j++)
					for (k = 0; k < dimension.z; k++)
						numeros [0, j, k] = numeros [dimension.x - 1, j, k] = -1;
				for (i = 1; i < dimension.x - 1; i++)
					for (j = 1; j < dimension.y - 1; j++)
						for (k = 1; k < dimension.z - 1; k++)
							numeros [i, j, k] = old_num [
								limites [0, 0, 0] + downsample * (i - 1),
								limites [0, 0, 1] + downsample * (j - 1),
								limites [0, 0, 2] + downsample * (k - 1)];
				desfase.x = limites [0, 0, 0] + dimension.x - 2;
				desfase.y = limites [0, 0, 1] - 1;
				desfase.z = limites [0, 0, 2] + dimension.z - 2;
				old_num = null;
				limites = null;
				centro = Vector3.Scale (offset / 2f, dimension.toVector3 ());
				posicion = new Vector3[dimension.max ()];
				visible = dimension.newArray<bool> ();
				for (i = 0; i < dimension.x; i++)
					for (j = 0; j < dimension.y; j++)
						for (k = 0; k < dimension.z; k++)
							visible [i, j, k] = numeros [i, j, k] >= 0;
				
				for (i = 0; i < dimension.x; i++)
					posicion [i].x = i * offset.x - centro.x;
				for (j = 0; j < dimension.y; j++)
					posicion [j].y = j * offset.y - centro.y;
				for (k = 0; k < dimension.z; k++)
					posicion [k].z = k * offset.z - centro.z;
			}
			limites = new int[dimension.x, dimension.y, 2];
			for (i = 0; i < dimension.x; i++)
				for (j = 0; j < dimension.y; j++) {
					for (limites [i, j, 0] = 0;
						limites [i, j, 0] < dimension.z && !visible [i, j, limites [i, j, 0]];
						limites [i, j, 0]++)
						;
					for (limites [i, j, 1] = dimension.z - 1;
						limites [i, j, 1] >= limites [i, j, 0] && !visible [i, j, limites [i, j, 1]];
						limites [i, j, 1]--)
						;
				}
			for (i = 1; i < dimension.x - 1; i++)
				for (j = 1; j < dimension.y - 1; j++)
					for (k = limites [i, j, 0] + 1; k < limites [i, j, 1]; k++)
						if (!visible [i, j, k] && 
						        visible [i - 1, j, k] && visible [i + 1, j, k] &&
						        visible [i, j - 1, k] && visible [i, j + 1, k] &&
						        visible [i, j, k - 1] && visible [i, j, k + 1]) {
							numeros [i, j, k] = 0;
							visible [i, j, k] = true;
						}
			plano = new int[dimension.max (), dimension.max (), 3];
		}
		return cantidad_leida / (float)CNT_MAX;
	}

	public Vector3 pos(IntVector i) {
		return new Vector3 (posicion [i.x].x, posicion [i.y].y, posicion [i.z].z);
	}

	public Vector3 pos(int i, int j, int k) {
		return new Vector3 (posicion [i].x, posicion [j].y, posicion [k].z);
	}

	private bool pos(out int x, out int y, out int z, Vector3 location) {
		x = Mathf.RoundToInt ((location.x + centro.x) / offset.x);
		y = Mathf.RoundToInt ((location.y + centro.y) / offset.y);
		z = Mathf.RoundToInt ((location.z + centro.z) / offset.z);
		return x >= 0 && x < dimension.x
			&& y >= 0 && y < dimension.y
			&& z >= 0 && z < dimension.z;
	}

	public float getRadius() {
		return centro.magnitude;
	}

	public void reset() {
		IntVector i = new IntVector ();
		for (i.x = 0; i.x < dimension.x; i.x++)
			for (i.y = 0; i.y < dimension.y; i.y++)
				for (i.z = limites [i.x, i.y, 0]; i.z <= limites [i.x, i.y, 1]; i.z++)
					visible [i.x, i.y, i.z] = numeros [i.x, i.y, i.z] >= 0;
	}

	public bool getFocus (Vector3 cameraNormal, out int x, out int y, out int z) {
		int i, j, k;
		Vector3 point = Vector3.zero - .6f * dimension.modulo() * cameraNormal;
		while (!pos (out i, out j, out k, point))
			point += cameraNormal;
		while (pos (out i, out j, out k, point)) {
			if (visible [i, j, k]) {
				x = i;
				y = j;
				z = k;
				return true;
			}
			point += cameraNormal;
		}
		x = y = z = -1;
		return false;
	}

	public Vector3 planeCut(IntVector corte, Vector3 normal) {
		
		Vector3 point = pos (corte) + 2f * normal;
		char eje = (Mathf.Abs (normal.z) > .3f) ? 'z' : ((Mathf.Abs (normal.x) > .3f) ? 'x' : 'y');
		int K = eje - 'x';
		int I = (K + 1) % 3;
		int J = (I + 1) % 3;
		IntVector ind = new IntVector ();
		Vector3 aux = new Vector3 ();
		bool hayCorte = false;
		for (ind [I] = 0; ind [I] < dimension [I]; ind [I]++) {
			aux [I] = (point [I] - posicion [ind [I]] [I]) * normal [I];
			for (ind [J] = 0; ind [J] < dimension [J]; ind [J]++) {
				aux [J] = (point [J] - posicion [ind [J]] [J]) * normal [J];
				aux [K] = point [K] + (aux [I] + aux [J]) / normal [K];
				ind [K] = Mathf.RoundToInt ((aux [K] + centro [K]) / offset [K]);
				plano [ind [I], ind [J], 0] = ind [K] + ((normal [K] < 0f) ? 1 : -1);
				if (ind [K] >= 0 && ind [K] < dimension [K])
					hayCorte = true;					
			}
		}
		if (!hayCorte) {
			Debug.Log ("No existe corte");
			return Vector3.one * Mathf.Infinity;
		}
		int k00, k01, k10, k11;
		for (ind [I] = 0; ind [I] < dimension [I]; ind [I]++)
			for (ind [J] = 0; ind [J] < dimension [J]; ind [J]++) {
				if (normal [K] > 0f) {
					k00 = 0;
					k01 = Mathf.Min (plano [ind [I], ind [J], 0], dimension [K]);
					k10 = k01;//Mathf.Max (0, plano [ind [I], ind [J], 0]);
					k11 = k10 + 1;
					if (ind [I] > 0)
						k11 = Mathf.Max (k11, plano [ind [I] - 1, ind [J], 0] - 1);
					if (ind [J] > 0)
						k11 = Mathf.Max (k11, plano [ind [I], ind [J] - 1, 0] - 1);
					if (ind [I] < dimension [I] - 1)
						k11 = Mathf.Max (k11, plano [ind [I] + 1, ind [J], 0] - 1);
					if (ind [J] < dimension [J] - 1)
						k11 = Mathf.Max (k11, plano [ind [I], ind [J] + 1, 0] - 1);
					k11 = Mathf.Min (k11, dimension [K] - 1);
					plano [ind [I], ind [J], 1] = k10;
					plano [ind [I], ind [J], 2] = k11;
				} else {
					k00 = Mathf.Max (0, plano [ind [I], ind [J], 0] + 1);
					k01 = dimension [K];
					k11 = k00 - 1;//Mathf.Min (plano [ind [I], ind [J], 0], dimension [K] - 1);
					k10 = k11 - 1;
					if (ind [I] > 0)
						k10 = Mathf.Min (k10, plano [ind [I] - 1, ind [J], 0] + 1);
					if (ind [J] > 0)
						k10 = Mathf.Min (k10, plano [ind [I], ind [J] - 1, 0] + 1);
					if (ind [I] < dimension [I] - 1)
						k10 = Mathf.Min (k10, plano [ind [I] + 1, ind [J], 0] + 1);
					if (ind [J] < dimension [J] - 1)
						k10 = Mathf.Min (k10, plano [ind [I], ind [J] + 1, 0] + 1);
					k10 = Mathf.Max (0, k10);
					plano [ind [I], ind [J], 1] = k10;
					plano [ind [I], ind [J], 2] = k11;
				}
				for (ind [K] = k00; ind [K] < k01; ind [K]++)
					visible [ind.x, ind.y, ind.z] = false;
			}
		return point;
	}

	private int valorCentro;
	List<IntVector> pila;

	public bool cluster(Vector3 normalCamara, int near, int far) {
		IntVector centro = new IntVector ();
		if (!getFocus (normalCamara, out centro.x, out centro.y, out centro.z))
			return false;
		valorCentro = centro.index (numeros);
		pila = new List<IntVector> ();
		pila.Add (centro);
		int i, j, k;
		for (i = 0; i < dimension.x; i++)
			for (j = 0; j < dimension.y; j++)
				for (k = limites [i, j, 0]; k <= limites [i, j, 1]; k++)
					visible [i, j, k] = false;
		cantidad_leida = 1;
		vivo = true;
		nearThresh = near;
		farThresh = far;
		loader = new Thread (clusterThread);
		loader.Start ();
		return true;
	}

	public bool cluster() {
		Debug.Log (cantidad_leida);
		return cantidad_leida == 0;
	}

	public void clusterThread() {
		IntVector punto, nuevo = new IntVector ();
		int valorPunto, i, j;
		while (pila.Count > 0 && vivo) {
			punto = pila [0];
			pila.RemoveAt (0);
			valorPunto = punto.index (numeros);
			nuevo.set (punto);
			for (i = 0; i < 3; i++)
				for (j = -1; j <= 1; j += 2) {
					nuevo [i] = punto [i] + j;
					if (nuevo [i] > 0 && nuevo [i] < dimension [i] && !nuevo.index (visible) &&
						Mathf.Abs (nuevo.index (numeros) - valorPunto) <= nearThresh &&
						Mathf.Abs (nuevo.index (numeros) - valorCentro) <= farThresh) {
						nuevo.index (visible, true);
						pila.Add (nuevo);
						nuevo = new IntVector (punto);
					}
				}
			cantidad_leida = pila.Count;
		}
	}
};
