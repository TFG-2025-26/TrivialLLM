using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("Panel del Quiz")]
    public GameObject quizPanel;

    [Header("Textos y botones")]
    public TextMeshProUGUI questionText;
    public Button[] optionsButtons;
    public TextMeshProUGUI answerText;

    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI modelAskingText;
    public TextMeshProUGUI currentTurnNameText;

    [Header("Fichas de los jugadores")]
    public TrivialPiece[] boardgamePieces; // Fichas que se mueven por el tablero
    public TrivialPiece[] scoreboardPieces; // Las de la interfaz

    [Header("Sprites feedback botones")]
    public Sprite normalButtonSprite;
    public Sprite correctButtonSprite;
    public Sprite incorrectButtonSprite;
    [Header("Sonidos respuesta")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;


    public AIService ai;
    private int correctAnswerIndex;

    [Header("UI del tablero")]
    public TextMeshProUGUI textWaiting;
    private Coroutine animationLoading;

    public TextMeshProUGUI textThinking;
    private Coroutine animationThinking;

    private bool isFinalRound = false;
    private int correctAnswerFinalCount = 0;
    private int currentQuestionFinal = 0;
    private List<string> topicsFinalRound = new List<string>();
    //private bool esTurnoHumano;

    private void Start()
    {
        // Panel de preguntas aparece apagado
        if(quizPanel != null) quizPanel.SetActive(false);
        // Texto de esperar aparece apagado
        if (textWaiting != null) textWaiting.gameObject.SetActive(false);
        // Texto de pensar aparece apagado
        if (textThinking != null) textThinking.gameObject.SetActive(false);

        ai =GameObject.Find("AIService").GetComponent<AIService>();
        ai.uiController = this; 
    }
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
        questionText.text = question.pregunta;

        ShowTextThinking();

        PlayerDescriptor currentPlayer = GameManager.GetInstance().GetPlayerCurrentTurn();
        //Debug.Log($"Turno de: {jugActual.nombre} | ¿Es humano?: {jugActual.esHumano}");

        // Mostrar tema y modelo que pregunta
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

        //if (GameManager.GetInstance() != null && GameManager.GetInstance().descriptorJug.Count > 0)
        //{
        //    esTurnoHumano = (GameManager.GetInstance().getJugTurnoActual().esHumano);
        //}

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
            //Debug.Log("Turno de la IA: " + jugActual.nombre + ". Contestando automaticamente...");
            StartCoroutine(WaitAndReplyAI());
        }
        correctAnswerIndex = question.respuesta_correcta;
    }

    private IEnumerator WaitAndReplyAI()
    {
        yield return new WaitForSeconds(1.0f);
        SendQuestion();
    }

    public void SendQuestion()
    {
        string prompt = questionText.text + "\nOpciones:\n";

        for (int i = 0; i < optionsButtons.Length; i++)
        {
            string option = optionsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {option}\n";
        }

        PlayerDescriptor player = GameManager.GetInstance().GetPlayerCurrentTurn();

        ai.ReplyQuestion(
            player.answerModel,        // modelo del jugador
            player.profile,        // perfil del jugador
            prompt,
            ai.currentDifficulty,
            (int indexRespuesta) =>
            {
                if (indexRespuesta >= 0)
                {
                    SelectAnswer(indexRespuesta);
                }
            }
        );
    }

    public void SelectAnswer(int index)
    {
        if(animationThinking != null)
        {
            StopCoroutine(animationThinking );
            animationThinking = null;
        }
        if(textThinking != null) textThinking.gameObject.SetActive(false);

        if(answerText != null)
        {
            answerText.text = /*index + ") " + */optionsButtons[index].GetComponentInChildren<TextMeshProUGUI>().text;
        }

        StartCoroutine(ShowVisualResult(index));
        
        //if (index == respuestaCorrecta)
        //{
        //    Debug.Log("Respuesta correcta");
        //    //txt.text = "Correcto";
        //    if (fichaJugador != null)
        //    {
        //        if(ai != null && !string.IsNullOrEmpty(ai.categoriaActual))
        //        {
        //           fichaJugador.GanarQuesito(ai.categoriaActual);
        //        }
        //        else
        //        {
        //            Debug.LogWarning("El tema actual no esta guardado en AIService.");
        //        }
                
        //    }
        //    else
        //    {
        //        Debug.LogError("¡Falta asignar la Ficha Jugador en el Inspector del UIController!");
        //    }
        //    GameManager.GetInstance().sigTurno();
        //}
        //else
        //{
        //    Debug.Log("Respuesta incorrecta");
        //    GameManager.GetInstance().sigTurno();
        //    //txt.text = "Incorrecto";
        //}
        //// Aqui se puede cargar otra pregunta, sumar puntos...
    }

    private IEnumerator ShowVisualResult(int index)
    {
        // Bloquear botones
        foreach (var button in optionsButtons) button.interactable = false;

        Image imgChosenButton = optionsButtons[index].GetComponent<Image>();

        // Comprobar acierto
        if (index == correctAnswerIndex)
        {
            // Cambiar a sprite correcto
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

        if (isFinalRound)
        {
            currentQuestionFinal++;
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

                if (correctAnswerFinalCount >= 4)
                {
                    questionText.text = $"¡Victoria! Has acertado {correctAnswerFinalCount} de 6.\n¡{GameManager.GetInstance().GetPlayerCurrentTurn().name} gana la partida!";

                    yield return new WaitForSeconds(4.0f);
                    SceneManager.LoadScene("EndScene");
                    yield break;
                }
                else
                {
                    questionText.text = $"Ronda fallida. Has acertado {correctAnswerFinalCount} de 6.\nSe necesitan al menos 4. ¡Inténtalo en el próximo turno!";
                    yield return new WaitForSeconds(3.0f);
                    isFinalRound = false;
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

    public void UpdateTurnSigns()
    {
        int turnoIndex = GameManager.GetInstance().GetIndexTurn();

        for (int i = 0; i < boardgamePieces.Length; i++)
        {
            bool esTurno = (i == turnoIndex);

            if (boardgamePieces[i] != null)
            {
                boardgamePieces[i].SetActiveTurn(esTurno);
            }

            if (scoreboardPieces[i] != null && scoreboardPieces.Length > i && scoreboardPieces[i] != null)
            {
                scoreboardPieces[i].SetActiveTurn(esTurno);
            }

        }
    }

    private Coroutine ShowText(TextMeshProUGUI text, Coroutine animation, string baseText)
    {
        if (text != null)
        {
            text.gameObject.SetActive(true);
            if (animation != null) StopCoroutine(animation);
            return StartCoroutine(StartAnimation(baseText, text));
        }
        return null;
    }
    public void ShowTextLoading()
    {
        animationLoading = ShowText(textWaiting, animationLoading, "Esperando pregunta");
    }
    public void ShowTextThinking()
    {
        animationThinking = ShowText(textThinking, animationThinking, "Pensando");
    }
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

    public void StartFinalRound()
    {
        isFinalRound = true;
        correctAnswerFinalCount = 0;
        currentQuestionFinal = 0;

        // Cargar las 6 categorias
        topicsFinalRound = new List<string> { "Ciencias", "Geografia", "Historia", "Arte y Literatura", "Deportes y Pasatiempos", "Entretenimiento" };

        NextQuestionFinalRound();
    }

    private void NextQuestionFinalRound()
    {
        string topic = topicsFinalRound[currentQuestionFinal];
        string[] dificultades = { "Facil", "Media", "Dificil" };
        string dificultadPregunta = dificultades[Random.Range(0, dificultades.Length)];

        PlayerDescriptor jug = GameManager.GetInstance().GetPlayerCurrentTurn();
        Debug.Log($"Ronda Final ({currentQuestionFinal + 1}/6) para {jug.name}: Tema {topic}, Dificultad {dificultadPregunta}");

        // Pedir pregunta 
        ai.RequestQuestion(jug.questionModel, jug.answerModel, topic, dificultadPregunta);
    }
}
