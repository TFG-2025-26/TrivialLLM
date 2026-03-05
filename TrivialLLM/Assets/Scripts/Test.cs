using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    /*public GeminiService geminiService;
    public CopilotService copilotService;
    public ChatGPTService chatGPTService;*/
    public AIService service;
    public TMP_Dropdown category;               //Categoria de la pregunta
    public TMP_Dropdown difficulty;             //Dificultad de la pregunta 
    public TMP_Text responseText;               // Donde se mostrará la respuesta

    public TMP_Dropdown modelQuestionType;      //Modelo que genera la pregunta
    public TMP_Dropdown modelAnswerType;        //Modelo que responde a la pregunta

    void Start()
    {
        Debug.Log("Test activo");
    }

    public void onClick()
    {
        // Obtener que IA genera la pregunta
        AIService.Models modeloPregunta = ObtenerModeloDropdown(modelQuestionType);

        // Obtener que IA va a responder (Solo en el modo de juego de partida de IA's)
        AIService.Models modeloRespuesta = AIService.Models.Copilot; // por defecto
        if(modelAnswerType != null )
        {
            modeloRespuesta = ObtenerModeloDropdown(modelAnswerType);
        }
        
        // Se pide la pregunta
        service.PedirPregunta(modeloPregunta,modeloRespuesta, category.options[category.value].text, difficulty.options[difficulty.value].text);

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


    private AIService.Models ObtenerModeloDropdown(TMP_Dropdown dropdown)
    {
        if(dropdown == null || dropdown.options.Count == 0)
        {
            return AIService.Models.Copilot;
        }

        string seleccionado = dropdown.options[dropdown.value].text;
        switch (seleccionado)
        {
            case "Gemini":
                return AIService.Models.Gemini;
            case "Copilot":
                return AIService.Models.Copilot;
            case "ChatGPT":
                return AIService.Models.ChatGPT;
            case "Azure":
                return AIService.Models.Azure;
            default:
                return AIService.Models.Copilot;
        }
    }
}
