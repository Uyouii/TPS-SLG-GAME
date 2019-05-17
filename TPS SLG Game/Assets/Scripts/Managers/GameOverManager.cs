using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public PlayerHealth playerHealth;

    private Text gameOverText;
    private Image screenFader;


    void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "GameOverText")
            {
                gameOverText = child.gameObject.GetComponent<Text>();
            }
            else if(child.gameObject.name == "ScreenFader")
            {
                screenFader = child.gameObject.GetComponent<Image>();
            }
        }
    }


    void Update()
    {
        if (playerHealth.currentHealth <= 0 )
        {
            if(gameOverText.color.a < 1f)
            {
                var tempColor = gameOverText.color;
                tempColor.a += 0.01f;
                gameOverText.color = tempColor;
            }
            if(screenFader.color.a < 0.9f)
            {
                var tempColor = screenFader.color;
                tempColor.a += 0.01f;
                screenFader.color = tempColor;
            }
            int highestScore = GameSettings.highestScore;
            if (GameSettings.score > highestScore)
                highestScore = GameSettings.score;
            gameOverText.text = "Game Over!\n\nHighest Score: " + highestScore;
        }
    }
}
