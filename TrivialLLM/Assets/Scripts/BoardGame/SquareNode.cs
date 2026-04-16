using System.Security.Cryptography.X509Certificates;
using UnityEngine;

/// <summary>
/// Script que sirve para informar a Unity que casillas estan conectadas entre sí y su tematica
/// </summary>
/// 
// Lista cerrada con las posibles opciones de las categorias de las casillas
public enum TrivialTopic
{
    Ciencias,
    Geografia,
    Historia,
    Arte_y_Literatura,
    Deportes_y_Pasatiempos,
    Entretenimiento,
    Dados,
    FinalCentro
}
public class SquareNode : MonoBehaviour
{
    [Header("Categoria de la casilla")]
    public TrivialTopic topic;

    [Header("Conexiones (Arrastra aqui otras casillas)")]
    // Casilla hacia el centro
    public SquareNode centre;
    // Casilla alejandose del centro
    public SquareNode outwards;
    // Siguiente casilla a la izquierda en el anillo exterior
    public SquareNode left;
    // Siguiente casilla a la derecha en el anillo exterior
    public SquareNode right;

    // Obtiene la opcion seleccionada y lo convierte a string
    public string getTopicString()
    {
        switch (topic)
        {
            case TrivialTopic.Ciencias: return "Ciencias";
            case TrivialTopic.Geografia: return "Geografia";
            case TrivialTopic.Historia: return "Historia";
            case TrivialTopic.Arte_y_Literatura: return "Arte y Literatura";
            case TrivialTopic.Deportes_y_Pasatiempos: return "Deportes y Pasatiempos";
            case TrivialTopic.Entretenimiento: return "Entretenimiento";
            case TrivialTopic.Dados: return "Dados";
            case TrivialTopic.FinalCentro: return "FinalCentro";
            default: return "Ciencias"; // Por seguridad
        }
    }
}
