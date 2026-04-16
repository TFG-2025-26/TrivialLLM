using UnityEngine;

[System.Serializable]
public class DescriptorJugador
{
    public string nombre;
    public bool esHumano;
    //Si no es humano, este es el modelo que responde
    public AIService.Models modelo;
    // Modelo que genera las preguntas a este jugador
    public AIService.Models modeloPreguntas;

    public string prompt;
    public int fichaIndex = -1;
}
