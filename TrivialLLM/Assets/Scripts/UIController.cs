using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI textPregunta;
    public Button[] botonesOpciones;

    private int respuestaCorrecta;

    public TextMeshProUGUI txt;

    public CopilotService copilot;
    public GeminiService gemini;

    public void MostrarPregunta(PreguntaOpciones p)
    {
        textPregunta.text = p.pregunta;

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = p.opciones[i];

            int index = i;
            //botonesOpciones[i].onClick.RemoveAllListeners();
            //botonesOpciones[i].onClick.AddListener(() => SeleccionarRespuesta(index));

        }

        respuestaCorrecta = p.respuesta_correcta;
    }

    public void mandarPregunta(string modelo)
    {
        string prompt = textPregunta.text + "\nOpciones:\n";
        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            string opcion = botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text;
            prompt += $"{i}) {opcion}\n";
        }
        switch (modelo)
        {
            case "Gemini":
                gemini.ContestarPregunta(prompt, (int indexRespuesta) =>
                {
                    if (indexRespuesta >= 0)
                    {
                        SeleccionarRespuesta(indexRespuesta);
                    }
                    else
                    {
                        txt.text = "Error al obtener la respuesta de Copilot";
                    }
                });
                
                break;
            case "Copilot":
                copilot.ContestarPregunta(prompt, (int indexRespuesta) =>
                {
                    if (indexRespuesta >= 0)
                    {
                        SeleccionarRespuesta(indexRespuesta);
                    }
                    else
                    {
                        txt.text = "Error al obtener la respuesta de Copilot";
                    }
                });
                break;
            case "ChatGPT":
                break;
            default:
                break;
        }
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
