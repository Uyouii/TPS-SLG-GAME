using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static int score;
    public static int money;

    public Text scoreText;
    public Text moneyText;


    void Awake ()
    {

    }


    void Update ()
    {
        scoreText.text = "Score: " + score;
        moneyText.text = "Money: " + money + "￥";
    }
}
