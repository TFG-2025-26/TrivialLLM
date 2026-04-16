using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public TMP_Dropdown modelRespuesta;
    public TMP_Dropdown modelPregunta;
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

    [Header("Fichas")]
    public Button[] botonesFichas;
    public GameObject[] prefabsFichas;
    private int fichaSeleccionadaActual;       // boton seleccionado sin haberlo confirmado
    private bool[] fichasOcupadas;  // true si ya se ha seleccionado

    // Modos de juego posible
   // public enum GameMode { HumanGame, AIGame }

    // Modo de juego actual
   // public GameMode currentMode;

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
            fichaSeleccionadaActual = -1;        
            fichasOcupadas = new bool[6];

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
            if (modelPregunta != null)
            {
                modelPregunta.gameObject.SetActive(false);
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
    //public void SetGameMode(GameMode mode)
    //{
    //    currentMode = mode;
    //    Debug.Log("Modo de juego actual: " + currentMode);
    //}

    // Metodos para los botones de StartScene
    //public void PlayHumanMode()
    //{
    //    SetGameMode(GameMode.HumanGame);
    //    SceneManager.LoadScene("HumanGameScene");
    //}

    //public void PlayAIMode()
    //{
    //    SetGameMode(GameMode.AIGame);
    //    SceneManager.LoadScene("IAGameScene");
    //}

    // Metodo que se asigna a los botones de las fichas en la UI
    // 0 al 5 (6 botones(
    public void ClickEnFicha(int index)
    {
        if (fichasOcupadas[index]) return; // si la ha seleccionado otro jugador

        fichaSeleccionadaActual = index;
        ActualizarVisualBotonesFichas();
    }

    private void ActualizarVisualBotonesFichas()
    {
        for (int i = 0; i < botonesFichas.Length; i++)
        {
            if (botonesFichas[i] == null) continue;

            Image img = botonesFichas[i].GetComponent<Image>();

            if (fichasOcupadas[i])
            {
                // Escogida por alguien, oscura y no pulsable
                botonesFichas[i].interactable = false;
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gris oscuro
                botonesFichas[i].transform.localScale = Vector3.one;
            }
            else if (i == fichaSeleccionadaActual)
            {
                botonesFichas[i].interactable = true;
                img.color = Color.white;
                botonesFichas[i].transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            }
            else
            {
                // Libre pero no seleccionada
                botonesFichas[i].interactable = true;
                img.color = Color.white;
                botonesFichas[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Asignar fichas sobrantes a los jugadores de las IA
    public void AsignarFichasLLM()
    {
        foreach (var jug in descriptorJug)
        {
            // Si es IA y no tiene ficha
            if (!jug.esHumano && jug.fichaIndex == -1)
            {
                // Dar la primera ficha libre
                for (int i = 0; i < fichasOcupadas.Length; i++)
                {
                    if (!fichasOcupadas[i])
                    {
                        jug.fichaIndex = i;
                        fichasOcupadas[i] = true;
                        break;
                    }
                }
            }
        }
        Debug.Log("Todas las fichas asignadas correctamente.");
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

        fichaSeleccionadaActual = -1;
        ActualizarVisualBotonesFichas();
        gameObject.GetComponent<AudioSource>().Play();

        if (!panelHumano.activeSelf)
        {
            panelHumano.SetActive(true);
        }
        if (!inputNombre.gameObject.activeSelf)
        {
            inputNombre.gameObject.SetActive(true);
        }
        if (modelPregunta != null)
        {
            modelPregunta.value = 0;
            modelPregunta.RefreshShownValue();
            modelPregunta.gameObject.SetActive(true);
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

        //  Comprobar que ha elegido una ficha
        if (fichaSeleccionadaActual == -1)
        {
            MostrarMensaje("Selecciona una ficha");
            return;
        }

        // Guardar que IA le preguntara
        AIService.Models quienPregunta = AIService.Models.Gemini; // Por defecto
        if (modelPregunta != null)
        {
            quienPregunta = (AIService.Models)modelPregunta.value;
        }
        if (ValidarNombre(out string nombreValido))
        {
            descriptorJug.Add(new DescriptorJugador { nombre = nombreValido, esHumano = true, fichaIndex = fichaSeleccionadaActual, modeloPreguntas = quienPregunta});
            numJugHumanos++;
            numTotalJugadores++;
            textoNumHumanos.text = numJugHumanos.ToString();

            fichasOcupadas[fichaSeleccionadaActual] = true;
            fichaSeleccionadaActual = -1;
            ActualizarVisualBotonesFichas();

            gameObject.GetComponent<AudioSource>().Play();
            MostrarMensaje(nombreValido + " registrado correctamente");
            inputNombre.text = "";
            if (panelHumano != null) panelHumano.SetActive(false);
            if (inputNombre != null) inputNombre.gameObject.SetActive(false);
            if (modelPregunta != null) modelPregunta.gameObject.SetActive(false);

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
        if (modelPregunta != null)
        {
            modelPregunta.value = 0;
            modelPregunta.RefreshShownValue();
            modelPregunta.gameObject.SetActive(true);
        }
        if(modelRespuesta != null)
        {
            modelRespuesta.value = 0;
            modelRespuesta.RefreshShownValue();
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

        // Guardar que IA le preguntara
        AIService.Models quienPregunta = AIService.Models.Gemini; // Por defecto
        if (modelPregunta != null)
        {
            quienPregunta = (AIService.Models)modelPregunta.value;
        }

        if (ValidarNombre(out string nombreValido))
        {
            //string modelo = modelRespuesta.options[modelRespuesta.value].text;
            //AIService.Models modeloSeleccionado;
            //switch (modelo)
            //{
            //    case "Gemini":
            //        modeloSeleccionado = AIService.Models.Gemini;
            //        break;
            //    case "Copilot":
            //        modeloSeleccionado = AIService.Models.Copilot;
            //        break;
            //    case "ChatGPT":
            //        modeloSeleccionado = AIService.Models.ChatGPT;
            //        break;
            //    case "Azure":
            //        modeloSeleccionado = AIService.Models.Azure;
            //        break;
            //    default:
            //        modeloSeleccionado = AIService.Models.Copilot;
            //        break;

            //}
            AIService.Models quienResponde = AIService.Models.Gemini; // Por defecto
            if (modelRespuesta != null)
            {
                quienResponde = (AIService.Models)modelRespuesta.value;
            }
            descriptorJug.Add(new DescriptorJugador { nombre = nombreValido, esHumano = false, modelo = quienResponde, prompt = promptText.text, fichaIndex = -1, modeloPreguntas = quienPregunta });
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
            if (modelPregunta != null) modelPregunta.gameObject.SetActive(false);
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
        Debug.Log("Siguiente turno: " + getJugTurnoActual().nombre);
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
