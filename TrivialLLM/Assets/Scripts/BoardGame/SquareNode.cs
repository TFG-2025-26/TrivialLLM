using UnityEngine;

/// <summary>
/// Script que sirve para informar a Unity que casillas estan conectadas entre sí
/// </summary>
public class SquareNode : MonoBehaviour
{
    [Header("Conexiones (Arrastra aqui otras casillas)")]

    // Casilla hacia el centro
    public SquareNode centre;
    // Casilla alejandose del centro
    public SquareNode outwards;
    // Siguiente casilla a la izquierda en el anillo exterior
    public SquareNode left;
    // Siguiente casilla a la derecha en el anillo exterior
    public SquareNode right;
}
