using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileExpolisionControl : MonoBehaviour {

    private AudioSource explosionAudio;

    private void Awake()
    {
        explosionAudio = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start () {
        explosionAudio.Play();
        Destroy(gameObject, 5);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
