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
    private Coroutine blinkingCoroutine; // Para gerenciar o efeito de piscar

    private const string NEXT_SCENE = "MenuScene"; // Nome da próxima cena

    void Start()
    {
        if (targetTextDisplay == null)  // Garante que o Game Manager esteja ativo
        {
            Debug.LogError("targetTextDisplay não está configurado!");
            return;
        }

        UpdateDisplay();
        blinkingCoroutine = StartCoroutine(BlinkTargetLetter());    // Inicia o efeito de piscar na letra 'E'
    }

    // --- Lógica de Piscar (Blinking) ---
    private IEnumerator BlinkTargetLetter()
    {
        bool isVisible = true;

        while (currentProgress < TARGET_WORD.Length)
        {
            yield return new WaitForSeconds(blinkInterval); // Espera o intervalo

            isVisible = !isVisible; // Alterna a visibilidade
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
                newDisplay += targetChar;   // Letra já digitada (permanece visível)
            }
            else if (i == currentProgress)
            {
                // Letra ATUAL a ser digitada
                if (visible)
                {
                    newDisplay += $"<color=#FFFFFF>{targetChar}</color>";   // Usa tag de cor para piscar (branca)
                }
                else
                {
                    newDisplay += "_";  // Invisível, mas mantém o _
                }
            }
            else
            {
                newDisplay += "_";  // Letras futuras
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
            char keyPress = char.ToUpper(e.character);  // Pega o caractere em MAIÚSCULA

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
            currentProgress++;  // ACERTOU: Avança o progresso

            UpdateDisplay(true);    // Garante que o display atualize para a nova letra

            if (currentProgress == TARGET_WORD.Length)
            {
                if (blinkingCoroutine != null)  // Terminou
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