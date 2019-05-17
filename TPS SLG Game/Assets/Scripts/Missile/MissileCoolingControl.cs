using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissileCoolingControl : MonoBehaviour {

    [HideInInspector]
    public static float missileCoolingTime;

    private GameObject fadeImage;
    private GameObject coolingTimeText;

    private void Awake()
    {
        missileCoolingTime = GameSettings.MISSILE_SHOOT_INTERVAL;

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "FadeImage")
            {
                fadeImage = child.gameObject;
            }
            else if(child.gameObject.name == "MissileCoolTimeText")
            {
                coolingTimeText = child.gameObject;
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (missileCoolingTime < GameSettings.MISSILE_SHOOT_INTERVAL)
        {
            fadeImage.GetComponent<Image>().enabled = true;
            coolingTimeText.GetComponent<Text>().enabled = true;

            int remainTime = (int)(GameSettings.MISSILE_SHOOT_INTERVAL - missileCoolingTime + 1);

            coolingTimeText.GetComponent<Text>().text = remainTime + " s";

        }
        else
        {
            fadeImage.GetComponent<Image>().enabled = false;
            coolingTimeText.GetComponent<Text>().enabled = false;
        }
	}
}
