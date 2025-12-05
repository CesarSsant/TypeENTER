using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // Necessário para Coroutines

public class StartGameManager : MonoBehaviour
{
    private const string TARGET_WORD = "ENTER";

    [SerializeField] private TextMeshProUGUI targetTextDisplay;

    // Configurações do efeito de piscar
    [Header("Blinking Settings")]
    [SerializeField] private float blinkInterval = 0.5f; // Intervalo de 0.5 segundos

    private int currentProgress = 0;
    private string currentDisplay = "";
    private Coroutine blinkingCoroutine; // Para gerenciar o efeito de piscar

    private const string NEXT_SCENE = "MenuScene"; // Nome da próxima cena

    void Start()
    {
        // Garante que o Game Manager esteja ativo
        if (targetTextDisplay == null)
        {
            Debug.LogError("targetTextDisplay não está configurado!");
            return;
        }

        UpdateDisplay();
        // Inicia o efeito de piscar na letra 'E'
        blinkingCoroutine = StartCoroutine(BlinkTargetLetter());
    }

    // --- Lógica de Piscar (Blinking) ---
    private IEnumerator BlinkTargetLetter()
    {
        bool isVisible = true;

        while (currentProgress < TARGET_WORD.Length)
        {
            // Espera o intervalo
            yield return new WaitForSeconds(blinkInterval);

            // Alterna a visibilidade
            isVisible = !isVisible;
            UpdateDisplay(isVisible);
        }
    }

    private void UpdateDisplay(bool visible = true)
    {
        string newDisplay = "";

        for (int i = 0; i < TARGET_WORD.Length; i++)
        {
            char targetChar = TARGET_WORD[i];

            if (i < currentProgress)
            {
                // Letra já digitada (permanece visível)
                newDisplay += targetChar;
            }
            else if (i == currentProgress)
            {
                // Letra ATUAL a ser digitada
                if (visible)
                {
                    // Usa tag de cor para piscar (Exemplo: Vermelho claro)
                    newDisplay += $"<color=#FFFFFF>{targetChar}</color>";
                }
                else
                {
                    // Invisível, mas mantém o espaço
                    newDisplay += "_";
                }
            }
            else
            {
                // Letras futuras (ainda não mostradas, usaremos o espaço)
                newDisplay += "_";
            }
        }
        targetTextDisplay.text = newDisplay;
    }

    // --- Lógica de Input ---
    private void OnGUI()
    {
        Event e = Event.current;

        if (e.isKey && e.type == EventType.KeyDown)
        {
            // Pega o caractere em MAIÚSCULA
            char keyPress = char.ToUpper(e.character);

            if (currentProgress < TARGET_WORD.Length)
            {
                CheckInput(keyPress);
            }
        }
    }

    private void CheckInput(char keyPress)
    {
        char targetChar = TARGET_WORD[currentProgress];

        if (keyPress == targetChar)
        {
            // 1. ACERTOU: Avança o progresso
            currentProgress++;

            // 2. Garante que o display atualize para a nova letra (visível)
            UpdateDisplay(true);

            if (currentProgress == TARGET_WORD.Length)
            {
                // Jogo Terminado
                if (blinkingCoroutine != null)
                {
                    StopCoroutine(blinkingCoroutine); // Para o pisca-pisca
                }

                targetTextDisplay.text = string.Join("", TARGET_WORD.ToCharArray()); // "ENTER" completo
                Invoke("LoadMenuScene", 0.7f);
            }
        }
    }

    private void LoadMenuScene()
    {
        SceneManager.LoadScene(NEXT_SCENE);
    }
}