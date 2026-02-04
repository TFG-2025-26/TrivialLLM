using TMPro;
using UnityEngine;

public class TestGemini : MonoBehaviour
{
    public GeminiService geminiService;
    public TMP_InputField inputField; // donde escribe el usuario
    public TMP_Text responseText;     // donde se mostrará la respuesta

    void Start()
    {
        Debug.Log("TestGemini activo");
    }

    public void onClick()
    {
        string prompt = inputField.text;
        Debug.Log("Prompt enviado: " + prompt);

        // Llamamos a Gemini y pasamos un callback para actualizar la UI
        geminiService.sendPrompt(prompt, (string result) =>
        {
            responseText.text = result; // mostramos solo la respuesta generada
            Debug.Log("Respuesta mostrada en TMP_Text");
        });
    }

}
