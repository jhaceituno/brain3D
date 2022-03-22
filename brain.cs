using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
using UnityEngine.VR;
#endif

public class brain : MonoBehaviour {

	public TextAsset pointCloudFile, dfsCortexFile;
	public GameObject slider, imagen;
	private LoaderSlider loader;
	private DFSmesh dfs;
	private PointCloud pc;
	private int etapa, i, total;
	private float value;
	public Shader brainShader, cortexShader;
	//public bool debug, color;
	[Range(0,4)]
	public int downsamplingFactor = 1;
	public GameObject ring;
	private Material ringMat;
	public Texture[] ringRGB;
	private BrainMesh bmesh;
	private float triggerTime;
	public float sliceTime = 1f;
	public float clusterTime = 3f;
	public float resetTime = 5f;
	bool clustered;
	private IntVector focus;
	public int nearThresh = 5, farThresh = 10;
	//private MeshReader reader;

	void Start () {
		etapa = 0;
		focus = new IntVector ();
		triggerTime = 0f;
		ring.SetActive (false);
		ringMat = ring.GetComponent<MeshRenderer> ().materials [0];
		ringMat.mainTexture = ringRGB [1];
	}

	#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
	private bool checkTrigger() {
		return Input.GetMouseButton(0) | GvrController.ClickButton;
	}
	#else
	private bool checkTrigger() {
		return Input.GetMouseButton (0);
	}
	#endif

	bool doneLoading(float percentage = -1) {
		if (percentage < 0)
			slider.SetActive (false);
		else {
			if (!slider.activeSelf)
				slider.SetActive (true);
			slider.transform.localScale = new Vector3 (300f * percentage, 20f, 1f);
			slider.transform.localPosition = new Vector3 (150f * (percentage - 1f), 0f, 300f);
		}
		return percentage >= 1f;
	}

	void Update () {
		switch (etapa) {
		case 0:
			pc = //debug ? new PointCloud (20 * (downsamplingFactor + 1)) :
				new PointCloud (pointCloudFile, downsamplingFactor);
			dfs = new DFSmesh (dfsCortexFile, cortexShader);
			etapa++;
			break;
		case 1:
			if (doneLoading (pc.done ())) {
				//reader.desfase = pc.desfase;
				bmesh = new BrainMesh (brainShader, pc);//, color);
				dfs.top = pc.centro.y;
				etapa++;
			}
			break;
		case 2:
			if (doneLoading (dfs.done ())) {
				doneLoading ();
				Camera.main.transform.position = 2f * pc.getRadius () * Vector3.forward;
				Camera.main.transform.LookAt (Vector3.zero);
				ring.transform.localPosition = .5f * Camera.main.transform.position;
				//reader = new MeshReader (3377);
				clustered = false;
				etapa++;
			}
			break;
		case 3:
			if (checkTrigger ()) {
				if (clustered) 
					triggerTime = resetTime - Time.deltaTime;
				else {
					triggerTime += Time.deltaTime;
					if (triggerTime < resetTime) {
						ring.SetActive (true);
						ringMat.mainTexture = ringRGB [(triggerTime < sliceTime) ? 1 : (triggerTime < clusterTime) ? 0 : 2];
					} else
						ring.SetActive (false);
				}
			} else if (triggerTime > 0) {
				if (triggerTime < sliceTime) {
					Vector3 normalCamara = -Camera.main.transform.position.normalized;
					if (pc.getFocus (normalCamara, out focus.x, out focus.y, out focus.z)) {
						pc.planeCut (focus, normalCamara);
						dfs.planeCut (pc.pos (focus), normalCamara);
						bmesh.retriangulate ();
					}
				} else if (triggerTime < clusterTime) {
					if (pc.cluster (-Camera.main.transform.position.normalized, nearThresh, farThresh)) {
						
						etapa++;
						clustered = true;
					}
				} else if (triggerTime < resetTime) {
					//reader.reset();
					pc.reset ();
					dfs.reset ();
					bmesh.retriangulate ();
					clustered = false;
				}
				triggerTime = 0f;
				ring.SetActive (false);
			}
			break;
		case 4:
			if (pc.cluster ()) {
				dfs.hide ();
				bmesh.retriangulate ();
				etapa--;
			} else if (checkTrigger ()) {
				pc.reset ();
				dfs.reset ();
				bmesh.retriangulate ();
				clustered = false;
			}
			break;
		}
	}

	void OnApplicationQuit() {
		pc.cleanse ();
		//eader.close ();
	}
}