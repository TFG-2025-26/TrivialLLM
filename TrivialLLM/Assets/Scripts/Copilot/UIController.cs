using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI textPregunta;
    public Button[] botonesOpciones;

    private int respuestaCorrecta;

    public TextMeshProUGUI txt;

    public void MostrarPregunta(PreguntaOpciones p)
    {
        textPregunta.text = p.pregunta;

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = p.opciones[i];

            int index = i;
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => SeleccionarRespuesta(index));

        }

        respuestaCorrecta = p.respuesta_correcta;
    }

    private void SeleccionarRespuesta(int index)
    {
        if (index == respuestaCorrecta)
        {
            Debug.Log("Respuesta correcta");
            txt.text = "Correcto";
        }
        else
        {
            Debug.Log("Respuesta incorrecta");
            txt.text = "Incorrecto";
        }
        // Aqui se puede cargar otra pregunta, sumar puntos...
    }
}
