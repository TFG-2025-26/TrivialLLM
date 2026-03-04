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

    public void MostrarPregunta(PreguntaOpciones p)
    {
        textPregunta.text = p.pregunta;

        // Comprobar el modo de juego
        bool esTurnoHumano = true; // por defecto

        if (GameManager.GetInstance() != null)
        {
            esTurnoHumano = (GameManager.GetInstance().currentMode == GameManager.GameMode.HumanGame);
        }

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = p.opciones[i];

            // Los botones son interactuables dependiendo del modo de juego
            botonesOpciones[i].interactable = esTurnoHumano;

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

        ai.ContestarPregunta(modeloSeleccionado,prompt, (int indexRespuesta) =>
        {
            if (indexRespuesta >= 0)
            {
                Debug.Log("UICONTROLLER");
                SeleccionarRespuesta(indexRespuesta);
            }
            else
            {
                txt.text = "Error al obtener la respuesta de "+ modeloSeleccionado.ToString();
            }
        });
    }

    public void SeleccionarRespuesta(int index)
    {
        txt.text =index+") "+botonesOpciones[index].GetComponentInChildren<TextMeshProUGUI>().text;
        if (index == respuestaCorrecta)
        {
            Debug.Log("Respuesta correcta");
            //txt.text = "Correcto";
        }
        else
        {
            Debug.Log("Respuesta incorrecta");
            //txt.text = "Incorrecto";
        }
        // Aqui se puede cargar otra pregunta, sumar puntos...
    }
}
