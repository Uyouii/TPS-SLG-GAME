using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextLevelTimeControl : MonoBehaviour {

    [HideInInspector]
    public static float nextLevelTime;

    Text nextLevelTimeText;
    Text nextLevelTimeTips;

    private void Awake()
    {
        nextLevelTime = 0;
        nextLevelTimeText = GetComponent<Text>();

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "NextMonsterTip")
            {
                nextLevelTimeTips = child.gameObject.GetComponent<Text>();
                break;
            }
        }
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		if(nextLevelTime > 0)
        {
            nextLevelTimeText.enabled = true;
            nextLevelTimeTips.enabled = true;
            nextLevelTime -= Time.deltaTime;
            nextLevelTimeText.text = (int)(nextLevelTime + 1) + " s";
        }
        else
        {
            nextLevelTimeText.enabled = false;
            nextLevelTimeTips.enabled = false;
        }
	}
}
