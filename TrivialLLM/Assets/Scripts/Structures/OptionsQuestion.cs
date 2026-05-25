/// <summary>
/// Representa una pregunta de Trivial generada o recibida desde un LLM
/// </summary>
[System.Serializable]
public class OptionsQuestion
{
    public string pregunta;         // Texto de la pregunta
    public string[] opciones;       // Lista de opciones de respuesta
    public int respuesta_correcta;  // Indice de la respuesta correcta
}
