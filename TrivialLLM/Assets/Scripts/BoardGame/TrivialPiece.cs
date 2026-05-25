using UnityEngine;

/// <summary>
/// Se encarga de gestionar los quesitos de la ficha
/// Sirve para los marcadores de la UI y las fichas en el tablero
/// </summary>
public class TrivialPiece : MonoBehaviour
{
    // Arrastrar aqui los quesitos desde el Inspector
    [Header("Quesitos")]
    public GameObject green_wedge;
    public GameObject blue_wedge;
    public GameObject yellow_wedge;
    public GameObject purple_wedge;
    public GameObject orange_wedge;
    public GameObject pink_wedge;

    
    [Header("Indicador de turno")]
    public bool isUIscoreboard = false; // Indica si es marcador de UI
    public GameObject turnLight;        // Si es la ficha en el tablero, tiene asociada una luz

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (isUIscoreboard)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null ) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    // Funcion llamada cuando un jugador gana una categoria
    public void WinWedge(string category)
    {
        // Activa el questio de la categoria correspondiente
        switch (category.ToLower())
        {
            case "ciencias":
                green_wedge.SetActive(true);
                break;
            case "geografia":
                blue_wedge.SetActive(true);
                break;
            case "historia":
                yellow_wedge.SetActive(true);
                break;
            case "arte y literatura":
                purple_wedge.SetActive(true);
                break;
            case "deportes y pasatiempos":
                orange_wedge.SetActive(true);
                break;
            case "entretenimiento":
                pink_wedge.SetActive(true);
                break;

            default:
                Debug.LogWarning("Categoría no reconocida: " + category);
                break;
        }
    }

    // Metodo para apagar/encender el resaltado del turno
    public void SetActiveTurn(bool activo)
    {
        // En la interfaz 
        if (isUIscoreboard)
        {
            // Opaco en su turno, transparente si no
            if (activo)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
            else
            {
                if (canvasGroup != null) canvasGroup.alpha = 0.5f; // 50% transparente
            }
        }
        // En el tablero
        else
        {
            // Encender / apagar la luz
            if (turnLight != null)
            {
                turnLight.SetActive(activo);
            }
        }
    }

    // Comprueba si la ficha ya tiene un quesito especifico ganado
    public bool HaveWedge(string topic)
    {
        switch (topic.ToLower())
        {
            case "ciencias":
                return green_wedge != null && green_wedge.activeSelf;
            case "geografia":
                return blue_wedge != null && blue_wedge.activeSelf;
            case "historia":
                return yellow_wedge != null && yellow_wedge.activeSelf;
            case "arte y literatura":
                return purple_wedge != null && purple_wedge.activeSelf;
            case "deportes y pasatiempos":
                return orange_wedge != null && orange_wedge.activeSelf;
            case "entretenimiento":
                return pink_wedge != null && pink_wedge.activeSelf;

            default:
                Debug.LogWarning("Categoria no reconocida al comprobar quesito: " + topic);
                return false;
        }
    }

    // Comprobar si la ficha tiene los 6 quesitos conseguidos
    public bool HaveAllWedges()
    {
        bool winGreen = green_wedge != null && green_wedge.activeSelf;
        bool winBlue = blue_wedge != null && blue_wedge.activeSelf;
        bool winYellow = yellow_wedge != null && yellow_wedge.activeSelf;
        bool winPurple = purple_wedge != null && purple_wedge.activeSelf;
        bool winOrange = orange_wedge != null && orange_wedge.activeSelf;
        bool winPink = pink_wedge != null && pink_wedge.activeSelf;

        return winGreen && winBlue && winYellow && winPurple && winOrange && winPink;

    }

    // Devuelve el numero total de quesitos ganados
    public int GetCountWedges()
    {
        int count = 0;
        if (green_wedge != null && green_wedge.activeSelf) count++;
        if (blue_wedge != null && blue_wedge.activeSelf) count++;
        if (yellow_wedge != null && yellow_wedge.activeSelf) count++;
        if (purple_wedge != null && purple_wedge.activeSelf) count++;
        if (orange_wedge != null && orange_wedge.activeSelf) count++;
        if (pink_wedge != null && pink_wedge.activeSelf) count++;
        return count;
    }
}
