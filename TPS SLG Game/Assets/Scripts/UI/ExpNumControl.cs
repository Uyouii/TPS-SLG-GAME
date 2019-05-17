using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpNumControl : MonoBehaviour {

    private Slider expSlider;
    private int[] PLAYER_LEVEL_NEED_EXP; 

    private void Awake()
    {
        expSlider = GetComponent<Slider>();
    }

    // Use this for initialization
    void Start () {
        PLAYER_LEVEL_NEED_EXP = new int[]{
            0, 50, 150, 300, 500, 750, 1050, 1400, 1800, 2250, 2750
        };
    }
	
	// Update is called once per frame
	void Update () {

        if(GameSettings.playerLevel <= PLAYER_LEVEL_NEED_EXP.Length - 1)
        {
            int playerLevel = GameSettings.playerLevel;
            int levelNeedExp = PLAYER_LEVEL_NEED_EXP[playerLevel] - PLAYER_LEVEL_NEED_EXP[playerLevel - 1];
            int actualExp = GameSettings.playerExp - PLAYER_LEVEL_NEED_EXP[playerLevel - 1];
            expSlider.value = actualExp * 100 / levelNeedExp;
        }
        // player level is at top
        else
        {
            expSlider.value = 100;
        }
	}
}
