using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Tooltip("The player dinosaur")]
    public DinoAgent player;

    [Tooltip("The AI controlled dinosaur")]
    public DinoAgent ai;

    [Tooltip("The text object to be displayed when the game is over")]
    public TextMeshProUGUI gameOverTextObject;

    // If the game is over
    private bool gameOver = false;

    /// <summary>
    /// Called by the losing dinosaur
    /// </summary>
    /// <param name="name">Name of the dinosaur gameobject which lost</param>
    public void Lost(string name)
    {
        if (!gameOver && !ai.trainingMode && !player.trainingMode)
        {
            gameOver = true;
            player.FreezeAgent();
            ai.FreezeAgent();
            if (name == "ChromePlayerDino")
            {
                gameOverTextObject.text = "Player lost, press enter to play again";
            }
            else if (name == "ChromeAIDino")
            {
                gameOverTextObject.text = "AI lost, press enter to play again";
            }
        }
    }

    // Called every frame
    private void Update()
    {
        if (gameOver && Input.GetKey(KeyCode.Return) && !ai.trainingMode && !player.trainingMode)
        {
            gameOver = false;
            player.UnfreezeAgent();
            ai.UnfreezeAgent();
            gameOverTextObject.text = "";
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
