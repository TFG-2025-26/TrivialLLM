using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GeminiService geminiService;
    public CopilotService copilotService;
    public TMP_Dropdown category; //Categoria de la pregunta
    public TMP_Dropdown difficult; //Dificultad de la pregunta 
    public TMP_Text responseText;     // donde se mostrará la respuesta
    public TMP_Dropdown modelType; //Modelo que la genera

    void Start()
    {
        Debug.Log("Test activo");
    }

    public void onClick()
    {
      
        if (modelType.options[modelType.value].text == "Gemini")
        {
            string promptText = $"Genera una pregunta tipo test con 4 opciones y solo una opción correcta de categoría {category.options[category.value].text} y dificultad máxima {difficult.options[difficult.value].text}";
            Debug.Log("Prompt enviado al backend por Gemini: " + promptText);
            geminiService.PedirPregunta(promptText, modelType.options[modelType.value].text, (string result) =>
            {
                if (result == null)
                {
                    responseText.text = "Error al obtener la pregunta";
                    return;
                }

                responseText.text = result;
                Debug.Log("Respuesta recibida del backend");
            });
        }
        else if (modelType.options[modelType.value].text == "Copilot")
        {
            copilotService.PedirPregunta(category.options[category.value].text,difficult.options[difficult.value].text);
        }

    }
}
