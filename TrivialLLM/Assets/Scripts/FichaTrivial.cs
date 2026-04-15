using UnityEngine;
using UnityEngine.UI;

public class FichaTrivial : MonoBehaviour
{
    // Arrastrar aqui los quesitos desde el Inspector
    public GameObject q_verde;
    public GameObject q_azul;
    public GameObject q_amarillo;
    public GameObject q_morado;
    public GameObject q_naranja;
    public GameObject q_rosa;

    // Funcion llamada cuando un jugador gana una categoria
    public void GanarQuesito(string categoria)
    {
        switch (categoria.ToLower())
        {
            case "ciencias":
                q_verde.SetActive(true);
                break;
            case "geografia":
                q_azul.SetActive(true);
                break;
            case "historia":
                q_amarillo.SetActive(true);
                break;
            case "arte y literatura":
                q_morado.SetActive(true);
                break;
            case "deportes y pasatiempos":
                q_naranja.SetActive(true);
                break;
            case "entretenimiento":
                q_rosa.SetActive(true);
                break;

            default:
                Debug.LogWarning("Categoría no reconocida: " + categoria);
                break;
        }
    }
}
