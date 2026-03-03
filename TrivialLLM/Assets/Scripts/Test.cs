using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    /*public GeminiService geminiService;
    public CopilotService copilotService;
    public ChatGPTService chatGPTService;*/
    public AIService service;
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
        AIService.Models modeloSeleccionado;
        switch (modelType.options[modelType.value].text)
        {
            case "Gemini":
                modeloSeleccionado=AIService.Models.Gemini; 
                break;
            case "Copilot":
                modeloSeleccionado=AIService.Models.Copilot; 
                break;
            case "ChatGPT":
                modeloSeleccionado =AIService.Models.ChatGPT;
                break;
            default:
                modeloSeleccionado = AIService.Models.Copilot;
                break;
        }

        service.PedirPregunta(modeloSeleccionado,category.options[category.value].text, difficult.options[difficult.value].text);
       /* if (modelType.options[modelType.value].text == "Gemini")
        {
            string promptText = $"Genera una pregunta tipo test con 4 opciones y solo una opción correcta de categoría {category.options[category.value].text} y dificultad máxima {difficult.options[difficult.value].text}";
            Debug.Log("Prompt enviado al backend por Gemini: " + promptText);
            geminiService.PedirPregunta(promptText, modelType.options[modelType.value].text);
        }
        else if (modelType.options[modelType.value].text == "Copilot")
        {
            copilotService.PedirPregunta(category.options[category.value].text,difficult.options[difficult.value].text);
        }
        else if (modelType.options[modelType.value].text == "ChatGPT")
        {
            string promptText = $"Genera una pregunta tipo test con 4 opciones y solo una opción correcta de categoría {category.options[category.value].text} y dificultad máxima {difficult.options[difficult.value].text}";
            Debug.Log("Prompt enviado al backend por ChatGPT: " + promptText);
            chatGPTService.PedirPregunta(category.options[category.value].text, difficult.options[difficult.value].text);
        }*/

    }
}
