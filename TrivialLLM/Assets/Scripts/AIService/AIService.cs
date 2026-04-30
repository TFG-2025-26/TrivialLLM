using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class AIService : MonoBehaviour
{
    private const string BASE_URL = "http://127.0.0.1:8000";
    // si lo subo a un servidor https://mi-backend.com/trivial

    //private const string url = "https://tfg-trivial-backend-cvgkbaehb5bse0gf.westeurope-01.azurewebsites.net/trivial";

    private string urlTrivial => BASE_URL + "/trivial";
    private string urlProfile => BASE_URL + "/profile";

    public UIController uiController;
    public GameManager gameManager;

    public enum Models {Gemini,Copilot,ChatGPT, Azure};

    public Models modeloPregunta;
    public Models modeloRespuesta;

    public string categoriaActual;
    public static AIService Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

    public void ContestarPregunta(Models model, PlayerProfile perfil, string pregunta, System.Action<int> callback)
    {
        string context = BuildRoleContext(perfil);
        string prompt = context + "\nPregunta:\n" + pregunta;

        StartCoroutine(EnviarPrompt(model, prompt, false, callback));
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

    private string BuildRoleContext(PlayerProfile p)
    {
        if (p == null) return "";

        string strong = p.strongCategories != null ? string.Join(", ", p.strongCategories) : "none";
        string weak = p.weakCategories != null ? string.Join(", ", p.weakCategories) : "none";

        return $@"
Eres una IA que responde preguntas tipo test.

PERFIL DEL JUGADOR:
- Precisión base: {p.accuracyBase}
- Categorías fuertes: {strong}
- Categorías débiles: {weak}

REGLA CRÍTICA (OBLIGATORIA):
Debes responder como si lanzaras un dado.

SIMULACIÓN:
1. Si la categoría es débil → 70% de fallar
2. Si es fuerte → 70% de acertar
3. Si accuracyBase < 0.3 → aumenta probabilidad de error

IMPORTANTE:
NO intentes ser correcto siempre.
Tu objetivo es simular comportamiento humano imperfecto.

Responde SOLO con el índice (0-3).
";
    }

    public IEnumerator PedirPerfil(string role, System.Action<PlayerProfile> callback)
    {
        RoleRequest req = new RoleRequest { role = role };

        string jsonBody = JsonUtility.ToJson(req);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        Debug.Log("ENVIANDO PERFIL: " + jsonBody);

        UnityWebRequest www = new UnityWebRequest(urlProfile, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        Debug.Log("RESULTADO: " + www.result);
        Debug.Log("RESPUESTA: " + www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ ERROR PERFIL: " + www.error);
            Debug.LogError("❌ BODY: " + www.downloadHandler.text);
            yield break;
        }

        PlayerProfile perfil;

        try
        {
            perfil = JsonUtility.FromJson<PlayerProfile>(www.downloadHandler.text);
        }
        catch
        {
            Debug.LogError(" JSON inválido en perfil");
            yield break;
        }

        if (perfil == null)
        {
            Debug.LogError(" Perfil NULL");
            yield break;
        }

        callback?.Invoke(perfil);
    }

    private System.Collections.IEnumerator EnviarPrompt(Models model,string prompt, bool esPregunta, System.Action<int> callback = null)
    {
        PromptRequest req = CreateRequest(model, prompt, esPregunta);
        string jsonBody = JsonUtility.ToJson(req);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest www = new UnityWebRequest(urlTrivial, "POST");
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
                    Debug.LogError("No se pudo parsear incluso despues de limpiar: '" + responseText + "'");
                }
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
    }

    private void MostrarPregunta(PreguntaOpciones pregunta)
    {
        uiController.MostrarPregunta(pregunta);
    }
}
