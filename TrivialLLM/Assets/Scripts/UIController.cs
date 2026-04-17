using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    //private bool esTurnoHumano;

    private void Start()
    {
        // Panel de preguntas aparece apagado
        if(quizPanel != null) quizPanel.SetActive(false);
    }
    public void MostrarPregunta(PreguntaOpciones p)
    {
        if(p == null || p.opciones == null || p.opciones.Length < botonesOpciones.Length)
        {
            Debug.LogError("Error: La IA no devolvio las opciones correctamente.");
            textPregunta.text = "Error al generar la pregunta. Vuelve a intentarlo";
            return;
        }

        // Mostrar panel del quiz
        quizPanel.SetActive(true);
        textPregunta.text = p.pregunta;

        DescriptorJugador jugActual = GameManager.GetInstance().getJugTurnoActual();
        Debug.Log($"Turno de: {jugActual.nombre} | ¿Es humano?: {jugActual.esHumano}");

        // Mostrar tema y modelo que pregunta
        if (textTema != null && ai != null)
        {
            textTema.text = "Tema: " + ai.categoriaActual;
        }
        if (textModeloPregunta != null && jugActual != null)
        {
            textModeloPregunta.text = "Pregunta: " + jugActual.modeloPreguntas.ToString();
        }
        // Comprobar el modo de juego
        // esTurnoHumano = true; // por defecto

        //if (GameManager.GetInstance() != null)
        //{
        //    esTurnoHumano = (GameManager.GetInstance().getJugTurnoActual().esHumano);
        //}

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
            StartCoroutine(EsperarYContestarIA(jugActual.modelo));
        }
        respuestaCorrecta = p.respuesta_correcta;
    }

    private IEnumerator EsperarYContestarIA(AIService.Models modelo)
    {
        yield return new WaitForSeconds(2.0f);
        MandarPregunta(modelo);
    }

    public void MandarPregunta(AIService.Models modeloSeleccionado)
    {
        string prompt = textPregunta.text + "\nOpciones:\n";
        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            string opcion = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {opcion}\n";
        }

        ai.ContestarPregunta(modeloSeleccionado, prompt, (int indexRespuesta) =>
        {
            if (indexRespuesta >= 0)
            {
                Debug.Log("UICONTROLLER");
                SeleccionarRespuesta(indexRespuesta);
            }
            else
            {
                if (textRespuesta != null) textRespuesta.text = "Error al obtener la respuesta de " + modeloSeleccionado.ToString();
            }
        });
        
    }

    public void SeleccionarRespuesta(int index)
    {
        if(textRespuesta != null)
        {
            textRespuesta.text = index + ") " + botonesOpciones[index].GetComponentInChildren<TextMeshProUGUI>().text;
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
        //            fichaJugador.GanarQuesito(ai.categoriaActual);
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
        yield return new WaitForSeconds(6.0f);

        // Desactivar el panel del quiz y pasar el turno
        quizPanel.SetActive(false);

        // Borrar texto de la pregunta y  de respuesta de la IA
        if (textPregunta != null) textPregunta.text = "";
        if (textRespuesta != null) textRespuesta.text = "";
        if (GameManager.GetInstance() != null) GameManager.GetInstance().sigTurno();
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
}
