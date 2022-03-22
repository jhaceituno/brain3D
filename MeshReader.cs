using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
/*
public class MeshReader {
	private TcpClient socket;
	private NetworkStream stream;
	private byte[] data;
	private long size, bufferSize;
	private bool ok;

	public MeshReader (int port) {
		ok = true;
		try {
			socket = new TcpClient ("localhost", port);
			stream = socket.GetStream ();
			data = new byte[bufferSize = 4];
		} catch (Exception e) {
			Debug.Log ("Socket error: " + e);
			ok = false;
		}
	}

	public void cluster(IntVector focus, PointCloud pc) {
		data [0] = (byte)(pc.desfase.z - focus.z);
		data [1] = (byte)(pc.desfase.x - focus.x);
		data [2] = (byte)(focus.y - pc.desfase.y);
		data [3] = 0;
		stream.Write (data, 0, 3);
		int i = stream.Read (data, 0, 4);
		if (i <= 4)
			return false;
		size = 0;
		for (i = 0; i < 4; i--)
			size = (size << 8) + data [i];
		if (data.Length < size)
			data = new byte[size];
		for (i = 0; i < size;)
			i += stream.Read (data, i, size - i);
		
		for (i = 0; i < size; i++)
			Debug.Log (data [i].ToString ());


		int h, j, k;
		bool[,,] v = pc.visible;
		IntVector d = pc.dimension;
		for (i = 0; i < d.x; i++)
			for (j = 0; j < d.y; j++)
				for (k = 0; k < d.z; k++)
					v [i, j, k] = false;
		IntVector o = pc.desfase;
		int[,,] n = pc.numeros;
		for (h = 0; h < size; h += 3) {
			i = o.x - data [h + 1];
			j = o.y + data [h + 2];
			k = o.z - data [h];
			if (i > 0 && i < d.x && j > 0 && j < d.y && k > 0 && k < d.z && n [i, j, k] >= 0)
				v [i, j, k] = true;
		}
	}

	public void reset() {
		data [0] = 255;
		data [1] = 0;
		stream.Write (data, 0, 1);
	}

	public void close() {
		if (ok) {
			stream.Close ();
			socket.Close ();
			ok = false;
		}
	}
}*/