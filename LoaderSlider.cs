using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoaderSlider {
	
	private GameObject loader;
	private Slider barra;
	private Text valor, texto;

	public LoaderSlider (GameObject slider) {
		loader = slider;
		barra = slider.transform.Find ("barra").GetComponent<Slider> ();
		valor = slider.transform.Find ("valor").GetComponent<Text> ();
		texto = slider.transform.Find ("texto").GetComponent<Text> ();
		loader.SetActive (true);
	}

	public void setText(string text) {
		texto.text = text;
	}

	public bool setValue(float value) {
		barra.value = value;
		valor.text = Mathf.RoundToInt(barra.value * 100f).ToString() + '%';
		return value >= 1f;
	}

	public void setActive(bool active) {
		loader.SetActive (active);
	}
}
