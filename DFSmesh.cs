using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class DFSmesh {

	private Material material;
	private GameObject obj;
	private GameObject [] sub;
	private int hdrsize, nTriang, nVertic, k;
	private float total;
	public float top;
	private bool vivo;
	private Thread loader;
	private byte[] bytes;
	private Vector3[][] baseTri;
	private Vector3 centro;
	private int[] triangles;
	private int[][] origTriangles;
	private bool[][] visible;
	private Vector3[] vertices;
	private Mesh[] meshes;

	public DFSmesh(TextAsset dfs, Shader shader) {
		vivo = true;
		top = 0f;
		material = new Material(shader);
		bytes = dfs.bytes;
		hdrsize = System.BitConverter.ToInt32 (bytes, 12);
		nTriang = System.BitConverter.ToInt32 (bytes, 24);
		nVertic = System.BitConverter.ToInt32 (bytes, 28);
		obj = new GameObject("cortex");
		total = nTriang * 3 + nVertic;
		triangles = new int[nTriang * 3];
		vertices = new Vector3[nVertic];
		k = 0;
		loader = new Thread (read);
		loader.Start ();
	}

	public void cleanse() {
		vivo = false;
	}

	private void read() {		
		int i, j = hdrsize - 4;
		for (i = 0; i < triangles.Length && vivo; i++, k++)
			triangles [i] = System.BitConverter.ToInt32 (bytes, j += 4);
		Vector3 minimo, maximo;
		minimo = Vector3.one * Mathf.Infinity;
		maximo = Vector3.one * -Mathf.Infinity;
		for (i = 0; i < nVertic && vivo; i++, k++) {
			vertices [i] = new Vector3 (
				System.BitConverter.ToSingle (bytes, j += 4),
				System.BitConverter.ToSingle (bytes, j += 4),
				System.BitConverter.ToSingle (bytes, j += 4));
			if (minimo.x > vertices [i].x)
				minimo.x = vertices [i].x;
			if (minimo.y > vertices [i].y)
				minimo.y = vertices [i].y;
			if (minimo.z > vertices [i].z)
				minimo.z = vertices [i].z;
			if (maximo.x < vertices [i].x)
				maximo.x = vertices [i].x;
			if (maximo.y < vertices [i].y)
				maximo.y = vertices [i].y;
			if (maximo.z < vertices [i].z)
				maximo.z = vertices [i].z;
		}
		centro = .5f * (minimo + maximo);
		centro.z -= minimo.z;
		k = Mathf.CeilToInt(total);
		bytes = null;
	}

	public float done() {
		if (bytes == null) {			
			int i, j, k;
			centro.z += top;
			int[] corr = new int[vertices.Length];
			List<Mesh> lMesh = new List<Mesh> ();
			List<int> lTriangles = new List<int> ();
			List<int> lIndices = new List<int> ();
			for (j = 0; j < triangles.Length;) {
				lTriangles.Clear ();
				lIndices.Clear ();
				for (i = 0; i < vertices.Length; i++)
					corr [i] = -1;
				while (lIndices.Count < 64998 && j < triangles.Length) {
					for (k = 0; k < 3; k++, j++) {
						if (corr [triangles [j]] < 0) {
							corr [triangles [j]] = lIndices.Count;
							lIndices.Add (triangles [j]);
						}
						lTriangles.Add (corr [triangles [j]]);
					}
				}				
				Mesh mesh = new Mesh ();
				mesh.name = "cortexMesh" + lMesh.Count;
				Vector3[] tvertices = new Vector3[lIndices.Count];
				for (i = 0; i < tvertices.Length; i++) {
					tvertices [i].x = centro.y - vertices [lIndices [i]].y;
					tvertices [i].y = vertices [lIndices [i]].z - centro.z;
					tvertices [i].z = centro.x - vertices [lIndices [i]].x;
				}
				mesh.vertices = tvertices;
				mesh.triangles = lTriangles.ToArray ();
				mesh.RecalculateNormals ();
				lMesh.Add (mesh);
			}
			meshes = lMesh.ToArray ();
			origTriangles = new int[meshes.Length][];
			visible = new bool[meshes.Length][];
			sub = new GameObject[meshes.Length];
			obj.transform.position = Vector3.zero;
			for (i = 0; i < sub.Length; i++) {
				sub [i] = new GameObject ("subcortex" + i);
				Renderer rend = sub [i].AddComponent<MeshRenderer> ();
				rend.material = material;
				rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				sub [i].AddComponent<MeshFilter> ().mesh = meshes [i];
				sub [i].transform.parent = obj.transform;
				sub [i].transform.localPosition = Vector3.zero;
				origTriangles [i] = meshes [i].triangles;
				visible[i] = new bool[meshes[i].vertices.Length];
				for (j = 0; j < visible [i].Length; j++)
					visible [i] [j] = true;
			}
		}
		return k / total;
	}

	public void planeCut(Vector3 focus, Vector3 normal) {
		if (focus.x == Mathf.Infinity)
			return;
		int i, j, k;
		Vector3[] vert;
		int[] tri;
		List<int> newTriangles = new List<int> ();
		for (i = 0; i < meshes.Length; i++) {
			if (meshes [i].triangles == null)
				continue;
			vert = meshes [i].vertices;
			for (j = k = 0; j < vert.Length; j++)
				if (visible[i][j] && Vector3.Dot(normal, focus - vert [j]) > 0f) {
					visible [i] [j] = false;
					k++;
				}
			if (k == vert.Length)
				meshes [i].triangles = null;
				// ¿Hace falta actualizar el componente MeshFilter?
			else if (k > 0) {
				newTriangles.Clear ();
				tri = meshes [i].triangles;
				for (j = 0; j < tri.Length; j += 3)
					if (visible [i] [tri [j]] &&
					    visible [i] [tri [j + 1]] &&
					    visible [i] [tri [j + 2]]) {
						newTriangles.Add (tri [j]);
						newTriangles.Add (tri [j + 1]);
						newTriangles.Add (tri [j + 2]);
					}
				meshes [i].triangles = newTriangles.ToArray ();
			}
		}
	}

	public void reset() {
		obj.SetActive (true);
		for (int i = 0; i < meshes.Length; i++) {
			meshes [i].triangles = origTriangles [i];
			for (int j = 0; j < visible [i].Length; j++)
				visible [i] [j] = true;
		}
	}

	public void hide() {
		obj.SetActive (false);
	}
}

/*public class VoxelMgr {
	public GameObject root;
	private Material material;
	private int matRes;
	private Mesh[] mesh;

	public VoxelMgr(bool color = false, int depth = 256) {
		root = new GameObject ("VoxelMgr");
		material = new Material (Shader.Find ("Unlit/Texture"));
		material.name = color ? "ColorScale" : "GrayScale";
		int n = Mathf.RoundToInt (Mathf.Pow (2f, Mathf.Ceil (Mathf.Log (Mathf.Sqrt (depth)) / Mathf.Log (2f))));
		matRes = n * n;
		Texture2D texture = new Texture2D (n, n);
		if (color)
			for (int k = 0; k < matRes; k++)
				texture.SetPixel (k / n, k % n, Color.HSVToRGB (k / (float)matRes, 1f, 1f));
		else
			for (int k = 0; k < matRes; k++)
				texture.SetPixel (k / n, k % n, Color.HSVToRGB (1f, 0f, k / (float)matRes));			
		texture.filterMode = FilterMode.Point;
		texture.Apply ();
		material.SetTexture ("_MainTex", texture);

		float r = .5f;
		Vector3[] vertices = new Vector3[] {
			new Vector3 ( r,  r,  r),
			new Vector3 ( r,  r, -r),
			new Vector3 ( r, -r, -r),
			new Vector3 ( r, -r,  r),
			new Vector3 (-r, -r,  r),
			new Vector3 (-r,  r,  r),
			new Vector3 (-r,  r, -r),
			new Vector3 (-r, -r, -r)
		};
		int[] triangles = new int[36];
		for (int k = 0; k < 6; k++) {
			triangles [3 * k] = 0;
			triangles [3 * k + 2] = k + 1;
			triangles [3 * k + 1] = ((k + 1) % 6) + 1;
			triangles [3 * k + 18] = 7;
			triangles [3 * k + 19] = k + 1;
			triangles [3 * k + 20] = ((k + 1) % 6) + 1;
		}
		mesh = new Mesh [matRes--];
		Vector2[] uv = new Vector2[4];
		float i, j, d0 = .1f / n, d1 = .9f / n;
		for (int k = 0; k <= matRes; k++) {			
			i = (k / n) / (float)n;
			j = (k % n) / (float)n;
			uv [0] = new Vector2 (i + d0, j + d0);
			uv [1] = new Vector2 (i + d0, j + d1);
			uv [2] = new Vector2 (i + d1, j + d0);
			uv [3] = new Vector2 (i + d1, j + d1);
			mesh [k] = new Mesh ();
			mesh [k].vertices = vertices;
			mesh [k].triangles = triangles;
			mesh [k].uv = new Vector2[] { uv [3], uv [1], uv [0], uv [2], uv [0], uv [1], uv [0], uv [3] };
			mesh [k].RecalculateBounds ();
			//mesh [k].Optimize ();
		}
	}

	public GameObject makeVoxel(Vector3 position, int value, bool active) {
		GameObject obj = new GameObject ("voxel");
		obj.transform.SetParent (root.transform);
		Renderer rend = obj.AddComponent<MeshRenderer> ();
		rend.material = material;
		rend.receiveShadows = false;
		rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		obj.transform.position = position;
		obj.AddComponent<MeshFilter> ().mesh = value > 0 ? mesh [value - 1] : null;
		obj.SetActive (active);
		return obj;
	}
}
*/