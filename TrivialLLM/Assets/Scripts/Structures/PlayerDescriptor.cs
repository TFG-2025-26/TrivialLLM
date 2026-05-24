using UnityEngine;

[System.Serializable]
public class PlayerDescriptor
{
    public string name;
    public bool isHuman;
    //Si no es humano, este es el modelo que responde
    public AIService.Models answerModel;
    // Modelo que genera las preguntas a este jugador
    public AIService.Models questionModel;

    //public string prompt;
    public PlayerProfile profile;
    public int indexPiece = -1;
}
