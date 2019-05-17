using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class VolumeHandler : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		if(GameObject.Find("EffectsSlider"))
		GameObject.Find("EffectsSlider").GetComponent<Slider>().onValueChanged.AddListener(SetVolume);
	}

	void SetVolume(float volume)
	{
		GetComponent<AudioSource>().volume = volume;
	}

	void OnDestroy()
	{
		if(GameObject.Find("EffectsSlider"))
		GameObject.Find("EffectsSlider").GetComponent<Slider>().onValueChanged.RemoveListener(SetVolume);
	}
}
