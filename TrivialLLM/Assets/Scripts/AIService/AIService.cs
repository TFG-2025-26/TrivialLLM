using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AIService : MonoBehaviour
{
// private const string url = "http://127.0.0.1:8000/trivial";
    // si lo subo a un servidor https://mi-backend.com/trivial

    private const string url = "https://tfg-trivial-backend-cvgkbaehb5bse0gf.westeurope-01.azurewebsites.net/trivial";
    public UIController uiController;
    public GameManager gameManager;

    public enum Models {Gemini,Copilot,ChatGPT, Azure};

    public Models modeloPregunta;
    public Models modeloRespuesta;

    public string categoriaActual;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void PedirPregunta(Models modeloPregunta, Models modeloRespuesta, string tema, string dificultad)
    {
        this.modeloPregunta = modeloPregunta;
        this.modeloRespuesta = modeloRespuesta;
        this.categoriaActual = tema;
        string prompt = CrearPromptPregunta(tema, dificultad);
        StartCoroutine(EnviarPrompt(this.modeloPregunta,prompt, true));
    }

    public void ContestarPregunta(Models model,string prompt, System.Action<int> callback)
    {
        //modeloRespuesta = model;
        modeloRespuesta = GameManager.GetInstance().getJugTurnoActual().modelo;
        Debug.Log("Respuesta generada por: "+ modeloRespuesta.ToString());
        Debug.Log(GameManager.GetInstance().getJugTurnoActual().prompt + ". " + prompt);

        StartCoroutine(EnviarPrompt(modeloRespuesta, GameManager.GetInstance().getJugTurnoActual().prompt+". "+prompt, false, callback));
    }

    private string CrearPromptPregunta(string tema, string dificultad)
    {
        int seed = Random.Range(0, 100000);
        return
            $@"Act�a como un generador de peguntas de trivial.

            Tema: {tema}
            Dificultad: {dificultad}

            Instrucciones:
                - Genera UNA pregunta original y única, no repitas preguntas comunes
                - La pregunta debe ser relevante para la categoría y la dificultad
                - Varía el estilo: puede ser de opción múltiple directa, de deducción, de comparación, curiosidades...
                - Añade un toque creativo o curioso para que no se repita
                - Usa la semilla de variación: {seed}

            Devu�lveme SOLO un JSON v�lido con este formato exacto:

            {{
                ""pregunta"": ""texto de la pregunta"",
                ""opciones"": [
                    ""opci�n 0"",
                    ""opci�n 1"",
                    ""opci�n 2"",
                    ""opci�n 3""
                 ],
                 ""respuesta_correcta"": INDICE_CORRECTO
            }}

        No a�adas comentarios, explicaciones ni texto fuera del JSON.";
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
            Debug.Log("LLM: " + responseText);

            if(responseText.Contains("\"error\""))
            {
                Debug.LogError("Error devuelto por backend: " + responseText);
                yield break;
            }

            if (esPregunta)
            {
                Debug.Log("Pregunta generada por: " + model.ToString());
                // Asumir que la respuesta es directamente el JSON de PreguntaOpciones
                PreguntaOpciones pregunta = JsonUtility.FromJson<PreguntaOpciones>(responseText);

                // Mostrar la pregunta en pantalla
                MostrarPregunta(pregunta);

                // Comprobar el modo de juego
                if (GameManager.GetInstance() != null &&
                    !GameManager.GetInstance().getJugTurnoActual().esHumano)
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
                string cleanAnswer = responseText.Trim().Replace("\"", "").Replace("\r", "").Replace("\n", "");
                if (int.TryParse(cleanAnswer, out int indexRespuesta))
                {
                    Debug.Log("AIService");
                    callback?.Invoke(indexRespuesta);
                   // GameManager.GetInstance().sigTurno();
                }
                else
                {
                    Debug.LogError("No se pudo parsear incluso despu�s de limpiar: '" + responseText + "'");
                }

                //if (model != Models.Gemini)
                //{
                //    if (int.TryParse(responseText.Trim(), out int indexRespuesta))
                //    {
                //        callback?.Invoke(indexRespuesta);
                //    }
                //    else
                //    {
                //        Debug.Log("Error al obtener la respuesta por " + model.ToString());
                //    }
                //}
                //else
                //{
                //    string cleanAnswer = responseText.Trim().Replace("\"", "").Replace("\r", "").Replace("\n", "");
                //    if (int.TryParse(cleanAnswer, out int indexRespuesta))
                //    {
                //        callback?.Invoke(indexRespuesta);
                //    }
                //    else
                //    {
                //        Debug.LogError("No se pudo parsear incluso despu�s de limpiar: '" + responseText + "'");
                //    }
                //}
            }
        }
    }

    private PromptRequest CreateRequest(Models model, string prompt, bool esPregunta)
    {
        return new PromptRequest
        {
            prompt = prompt,
            model = model.ToString(),
            isAnswering = !esPregunta
        };
        //switch (model)
        //{
        //    case Models.ChatGPT:
        //        return new PromptGPTData
        //        {
                   
        //        };

        //    case Models.Gemini:
        //        return new PromptRequest
        //        {
        //            prompt = prompt,
        //            model = model.ToString(),
        //            isAnswering = !esPregunta
        //        };

        //    case Models.Copilot:
        //        return new PromptRequest
        //        {
        //            prompt = prompt,
        //            model = model.ToString(),
        //            isAnswering = !esPregunta
        //        };

        //    case Models.Azure:
        //        return new PromptRequest
        //        {
        //            prompt = prompt,
        //            model = model.ToString(),
        //            isAnswering = !esPregunta
        //        };

        //    default:
        //        throw new System.Exception("Modelo no soportado");
        //}
    }

    private void MostrarPregunta(PreguntaOpciones pregunta)
    {
        uiController.MostrarPregunta(pregunta);
    }
}
