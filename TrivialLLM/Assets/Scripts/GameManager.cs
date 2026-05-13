using System;
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
    public Button buttonStartGame;

    //public UIController uiController;

    [Header ("Variables de jugadores")]
    //public int numMaxJugadores=1;
    //public int numMaxLLMS = 3;
    private int turno;
    private int numTotalJugadores;
    private int numJugHumanos;
    private int numLLMS;

    public int rondaActual = 1;

    public List<DescriptorJugador> descriptorJug;

    [Header("Fichas")]
    public Button[] botonesFichas;
    public GameObject[] prefabsFichas;
    private int fichaSeleccionadaActual;       // boton seleccionado sin haberlo confirmado
    private bool[] fichasOcupadas;  // true si ya se ha seleccionado


    //Manejo de movimientos
    int actMoves=0;
    int turnMoves = 0;
    bool diceThrew = false;

    List<SquareNode> posdst =new List<SquareNode>();
    [SerializeField]
    GameObject placeToMove;

    List<GameObject> physPlaceToMove =new List<GameObject>();
    SquareNode selectedNode = null;
    bool selectedMove = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Si el gamemanager nuevo tiene asignado el boton de inicio, estamos en el menu de configuracion
            if (this.buttonStartGame != null)
            {
                // Se acaba de terminar una partida
                // Destruir el gamemanger viejo para limpiar toda la informacion anterior
                Destroy(instance.gameObject);

                // Asignar el nuevo gamemanager
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // En otra escena hay un gamemanager duplicado por error, se destruye
                Destroy(gameObject);
                return;
            }
        }
        else if (instance == null)
        {
            // Es la primera vez que se abre el juego
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Inicializacion de variables y limpieza de nueva partida
        numJugHumanos = 0;
        numLLMS = 0;
        numTotalJugadores = 0;
        turno = 0;
        rondaActual = 1;
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
        if (buttonStartGame != null)
        {
            buttonStartGame.interactable = false;
        }
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    private void CheckStartButton()
    {
        if (buttonStartGame != null)
        {
            // El boton solo es interactuable cuando el num de jugadores total es mayor o igual que 2
            buttonStartGame.interactable = (numTotalJugadores >= 2);
        }
    }
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
            AbrirPanelHumano();
            // panelHumano.SetActive(true);
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

            CheckStartButton();

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
            AbrirPanelLLM();
            //panelLLM.SetActive(true);
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

        AIService.Models quienPregunta = AIService.Models.Gemini; // Por defecto
        if (modelPregunta != null)
            quienPregunta = (AIService.Models)modelPregunta.value;

        if (!ValidarNombre(out string nombreValido))
            return;

        AIService.Models quienResponde = AIService.Models.Gemini; // Por defecto
        if (modelRespuesta != null)
            quienResponde = (AIService.Models)modelRespuesta.value;

        string rol = promptText.text;

        ShowLoadingMessage("Registrando jugador. Puede tardar unos segundos.");

        StartCoroutine(
            GameObject.Find("AIService")
            .GetComponent<AIService>()
            .PedirPerfil(rol, (perfil) =>
            {
                if(perfil == null)
                {
                    Debug.LogError("Perfil nulo, LLM no registrado");
                    MostrarMensaje("Error al registrar LLM");
                    return;
                }

                Debug.Log("Llega");
                descriptorJug.Add(new DescriptorJugador
                {
                    nombre = nombreValido,
                    esHumano = false,
                    modelo = quienResponde,
                    modeloPreguntas = quienPregunta,
                    perfil = perfil,
                    fichaIndex = -1
                });

                numLLMS++;
                numTotalJugadores++;
                if (textoNumLLMS != null) textoNumLLMS.text = numLLMS.ToString();

                CheckStartButton();

                gameObject.GetComponent<AudioSource>().Play();
                MostrarMensaje(nombreValido + " registrado correctamente");
                //Debug.Log("LLM registrado correctamente: " + nombreValido);

                if (inputNombre != null) inputNombre.text = "";
                if (promptText != null) promptText.text = "";

                if(panelLLM != null) panelLLM.SetActive(false);
                if(inputNombre != null) inputNombre.gameObject.SetActive(false);
                if (modelPregunta != null) modelPregunta.gameObject.SetActive(false);

            
            })
        );
    }

    public void AbrirPanelHumano()
    {
        // Ocultar el panel contrario
        if (panelLLM != null) panelLLM.SetActive(false);

        // Limpiar los campos
        if (inputNombre != null) inputNombre.text = "";
        if (promptText != null) promptText.text = "";
        if (modelPregunta != null) modelPregunta.value = 0;
        if (modelRespuesta != null) modelRespuesta.value = 0;

        // Mostrar el panel seleccionado
        if (panelHumano != null) panelHumano.SetActive(true);
    }

    public void AbrirPanelLLM()
    {
        // Ocultar el panel contrario
        if (panelHumano != null) panelHumano.SetActive(false);

        // Limpiar los campos
        if (inputNombre != null) inputNombre.text = "";
        if (promptText != null) promptText.text = "";
        if (modelPregunta != null) modelPregunta.value = 0;
        if (modelRespuesta != null) modelRespuesta.value = 0;

        // Mostrar el panel seleccionado
        if (panelLLM != null) panelLLM.SetActive(true);
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

    private void ShowLoadingMessage(string mensaje)
    {
        if (textConfirmacion != null)
        {
            StopAllCoroutines();
            textConfirmacion.text = mensaje;
            textConfirmacion.gameObject.SetActive(true);
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

                    CheckStartButton();
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

                    CheckStartButton();
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
        if (numTotalJugadores == 0 || descriptorJug.Count == 0) return null;
        return descriptorJug[turno];
        //return descriptorJug[(turno - 1) % numTotalJugadores];

        //// Por seguridad, si la lista esta vacia devuelve un humano generico
        //if (descriptorJug.Count == 0)
        //{
        //    return new DescriptorJugador { esHumano = true };
        //}
        //return descriptorJug[GetTurnoIndex()];
    }

    public void sigTurno()
    {
        // Avanza turno
        turno++;

        if (turno >= numTotalJugadores)
        {
            turno = 0;
            rondaActual++;
        }

        Debug.Log("Siguiente turno: " + getJugTurnoActual().nombre);

        // Resetear estados del tablero y movimiento para el nuevo turno
        diceThrew = false;
        cleanDstBoard();
        selectedMove = false;

        DiceThrow dice = FindFirstObjectByType<DiceThrow>();
        // Flujo normal
        if(dice != null)
        {
            if (descriptorJug[turno].esHumano)
            {
                // Si es humano, activar boton de dados
                dice.ActivarBotonLanzar();
            }
            else
            {
                if (dice.textThrow != null)
                {
                    dice.textThrow.SetActive(false);
                }
            }
        }
        
        //FichaTrivial nextPiece = piecesList[(turno - 1) % numTotalJugadores];
        //uiController.setCurrentPiece(nextPiece);
    }

    public int GetTurnoIndex()
    {
        if(numTotalJugadores == 0) return 0;
        return turno;
    }

    public int GetTurnoAbsoluto()
    {
        return turno;
    }

    public void setTurnMoves(int moves)
    {
        turnMoves= moves;
        actMoves = moves;
        diceThrew = true;

    }
    public void wasteMovement()
    {
        //actMoves--;
        diceThrew = false;
    }

    public int getRemainingMoves()
    {
        return actMoves;
    }

    public void showPosibleDestinations()
    {
        //Debug.Log(posdst.Count);
        foreach(SquareNode nod in posdst)
        {
            GameObject resp= Instantiate(placeToMove, nod.transform.position,Quaternion.Euler(90,0,0));
            resp.transform.position += new Vector3(0, 0.1f, 0);
            
            SelectSender sender = resp.GetComponent<SelectSender>();
            if(sender != null)
            {
                sender.nodoDestino = nod;
            }
            physPlaceToMove.Add(resp);
        }
        
    }

    public void receiveSelectedNode(SquareNode nod)
    {
        selectedMove = true;
        selectedNode = nod;
    }
    public void recieveSelectedTransform(Transform trf)
    {
        selectedMove = true;
        Debug.Log("HOLAAAAA");
        foreach (SquareNode nod in posdst)
        {
            if(nod.transform == trf)
            {
                selectedNode = nod;
                break; 
            }
        }
    }
    public SquareNode GetSelectedDst()
    {
        return selectedNode;
    }

    public bool IsDiceThrown()
    {
        return diceThrew;
    }

    public void addToPosibleDestination(SquareNode nod)
    {
        posdst.Add(nod);
    }
    

    public void cleanDstBoard()
    {
        foreach(GameObject gobj in physPlaceToMove)
        {
            Destroy(gobj);
        }
        physPlaceToMove.Clear();
        posdst.Clear();

    }

    public bool getSelectedStatus()
    {
        return selectedMove;
    }

    public void setSelectedStatus(bool status)
    {
        selectedMove = status;
    }
}
