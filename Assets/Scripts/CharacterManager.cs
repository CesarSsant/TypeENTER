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

    // --- Variáveis de Jogo ---
    private char currentChar;
    private string currentCharacterSet;
    private Queue<char> futureCharacters = new Queue<char>(); // Fila de letras futuras
    private StringBuilder correctHistory = new StringBuilder(); // Histórico de acertos
    private const int PREVIEW_COUNT = 50; // Quantos caracteres futuros mostrar/gerar

    // Dicionário para armazenar os conjuntos de caracteres para cada dificuldade
    // Esta estrutura é o que garante a escalabilidade!
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

        // Preenche a fila até atingir o PREVIEW_COUNT inicial
        for (int i = 0; i < PREVIEW_COUNT; i++)
        {
            FillFutureQueue(); // Adiciona um caractere aleatório
        }

        // Zera os textos de histórico e pulados (se o GameManager ainda não fez)
        correctCTxt.text = "";
        skippedCTxt.text = "";
    }
    
    public void SetNewCharacter()   // Chamado pelo GameManager em StartGame e a cada acerto
    {
        // 1. ANTES DE REMOVER o caractere antigo (agora o currentChar), 
        //    ADICIONAMOS um novo caractere ao final da fila.
        FillFutureQueue();

        // 2. Remove o caractere que está na "frente" da fila para ser o novo ativo.
        currentChar = futureCharacters.Dequeue();

        // 3. Atualiza a UI (código da fila e caracteres central/futuros)
        activeCTxt.text = currentChar.ToString();
        UpdateFutureUI();

        // TODO: Feedback visual/animação para o novo caractere (ex: fade-in)
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

        // Pega a substring do início (do 0) até o limite
        futureCTxt.text = new string(futureArr, 0, length);
    }
    public void SkipCharacter()
    {
        if (!GameManager.Instance.isGameActive) return;

        // 1. Lógica de Perda de Ponto
        GameManager.Instance.AddScore(-1); // Perde 1 ponto

        // 2. Atualiza UI de Pulados (Abaixo)
        skippedCTxt.text = currentChar.ToString(); // Exibe a letra pulada

        // 3. Adiciona o caractere pulado ao histórico para fins de debug/visualização
        // (Opcional, mas mantém a letra no histórico correto por um momento antes de sumir na próxima jogada)

        // 4. Sorteia o próximo (A letra pulada é perdida/removida)
        SetNewCharacter();
    }

    public void CheckInput(char keyPress)   // Recebe o caractere exato do InputManager via evento
    {
        char targetChar = currentChar;
        char inputChar = keyPress;

        if (GameManager.Instance.currentMode == GameMode.Easy)
        {
            // Regra do Fácil: Flexível (Padroniza para minúsculo na comparação)
            if (char.IsLetter(targetChar))
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