using TMPro;
using UnityEngine;

public class TestGemini : MonoBehaviour
{
    public GeminiService geminiService;
    public TMP_InputField inputField; // donde escribe el usuario
    public TMP_Text responseText;     // donde se mostrará la respuesta
    public Models.ModelTypes modelType;

    void Start()
    {
        Debug.Log("TestGemini activo");
    }

    public void onClick()
    {
        string prompt = inputField.text;
        Debug.Log("Prompt enviado al backend: " + prompt);

        geminiService.PedirPregunta(prompt,modelType.ToString(), (string result) =>
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
}
