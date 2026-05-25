using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Se encarga de gestionar el dado del tablero
/// </summary>
public class DiceThrow : MonoBehaviour
{
    [SerializeField]
    Text result;                 // Texto que muestra el resultado obtenido
    [SerializeField]
    Image diceImage;             // Imagen del dado que se actualiza con la cara correspondiente
    [SerializeField]
    Sprite[] diceFaces;          // Sprites de todas las caras del dado

    public Button throwButton;   // Boton para lanzar

    public GameObject throwText; // Aviso para los jugadores humanos

    bool throwed=false;          // Indica si se ha lanzado ya
    int startFace;               // Indice de la cara que se muestra durante la animacion
    float countToChange = 0.0f;  // Temporizador para cambiar la cara del dado

    void Start()
    {
        startFace = 0;

        if(GameManager.GetInstance() != null)
        {
            // Si el primer jugador es un humano, se muestra el texto de recordatorio para lanzar
            PlayerDescriptor firstPlayer = GameManager.GetInstance().GetPlayerCurrentTurn();

            if(firstPlayer != null && firstPlayer.isHuman)
            {
                throwText.SetActive(true);
            }
        }
    }

    void Update()
    {
        // Mientras el dado no haya sido lanzado, se actualiza su imagen
        if (!throwed)
        {
            // Cuando llega a la ultima cara, vuelve a la primera
            if (startFace >= diceFaces.Length-1)
            {
                startFace = 0;
            }
            else
            {
                startFace++;
            }

            // Cambiar la imagen
            if (countToChange > 0.4f)
            {
                diceImage.sprite = diceFaces[startFace];
                countToChange = 0.0f;
            }
            else
            {
                countToChange += Time.deltaTime;
            }
        }
        // Mostrar el resultado
        ShowMovementsText();
    }

    // Lanza el dado y obtiene un numero aleatorio entre 1 y 6
    public void ReleaseNumber()
    {
        if (!throwed)
        {
            throwed = true;

            // Desactivar boton en cuanto se hace clic
            if (throwButton != null) throwButton.interactable = false;

            // Ocultar recordatorio
            if(throwText != null) throwText.SetActive(false);

            // Generar numero aleatorio entre 1 y 7
            int diceNum = Random.Range(1, 7);

            // Informar al GameManager del numero de movimientos disponibles
            GameManager.GetInstance().SetTurnMoves(diceNum);

            // Mostrar la cara del dado correspondiente al resultado obtenido
            diceImage.sprite= diceFaces[diceNum-1];
        }
        else
        {
            throwed = false;
        }
    }
    // Funcion que fuerza la tirada automatica cuando se cae en la casilla de los dados para tirar otra vez
    public void SquareThrowAgain()
    {
        throwed = false;
        ReleaseNumber();
    }

    // Activa el boton de lanzar una vez se haya respondido a la pregunta y pase al siguiente turno
    public void ActiveThrowButton()
    {
        if (throwButton != null) throwButton.interactable = true;

        if(throwText != null) throwText.SetActive(true);
        throwed = false;
    }

    // Muestra el resultado obtenido
    void ShowMovementsText()
    {
        if(result != null && GameManager.GetInstance() != null)
        {
            result.text = "Puedes avanzar " + GameManager.GetInstance().GetRemainingMoves() + " casilla/s";
        }
    }

    // Lanza el dado de forma automatica
    public void ReleaseNumberAutomatic()
    {
        if (throwButton != null)
        {
            ReleaseNumber();
        }
    }
}
