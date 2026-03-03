using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using System;

public class ChatGPTService : MonoBehaviour
{
    [SerializeField] private string apiUrl = "http://127.0.0.1:8000/trivial"; // URL de mi API
    // si lo subo a un servidor https://mi-backend.com/trivial

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

    private System.Collections.IEnumerator EnviarPrompt(string prompt, bool esPregunta, System.Action<int> callback = null)
    {
        PromptGPTData req = new PromptGPTData { 
            prompt = prompt,
            model = "ChatGPT", 
            max_tokens = 700, 
            temperature = 0,
            isAnswering=true
        };
        string jsonBody = JsonUtility.ToJson(req);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest www = new UnityWebRequest(apiUrl, "POST");
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
                // Asumir que la respuesta es directamente el JSON de PreguntaOpciones
                PreguntaOpciones pregunta = JsonUtility.FromJson<PreguntaOpciones>(responseText);

                // Ahora se puede pasar a la UI/ logica del juego
                MostrarPregunta(pregunta);
                uiController.mandarPregunta("ChatGPT");
            }
            else
            {
                if (int.TryParse(responseText.Trim(), out int indexRespuesta))
                {
                    callback?.Invoke(indexRespuesta);
                }
            }
        }
    }
    private void MostrarPregunta(PreguntaOpciones pregunta)
    {
        uiController.MostrarPregunta(pregunta);
    }
}

[Serializable]
public class PromptGPTData
{
    public string model;
    //public string input;
    public string prompt;
    public int max_tokens;
    public int temperature;
    public bool isAnswering;
}