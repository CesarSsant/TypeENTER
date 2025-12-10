using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    // Enumeração para o modo de jogo (acessível publicamente)
    public enum GameMode { Easy, Medium, Hard }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI activeCTxt;
    [SerializeField] private TextMeshProUGUI futureCTxt;
    [SerializeField] private TextMeshProUGUI correctCTxt;
    [SerializeField] private TextMeshProUGUI skippedCTxt;

    [Header("Feedback Visual (Case & Digit)")]
    [SerializeField] private GameObject uppercaseFeedbackObject; // UI para Maiúsculas
    [SerializeField] private GameObject digitFeedbackObject;     // UI para Números

    // --- Variáveis de Jogo ---
    private char currentChar;
    private string currentCharacterSet;
    private Queue<char> futureCharacters = new Queue<char>(); // Fila de letras futuras
    private StringBuilder correctHistory = new StringBuilder(); // Histórico de acertos
    private const int PREVIEW_COUNT = 50; // Quantos caracteres futuros mostrar/gerar

    // Dicionário para armazenar os conjuntos de caracteres para cada dificuldade
    private Dictionary<GameMode, string> characterSets = new Dictionary<GameMode, string>()
    {
        { GameMode.Easy, "abcdefghijklmnopqrstuvwxyz" },
        { GameMode.Medium, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" },
        { GameMode.Hard, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{};:'\",.<>/?`~" }
    };

    private void Awake()
    {
        // Inicialização do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Chamado para configurar qual conjunto de caracteres usar
    public void SetGameMode(GameMode newMode)
    {
        if (characterSets.ContainsKey(newMode))
        {
            currentCharacterSet = characterSets[newMode];
        }
        else
        {
            Debug.LogError("Modo de jogo não configurado no dicionário!");
            currentCharacterSet = characterSets[GameMode.Easy]; // Fallback
        }
    }

    public void InitializeQueue()
    {
        // Limpa a fila caso o jogo seja reiniciado
        futureCharacters.Clear();
        correctHistory.Clear();

        for (int i = 0; i < PREVIEW_COUNT; i++) // Preenche a fila até atingir o PREVIEW_COUNT inicial
        {
            FillFutureQueue(); // Adiciona um caractere aleatório
        }

        // Zera os textos de histórico e pulados
        correctCTxt.text = "";
        skippedCTxt.text = "";
    }

    public void SetNewCharacter()   // Chamado pelo GameManager em StartGame e a cada acerto
    {
        FillFutureQueue();

        currentChar = futureCharacters.Dequeue();   // Remove o caractere que está na "frente" da fila para ser o novo ativo

        // Limpa o estado anterior: Desativa ambos os feedbacks por padrão antes de qualquer checagem
        uppercaseFeedbackObject.SetActive(false);
        digitFeedbackObject.SetActive(false);

        if (GameManager.Instance.currentMode != GameMode.Easy)  // O feedback é relevante apenas nos modos Médio e Difícil
        {
            
            if (char.IsLetter(currentChar) && char.IsUpper(currentChar))    // Checa se é MAIÚSCULA
            {
                uppercaseFeedbackObject.SetActive(true);
            }
                        
            else if (char.IsDigit(currentChar)) // Checa se é NÚMERO/DÍGITO
            {
                digitFeedbackObject.SetActive(true);
            }
        }

        // Atualiza a UI (código da fila e caracteres central/futuros)
        activeCTxt.text = currentChar.ToString();
        UpdateFutureUI();

        // TODO: Feedback visual para o novo caractere
    }

    private void FillFutureQueue()
    {
        if (string.IsNullOrEmpty(currentCharacterSet))
        {
            SetGameMode(GameMode.Easy); // Fallback, garante que a string não é nula
        }

        int randomIndex = Random.Range(0, currentCharacterSet.Length);
        char randomChar = currentCharacterSet[randomIndex];

        futureCharacters.Enqueue(randomChar);
    }
    private void UpdateFutureUI()
    {
        char[] futureArr = futureCharacters.ToArray();
        int length = Mathf.Min(futureArr.Length, PREVIEW_COUNT);

        futureCTxt.text = new string(futureArr, 0, length); // Pega a substring do início (do 0) até o limite
    }
    public void SkipCharacter()
    {
        if (!GameManager.Instance.isGameActive) return;

        GameManager.Instance.AddScore(-1); // Perde 1 ponto

        skippedCTxt.text = currentChar.ToString(); // Exibe a letra pulada

        SetNewCharacter();  // Sorteia o próximo (A letra pulada é perdida/removida)
    }

    public void CheckInput(char keyPress)   // Recebe o caractere exato do InputManager via evento
    {
        char targetChar = currentChar;
        char inputChar = keyPress;

        if (GameManager.Instance.currentMode == GameMode.Easy)
        {
            if (char.IsLetter(targetChar))  // Regra do Fácil: Flexível (Padroniza para minúsculo na comparação)
            {
                targetChar = char.ToLower(currentChar);
                inputChar = char.ToLower(keyPress);
            }
        }

        if (inputChar == targetChar)
        {
            GameManager.Instance.AddScore(1);

            correctHistory.Append(targetChar);  // Atualiza Histórico de Acertos

            if (correctHistory.Length > PREVIEW_COUNT)  // Limitamos a exibição: pega a substring do fim
            {
                correctCTxt.text = correctHistory.ToString(correctHistory.Length - PREVIEW_COUNT, PREVIEW_COUNT);   // Garante que só mostra os últimos 'N' caracteres
            }
            else
            {
                correctCTxt.text = correctHistory.ToString();
            }

            skippedCTxt.text = "";  // Limpa o display de Pulados

            SetNewCharacter();

            // Se errou, não acontece nada
        }
    }
}