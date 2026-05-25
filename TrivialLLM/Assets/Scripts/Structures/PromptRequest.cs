/// <summary>
/// Peticion enviada desde Unity al backend
/// </summary>
[System.Serializable]
public class PromptRequest
{
    public string prompt;       // Prompt que se envia al LLM. Contiene instrucciones para generar una pregunta o responderla
    public string model;        // Modelo que se va a utilizar
    public bool isAnswering;    // Indica el tipo de peticion, false debe generar una pregunta : true debe responder a una pregunta
}
