using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Se encarga de gestionar la interfaz durante la partida
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Panel del Quiz")]
    public GameObject quizPanel;                    // Panel que muestra las preguntas y sus opciones

    [Header("Textos y botones")]
    public TextMeshProUGUI questionText;            // Texto de la pregunta
    public Button[] optionsButtons;                 // Botones con las opciones de respuestas
    public TextMeshProUGUI answerText;              // Texto de la respuesta

    public TextMeshProUGUI categoryText;            // Texto con la categoria de la pregunta
    public TextMeshProUGUI modelAskingText;         // Texto que indica el modelo que pregunta
    public TextMeshProUGUI currentTurnNameText;     // Texto con el nombre del jugador del turno actual
    public TextMeshProUGUI correctAnswerCountText;  // Texto con el nombre del jugador del turno actual

    [Header("Fichas de los jugadores")]
    public TrivialPiece[] boardgamePieces;          // Fichas que se mueven por el tablero
    public TrivialPiece[] scoreboardPieces;         // Fichas de la interfaz

    [Header("Sprites feedback botones")]
    public Sprite normalButtonSprite;  
    public Sprite correctButtonSprite;
    public Sprite incorrectButtonSprite;
    [Header("Sonidos respuesta")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    public AIService ai;                            // Referencia a AIService
    private int correctAnswerIndex;                 // Indice de la respuesta correcta

    [Header("UI del tablero")]
    public TextMeshProUGUI textWaiting;             // Texto de espera
    private Coroutine animationLoading;             // Animacion de espera

    public TextMeshProUGUI textThinking;            // Texto de pensando 
    private Coroutine animationThinking;            // Animacion de espera

    private bool isFinalRound = false;              // Indica si es la ronda final
    private int correctAnswerFinalCount = 0;        // Contador de respuestas correctas en la ronda final
    private int currentQuestionFinal = 0;           // Indica el indice de la pregunta actual en la ronda final
    private List<string> categoriesFinalRound = new List<string>();

    private void Start()
    {
        // Panel de preguntas aparece apagado
        if(quizPanel != null) quizPanel.SetActive(false);
        // Texto de esperar aparece apagado
        if (textWaiting != null) textWaiting.gameObject.SetActive(false);
        // Texto de pensar aparece apagado
        if (textThinking != null) textThinking.gameObject.SetActive(false);
        // Texto de numero de aciertos en la final aparece apagado
        if (correctAnswerCountText != null) correctAnswerCountText.gameObject.SetActive(false);

        ai = GameObject.Find("AIService").GetComponent<AIService>();
        ai.uiController = this; 
    }

    // Muestra la pregunta
    public void ShowQuestion(OptionsQuestion question)
    {
        if(question == null || question.opciones == null || question.opciones.Length < optionsButtons.Length)
        {
            Debug.LogError("Error: La IA no devolvio las opciones correctamente.");
            questionText.text = "Error al generar la pregunta. Vuelve a intentarlo";
            return;
        }

        // Apagar animacion
        if (animationLoading != null)
        {
            StopCoroutine(animationLoading);
            animationLoading = null;
        }

        if(textWaiting != null) textWaiting.gameObject.SetActive(false);

        // Mostrar panel del quiz
        quizPanel.SetActive(true);
        // Asignar texto de la pregunta
        questionText.text = question.pregunta;

        // Mostrar animacion de pensar
        ShowTextThinking();

        // Obtener la informacion del jugador del turno actual
        PlayerDescriptor currentPlayer = GameManager.GetInstance().GetPlayerCurrentTurn();

        // Mostrar tema, modelo que pregunta y nombre del jugador
        if (categoryText != null && ai != null)
        {
            categoryText.text = "Tema: " + ai.currentCategory;
        }
        if (modelAskingText != null && currentPlayer != null)
        {
            modelAskingText.text = "Pregunta: " + currentPlayer.questionModel.ToString();
        }
        if (currentTurnNameText != null && currentPlayer != null)
        {
            currentTurnNameText.text = "Turno: " + currentPlayer.name;
        }

        // Asigna las opciones a los botones
        for (int i = 0; i < optionsButtons.Length; i++)
        {
            optionsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.opciones[i];

            // Resetear sprite normal al cargar nueva pregunta
            optionsButtons[i].GetComponent<Image>().sprite = normalButtonSprite;

            // Los botones son interactuables solo para jugadores humanos
            optionsButtons[i].interactable = currentPlayer.isHuman;

            int index = i;
            optionsButtons[i].onClick.RemoveAllListeners();
            optionsButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }

        // La IA responde automaticamente
        if(!currentPlayer.isHuman)
        {
            StartCoroutine(WaitAndReplyAI());
        }

        // Asignar el indice de la respuesta correcta
        correctAnswerIndex = question.respuesta_correcta;
    }

    // Espera para dar tiempo a leer la pregunta. Tras la espera, se envia la pregunta para que responda
    private IEnumerator WaitAndReplyAI()
    {
        yield return new WaitForSeconds(1.0f);
        SendQuestion();
    }

    // Enviar la pregunta al modelo para que responde
    public void SendQuestion()
    {
        // Se crea el prompt con la pregunta y las opciones
        string prompt = questionText.text + "\nOpciones:\n";

        for (int i = 0; i < optionsButtons.Length; i++)
        {
            string option = optionsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {option}\n";
        }

        // Obtener la informacion del jugador del turno actual
        PlayerDescriptor player = GameManager.GetInstance().GetPlayerCurrentTurn();

        // El modelo responde
        ai.ReplyQuestion(
            player.answerModel,         // Modelo del jugador
            player.profile,             // Perfil del jugador
            prompt,
            ai.currentDifficulty,       // Dificultad de la pregunta
            (int indexRespuesta) =>
            {
                if (indexRespuesta >= 0)
                {
                    SelectAnswer(indexRespuesta);   // Selecciona una respuesta
                }
            }
        );
    }

    // Realiza las comprobaciones al seleccionar una respuesta
    public void SelectAnswer(int index)
    {
        // Parar la animacion de pensar
        if(animationThinking != null)
        {
            StopCoroutine(animationThinking );
            animationThinking = null;
        }
        if(textThinking != null) textThinking.gameObject.SetActive(false);

        // Mostrar la respuesta que ha elegido el jugador segun la opcion seleccionada
        if(answerText != null)
        {
            answerText.text = optionsButtons[index].GetComponentInChildren<TextMeshProUGUI>().text;
        }

        // Mostrar el resultado
        StartCoroutine(ShowVisualResult(index));
    }

    // Muestra de manera visual el resultado de la respuesta a la pregunta
    private IEnumerator ShowVisualResult(int index)
    {
        // Bloquear botones
        foreach (var button in optionsButtons) button.interactable = false;

        Image imgChosenButton = optionsButtons[index].GetComponent<Image>();

        // Comprobar acierto
        if (index == correctAnswerIndex)
        {
            // Cambiar al sprite correcto
            imgChosenButton.sprite = correctButtonSprite;
            gameObject.GetComponent<AudioSource>().PlayOneShot(correctSound);

            // Si es ronda final, se suma acierto
            if (isFinalRound) correctAnswerFinalCount++;

            // Siguiente turno
            int indexTurn = 0;
            if (GameManager.GetInstance() != null) indexTurn = GameManager.GetInstance().GetIndexTurn();

            // Dar los quesitos a las fichas
            if (ai != null && !string.IsNullOrEmpty(ai.currentCategory))
            {
                if (boardgamePieces.Length > indexTurn && boardgamePieces[indexTurn] != null)
                {
                    boardgamePieces[indexTurn].WinWedge(ai.currentCategory);
                }
                if (scoreboardPieces.Length > indexTurn && scoreboardPieces[indexTurn] != null)
                {
                    scoreboardPieces[indexTurn].WinWedge(ai.currentCategory);
                }
            }
        }
        else
        {
            // Cambiar a sprite incorrecto
            imgChosenButton.sprite = incorrectButtonSprite;
            gameObject.GetComponent<AudioSource>().PlayOneShot(incorrectSound);

            // Mostrar cual era la correcta
            optionsButtons[correctAnswerIndex].GetComponent<Image>().sprite = correctButtonSprite;
        }

        // Esperar para ver el resultado
        yield return new WaitForSeconds(3.0f);

        // Borrar texto de la pregunta, de respuesta y de las opciones
        if (questionText != null) questionText.text = "";
        if (answerText != null) answerText.text = "";

        foreach (var button in optionsButtons)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = "";
            button.GetComponent<Image>().sprite = normalButtonSprite;
        }

        // Si es la ronda final
        if (isFinalRound)
        {
            currentQuestionFinal++;

            // Actualizar texto aciertos
            UpdateFinalRoundCorrectText();

            if(currentQuestionFinal < 6)
            {
                // Quedan preguntas, pasar a la siguiente
                NextQuestionFinalRound();
                yield break;
            }
            else
            {
                // Terminan las 6 preguntas. Comprobar si ha ganado.
                quizPanel.SetActive(true);

                // Si el jugador ha acertado 4 de 6, se muestra mensaje de victoria y se cambia de escena
                if (correctAnswerFinalCount >= 4)
                {
                    questionText.text = $"¡Victoria! Has acertado {correctAnswerFinalCount} de 6.\n¡{GameManager.GetInstance().GetPlayerCurrentTurn().name} gana la partida!";

                    yield return new WaitForSeconds(4.0f);
                    SceneManager.LoadScene("EndScene");
                    yield break;
                }
                // Si el jugador no ha acertado 4 de 6, se muestra mensaje de derrota y se sigue jugando
                else
                {
                    questionText.text = $"Ronda fallida. Has acertado {correctAnswerFinalCount} de 6.\nSe necesitan al menos 4. ¡Inténtalo en el próximo turno!";
                    yield return new WaitForSeconds(3.0f);
                    isFinalRound = false;

                    // Desactivar texto de aciertos
                    if (correctAnswerCountText != null)
                    {
                        correctAnswerCountText.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Desactivar el panel del quiz y pasar el turno
        quizPanel.SetActive(false);

        // Resetear el dado y el estado de movimiento antes de cambiar de turno
        DiceThrow diceUI = FindFirstObjectByType<DiceThrow>();
        if (diceUI != null)
        {
            diceUI.ActiveThrowButton();
        }

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().WasteMovement();
            GameManager.GetInstance().SetSelectedStatus(false);
            GameManager.GetInstance().CleanDstBoard();
        }

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().NextTurn();
        }
        UpdateTurnSigns();
    }

    // Inicia la ronda final
    public void StartFinalRound()
    {
        isFinalRound = true;
        correctAnswerFinalCount = 0;
        currentQuestionFinal = 0;

        // Establecer el texto de numero de aciertos a 0
        if (correctAnswerCountText != null)
        {
            correctAnswerCountText.gameObject.SetActive(true);
            UpdateFinalRoundCorrectText();
        }

        // Cargar las 6 categorias
        categoriesFinalRound = new List<string> { "Ciencias", "Geografia", "Historia", "Arte y Literatura", "Deportes y Pasatiempos", "Entretenimiento" };

        // Muestra la primera pregunta
        NextQuestionFinalRound();
    }

    // Muestra y pide las preguntas de la ronda final
    private void NextQuestionFinalRound()
    {
        // Categoria actual segun el indice 
        string category = categoriesFinalRound[currentQuestionFinal];
        // Elige una dificultad aleatoria
        string[] difficulties = { "Facil", "Media", "Dificil" };
        string questionDifficulty = difficulties[Random.Range(0, difficulties.Length)];

        // Obtiene la informacion del jugador actual
        PlayerDescriptor player = GameManager.GetInstance().GetPlayerCurrentTurn();
        Debug.Log($"Ronda Final ({currentQuestionFinal + 1}/6) para {player.name}: Tema {category}, Dificultad {questionDifficulty}");

        // Pide la pregunta al modelo correspondiente, con la categoria y dificultad que toca 
        ai.RequestQuestion(player.questionModel, player.answerModel, category, questionDifficulty);
    }

    // Actualiza el texto de numero de aciertos en la ronda final
    private void UpdateFinalRoundCorrectText()
    {
        if (correctAnswerCountText != null)
        {
            correctAnswerCountText.text = "Aciertos: " + correctAnswerFinalCount + " /6";
        }
    }
    // Actualiza los indicadores de turno
    public void UpdateTurnSigns()
    {
        // Obtener el indice del turno actual
        int turnoIndex = GameManager.GetInstance().GetIndexTurn();

        for (int i = 0; i < boardgamePieces.Length; i++)
        {
            bool esTurno = (i == turnoIndex);

            // Se activa la luz de la ficha en el tablero
            if (boardgamePieces[i] != null)
            {
                boardgamePieces[i].SetActiveTurn(esTurno);
            }

            // Se hace opaca la ficha en el marcadoz de la UI
            if (scoreboardPieces[i] != null && scoreboardPieces.Length > i && scoreboardPieces[i] != null)
            {
                scoreboardPieces[i].SetActiveTurn(esTurno);
            }
        }
    }

    // Para mostrar textos
    private Coroutine ShowText(TextMeshProUGUI text, Coroutine animation, string baseText)
    {
        if (text != null)
        {
            // Muestra el texto
            text.gameObject.SetActive(true);
            if (animation != null) StopCoroutine(animation);

            // Comienza la animacion
            return StartCoroutine(StartAnimation(baseText, text));
        }
        return null;
    }

    // Mostrar texto de cargando
    public void ShowTextLoading()
    {
        animationLoading = ShowText(textWaiting, animationLoading, "Esperando pregunta");
    }

    // Mostrar texto de pensando
    public void ShowTextThinking()
    {
        animationThinking = ShowText(textThinking, animationThinking, "Pensando");
    }

    // Empieza una animacion de texto añadiendo cierto tiempo un .
    private IEnumerator StartAnimation(string baseText, TextMeshProUGUI text)
    {
        int dots = 0;
        while (true)
        {
            string textDots = new string('.', dots);
            text.text = baseText + textDots;

            dots++;
            if (dots > 3) dots = 0;

            yield return new WaitForSeconds(0.4f);
        }
    }
}
