using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Referências
    [Header("Dependencies")]
    public CharacterManager characterManager;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Audio References")]
    [SerializeField] private AudioSource musicAudioSource; // AudioSource para a música de fundo (Pitch/Loop)
    [SerializeField] private AudioSource sfxAudioSource;   // AudioSource para SFX (contagem, Time Over, etc.)
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip endMusic;
    [SerializeField] private AudioClip readyClip;
    [SerializeField] private AudioClip setClip;
    [SerializeField] private AudioClip goClip;
    [SerializeField] private AudioClip hurryUpClip;
    [SerializeField] private AudioClip timeOverClip;
    [SerializeField] private AudioClip[] countdownBeepClip = new AudioClip[5];

    // Variáveis de Estado
    public bool isGameActive = false;
    private int score = 0;
    private float timeLeft = 20.0f;
    public CharacterManager.GameMode currentMode = CharacterManager.GameMode.Easy;
    private const string DIFFICULTY_KEY = "SelectedDifficulty";
    private float lastKeyTime = 0f; // Tempo em que a última tecla correta foi digitada
    private float typingSpeed = 0f; // Velocidade atual de digitação (para controle do pitch)
    private float maxPitch = 1.2f;
    private float minPitch = 1.0f;
    private float pitchDamping = 1.0f; // Velocidade de retorno do pitch para 1.0
    private bool hurryUpPlayed = false;
    private bool countdownStarted = false;
    private float nextHurryUpTime = 0f;

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
        isGameActive = false;
        UpdateScoreUI();

        musicAudioSource.clip = backgroundMusic;
        musicAudioSource.Play();
        StartCoroutine(StartSequenceAndActivateGame());
    }

    // Chamado pelo CharacterManager em caso de acerto
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();

        if (points > 0)
        {
            float timeSinceLastKey = Time.time - lastKeyTime;
            lastKeyTime = Time.time;

            typingSpeed = 1f / Mathf.Max(timeSinceLastKey, 0.1f);
        }

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

            ControlMusicPitch();

            HandleTimeAlerts();

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                UpdateTimerUI();
                EndGame();
            }
        }
    }

    private void ControlMusicPitch()
    {
        float targetPitch = Mathf.Clamp(minPitch + (typingSpeed / 10f) * (maxPitch - minPitch), minPitch, maxPitch);

        musicAudioSource.pitch = Mathf.Lerp(musicAudioSource.pitch, targetPitch, Time.deltaTime * pitchDamping);

        typingSpeed = Mathf.Max(0f, typingSpeed - Time.deltaTime * 5f);
    }

    public void PlayStartSequence()
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(readyClip);
            StartCoroutine(PlaySequenceCoroutine());
        }
    }

    private IEnumerator StartSequenceAndActivateGame()
    {
        if (sfxAudioSource != null && readyClip != null)
        {
            sfxAudioSource.PlayOneShot(readyClip);
            yield return new WaitForSeconds(readyClip.length * 0.8f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (sfxAudioSource != null && setClip != null)
        {
            sfxAudioSource.PlayOneShot(setClip);
            yield return new WaitForSeconds(setClip.length * 0.8f);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (sfxAudioSource != null && goClip != null)
        {
            sfxAudioSource.PlayOneShot(goClip);
        }

        isGameActive = true;
        characterManager.InitializeQueue(); // Inicializa a fila completa de futuros caracteres
        characterManager.SetNewCharacter(); // Remove o primeiro da fila para ser o ativo

        musicAudioSource.clip = backgroundMusic;
        musicAudioSource.Play();
    }
    private void HandleTimeAlerts()
    {
        if (!hurryUpPlayed && timeLeft <15f && timeLeft > 10f)
        {
            if (Time.time > nextHurryUpTime)
            {
                sfxAudioSource.PlayOneShot(hurryUpClip);

                nextHurryUpTime = Time.time + Random.Range(10f, 20f);

                hurryUpPlayed = true;
            }
        }

        if (!countdownStarted && timeLeft < 5.5f)
        {
            countdownStarted = true;
            StartCoroutine(PlayCountdown());
        }
    }

    private IEnumerator PlayCountdown()
    {
        for (int i = 0; i < countdownBeepClip.Length; i++)
        {
            if (sfxAudioSource != null && countdownBeepClip[i] != null)
            {
                sfxAudioSource.PlayOneShot(countdownBeepClip[i]);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator PlaySequenceCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        sfxAudioSource.PlayOneShot(setClip);

        yield return new WaitForSeconds(0.7f);
        sfxAudioSource.PlayOneShot(goClip);
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log("Fim de Jogo! Seu score final: " + score);

        musicAudioSource.Stop();
        musicAudioSource.clip = endMusic;
        musicAudioSource.Play();
        sfxAudioSource.PlayOneShot(timeOverClip);

        // TODO: Mostrar painel de GameOver e opções de recomeçar
    }
}