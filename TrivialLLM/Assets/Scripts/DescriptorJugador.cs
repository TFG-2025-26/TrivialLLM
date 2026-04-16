using UnityEngine;

[System.Serializable]
public class DescriptorJugador
{
    public string nombre;
    public bool esHumano;
    //Si no es humano
    public AIService.Models modelo;
    public string prompt;
    public int fichaIndex = -1;
}
