using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public TextMeshProUGUI textoNumHumanos;
    public TextMeshProUGUI textoNumLLMS;

    public TextMeshProUGUI textConfirmacion;

    public GameObject panel;
    public TMP_Dropdown model;
    public TMP_InputField promptText;

    //public UIController uiController;

    public int numMaxJugadores=1;
    public int numMaxLLMS = 3;
    private int numTotalJugadores;
    private int turno;

    private int numJug;
    private int numLLMS;
    private int numLLMSRegistrados;

    public List<DescriptorJugador> descriptorJug;

    // Modos de juego posible
    public enum GameMode { HumanGame, AIGame }

    // Modo de juego actual
    public GameMode currentMode;

    //Manejo de movimientos
    int actMoves=0;
    int turnMoves = 0;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            numJug = 0;
            numLLMS = 0;
            numLLMSRegistrados = 0;
            numTotalJugadores = 0;
            turno = 1;
            descriptorJug = new List<DescriptorJugador>();

            if (panel != null )
            {
                panel.gameObject.SetActive(false);
            }
            // currentMode = GameMode.AIGame;

            // Pruebas en la escena del tablero
            // Le forzamos 1 jugador humano automáticamente para que no dé error
            //descriptorJug.Add(new DescriptorJugador { esHumano = true });
           // numJug = 1;
            //numTotalJugadores = 1;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    // Establece el modo de juego desde los botones del menu principal
    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
        Debug.Log("Modo de juego actual: " + currentMode);
    }

    // Metodos para los botones de StartScene
    public void PlayHumanMode()
    {
        SetGameMode(GameMode.HumanGame);
        SceneManager.LoadScene("HumanGameScene");
    }

    public void PlayAIMode()
    {
        SetGameMode(GameMode.AIGame);
        SceneManager.LoadScene("IAGameScene");
    }

    public void addHuman()
    {
        if (numJug < numMaxJugadores)
        {
            descriptorJug.Add(new DescriptorJugador { esHumano = true });
            gameObject.GetComponent<AudioSource>().Play();
            numJug++;
            numTotalJugadores++;
            textoNumHumanos.text = numJug.ToString();
        }
    }
    public void addLLM()
    {
        if (numLLMS < numMaxLLMS && numLLMS <= numLLMSRegistrados)
        {
            //descriptorJug.Add(new DescriptorJugador { esHumano = false, modelo=model, prompt=promptDes });
            numLLMS++;
            gameObject.GetComponent<AudioSource>().Play();
            numTotalJugadores++;
            textoNumLLMS.text = numLLMS.ToString();
            if(!panel.activeSelf && numLLMS > 0)
            {
                panel.SetActive(true);
            }
          
        }
    }

    public void registrarLLM()
    {
        if (numLLMS < numMaxLLMS)
        {
            string modelo = model.options[model.value].text;
            AIService.Models modeloSeleccionado;
            switch (modelo)
            {
                case "Gemini":
                    modeloSeleccionado = AIService.Models.Gemini; 
                    break;
                case "Copilot":
                    modeloSeleccionado = AIService.Models.Copilot;
                    break;
                case "ChatGPT":
                    modeloSeleccionado = AIService.Models.ChatGPT;
                    break;
                case "Azure":
                    modeloSeleccionado = AIService.Models.Azure;
                    break;
                default:
                    modeloSeleccionado = AIService.Models.Copilot;
                    break;
                    
            }
            descriptorJug.Add(new DescriptorJugador { esHumano = false, modelo =modeloSeleccionado, prompt=promptText.text });
            // Debug del último jugador agregado
            DescriptorJugador ultimo = descriptorJug[descriptorJug.Count - 1];
            Debug.Log("Último LLM agregado -> Modelo: " + ultimo.modelo
                      + ", Prompt: " + ultimo.prompt
                      + ", EsHumano: " + ultimo.esHumano);

            numLLMSRegistrados++;

            gameObject.GetComponent<AudioSource>().Play();

            if (textConfirmacion != null)
            {
                StopAllCoroutines();
                textConfirmacion.text = ultimo.modelo.ToString() + " registrado correctamente";
                textConfirmacion.gameObject.SetActive(true);
                StartCoroutine(OcultarTexto(1.5f));
            }
        }
    }

    public void quitarHum()
    {
        if (numJug > 0)
        {
            int i = descriptorJug.Count - 1;
            bool enc = false;
            while(!enc && i >= 0) {
                if (descriptorJug[i].esHumano)
                {
                    enc = true;
                    descriptorJug.RemoveAt(i);
                    gameObject.GetComponent<AudioSource>().Play();
                    numJug--;
                    numTotalJugadores--;
                    textoNumHumanos.text= numJug.ToString();
                }
                else
                {
                    i--;
                }
            }
        }
    }
    public void quitarLLM()
    {
        if (numLLMS > 0)
        {
            int i = descriptorJug.Count - 1;
            bool enc = false;
            while (!enc && i >= 0)
            {
                if (!descriptorJug[i].esHumano)
                {
                    enc = true;
                    descriptorJug.RemoveAt(i);
                    gameObject.GetComponent<AudioSource>().Play();
                    numLLMS--;
                    if(numLLMSRegistrados>0)
                        numLLMSRegistrados--;
                    numTotalJugadores--;
                    textoNumLLMS.text= numLLMS.ToString();
                    if (numLLMS <= 0)
                    {
                        panel.SetActive(false);
                    }
                }
                else
                {
                    i--;
                }
            }
            if(!enc &&  numLLMS > 0)
            {
                numLLMS = 0;
                textoNumLLMS.text = numLLMS.ToString();
                panel.SetActive(false);
            }
        }
    }

    public int getNumHuman()
    {
        return numJug;
    }

    public int getNumLLMS()
    {
        return numLLMS;
    }

    public DescriptorJugador getJugTurnoActual()
    {
        return descriptorJug[(turno - 1) % numTotalJugadores];
    }

    public void sigTurno()
    {
        turno++;

        //FichaTrivial nextPiece = piecesList[(turno - 1) % numTotalJugadores];
        //uiController.setCurrentPiece(nextPiece);
    }

    public int GetTurnoIndex()
    {
        if(numTotalJugadores == 0) return 0;
        return (turno - 1) % numTotalJugadores;
    }
    
    public void setTurnMoves(int moves)
    {
        turnMoves= moves;
        actMoves = moves;

    }
    public void wasteMovement()
    {
        actMoves--;
    }

    public int getRemainingMoves()
    {
        return actMoves;
    }
    
    // Corrutina para desactivar el texto despues de X segundos
    private IEnumerator OcultarTexto(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if(textConfirmacion != null)
        {
            textConfirmacion.gameObject.SetActive(false);
        }
    }


}
