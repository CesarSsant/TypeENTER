using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Referências
    [Header("Dependencies")]
    public CharacterManager characterManager;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    // Variáveis de Estado
    public bool isGameActive = false;
    private int score = 0;
    private float timeLeft = 20.0f;
    public CharacterManager.GameMode currentMode = CharacterManager.GameMode.Easy;
    private const string DIFFICULTY_KEY = "SelectedDifficulty";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        int modeIndex = PlayerPrefs.GetInt(DIFFICULTY_KEY, 0);
        CharacterManager.GameMode selectedMode = (CharacterManager.GameMode)modeIndex;

        currentMode = selectedMode;
        characterManager.SetGameMode(currentMode);

        StartGame();
        PlayerPrefs.DeleteKey(DIFFICULTY_KEY);
    }

    public void StartGame()
    {
        score = 0;
        timeLeft = 20.0f;
        isGameActive = true;
        UpdateScoreUI();
                
        characterManager.InitializeQueue(); // Inicializa a fila completa de futuros caracteres
        characterManager.SetNewCharacter(); // Remove o primeiro da fila para ser o ativo
    }

    // Chamado pelo CharacterManager em caso de acerto
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();

        // TODO: Feedback visual/sonoro de ponto pode ser acionado aqui
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    private void UpdateTimerUI()
    {        
        timerText.text = timeLeft.ToString("F2");   // Formata o tempo para mostrar duas casas decimais
    }

    void Update()
    {
        if (isGameActive)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                UpdateTimerUI();
                EndGame();
            }
        }
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log("Fim de Jogo! Seu score final: " + score);

        // TODO: Mostrar painel de GameOver e opções de recomeçar
    }
}