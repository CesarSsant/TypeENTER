using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;

    private void Update()
    {
        scoreText.text = ("SCORE:  " + GameManager.Instance.GetScore().ToString());

        if (Input.GetKey(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
