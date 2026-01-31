using UnityEngine;

public class TestGemini : MonoBehaviour
{
    public GeminiService geminiService;
    void Start()
    {
        geminiService.sendPrompt("Funciona esto?");
    }

}
