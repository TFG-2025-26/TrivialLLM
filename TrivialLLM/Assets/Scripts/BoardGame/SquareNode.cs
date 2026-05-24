using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script que sirve para informar a Unity que casillas estan conectadas entre sí y su tematica
/// </summary>
/// 
// Lista cerrada con las posibles opciones de las categorias de las casillas
public enum TrivialCategories
{
    Ciencias,
    Geografia,
    Historia,
    Arte_y_Literatura,
    Deportes_y_Pasatiempos,
    Entretenimiento,
    Dados,
    Final
}
public class SquareNode : MonoBehaviour
{
    [Header("Categoria de la casilla")]
    public TrivialCategories category;

    [Header("Conexiones normales(Arrastra aqui otras casillas)")]
    // Casilla hacia el centro
    public SquareNode centre;
    // Casilla alejandose del centro
    public SquareNode outwards;
    // Siguiente casilla a la izquierda en el anillo exterior
    public SquareNode left;
    // Siguiente casilla a la derecha en el anillo exterior
    public SquareNode right;

    [Header("Conexiones extra(Solo para la casilla central)")]
    [Tooltip("Agrega aqui los 6 inicios de los caminos si esta es la casilla central")]
    public List<SquareNode> extraNodes = new List<SquareNode>();

    // Recopila todas las casillas conectadas a esta de forma limpia
    public List<SquareNode> ObtenerVecinos()
    {
        List<SquareNode> neighbours = new List<SquareNode>();

        if(centre != null) neighbours.Add(centre);
        if (outwards != null) neighbours.Add(outwards);
        if (left != null) neighbours.Add(left);
        if (right != null) neighbours.Add(right);

        // Agregar las extra si las hubiera , para el centro
        foreach(SquareNode extra in extraNodes)
        {
            if (extra != null) neighbours.Add (extra);
        }
        return neighbours;
    }
    // Obtiene la opcion seleccionada y lo convierte a string
    public string getTopicString()
    {
        switch (category)
        {
            case TrivialCategories.Ciencias: return "Ciencias";
            case TrivialCategories.Geografia: return "Geografia";
            case TrivialCategories.Historia: return "Historia";
            case TrivialCategories.Arte_y_Literatura: return "Arte y Literatura";
            case TrivialCategories.Deportes_y_Pasatiempos: return "Deportes y Pasatiempos";
            case TrivialCategories.Entretenimiento: return "Entretenimiento";
            case TrivialCategories.Dados: return "Dados";
            case TrivialCategories.Final: return "FinalCentro";
            default: return "Ciencias"; // Por seguridad
        }
    }
}
