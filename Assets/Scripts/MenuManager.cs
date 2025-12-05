using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para o Button

public class MenuManager : MonoBehaviour
{
    private const string DIFFICULTY_KEY = "SelectedDifficulty"; // Chave para salvar a dificuldade

    [SerializeField] private Button playButton; // Botão "Play" para habilitar/desabilitar
    private int selectedModeIndex = -1; // -1 significa que nada foi selecionado

    void Start()
    {
        if (playButton != null) // Garante que o botão Play começa desabilitado
        {
            playButton.interactable = false;
        }
    }

    // Chamado pelo OnClick() dos botões de dificuldade (Easy, Medium, Hard)
    public void SelectDifficulty(int modeIndex) // 0=Easy, 1=Medium, 2=Hard
    {
        selectedModeIndex = modeIndex;

        if (playButton != null) // Habilita o botão Play
        {
            playButton.interactable = true;
        }

        // TODO: Feedback visual nos botões de dificuldade
    }

    // Chamado pelo OnClick() do botão "Play"
    public void StartGame()
    {
        if (selectedModeIndex != -1)
        {
            // Salva a dificuldade para o GameManager ler
            PlayerPrefs.SetInt(DIFFICULTY_KEY, selectedModeIndex);
            PlayerPrefs.Save();

            SceneManager.LoadScene("GameScene");
        }
    }
}