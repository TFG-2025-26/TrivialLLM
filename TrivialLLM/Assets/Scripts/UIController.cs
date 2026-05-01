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
    public TextMeshProUGUI textPregunta;
    public Button[] botonesOpciones;
    public TextMeshProUGUI textRespuesta;

    public TextMeshProUGUI textTema;
    public TextMeshProUGUI textModeloPregunta;

    [Header("Fichas de los jugadores")]
    public FichaTrivial[] fichasTablero; // Fichas que se mueven por el tablero
    public FichaTrivial[] fichasMarcadores; // Las de la interfaz

    [Header("Sprites feedback botones")]
    public Sprite spriteBotonNormal;
    public Sprite spriteBotonCorrecto;
    public Sprite spriteBotonIncorrecto;
    [Header("Sonidos respuesta")]
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;


    public AIService ai;
    private int respuestaCorrecta;

    [Header("UI del tablero")]
    public TextMeshProUGUI textWaiting;
    private Coroutine animationLoading;

    private bool isFinalRound = false;
    private int numCorrectAnswerFinal = 0;
    private int currentQuestionFinal = 0;
    private List<string> topicsFinalRound = new List<string>();
    //private bool esTurnoHumano;

    private void Start()
    {
        // Panel de preguntas aparece apagado
        if(quizPanel != null) quizPanel.SetActive(false);
        // Texto de esperar aparece apagado
        if (textWaiting != null) textWaiting.gameObject.SetActive(false);

        ai =GameObject.Find("AIService").GetComponent<AIService>();
        ai.uiController = this; 
    }
    public void MostrarPregunta(PreguntaOpciones p)
    {
        if(p == null || p.opciones == null || p.opciones.Length < botonesOpciones.Length)
        {
            Debug.LogError("Error: La IA no devolvio las opciones correctamente.");
            textPregunta.text = "Error al generar la pregunta. Vuelve a intentarlo";
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
        textPregunta.text = p.pregunta;

        DescriptorJugador jugActual = GameManager.GetInstance().getJugTurnoActual();
        //Debug.Log($"Turno de: {jugActual.nombre} | ¿Es humano?: {jugActual.esHumano}");

        // Mostrar tema y modelo que pregunta
        if (textTema != null && ai != null)
        {
            textTema.text = "Tema: " + ai.categoriaActual;
        }
        if (textModeloPregunta != null && jugActual != null)
        {
            textModeloPregunta.text = "Pregunta: " + jugActual.modeloPreguntas.ToString();
        }

        //if (GameManager.GetInstance() != null && GameManager.GetInstance().descriptorJug.Count > 0)
        //{
        //    esTurnoHumano = (GameManager.GetInstance().getJugTurnoActual().esHumano);
        //}

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = p.opciones[i];

            // Resetear sprite normal al cargar nueva pregunta
            botonesOpciones[i].GetComponent<Image>().sprite = spriteBotonNormal;

            // Los botones son interactuables solo para jugadores humanos
            botonesOpciones[i].interactable = jugActual.esHumano;

            int index = i;
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => SeleccionarRespuesta(index));

        }

        // La IA responde automaticamente
        if(!jugActual.esHumano)
        {
            Debug.Log("Turno de la IA: " + jugActual.nombre + ". Contestando automaticamente...");
            StartCoroutine(EsperarYContestarIA());
        }
        respuestaCorrecta = p.respuesta_correcta;
    }

    private IEnumerator EsperarYContestarIA()
    {
        yield return new WaitForSeconds(5.0f);
        MandarPregunta();
    }

    public void MandarPregunta()
    {
        string prompt = textPregunta.text + "\nOpciones:\n";

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            string opcion = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {opcion}\n";
        }

        DescriptorJugador jug = GameManager.GetInstance().getJugTurnoActual();

        ai.ContestarPregunta(
            jug.modelo,        // modelo del jugador
            jug.perfil,        // perfil del jugador
            prompt,
            (int indexRespuesta) =>
            {
                if (indexRespuesta >= 0)
                {
                    SeleccionarRespuesta(indexRespuesta);
                }
            }
        );
    }

    public void SeleccionarRespuesta(int index)
    {
        if(textRespuesta != null)
        {
            textRespuesta.text = /*index + ") " + */botonesOpciones[index].GetComponentInChildren<TextMeshProUGUI>().text;
        }

        StartCoroutine(MostrarResultadoVisual(index));
        
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

    private IEnumerator MostrarResultadoVisual(int index)
    {
        // Bloquear botones
        foreach (var boton in botonesOpciones) boton.interactable = false;

        Image imgBotonElegido = botonesOpciones[index].GetComponent<Image>();

        // Comprobar acierto
        if (index == respuestaCorrecta)
        {
            // Cambiar a sprite correcto
            imgBotonElegido.sprite = spriteBotonCorrecto;
            gameObject.GetComponent<AudioSource>().PlayOneShot(sonidoCorrecto);

            // Si es ronda final, se suma acierto
            if (isFinalRound) numCorrectAnswerFinal++;

            // Siguiente turno
            int turnoIndex = 0;
            if (GameManager.GetInstance() != null) turnoIndex = GameManager.GetInstance().GetTurnoIndex();

            // Dar los quesitos a las fichas
            if (ai != null && !string.IsNullOrEmpty(ai.categoriaActual))
            {
                if (fichasTablero.Length > turnoIndex && fichasTablero[turnoIndex] != null)
                {
                    fichasTablero[turnoIndex].GanarQuesito(ai.categoriaActual);
                }
                if (fichasMarcadores.Length > turnoIndex && fichasMarcadores[turnoIndex] != null)
                {
                    fichasMarcadores[turnoIndex].GanarQuesito(ai.categoriaActual);
                }
            }
        }
        else
        {
            // Cambiar a sprite incorrecto
            imgBotonElegido.sprite = spriteBotonIncorrecto;
            gameObject.GetComponent<AudioSource>().PlayOneShot(sonidoIncorrecto);

            // Mostrar cual era la correcta
            botonesOpciones[respuestaCorrecta].GetComponent<Image>().sprite = spriteBotonCorrecto;

        }

        // Esperar para ver el resultado
        yield return new WaitForSeconds(3.0f);



        // Borrar texto de la pregunta y  de respuesta de la IA
        if (textPregunta != null) textPregunta.text = "";
        if (textRespuesta != null) textRespuesta.text = "";

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

                if (numCorrectAnswerFinal >= 4)
                {
                    textPregunta.text = $"¡Victoria! Has acertado {numCorrectAnswerFinal} de 6.\n¡{GameManager.GetInstance().getJugTurnoActual().nombre} gana la partida!";

                    yield return new WaitForSeconds(4.0f);
                    SceneManager.LoadScene("EndScene");
                    yield break;
                }
                else
                {
                    textPregunta.text = $"Ronda fallida. Has acertado {numCorrectAnswerFinal} de 6.\nSe necesitan al menos 4. ¡Inténtalo en el próximo turno!";
                    yield return new WaitForSeconds(3.0f);
                    isFinalRound = false;
                }
            }
        }

        // Desactivar el panel del quiz y pasar el turno
        quizPanel.SetActive(false);
        // Resetear el dado y el estado de movimiento antes de cambiar de turno
        DiceTrows dadoUI = FindFirstObjectByType<DiceTrows>();
        if (dadoUI != null)
        {
            dadoUI.ActivarBotonLanzar();
        }

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().wasteMovement();
            GameManager.GetInstance().setSelectedStatus(false);
            GameManager.GetInstance().cleanDstBoard();
        }

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().sigTurno();
        }
        ActualizarIndicadoresTurno();
    }

    public void ActualizarIndicadoresTurno()
    {
        int turnoIndex = GameManager.GetInstance().GetTurnoIndex();

        for (int i = 0; i < fichasTablero.Length; i++)
        {
            bool esTurno = (i == turnoIndex);

            if (fichasTablero[i] != null)
            {
                fichasTablero[i].SetTurnoActivo(esTurno);
            }

            if (fichasMarcadores[i] != null && fichasMarcadores.Length > i && fichasMarcadores[i] != null)
            {
                fichasMarcadores[i].SetTurnoActivo(esTurno);
            }

        }
    }

    public void ShowTextLoading()
    {
        if (textWaiting != null)
        {
            textWaiting.gameObject.SetActive(true);
            if(animationLoading != null) StopCoroutine(animationLoading);
            animationLoading = StartCoroutine(StartAnimationLoading());
        }
    }

    private IEnumerator StartAnimationLoading()
    {
        string baseText = "Esperando pregunta";
        int dots = 0;

        while (true)
        {
            string textDots = new string('.', dots);
            textWaiting.text = baseText + textDots;

            dots++;
            if (dots > 3) dots = 0;

            yield return new WaitForSeconds(0.4f);
        }
    }

    public void StartFinalRound()
    {
        isFinalRound = true;
        numCorrectAnswerFinal = 0;
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

        DescriptorJugador jug = GameManager.GetInstance().getJugTurnoActual();
        Debug.Log($"Ronda Final ({currentQuestionFinal + 1}/6) para {jug.nombre}: Tema {topic}, Dificultad {dificultadPregunta}");

        // Pedir pregunta 
        ai.PedirPregunta(jug.modeloPreguntas, jug.modelo, topic, dificultadPregunta);
    }
}
