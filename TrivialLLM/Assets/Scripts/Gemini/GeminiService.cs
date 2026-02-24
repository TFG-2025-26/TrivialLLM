using System.Collections;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService : MonoBehaviour
{
    private const string url = "http://127.0.0.1:8000/trivial";
    public UIController uiController;

    public void PedirPregunta(string tema, string dificultad)
    {
        string prompt = CrearPromptPregunta(tema, dificultad);
        StartCoroutine(EnviarPrompt(prompt, true));
    }

    public void ContestarPregunta(string prompt, System.Action<int> callback)
    {
        StartCoroutine(EnviarPrompt(prompt, false, callback));
    }

    private string CrearPromptPregunta(string tema, string dificultad)
    {
        return
            $@"Actúa como un generador de peguntas de trivial.

            Tema: {tema}
            Dificultad: {dificultad}

            Devuélveme SOLO un JSON válido con este formato exacto:

            {{
                ""pregunta"": ""texto de la pregunta"",
                ""opciones"": [
                    ""opción 0"",
                    ""opción 1"",
                    ""opción 2"",
                    ""opción 3""
                 ],
                 ""respuesta_correcta"": INDICE_CORRECTO
            }}

        No añadas comentarios, explicaciones ni texto fuera del JSON.";
    }



    private IEnumerator EnviarPrompt(string prompt, bool esPregunta, System.Action<int> callback = null)
    {
        PromptRequest req = new PromptRequest { prompt = prompt, model = "Gemini", isAnswering = !esPregunta };
        string json = JsonUtility.ToJson(req);  

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error LLM: " + request.downloadHandler.error);
        }
        else
        {
            Debug.Log("Respuesta backend:");
            Debug.Log(request.downloadHandler.text);
            string answer = request.downloadHandler.text;
            if (esPregunta)
            {
              
                PreguntaOpciones ask = JsonUtility.FromJson<PreguntaOpciones>(answer);
                uiController.MostrarPregunta(ask);
                uiController.mandarPregunta("Gemini");
            }
            else
            {
                if (int.TryParse(answer.Trim(), out int indexRespuesta))
                {
                    callback?.Invoke(indexRespuesta);
                }
            }
           
        }
    }
}
