using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DiceTrows : MonoBehaviour
{
    [SerializeField]
    Text resultado;
    [SerializeField]
    Image diceImage;
    [SerializeField]
    Sprite[] dicefaces;

    public Button botonLanzar;

    bool throwed=false;
    int startFace;
    float countToChange = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startFace = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!throwed)
        {
            if (startFace >= dicefaces.Length-1)
            {
                startFace = 0;
            }
            else
            {
                startFace++;
            }
            if (countToChange > 0.4f)
            {
                diceImage.sprite = dicefaces[startFace];
                countToChange = 0.0f;
            }
            else
            {
                countToChange += Time.deltaTime;
            }
        }
        showMovementsText();
    }
    public void releaseNumber()
    {
        if (!throwed)
        {
            throwed = true;

            // Desactivar boton en cuanto se hace clic
            if (botonLanzar != null) botonLanzar.interactable = false;

            int diceNum = Random.Range(1, 7);
            GameManager.GetInstance().setTurnMoves(diceNum);
            //resultado.text = "Puedes avanzar " + GameManager.GetInstance().getRemainingMoves() + " casilla/s";

            diceImage.sprite= dicefaces[diceNum-1];
        }
        else
        {
            throwed = false;
        }
    }
    // Funcion que fuerza la tirada automatica cuando se cae en la casilla de los dados para tirar otra vez
    public void squareThrowAgain()
    {
        throwed = false;
        releaseNumber();
    }

    // Activar el boton de lanzar una vez se haya respondido a la pregunta y pase al siguiente turno
    public void ActivarBotonLanzar()
    {
        if (botonLanzar != null) botonLanzar.interactable = true;
        throwed = false;
    }

    void showMovementsText()
    {
        resultado.text = "Puedes avanzar " + GameManager.GetInstance().getRemainingMoves() + " casilla/s";
    }
}
