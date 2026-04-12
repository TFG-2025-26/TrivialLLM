using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI textPregunta;
    public Button[] botonesOpciones;

    private int respuestaCorrecta;

    public TextMeshProUGUI txt;


    /* public CopilotService copilot;
     public GeminiService gemini;
     public ChatGPTService chatGPT;*/
    public AIService ai;
    public FichaTrivial fichaJugador;

    private bool esTurnoHumano;

    public void MostrarPregunta(PreguntaOpciones p)
    {
        if(p == null || p.opciones == null || p.opciones.Length < botonesOpciones.Length)
        {
            Debug.LogError("Error: La IA no devolvio las opciones correctamente.");
            textPregunta.text = "Error al generar la pregunta. Vuelve a intentarlo";
            return;
        }
        textPregunta.text = p.pregunta;

        // Comprobar el modo de juego
        esTurnoHumano = true; // por defecto

       if (GameManager.GetInstance() != null)
        {
            esTurnoHumano = (GameManager.GetInstance().getJugTurnoActual().esHumano);
        }

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = p.opciones[i];

            // Los botones son interactuables dependiendo del modo de juego
            botonesOpciones[i].interactable = true;

            int index = i;
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => SeleccionarRespuesta(index));

        }

        respuestaCorrecta = p.respuesta_correcta;
    }

    public void mandarPregunta(AIService.Models modeloSeleccionado)
    {
        string prompt = textPregunta.text + "\nOpciones:\n";
        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            string opcion = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {opcion}\n";
        }

        if (!esTurnoHumano)
        {
            ai.ContestarPregunta(modeloSeleccionado, prompt, (int indexRespuesta) =>
            {
                if (indexRespuesta >= 0)
                {
                    Debug.Log("UICONTROLLER");
                    SeleccionarRespuesta(indexRespuesta);
                }
                else
                {
                    txt.text = "Error al obtener la respuesta de " + modeloSeleccionado.ToString();
                }
            });
        }
    }

    public void SeleccionarRespuesta(int index)
    {
        if(txt != null)
        {
            txt.text = index + ") " + botonesOpciones[index].GetComponentInChildren<TextMeshProUGUI>().text;
        }
        
        if (index == respuestaCorrecta)
        {
            Debug.Log("Respuesta correcta");
            //txt.text = "Correcto";
            if (fichaJugador != null)
            {
                if(ai != null && !string.IsNullOrEmpty(ai.categoriaActual))
                {
                    fichaJugador.GanarQuesito(ai.categoriaActual);
                }
                else
                {
                    Debug.LogWarning("El tema actual no esta guardado en AIService.");
                }
                
            }
            else
            {
                Debug.LogError("¡Falta asignar la Ficha Jugador en el Inspector del UIController!");
            }
            GameManager.GetInstance().sigTurno();
        }
        else
        {
            Debug.Log("Respuesta incorrecta");
            GameManager.GetInstance().sigTurno();
            //txt.text = "Incorrecto";
        }
        // Aqui se puede cargar otra pregunta, sumar puntos...
    }

    public void setCurrentPiece(FichaTrivial newPiece)
    {
        fichaJugador = newPiece;    
    }
}
