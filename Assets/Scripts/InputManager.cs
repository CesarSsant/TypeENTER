using UnityEngine;
using UnityEngine.Search;

// Define um delegate para enviar o caractere digitado para quem precisar
public delegate void KeyPressedAction(char key);

public class InputManager : MonoBehaviour
{
    public static event KeyPressedAction OnKeyPressed;  // Evento que outros scripts (como CharacterManager) podem assinar

    private CharacterManager characterManager;

    [Header("Typing Sound Feedback")]
    [SerializeField] private AudioSource typingSFXSource; // AudioSource
    [SerializeField] private AudioClip[] typingClips;      // Array de sons de teclas

    void Start()
    {
        characterManager = FindFirstObjectByType<CharacterManager>();    // Encontra a referência, já que CharacterManager é vital

        // Garante que o CharacterManager assine este evento
        if (characterManager != null)
        {
            OnKeyPressed += characterManager.CheckInput;
        }
    }

    void OnDestroy()
    {
        // Limpeza essencial: remove o CharacterManager do evento ao destruir
        if (characterManager != null)
        {
            OnKeyPressed -= characterManager.CheckInput;
        }
    }

    private void OnGUI()
    {
        Event e = Event.current;    // Captura o evento atual de input.


        if (e.isKey && e.type == EventType.KeyDown) // Garante que é um evento de tecla sendo pressionada.
        {
            if (e.character != '\0')    // Garante que a tecla pressionada resultou em um caractere (não é SHIFT, ALT, etc.). O caractere '0' ou '\0' geralmente indica teclas não imprimíveis.
            {
                if (GameManager.Instance.isGameActive)  // Verifica se o jogo está ativo antes de enviar o input. Evita erros durante o EndGame.
                {
                    PlayRandomTypingSound();
                    OnKeyPressed?.Invoke(e.character);  // Invoca o evento, enviando o caractere exato digitado. Isso permite que o CharacterManager faça a checagem.
                }
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

    private void Update()
    {
        if (GameManager.Instance.isGameActive)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    if (characterManager != null)
                    {
                        characterManager.SkipCharacter();
                    }
                }
            }
        }
    }
}