using UnityEngine;

public class IntVector {
	public int x, y, z;
	public IntVector(int a = 0) {
		set (a, a, a);
	}
	public IntVector(int a, int b, int c) {
		set (a, b, c);
	}
	public void set (int a, int b, int c) {
		x = a;
		y = b;
		z = c;
	}
	public IntVector(IntVector v) {
		set (v);
	}
	public void set (IntVector v) {
		x = v.x;
		y = v.y;
		z = v.z;
	}
	public void set(char axis, int a, int b, int c) {
		if (axis == 'z')
			set (a, b, c);
		else if (axis == 'x')
			set (b, c, a);
		else
			set (c, a, b);
	}
	public int this[int i] {
		get { return (i % 3 == 0) ? x : (i % 3 == 1) ? y : z; }
		set {
			if (i % 3 == 0)
				x = value;
			else if (i % 3 == 1)
				y = value;
			else
				z = value;
		}
	}
	public float modulo() {
		return Mathf.Sqrt (x * x + y * y + z * z);
	}
	public void round(Vector3 v) {
		x = Mathf.RoundToInt (v.x);
		y = Mathf.RoundToInt (v.y);
		z = Mathf.RoundToInt (v.z);
	}
	public int prod() {
		return x * y * z;
	}
	public Vector3 toVector3() {
		return new Vector3 (x, y, z);
	}
	public Vector3 scale(float n) {
		return new Vector3 (x * n, y * n, z * n);
	}
	public Vector3 scale(Vector3 v) {
		return new Vector3 (x * v.x, y * v.y, z * v.z);
	}
	public Vector3 scale(float a, float b, float c) {
		return new Vector3 (x * a, y * b, z * c);
	}
	public int max() {
		return Mathf.Max (x, y, z);
	}
	public int min() {
		return Mathf.Min (x, y, z);
	}
	public static IntVector operator+ (IntVector a, IntVector b) {
		return new IntVector (a.x + b.x, a.y + b.y, a.z + b.z);
	}
	public static IntVector operator+ (IntVector a, int b) {
		return new IntVector (a.x + b, a.y + b, a.z + b);
	}
	public static IntVector operator/ (IntVector a, int b) {
		return new IntVector (a.x / b, a.y / b, a.z / b);
	}
	public static IntVector operator% (IntVector a, int b) {
		return new IntVector (a.x % b, a.y % b, a.z % b);
	}
	public static Vector3 operator+ (IntVector a, float b) {
		return new Vector3 (a.x + b, a.y + b, a.z + b);
	}
	public static IntVector operator- (IntVector a, IntVector b) {
		return new IntVector (a.x - b.x, a.y - b.y, a.z - b.z);
	}
	public static bool operator< (IntVector a, IntVector b) {
		return a.x < b.x && a.y < b.y && a.z < b.z;
	}
	public static bool operator> (IntVector a, IntVector b) {
		return a.x > b.x && a.y > b.y && a.z > b.z;
	}
	public static bool operator== (IntVector a, IntVector b) {
		return a.x == b.x && a.y == b.y && a.z == b.z;
	}
	public static bool operator!= (IntVector a, IntVector b) {
		return a.x != b.x || a.y != b.y || a.z == b.z;
	}
	public override bool Equals (object obj) {
		return base.Equals (obj);
	}
	public override int GetHashCode () {
		return base.GetHashCode ();
	}
	public void min(int a, int b, int c) {
		if (x > a)
			x = a;
		if (y > b)
			y = b;
		if (z > c)
			z = c;
	}
	public void max(int a, int b, int c) {
		if (x < a)
			x = a;
		if (y < b)
			y = b;
		if (z < c)
			z = c;
	}
	public void min(IntVector v) {
		if (x > v.x)
			x = v.x;
		if (y > v.y)
			y = v.y;
		if (z > v.z)
			z = v.z;
	}
	public void max(IntVector v) {
		if (x < v.x)
			x = v.x;
		if (y < v.y)
			y = v.y;
		if (z < v.z)
			z = v.z;
	}
	public override string ToString () {
		return "[" + x + "," + y + "," + z + "]";
	}

	public T [,,] newArray<T> () {
		return new T[x, y, z];
	}

	public T index<T> (T [,,] array) {
		return array [x, y, z];
	}

	public void index<T> (T [,,] array, T value) {
		array [x, y, z] = value;
	}
}