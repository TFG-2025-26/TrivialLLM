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
            if (startFace >= dicefaces.Length)
            {
                startFace = 0;
            }
            else
            {
                startFace++;
            }
            if (countToChange > 0.2f)
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
    void showMovementsText()
    {
        resultado.text = "Puedes avanzar " + GameManager.GetInstance().getRemainingMoves() + " casilla/s";
    }
}
