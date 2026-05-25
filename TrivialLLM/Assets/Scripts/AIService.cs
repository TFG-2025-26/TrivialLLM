using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Se encarga de gestionar la comunicacion entre Unity y el backend
/// Permite generar preguntas, responder preguntas y crear perfiles de jugadores a partir de roles
/// </summary>
public class AIService : MonoBehaviour
{
    // Comentar/descomentar segun cual se quiera acceder

    private const string BASE_URL = "http://127.0.0.1:8000";  // URL base del backend cuando se ejecuta en LOCAL

    //private const string BASE_URL = "https://tfg-trivial-backend-cvgkbaehb5bse0gf.westeurope-01.azurewebsites.net";   // URL base del backend desplegado en el SERVIDOR AZURE

    private string urlTrivial => BASE_URL + "/trivial";     // Endpoint del backend encargado de generar preguntas y responderlas

    private string urlProfile => BASE_URL + "/profile";     // Endpoint del backend encargado de generar perfiles de jugador a partir de roles

    public UIController uiController;                       // Referencia al controlador de interfaz
    public GameManager gameManager;                         // Referencia al GameManager para acceder a la informacion de la partida

    public enum Models {Gemini,Copilot,ChatGPT, Azure};     // Modelos disponibles para generar preguntas o responderlas

    public Models questionModel;                            // Modelo encargado de generar la pregunta actual
    public Models answerModel;                              // Modelo encargado de responder la pregunta actual

    public string currentCategory;                          // Categoria de la pregunta actual
    public string currentDifficulty;                        // Dificultad de la pregunta actual
    public static AIService Instance;                       // Instancia estatica para manterner un unico AIService durante la partida

    private void Awake()
    {
        // Si ya existe un AIService, se destruye este para evitar duplicados
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Se guarda esta instancia y se mantiene al cambiar de escena
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Buscar el gamemanager de la escena
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Solicita al backend una pregunta de Trivial, segun la categoria, dificultad y modelo indicados
    public void RequestQuestion(Models questionModel, Models answerModel, string category, string difficulty)
    {
        // Modelo que generara la pregunta
        this.questionModel = questionModel;
        // Modelo que respondera la pregunta
        this.answerModel = answerModel;
        // Categoria de la pregunta
        this.currentCategory = category;
        // Dificultad de la preguna
        this.currentDifficulty = difficulty;

        // Construir el prompt que se enviara al modelo
        string prompt = BuildQuestionPrompt(category, difficulty);

        // Se envia la peteicion de que se quiere generar una pregunta al backend indicado
        StartCoroutine(SendPrompt(this.questionModel,prompt, true));
    }

    // Construye el prompt utilizado para pedir al LLM que genere una pregunta de Trivial
    private string BuildQuestionPrompt(string category, string difficulty)
    {
        // Semilla aleatoria para favorecer que el modelo genere preguntas variadas
        int seed = Random.Range(0, 100000);

        // Prompt completo para generar una pregunta
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

    // Solicita al backend que un modelo responda una pregunta segun el perfil del jugador
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

        // Se crea el contexto del rol para que el modelo responda de forma coherente con el perfil
        string context = BuildRoleContext(profile, difficulty);

        // Se añade la pregunta al contexto del rol
        string prompt = context + "\nPregunta:\n" + question;

        // Se envia la peticion al backend de que se quiere responder una pregunta al backend indicado
        StartCoroutine(SendPrompt(model, prompt, false, callback));
    }

    // Construye el contexto de rol que se enviara al modelo cuando tenga que responder
    // Obliga al modelo a comportarse segun el perfil generado para el jugador
    private string BuildRoleContext(PlayerProfile profile, string difficulty)
    {
        if (profile == null) return "";

        // Se convierten las listas de categorias fuertes y debiles en texto
        string strong = profile.strongCategories != null ? string.Join(", ", profile.strongCategories) : "none";
        string weak = profile.weakCategories != null ? string.Join(", ", profile.weakCategories) : "none";

        // Prompt con las instrucciones de comportamiento del jugador
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

    // Solicia al backend la creacion de un perfil de jugador a partir de un rol escrito por el usuario
    public IEnumerator RequestProfile(string role, System.Action<PlayerProfile> callback)
    {
        // Objeto que se enciara al backend
        RoleRequest req = new RoleRequest { role = role };

        // Convertir la peticion a JSON
        string jsonBody = JsonUtility.ToJson(req);
        // Codificar el JSON en bytes para poder enviarlo en el cuerpo de la peticion HTTP
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        // Crear una peticion POST al endpoint de perfiles
        UnityWebRequest www = new UnityWebRequest(urlProfile, "POST");
        // Añadir el cuerpo de la peticion
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        // Se prepeara el buffer donde se recibira la respuesta
        www.downloadHandler = new DownloadHandlerBuffer();
        // Se indica que el contenido enviado esta en formato JSON
        www.SetRequestHeader("Content-Type", "application/json");

        // Se envia la peticion y se espera la respuesta del backend
        yield return www.SendWebRequest();

        //Debug.Log("RESULTADO: " + www.result);
        //Debug.Log("RESPUESTA: " + www.downloadHandler.text);

        // Si la peticion falla, se muestra el error y se detiene la corrutina
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ERROR PERFIL: " + www.error);
            Debug.LogError("BODY: " + www.downloadHandler.text);
            yield break;
        }

        PlayerProfile profile;

        try
        {
            // Se cinvierte el JSON recibido en un objeto PlayerProfile
            profile = JsonUtility.FromJson<PlayerProfile>(www.downloadHandler.text);
        }
        catch
        {
            // Si el JSON no tiene un formato valido, se muestra un error
            Debug.LogError(" JSON inválido en perfil");
            yield break;
        }

        // Si por algun motivo el perfil es nulo, se detiene el proceso
        if (profile == null)
        {
            Debug.LogError(" Perfil NULL");
            yield break;
        }

        // Se devuelve el perfil generado al metodo que llamo a esta corrutina
        callback?.Invoke(profile);
    }

    // Envia un prompt al backend para generar una pregunta o responderla
    // Si el modelo elegido falla, intenta hacer la peticion usando Copilot como alternativa
    private System.Collections.IEnumerator SendPrompt(Models model,string prompt, bool isQuestion, System.Action<int> callback = null)
    {
        // Crear peticion que se enviara al backend
        PromptRequest req = CreateRequest(model, prompt, isQuestion);

        // Convertir la peticion a JSON
        string jsonBody = JsonUtility.ToJson(req);

        // Se codifica el JSON en bytes para enviarlo en el cuerpo HTTP
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // Se crea una peticion POST al endpoint principal del backend
        UnityWebRequest www = new UnityWebRequest(urlTrivial, "POST");
        // Añadir el cuerpo de la peticion
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        // Se prepeara el buffer donde se recibira la respuesta
        www.downloadHandler = new DownloadHandlerBuffer();
        // Se indican los formatos de envio y recepcion
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        // Enviar la peticion y esperar la respuesta
        yield return www.SendWebRequest();

        // Si se produce un error de red o conexion con el backend
        if (www.result != UnityWebRequest.Result.Success)
        {
            // Si el modelo que ha fallado no era Copilot, se reintenta automaticamente con Copilot
            if (model != Models.Copilot)
            {
                Debug.LogWarning($"Fallo de red con {model}. Reintentando automáticamente con Copilot...");
                yield return StartCoroutine(SendPrompt(Models.Copilot, prompt, isQuestion, callback));
            }

            // Detener la ejecucion de esta peticion
            yield break;
        }

        else
        {
            // Texto devuelto por el backend
            string responseText = www.downloadHandler.text;

            // Si el backend devuelve un JSON con error, se gestiona como fallo
            if(responseText.Contains("\"error\""))
            {
                Debug.LogError("Error devuelto por backend: " + responseText);

                // Si no era Copilot, se reintenta con Copilot como modelo de respaldo
                if (model != Models.Copilot)
                {
                    //Debug.LogWarning($"Fallo de API (Ej. Cuota excedida) con {model}. Reintentando automáticamente con Copilot...");
                    yield return StartCoroutine(SendPrompt(Models.Copilot, prompt, isQuestion, callback));
                }
                yield break;
            }

            // La respuesta del backend corresponde a una generacion de pregunta
            if (isQuestion)
            {
                // Se convierte el JSON recibido en un objeto OptionQuestion
                OptionsQuestion pregunta = JsonUtility.FromJson<OptionsQuestion>(responseText);

                // Mostrar la pregunta en pantalla
                ShowQuestion(pregunta);
            }
            // La respuesta del backend corresponde al indice de una respuesta
            else
            {
                // Se limpia la respuesta para quedarnos solo con el numero
                string cleanAnswer = responseText.Trim().Replace("\"", "").Replace("\r", "").Replace("\n", "");
                
                // Intentar convertir  la respuesta limpia a entero
                if (int.TryParse(cleanAnswer, out int indexRespuesta))
                {
                    // Se devuelve el indice de respuesta mediante el callback
                    callback?.Invoke(indexRespuesta);
                }
                else
                {
                    // Si no se puede convertir, se informa del error
                    Debug.LogError("No se pudo parsear incluso despues de limpiar: '" + responseText + "'");
                }
            }
        }
    }

    // Crea la estructura de datos que se enviara al backend
    private PromptRequest CreateRequest(Models model, string prompt, bool isQuestion)
    {
        // Objeto PromptRequest listo para convertirse en JSON
        return new PromptRequest
        {
            prompt = prompt,                // Prompt enviado al backend
            model = model.ToString(),       // Modelo seleccionado 
            isAnswering = !isQuestion       // Indica si la peticion es para generar pregunta o respuesta
        };
    }

    // Envia la pregunta generada al UIController para mostrarla por pantalla
    private void ShowQuestion(OptionsQuestion question)
    {
        uiController.ShowQuestion(question);
    }
}
