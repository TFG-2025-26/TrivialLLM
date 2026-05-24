using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public class AIService : MonoBehaviour
{
    // LOCAL
    //private const string BASE_URL = "http://127.0.0.1:8000";

    // SERVIDOR AZURE
    private const string BASE_URL = "https://tfg-trivial-backend-cvgkbaehb5bse0gf.westeurope-01.azurewebsites.net";

    private string urlTrivial => BASE_URL + "/trivial";
    private string urlProfile => BASE_URL + "/profile";

    public UIController uiController;
    public GameManager gameManager;

    public enum Models {Gemini,Copilot,ChatGPT, Azure};

    public Models questionModel;
    public Models answerModel;

    public string currentCategory;
    public string currentDifficulty;
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

    
    public void RequestQuestion(Models questionModel, Models answerModel, string category, string difficulty)
    {
        this.questionModel = questionModel;
        this.answerModel = answerModel;
        this.currentCategory = category;
        this.currentDifficulty = difficulty;
        string prompt = BuildQuestionPrompt(category, difficulty);
        StartCoroutine(SendPrompt(this.questionModel,prompt, true));
    }

    public void ReplyQuestion(Models model, PlayerProfile profile, string question, string difficulty, System.Action<int> callback)
    {
        if (profile != null)
        {
            string strong = profile.strongCategories != null ? string.Join(", ", profile.strongCategories) : "Ninguno";
            string weak = profile.weakCategories != null ? string.Join(", ", profile.weakCategories) : "Ninguno";

            Debug.Log($"<color=cyan>[COMPROBACIÓN DE ROL IA]</color> Tema de la pregunta: <b>{currentCategory}</b>");
            Debug.Log($"<color=green>Temas Fuertes:</color> {strong}");
            Debug.Log($"<color=red>Temas Débiles:</color> {weak}");
            Debug.Log($"Nivel de Inteligencia (Accuracy): {profile.accuracyBase}");
        }
        string context = BuildRoleContext(profile, difficulty);
        string prompt = context + "\nPregunta:\n" + question;

        StartCoroutine(SendPrompt(model, prompt, false, callback));
    }

    private string BuildQuestionPrompt(string category, string difficulty)
    {
        int seed = Random.Range(0, 100000);
        return
            $@"Actúa como un experto creador de peguntas para el clásico juego de mesa Trivial Pursuit.

            Tema: {category}
            Dificultad: {difficulty}

            Instrucciones OBLIGATORIAS:
                - Estilo Trivial: La pregunta debe seguir el estilo clásico de un juego de mesa. Sé conciso y no te vayas por las ramas con introducciones largas.
                - Contexto claro: Especifica el sujeto exacto, si nombras personajes o eventos, nombra la obra claramente para evitar ambigüedades.
                - Cero pistas: No incluyas la respuesta correcta ni pistas evidentes dentro de la pregunta.
                - Originalidad: Genera UNA pregunta original y única, no repitas preguntas comunes.
                - La pregunta debe ser relevante para la categoría y la dificultad.
                - Varía el estilo: puede ser de opción múltiple directa, de deducción, de comparación, curiosidades...
                - Añade un toque creativo o curioso para que no se repita.
                - Usa la semilla de variación: {seed}
                

            Devuélveme SOLO un JSON valido con este formato exacto:

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

    private string BuildRoleContext(PlayerProfile profile, string difficulty)
    {
        if (profile == null) return "";

        string strong = profile.strongCategories != null ? string.Join(", ", profile.strongCategories) : "none";
        string weak = profile.weakCategories != null ? string.Join(", ", profile.weakCategories) : "none";

        return $@"
            Actúa exactamente como un jugador humano de Trivial respondiendo a una pregunta con este perfil:

            PERFIL DEL JUGADOR:
            - Nivel de inteligencia general: {profile.accuracyBase} (0.0 es ignorante, 1.0 es experto)
            - Temas que domina: {strong}
            - Temas débiles y que desconoce por completo: {weak}
            - Años de conocimiento: Desde {profile.knowledgeStart} hasta {profile.knowledgeCutoff}
            - Nivel de caos: {profile.randomness}

            DATOS DE LA PREGUNTA ACTUAL:
            - Tema: {currentCategory}
            - Dificultad de la pregunta actual: {difficulty}


            INSTRUCCIONES DE RAZONAMIENTO (OBLIGATORIAS):
            Tu objetivo principal es simular un comportamiento humano realista basándote en tu perfil.
            Debes decidir qué responder aplicando estas reglas paso a paso:

            1. Si el tema actual ({currentCategory}) coincide o está relacionado con tus temas dominados ({strong}), DEBES ELEGIR LA RESPUESTA CORRECTA OBLIGATORIAMENTE, sin importar tu nivel de inteligencia ni la dificultad.
            2. Si el tema actual ({currentCategory}) coincide o está relacionado con tus temas desconocidos ({weak}), DEBES ELEGIR UNA RESPUESTA INCORRECTA DELIBERADAMENTE, sin importar tu nivel de inteligencia ni la dificultad.
            3. Si la pregunta menciona eventos, hechos, obras (películas, libros o series) o personas anteriores al año  {profile.knowledgeStart} o posteriores al año  {profile.knowledgeCutoff}, DEBES ELEGIR UNA RESPUESTA INCORRECTA DELIBERADAMENTE.
            4. Evaluación de dificultad e inteligencia (Si no cumple lo anterior):
            - Si la dificultad es 'Fácil', intenta acertar.
            - Si la dificultad es 'Difícil', y tu nivel de inteligencia ({profile.accuracyBase}) es menor a 0.7, DEBES ELEGIR UNA RESPUESTA INCORRECTA.
            - Si tu nivel de inteligencia ({profile.accuracyBase}) es menor a 0.3, DEBES ELEGIR UNA RESPUESTA INCORRECTA casi siempre.
            5. Si tu nivel de caos ({profile.randomness}) es mayor a 0.7, elige una respuesta totalmente AL AZAR ignorando todo lo demás.

            Responde SOLO con el índice (0-3).
            ";
    }

    public IEnumerator RequestProfile(string role, System.Action<PlayerProfile> callback)
    {
        RoleRequest req = new RoleRequest { role = role };

        string jsonBody = JsonUtility.ToJson(req);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        //Debug.Log("ENVIANDO PERFIL: " + jsonBody);

        UnityWebRequest www = new UnityWebRequest(urlProfile, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        Debug.Log("RESULTADO: " + www.result);
        Debug.Log("RESPUESTA: " + www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ERROR PERFIL: " + www.error);
            Debug.LogError("BODY: " + www.downloadHandler.text);
            yield break;
        }

        PlayerProfile profile;

        try
        {
            profile = JsonUtility.FromJson<PlayerProfile>(www.downloadHandler.text);
        }
        catch
        {
            Debug.LogError(" JSON inválido en perfil");
            yield break;
        }

        if (profile == null)
        {
            Debug.LogError(" Perfil NULL");
            yield break;
        }

        callback?.Invoke(profile);
    }

    private System.Collections.IEnumerator SendPrompt(Models model,string prompt, bool isQuestion, System.Action<int> callback = null)
    {
        PromptRequest req = CreateRequest(model, prompt, isQuestion);
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
            //Debug.LogError("Error LLM: " + www.error);

            if (model != Models.Copilot)
            {
                Debug.LogWarning($"Fallo de red con {model}. Reintentando automáticamente con Copilot...");
                yield return StartCoroutine(SendPrompt(Models.Copilot, prompt, isQuestion, callback));
            }

            yield break;
        }

        else
        {
            
            string responseText = www.downloadHandler.text;
           // Debug.Log("LLM: " + responseText);

            if(responseText.Contains("\"error\""))
            {
                Debug.LogError("Error devuelto por backend: " + responseText);
                if (model != Models.Copilot)
                {
                    //Debug.LogWarning($"Fallo de API (Ej. Cuota excedida) con {model}. Reintentando automáticamente con Copilot...");
                    yield return StartCoroutine(SendPrompt(Models.Copilot, prompt, isQuestion, callback));
                }
                yield break;
            }

            if (isQuestion)
            {
                //Debug.Log("Pregunta generada por: " + model.ToString());
                // Asumir que la respuesta es directamente el JSON de PreguntaOpciones
                OptionsQuestion pregunta = JsonUtility.FromJson<OptionsQuestion>(responseText);

                // Mostrar la pregunta en pantalla
                ShowQuestion(pregunta);
            }
            else
            {
                //Debug.Log("Respuesta generada por: "+ model.ToString());
                string cleanAnswer = responseText.Trim().Replace("\"", "").Replace("\r", "").Replace("\n", "");
                if (int.TryParse(cleanAnswer, out int indexRespuesta))
                {
                    //Debug.Log("AIService");
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

    private void ShowQuestion(OptionsQuestion pregunta)
    {
        uiController.ShowQuestion(pregunta);
    }
}
