using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AIService : MonoBehaviour
{
    private const string url = "http://127.0.0.1:8000/trivial";
    // si lo subo a un servidor https://mi-backend.com/trivial
    public UIController uiController;

    public enum Models {Gemini,Copilot,ChatGPT};

    public Models modeloPregunta;
    public Models modeloRespuesta;

    public void PedirPregunta(Models model,string tema, string dificultad)
    {
        modeloPregunta = model;
        string prompt = CrearPromptPregunta(tema, dificultad);
        StartCoroutine(EnviarPrompt(modeloPregunta,prompt, true));
    }

    public void ContestarPregunta(Models model,string prompt, System.Action<int> callback)
    {
        modeloRespuesta = model;
        StartCoroutine(EnviarPrompt(modeloRespuesta,prompt, false, callback));
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

    private System.Collections.IEnumerator EnviarPrompt(Models model,string prompt, bool esPregunta, System.Action<int> callback = null)
    {
        PromptRequest req = CreateRequest(model, prompt, esPregunta);
        string jsonBody = JsonUtility.ToJson(req);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error LLM: " + www.error);
        }

        else
        {
            
            string responseText = www.downloadHandler.text;
            Debug.Log("Respuesta LLM: " + responseText);

            if (esPregunta)
            {
                Debug.Log("Pregunta generada por: " + model.ToString());
                // Asumir que la respuesta es directamente el JSON de PreguntaOpciones
                PreguntaOpciones pregunta = JsonUtility.FromJson<PreguntaOpciones>(responseText);

                // Mostrar la pregunta en pantalla
                MostrarPregunta(pregunta);

                // Comprobar el modo de juego
                if (GameManager.GetInstance() != null &&
                    GameManager.GetInstance().currentMode == GameManager.GameMode.AIGame)
                {
                    Debug.Log("Esperando la respuesta de la IA.");
                    uiController.mandarPregunta(modeloRespuesta);
                }
                else
                {
                    Debug.Log("Esperando la respuesta del jugador.");
                }
            }
            else
            {
                Debug.Log("Respuesta generada por: "+ model.ToString());
                if (model != Models.Gemini)
                {
                    if (int.TryParse(responseText.Trim(), out int indexRespuesta))
                    {
                        callback?.Invoke(indexRespuesta);
                    }
                    else
                    {
                        Debug.Log("Error al obtener la respuesta por " + model.ToString());
                    }
                }
                else
                {
                    string cleanAnswer = responseText.Trim().Replace("\"", "").Replace("\r", "").Replace("\n", "");
                    if (int.TryParse(cleanAnswer, out int indexRespuesta))
                    {
                        callback?.Invoke(indexRespuesta);
                    }
                    else
                    {
                        Debug.LogError("No se pudo parsear incluso después de limpiar: '" + responseText + "'");
                    }
                }
            }
        }
    }

    private PromptRequest CreateRequest(Models model, string prompt, bool esPregunta)
    {
        switch (model)
        {
            case Models.ChatGPT:
                return new PromptGPTData
                {
                    prompt = prompt,
                    model = model.ToString(),
                    max_tokens = 700,
                    temperature = 0,
                    isAnswering = !esPregunta
                };

            case Models.Gemini:
                return new PromptRequest
                {
                    prompt = prompt,
                    model = model.ToString(),
                    isAnswering = !esPregunta
                };

            case Models.Copilot:
                return new PromptRequest
                {
                    prompt = prompt,
                    model = model.ToString(),
                    isAnswering = !esPregunta
                };

            default:
                throw new System.Exception("Modelo no soportado");
        }
    }

    private void MostrarPregunta(PreguntaOpciones pregunta)
    {
        uiController.MostrarPregunta(pregunta);
    }
}
