using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Header ("UI del Menu")]
    public TextMeshProUGUI textoNumHumanos;
    public TextMeshProUGUI textoNumLLMS;
    public TextMeshProUGUI textConfirmacion;
    public TextMeshProUGUI textLimite;
    public GameObject panelLLM;
    public GameObject panelHumano;
    public TMP_Dropdown model;
    public TMP_InputField promptText;
    public TMP_InputField inputNombre;

    //public UIController uiController;

    [Header ("Variables de jugadores")]
    //public int numMaxJugadores=1;
    //public int numMaxLLMS = 3;
    private int turno;
    private int numTotalJugadores;
    private int numJugHumanos;
    private int numLLMS;
   // private int numLLMSRegistrados;

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
            numJugHumanos = 0;
            numLLMS = 0;
           // numLLMSRegistrados = 0;
            numTotalJugadores = 0;
            turno = 1;
            descriptorJug = new List<DescriptorJugador>();

            if (panelLLM != null )
            {
                panelLLM.gameObject.SetActive(false);
            }
            if (panelHumano != null)
            {
                panelHumano.gameObject.SetActive(false);
            }
            if (inputNombre != null)
            {
                inputNombre.gameObject.SetActive(false);
            }
            // currentMode = GameMode.AIGame;
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

    // metodo para validar el nombre
    private bool ValidarNombre(out string resultadoNombre)
    {
        resultadoNombre = inputNombre.text;

        if(string.IsNullOrEmpty(resultadoNombre))
        {
            MostrarMensaje("Introduce un nombre primero");
            return false;
        }

        //// Maximo 27
        //if (resultadoNombre.Length > 27)
        //{
        //    resultadoNombre = resultadoNombre.Substring(0,27);  
        //    inputNombre.text = resultadoNombre; 
        //}

        return true;
    }
    public void addHuman()
    {
        if (numTotalJugadores >= 6)
        {
            Debug.Log("No se pueden añadir más de 6 jugadores");
            if (textLimite != null && !textLimite.gameObject.activeSelf)
            {
                textLimite.gameObject.SetActive(true);
            }
            return;
        }

        gameObject.GetComponent<AudioSource>().Play();

        if (!panelHumano.activeSelf)
        {
            panelHumano.SetActive(true);
        }
        if (!inputNombre.gameObject.activeSelf)
        {
            inputNombre.gameObject.SetActive(true);
        }
    }

    public void registrarHumano()
    {
        if (numTotalJugadores >= 6)
        {
            Debug.Log("No se pueden añadir más de 6 jugadores");
            if (textLimite != null && !textLimite.gameObject.activeSelf)
            {
                textLimite.gameObject.SetActive(true);
            }
            if (panelHumano != null)
            {
                panelHumano.SetActive(false);
            }
            return;
        }

        if (ValidarNombre(out string nombreValido))
        {
            descriptorJug.Add(new DescriptorJugador { nombre = nombreValido, esHumano = true });
            numJugHumanos++;
            numTotalJugadores++;
            textoNumHumanos.text = numJugHumanos.ToString();
            gameObject.GetComponent<AudioSource>().Play();
            MostrarMensaje(nombreValido + " registrado correctamente");
            inputNombre.text = "";
            if (panelHumano != null) panelHumano.SetActive(false);
            if (inputNombre != null) inputNombre.gameObject.SetActive(false);
        }
    }
    public void addLLM()
    {
        if (numTotalJugadores >= 6)
        {
            Debug.Log("No se pueden añadir más de 6 jugadores");
            if (textLimite != null && !textLimite.gameObject.activeSelf)
            {
                textLimite.gameObject.SetActive(true);
            }
            return;
        }

        gameObject.GetComponent<AudioSource>().Play();
        
        if (!panelLLM.activeSelf)
        {
            panelLLM.SetActive(true);
        }
        if(!inputNombre.gameObject.activeSelf)
        {
            inputNombre.gameObject.SetActive(true);
        }
        
    }

    public void registrarLLM()
    {
        if (numTotalJugadores >= 6)
        {
            Debug.Log("No se pueden añadir más de 6 jugadores");
            if (textLimite != null && !textLimite.gameObject.activeSelf)
            {
                textLimite.gameObject.SetActive(true);
            }
            if (panelLLM != null) panelLLM.SetActive(false);
            return;
        }

        if(ValidarNombre(out string nombreValido))
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
            descriptorJug.Add(new DescriptorJugador { nombre = nombreValido, esHumano = false, modelo = modeloSeleccionado, prompt = promptText.text });
            numLLMS++;
            numTotalJugadores++;
            textoNumLLMS.text = numLLMS.ToString();
            gameObject.GetComponent<AudioSource>().Play();
            // Debug del último jugador agregado
            DescriptorJugador ultimo = descriptorJug[descriptorJug.Count - 1];
            Debug.Log("Último LLM agregado -> Modelo: " + ultimo.modelo
                        + ", Prompt: " + ultimo.prompt
                        + ", EsHumano: " + ultimo.esHumano);

            MostrarMensaje(nombreValido + " registrado correctamente");

            inputNombre.text = "";
            if (panelLLM != null) panelLLM.SetActive(false);
            if (inputNombre != null) inputNombre.gameObject.SetActive(false);
        }
    }

    private void MostrarMensaje (string mensaje)
    {
        if (textConfirmacion != null)
        {
            StopAllCoroutines();
            textConfirmacion.text = mensaje;
            textConfirmacion.gameObject.SetActive(true);
            StartCoroutine(OcultarTexto(1.5f));
        }
    }
    // Corrutina para desactivar el texto despues de X segundos
    private IEnumerator OcultarTexto(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (textConfirmacion != null)
        {
            textConfirmacion.gameObject.SetActive(false);
        }
    }
    public void quitarHum()
    {
        if (numJugHumanos > 0)
        {
            int i = descriptorJug.Count - 1;
            bool enc = false;
            while(!enc && i >= 0) {
                if (descriptorJug[i].esHumano)
                {
                    enc = true;
                    descriptorJug.RemoveAt(i);
                    gameObject.GetComponent<AudioSource>().Play();
                    numJugHumanos--;
                    numTotalJugadores--;
                    textoNumHumanos.text= numJugHumanos.ToString();
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
                    numTotalJugadores--;
                    textoNumLLMS.text= numLLMS.ToString();
                    if (numLLMS <= 0)
                    {
                        panelLLM.SetActive(false);
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
                panelLLM.SetActive(false);
            }
        }
    }

    public int getNumHuman()
    {
        return numJugHumanos;
    }

    public int getNumLLMS()
    {
        return numLLMS;
    }

    public DescriptorJugador getJugTurnoActual()
    {
        return descriptorJug[(turno - 1) % numTotalJugadores];

        //// Por seguridad, si la lista esta vacia devuelve un humano generico
        //if (descriptorJug.Count == 0)
        //{
        //    return new DescriptorJugador { esHumano = true };
        //}
        //return descriptorJug[GetTurnoIndex()];
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
}
