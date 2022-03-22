using UnityEngine;
using System.Collections.Generic;

class SubMesh {
	private GameObject obj;
	private IntVector ini, fin;
	private List<int> triangulos;
	public int cnt;
	private int[,,] indice;

	public SubMesh(GameObject padre, Material material, int x, int y, int z, int lado, int [,,] valor, Color [] color) {
		int i, j, k, xf, yf, zf;
		cnt = 0;
		xf = Mathf.Min (valor.GetLength (0), x + lado);
		yf = Mathf.Min (valor.GetLength (1), y + lado);
		zf = Mathf.Min (valor.GetLength (2), z + lado);
		ini = new IntVector (xf, yf, zf);
		fin = new IntVector (x, y, z);
		for (i = x; i < xf; i++)
			for (j = y; j < yf; j++)
				for (k = z; k < zf; k++)
					if (valor [i, j, k] >= 0) {
						cnt++;
						if (ini.x > i)
							ini.x = i;
						if (ini.y > j)
							ini.y = j;
						if (ini.z > k)
							ini.z = k;
						if (fin.x <= i)
							fin.x = i + 1;
						if (fin.y <= j)
							fin.y = j + 1;
						if (fin.z <= k)
							fin.z = k + 1;
					}
		if (cnt == 0)
			return;
		if (--ini.x < x)
			ini.x = x;
		if (--ini.y < y)
			ini.y = y;
		if (--ini.z < z)
			ini.z = z;
		if (++fin.x > xf)
			fin.x = xf;
		if (++fin.y > yf)
			fin.y = yf;
		if (++fin.z > zf)
			fin.z = zf;
		obj = new GameObject("submesh" + x + "_" + y + "_" + z);
		obj.transform.SetParent (padre.transform);
		Renderer rend = obj.AddComponent<MeshRenderer> ();
		rend.material = material;
		rend.receiveShadows = false;
		rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		Mesh mesh = new Mesh ();
		triangulos = new List<int> ();
		int n = 0;
		Vector3[] v = new Vector3[cnt];
		Color[] u = new Color[cnt];
		indice = new int[fin.x - ini.x, fin.y - ini.y, fin.z - ini.z];
		for (i = ini.x; i < fin.x; i++)
			for (j = ini.y; j < fin.y; j++)
				for (k = ini.z; k < fin.z; k++)
					if (valor [i, j, k] >= 0) {
						indice [i - ini.x, j - ini.y, k - ini.z] = n;
						v [n] = new Vector3 (i, j, k);
						u [n] = color [valor [i, j, k]];
						n++;
					}
		fin.x--;
		fin.y--;
		fin.z--;
		mesh.vertices = v;
		mesh.colors = u;
		obj.AddComponent<MeshFilter> ().mesh = mesh;
	}

	public void triangulate(bool [,,] visible, IntVector[][] modelo, bool[,,] envolvente) {
		if (cnt == 0)
			return;
		int i, j, k, l, n;
		for (i = ini.x; i < fin.x; i++)
			for (j = ini.y; j < fin.y; j++)
				for (k = ini.z; k < fin.z; k++)
					if (!envolvente[i,j,k]) {
					for (l = n = 0; l < 8; l++)
						if (visible [i + l / 4, j + ((l / 2) % 2), k + (l % 2)])
							n |= (1 << l);
					for (l = 0; l < modelo [n].Length; l++)
						triangulos.Add (indice [
							i - ini.x + modelo [n] [l].x,
							j - ini.y + modelo [n] [l].y,
							k - ini.z + modelo [n] [l].z]);
				}
		obj.GetComponent<MeshFilter>().mesh.triangles = triangulos.ToArray ();
		triangulos.Clear ();
	}
};

public class BrainMesh {

	private GameObject padre;
	private PointCloud cloud;
	private IntVector[][] modelo;
	private IntVector[] esquinas;
	private SubMesh[,,] sub;
	private bool[,,] envolvente;
	private Color[] colorv;
	private IntVector tam;
	private int lado;

	public BrainMesh(Shader shader, PointCloud pc, bool color = false, int side = 40) {

		// Cada mesh sólo puede tener 2^16 vértices. Una malla cúbica por tanto
		// sólo puede tener como máximo 2^(16/3) = 40.3 vértices por lado.

		padre = new GameObject ("brain mesh");
		cloud = pc;
		computar_modelos ();
		colorv = new Color[pc.prof_color];
		float depth = pc.prof_color;
		int i, j, k, a, b, c;
		if (color)
			for (i = 0; i < pc.prof_color; i++)
				colorv [i] = Color.HSVToRGB (i / depth, 1f, 1f);
		else
			for (i = 0; i < pc.prof_color; i++)
				colorv [i] = Color.HSVToRGB (1f, 0f, i / depth);
		cloud.lado_sector = side - 1;
		tam = new IntVector (
			Mathf.CeilToInt (cloud.dimension.x / (float)cloud.lado_sector),
			Mathf.CeilToInt (cloud.dimension.y / (float)cloud.lado_sector),
			Mathf.CeilToInt (cloud.dimension.z / (float)cloud.lado_sector));

		envolvente = cloud.dimension.newArray<bool> ();
		int l, n;
		for (i = 0; i < cloud.dimension.x - 1; i++)
			for (j = 0; j < cloud.dimension.y - 1; j++)
				for (k = 0; k < cloud.dimension.z - 1; k++) {
					for (l = n = 0; l < 8; l++)
						if (cloud.visible [i + l / 4, j + ((l / 2) % 2), k + (l % 2)])
							n |= (1 << l);
					envolvente [i, j, k] = (n > 0 && n < 0xFF);
				}

		sub = tam.newArray<SubMesh> ();
		Material material = new Material (shader);
		for (i = a = 0; i < tam.x; i++, a += cloud.lado_sector)
			for (j = b = 0; j < tam.y; j++, b += cloud.lado_sector)
				for (k = c = 0; k < tam.z; k++, c += cloud.lado_sector) {
					sub [i, j, k] = new SubMesh (
						padre, material, a, b, c, side, cloud.numeros, colorv);
					sub [i, j, k].triangulate (cloud.visible, modelo, envolvente);
				}
		padre.transform.localScale = cloud.offset;
		padre.transform.localPosition = -cloud.centro;
	}

	public void retriangulate() {
		int i, j, k;
		for (i =  0; i < tam.x; i++)
			for (j = 0; j < tam.y; j++)
				for (k = 0; k < tam.z; k++)
					sub [i, j, k].triangulate (cloud.visible, modelo, envolvente);
	}

	private void comprobar_triangulo(List<int> l, int inicio) {
		l.Sort (inicio, 3, null);
		for (int i = 0; i < inicio; i += 3)
			if (l [i] == l [inicio] &&
			    l [i + 1] == l [inicio + 1] &&
			    l [i + 2] == l [inicio + 2]) {
				l.RemoveRange (inicio, 3);
				return;
			}
	}

	private void computar_modelos() {
		
		modelo = new IntVector[256][];
		esquinas = new IntVector[8];
		bool[,,] vis = new bool[2, 2, 2];
		bool[,,] revisado = new bool[2, 2, 2];
		List<int> aux = new List<int> ();
		IntVector[] stack = new IntVector[8];
		int[] diff = new int[]{ 0, 1, 1, 2, 1, 2, 2, 3 };

		int i, j, k, l, n, r, s, t, a, b, c, d, d1, d2;

		for (i = 0; i < 8; i++) {
			stack [i] = new IntVector ();
			esquinas [i] = new IntVector (i / 4, (i / 2) % 2, i % 2);
		}
		for (n = 0; n < 256; n++) {
			for (l = 0; l < 8; l++)
				vis [esquinas[l].x, esquinas[l].y, esquinas[l].z] = (n & (1 << l)) != 0;
			r = s = t = 0;
			for (i = 0; i < 2; i++)
				for (j = 0; j < 2; j++)
					for (k = 0; k < 2; k++) {
						revisado [i, j, k] = false;
						if (vis [i, j, k])
							r++;
					}
			if (r < 3 && r > 7)
				modelo [n] = null;
			else {
				aux.Clear ();
				for (i = 0; i < 2; i++)
					for (j = 0; j < 2; j++)
						for (k = 0; k < 2; k++)
							if (!vis [i, j, k] && !revisado [i, j, k]) {
								stack [0].set (i, j, k);
								revisado [i, j, k] = true;
								s = 1;
								d = 0;
								t = aux.Count;
								while (s > 0) {
									s--;
									a = stack [s].x;
									b = stack [s].y;
									c = stack [s].z;
									if (!revisado [1 - a, b, c]) {
										revisado [1 - a, b, c] = true;
										if (vis [1 - a, b, c])
											aux.Add (((1 - a) * 2 + b) * 2 + c);
										else
											stack [s++].set (1 - a, b, c);
									}
									if (!revisado [a, 1 - b, c]) {
										revisado [a, 1 - b, c] = true;
										if (vis [a, 1 - b, c])
											aux.Add ((a * 2 + (1 - b)) * 2 + c);
										else
											stack [s++].set (a, 1 - b, c);
									}
									if (!revisado [a, b, 1 - c]) {
										revisado [a, b, 1 - c] = true;
										if (vis [a, b, 1 - c])
											aux.Add ((a * 2 + b) * 2 + 1 - c);
										else
											stack [s++].set (a, b, 1 - c);
									}
								}
								for (a = 0; a < 2; a++)
									for (b = 0; b < 2; b++)
										for (c = 0; c < 2; c++)
											if (vis [a, b, c])
												revisado [a, b, c] = false;
								d = aux.Count - t;
								if (d < 3)
									aux.RemoveRange (t, d);
								else if (d == 3)
									comprobar_triangulo (aux, t);
								else {
									d1 = 0;
									a = b = 0;
									for (c = 0; c < 3; c++)
										for (d = c + 1; d < 4; d++) {
											d2 = diff [aux [t + d] ^ aux [t + c]];
											if (d2 > d1) {
												a = c;
												b = d;
												d1 = d2;
											}
										}
									for (c = 0; c == a || c == b; c++)
										;
									for (d = c + 1; d == a || d == b; d++)
										;
									a = aux [t + a];
									b = aux [t + b];
									c = aux [t + c];
									d = aux [t + d];
									aux [t] = aux [t + 3] = a;
									aux [t + 1] = b;
									aux [t + 2] = c;
									aux.Add (b);
									aux.Add (d);
									comprobar_triangulo (aux, t + 3);
									comprobar_triangulo (aux, t);
								}
							}
				modelo [n] = new IntVector[aux.Count];
				for (d = 0; d < aux.Count; d++)
					modelo [n] [d] = esquinas[aux [d]];
			}
		}
	}
};
