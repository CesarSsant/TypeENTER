using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;

    private void Start()
    {
        if (restartButton != null && menuButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            menuButton.onClick.AddListener(ReturnToMenu);
        }
        
    }

    private void Update()
    {
        scoreText.text = ("SCORE:  " + GameManager.Instance.GetScore().ToString());
        if (restartButton == null) 
        {
            if (Input.GetKey(KeyCode.R)) 
            {
                RestartGame();
            }
        }
        if (menuButton == null)
        {
            if (Input.GetKey(KeyCode.Escape)) 
            {
               ReturnToMenu();
            }
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
