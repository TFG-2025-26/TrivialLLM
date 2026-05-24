using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DiceThrow : MonoBehaviour
{
    [SerializeField]
    Text result;
    [SerializeField]
    Image diceImage;
    [SerializeField]
    Sprite[] diceFaces;

    public Button throwButton;

    public GameObject throwText;

    bool throwed=false;
    int startFace;
    float countToChange = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startFace = 0;

        if(GameManager.GetInstance() != null)
        {
            PlayerDescriptor firstPlayer = GameManager.GetInstance().GetPlayerCurrentTurn();

            if(firstPlayer != null && firstPlayer.isHuman)
            {
                throwText.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!throwed)
        {
            if (startFace >= diceFaces.Length-1)
            {
                startFace = 0;
            }
            else
            {
                startFace++;
            }
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
        ShowMovementsText();
    }
    public void ReleaseNumber()
    {
        if (!throwed)
        {
            throwed = true;

            // Desactivar boton en cuanto se hace clic
            if (throwButton != null) throwButton.interactable = false;

            if(throwText != null) throwText.SetActive(false);

            int diceNum = Random.Range(1, 7);
            GameManager.GetInstance().SetTurnMoves(diceNum);
            //resultado.text = "Puedes avanzar " + GameManager.GetInstance().getRemainingMoves() + " casilla/s";

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

    // Activar el boton de lanzar una vez se haya respondido a la pregunta y pase al siguiente turno
    public void ActiveThrowButton()
    {
        if (throwButton != null) throwButton.interactable = true;

        if(throwText != null) throwText.SetActive(true);
        throwed = false;
    }

    void ShowMovementsText()
    {
        result.text = "Puedes avanzar " + GameManager.GetInstance().GetRemainingMoves() + " casilla/s";
    }

    public void ReleaseNumberAutomatic()
    {
        if (throwButton != null)
        {
            ReleaseNumber();
        }
    }
}
