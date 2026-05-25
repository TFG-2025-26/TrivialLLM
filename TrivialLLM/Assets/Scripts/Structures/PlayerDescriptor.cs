/// <summary>
/// Almacena la informacion de un jugador, tanto humano o controlado por IA
/// </summary>
[System.Serializable]
public class PlayerDescriptor
{
    public string name;                     // Nombre del jugador
    public bool isHuman;                    // Indica si es humano o no
    public AIService.Models answerModel;    // Si no es humano, este es el modelo que responde
    public AIService.Models questionModel;  // Modelo que genera las preguntas a este jugador
    public PlayerProfile profile;           // Perfil del jugador generado a partir del rol asignado
    public int indexPiece = -1;             // Indice de la ficha asignada al jugador. -1 indica que todavia no tiene                 
}
