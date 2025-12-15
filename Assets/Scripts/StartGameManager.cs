using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StartGameManager : MonoBehaviour
{
    private const string TARGET_WORD = "ENTER";

    [Header("Animation References")]
    [SerializeField] private TextMeshProUGUI[] enterLetters;
    [SerializeField] private float waveHeight = 15f;
    [SerializeField] private float letterRiseDuration = 0.15f;
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private float totalAnimDuration = 5.0f;
        
    [Header("Blinking Settings")]   // Configurações do efeito de piscar
    [SerializeField] private float blinkInterval = 0.5f; // Intervalo de 0.5 segundos

    [Header("Typing Sound Feedback")]
    [SerializeField] private AudioSource typingSFXSource; // AudioSource
    [SerializeField] private AudioClip[] typingClips;   // Array de sons de teclas
    [SerializeField] private AudioSource completeSFXSource; // AudioSource
    [SerializeField] private AudioClip completeClip;   // Sons ao completar

    private int currentProgress = 0;
    private Coroutine blinkingCoroutine; // Para gerenciar o efeito de piscar

    private const string NEXT_SCENE = "MenuScene"; // Nome da próxima cena

    void Start()
    {
        for (int i = 0; i < enterLetters.Length; i++)
        {
            enterLetters[i].text = (i == 0) ? TARGET_WORD[0].ToString() : "_";
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
        for (int i = 0; i < enterLetters.Length; i++)
        {
            TextMeshProUGUI currentText = enterLetters[i];

            if (i < currentProgress)
            {
                // Letra já digitada (permanece visível)
                currentText.text = TARGET_WORD[i].ToString();
                currentText.color = Color.white;
            }
            else if (i == currentProgress)
            {
                // Letra ATUAL a ser digitada (Piscar)
                if (visible)
                {
                    currentText.text = $"<color=#FFFFFF>{TARGET_WORD[i]}</color>";
                }
                else
                {
                    // Deixa o caractere invisível, mas o objeto de texto deve estar lá.
                    currentText.text = "_";
                }
            }
            else
            {
                // Letras futuras
                currentText.text = "_";
                currentText.color = Color.white;
            }
        }
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
                PlayRandomTypingSound();
                CheckInput(keyPress);
            }
        }
    }

    private void PlayRandomTypingSound()
    {
        if (typingSFXSource != null && typingClips.Length > 0)
        {            
            int randomIndex = Random.Range(0, typingClips.Length);  // Sorteia um índice aleatório no array
                        
            typingSFXSource.PlayOneShot(typingClips[randomIndex]);  // Toca o clipe uma única vez
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

                UpdateFinalDisplay();
                StartCoroutine(AnimateWave());
            }
        }
    }

    private void UpdateFinalDisplay()
    {        
        for (int i = 0; i < enterLetters.Length; i++)   // Assegura que todas as letras estão no estado final (ENTER)
        {
            enterLetters[i].text = TARGET_WORD[i].ToString();
            enterLetters[i].color = Color.white;
        }
    }

    private IEnumerator AnimateWave()
    {
        float startTime = Time.time;

        yield return new WaitForSeconds(0.1f);
        completeSFXSource.PlayOneShot(completeClip);
        Invoke("LoadMenuScene", 6f);

        // Continua a onda enquanto o tempo total não tiver sido atingido
        while (Time.time < startTime + totalAnimDuration)
        {
            for (int i = 0; i < enterLetters.Length; i++)
            {
                StartCoroutine(PulseLetter(enterLetters[i]));

                // Espera o atraso para criar o efeito de onda
                yield return new WaitForSeconds(staggerDelay);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PulseLetter(TextMeshProUGUI letter)
    {
        Vector3 startPos = letter.transform.localPosition;
        Vector3 peakPos = startPos + Vector3.up * waveHeight;
        float duration = letterRiseDuration;
        float timer = 0f;
                
        while (timer < duration)
        {
            letter.transform.localPosition = Vector3.Lerp(startPos, peakPos, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        letter.transform.localPosition = peakPos;

        
        timer = 0f;

        while (timer < duration)
        {
            letter.transform.localPosition = Vector3.Lerp(peakPos, startPos, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        
        letter.transform.localPosition = startPos;
    }

    private void LoadMenuScene()
    {
        SceneManager.LoadScene(NEXT_SCENE);
    }
}