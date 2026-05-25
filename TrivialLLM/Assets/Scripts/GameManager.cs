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
    public Button addHumansButton;
    public Button addLLMsButton;
    public TextMeshProUGUI numHumansText;
    public TextMeshProUGUI numLLMsText;
    public TextMeshProUGUI confirmText;
    public TextMeshProUGUI limitText;
    public GameObject panelLLM;
    public GameObject panelHuman;
    public TMP_Dropdown answerModel;
    public TMP_Dropdown questionModel;
    public TMP_InputField promptText;
    public TMP_InputField nameInput;
    public Button startGameButton;

    private bool isRegisteringPlayer = false;

    [Header ("Variables de jugadores")]
    private int turn;
    private int playersCount;
    private int humansCount;
    private int LLMsCount;

    public List<PlayerDescriptor> playerDescriptor; // Lista con toda la informacion de los jugadores

    [Header("Fichas")]
    public Button[] piecesButtons;
    public GameObject[] piecesPrefabs;
    private int currentSelectedPiece;               // Boton con la ficha seleccionada sin haberla confirmado
    private bool[] takenPieces;                     // Lista con las fichas que ya se han seleccionado

    //Manejo de movimientos
    int actMoves=0;
    int turnMoves = 0;
    bool diceThrew = false;
    public int currentRound = 1;

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
            if (this.startGameButton != null)
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
        humansCount = 0;
        LLMsCount = 0;
        playersCount = 0;
        turn = 0;
        currentRound = 1;
        playerDescriptor = new List<PlayerDescriptor>();
        currentSelectedPiece = -1;        
        takenPieces = new bool[6];

        if (panelLLM != null )
        {
            panelLLM.gameObject.SetActive(false);
        }
        if (panelHuman != null)
        {
            panelHuman.gameObject.SetActive(false);
        }
        if (nameInput != null)
        {
            nameInput.gameObject.SetActive(false);
        }
        if (questionModel != null)
        {
            questionModel.gameObject.SetActive(false);
        }
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }
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

    // Getters y setters
    public static GameManager GetInstance()
    {
        return instance;
    }

    public int GetHumansCount()
    {
        return humansCount;
    }

    public int GetLLMsCount()
    {
        return LLMsCount;
    }
    public int GetIndexTurn()
    {
        if (playersCount == 0) return 0;
        return turn;
    }

    public int GetAbsTurn()
    {
        return turn;
    }

    public void SetTurnMoves(int moves)
    {
        turnMoves = moves;
        actMoves = moves;
        diceThrew = true;

    }
    public int GetRemainingMoves()
    {
        return actMoves;
    }

    public SquareNode GetSelectedDst()
    {
        return selectedNode;
    }

    // Indica si se ha lanzado el dado
    public bool IsDiceThrown()
    {
        return diceThrew;
    }

    public bool GetSelectedStatus()
    {
        return selectedMove;
    }

    public void SetSelectedStatus(bool status)
    {
        selectedMove = status;
    }

    private void SetMenuInteractable(bool interactable)
    {
        if (addHumansButton != null) addHumansButton.interactable = interactable;
        if (addLLMsButton != null) addLLMsButton.interactable = interactable;
        if (answerModel != null) answerModel.interactable = interactable;
        if (questionModel != null) questionModel.interactable = interactable;
        if (promptText != null) promptText.interactable = interactable;
        if (nameInput != null) nameInput.interactable = interactable;
        if (startGameButton != null) startGameButton.interactable = interactable;

        if (piecesButtons != null)
        {
            foreach (Button button in piecesButtons)
            {
                if (button != null)
                    button.interactable = interactable;
            }
        }
    }

    public PlayerDescriptor GetPlayerCurrentTurn()
    {
        if (playersCount == 0 || playerDescriptor.Count == 0) return null;
        return playerDescriptor[turn];
    }

    // Gestiona la admision de un jugador humano a la partida
    public void AddHuman()
    {
        // Si se esta registrando un jugador no se puede
        if (isRegisteringPlayer) return;

        // Si ya hay 6 jugadores en la partida no se puede
        if (playersCount >= 6)
        {
            // Se muestra un mensaje con el aviso del limite
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            return;
        }

        currentSelectedPiece = -1;
        // Actualizar el aspecto visual de todos los botones de fichas
        UpdateVisualPiecesButtons();
        gameObject.GetComponent<AudioSource>().Play();

        // Se muestra el panel para registrar jugadores humanos
        if (!panelHuman.activeSelf)
        {
            OpenPanelHuman();
        }
        // Se muestra el campo para introducir el nombre
        if (!nameInput.gameObject.activeSelf)
        {
            nameInput.gameObject.SetActive(true);
        }
        // Se muestra el seleccionador para elegir modelo que pregunta
        if (questionModel != null)
        {
            questionModel.value = 0;
            questionModel.RefreshShownValue();
            questionModel.gameObject.SetActive(true);
        }
    }

    // Gestiona el resgistro del jugador humano
    public void RegisterHuman()
    {
        // Si se esta registrando un jugador no se puede
        if (isRegisteringPlayer) return;

        // Si ya hay 6 jugadores en la partida no se puede
        if (playersCount >= 6)
        {
            // Se muestra un mensaje con el aviso del limite
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            // Se oculta el panel por seguridad
            if (panelHuman != null)
            {
                panelHuman.SetActive(false);
            }
            return;
        }

        //  Comprobar que ha elegido una ficha, si no se avisa
        if (currentSelectedPiece == -1)
        {
            ShowMessage("Selecciona una ficha");
            return;
        }

        // Guardar que IA le preguntara
        AIService.Models whoAsk = AIService.Models.Gemini; // Por defecto
        if (questionModel != null)
        {
            whoAsk = (AIService.Models)questionModel.value;
        }

        // Si se comprueba que el nombre es valido, se registra el jugador
        if (CheckName(out string nombreValido))
        {
            // Almacenar la informacion de este jugador
            playerDescriptor.Add(new PlayerDescriptor { name = nombreValido, isHuman = true, indexPiece = currentSelectedPiece, questionModel = whoAsk });

            // Aumentar los contadores
            humansCount++;
            playersCount++;

            numHumansText.text = humansCount.ToString();

            CheckStartButton();

            // Gestionar las fichas que se pueden seleccionar tras haber elgido una
            takenPieces[currentSelectedPiece] = true;
            currentSelectedPiece = -1;
            // Actualizar el aspecto visual de todos los botones de fichas
            UpdateVisualPiecesButtons();

            gameObject.GetComponent<AudioSource>().Play();

            // Mostrar mensaje de resgistro correcto
            ShowMessage(nombreValido + " registrado correctamente");

            // Resetear variables
            nameInput.text = "";
            if (panelHuman != null) panelHuman.SetActive(false);
            if (nameInput != null) nameInput.gameObject.SetActive(false);
            if (questionModel != null) questionModel.gameObject.SetActive(false);
        }
    }

    // Gestiona la admision de un agente LLM a la partida
    public void AddLLM()
    {
        // Si se esta registrando un jugador no se puede
        if (isRegisteringPlayer) return;

        // Si ya hay 6 jugadores en la partida no se puede
        if (playersCount >= 6)
        {
            // Se muestra un mensaje con el aviso del limite
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            return;
        }

        gameObject.GetComponent<AudioSource>().Play();

        // Mostrar el panel para registrar agentes LLM
        if (!panelLLM.activeSelf)
        {
            OpenPanelLLM();
        }

        // Se muestra el campo para introducir el nombre
        if (!nameInput.gameObject.activeSelf)
        {
            nameInput.gameObject.SetActive(true);
        }

        // Se muestra el seleccionador para elegir modelo que pregunta
        if (questionModel != null)
        {
            questionModel.value = 0;
            questionModel.RefreshShownValue();
            questionModel.gameObject.SetActive(true);
        }
        if (answerModel != null)
        {
            answerModel.value = 0;
            answerModel.RefreshShownValue();
        }
    }

    // Gestiona el resgistro del agente LLM
    public void RegisterLLM()
    {
        // Si se esta registrando un jugador no se puede
        if (isRegisteringPlayer) return;

        // Si ya hay 6 jugadores en la partida no se puede
        if (playersCount >= 6)
        {
            // Se muestra un mensaje con el aviso del limite
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            // Se oculta el panel por seguridad
            if (panelLLM != null) panelLLM.SetActive(false);
            return;
        }

        // Almacenar el modelo que pregunta
        AIService.Models whoAsk = AIService.Models.Gemini; // Por defecto
        if (questionModel != null)
            whoAsk = (AIService.Models)questionModel.value;

        // Comprobar si el nombre introducido es valido
        if (!CheckName(out string validName))
            return;

        // Almacenar el modelo que responde
        AIService.Models whoReply = AIService.Models.Gemini; // Por defecto
        if (answerModel != null)
            whoReply = (AIService.Models)answerModel.value;

        // Almacenar el texto del prompt del rol
        string role = promptText.text;

        isRegisteringPlayer = true;
        SetMenuInteractable(false);

        // Mostrar mensaje de registrando
        ShowLoadingMessage("Registrando jugador. Puede tardar unos segundos.");

        // Registrar el perfil del jugador con el rol introducido
        StartCoroutine(
            GameObject.Find("AIService")
            .GetComponent<AIService>()
            .RequestProfile(role, (profile) =>
            {
                isRegisteringPlayer = false;
                SetMenuInteractable(true);
                UpdateVisualPiecesButtons();

                // Si ha habido algun error durante el registro se avisa
                if (profile == null)
                {
                    Debug.LogError("Perfil nulo, LLM no registrado");
                    ShowMessage("Error al registrar LLM");
                    return;
                }

                // Almacenar la informacion de este jugador
                playerDescriptor.Add(new PlayerDescriptor
                {
                    name = validName,
                    isHuman = false,
                    answerModel = whoReply,
                    questionModel = whoAsk,
                    profile = profile,
                    indexPiece = -1
                });

                // Aumentar contadores
                LLMsCount++;
                playersCount++;

                if (numLLMsText != null) numLLMsText.text = LLMsCount.ToString();

                CheckStartButton();

                gameObject.GetComponent<AudioSource>().Play();

                // Mostrar mensaje de resgistro correcto
                ShowMessage(validName + " registrado correctamente");

                // Resetear variables
                if (nameInput != null) nameInput.text = "";
                if (promptText != null) promptText.text = "";
                if (panelLLM != null) panelLLM.SetActive(false);
                if (nameInput != null) nameInput.gameObject.SetActive(false);
                if (questionModel != null) questionModel.gameObject.SetActive(false);
            })
        );
    }

    // El boton de empezar partida solo es interactuable cuando el num de jugadores total es mayor o igual que 2
    private void CheckStartButton()
    {
        if (startGameButton != null)
        {
            startGameButton.interactable = (playersCount >= 2);
        }
    }

    // Guarda la ficha seleccionada por el jugador
    public void ClickOnPiece(int index)
    {
        // Si la ficha ya ha sido escogida por otro jugador, no permite seleccionarla
        if (takenPieces[index]) return; 

        // Guardar la ficha seleccionada temporalmente
        currentSelectedPiece = index;

        // Actualizar el aspecto visual de todos los botones de fichas
        UpdateVisualPiecesButtons();
    }

    // Actualiza el aspecto visual e interactivo de los botones de seleccion de fichas
    // Las escogidas se muestran en gris y no se pueden pulsar
    // La seleccionada se resalta
    // El resto de fichas disponibles permanecen con su aspecto normal
    private void UpdateVisualPiecesButtons()
    {
        for (int i = 0; i < piecesButtons.Length; i++)
        {
            // Si algun boton no esta asignado se salta para evitar errores
            if (piecesButtons[i] == null) continue;

            // Obtener componente Image del boton
            Image img = piecesButtons[i].GetComponent<Image>();

            // Si la ficha ya ha sido escogida
            if (takenPieces[i])
            {
                // Desactivar el boton
                piecesButtons[i].interactable = false;

                // Oscurecer la imagen
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f); 

                piecesButtons[i].transform.localScale = Vector3.one;
            }
            // La ficha esta libre y es la seleccionada
            else if (i == currentSelectedPiece)
            {
                // Se mantiene interactuable
                piecesButtons[i].interactable = true;

                // Se muestra con color normal
                img.color = Color.white;

                // Se aumenta ligeramente el tamaño para resaltar la seleccion
                piecesButtons[i].transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            }
            // La ficha esta libre pero no es la seleccionada
            else
            {
                // Se mantiene interactuable
                piecesButtons[i].interactable = true;

                // Se muestra con color normal
                img.color = Color.white;

                // Se deja con su tamaño original
                piecesButtons[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Asignar fichas sobrantes a los jugadores de las IA
    public void AsignLLMPieces()
    {
        foreach (var jug in playerDescriptor)
        {
            // Si es IA y no tiene ficha
            if (!jug.isHuman && jug.indexPiece == -1)
            {
                // Dar la primera ficha libre
                for (int i = 0; i < takenPieces.Length; i++)
                {
                    if (!takenPieces[i])
                    {
                        jug.indexPiece = i;
                        takenPieces[i] = true;
                        break;
                    }
                }
            }
        }
    }

    // Metodo para validar el nombre
    private bool CheckName(out string nameResult)
    {
        nameResult = nameInput.text;

        // Si el campo esta vacio, recordar que se debe introducir un nombre
        if(string.IsNullOrEmpty(nameResult))
        {
            ShowMessage("Introduce un nombre primero");
            return false;
        }

        return true;
    }
    
    // Muestra el panel para registrar jugador humano
    public void OpenPanelHuman()
    {
        // Ocultar el panel contrario
        if (panelLLM != null) panelLLM.SetActive(false);

        // Limpiar los campos
        if (nameInput != null) nameInput.text = "";
        if (promptText != null) promptText.text = "";
        if (questionModel != null) questionModel.value = 0;
        if (answerModel != null) answerModel.value = 0;

        // Mostrar el panel seleccionado
        if (panelHuman != null) panelHuman.SetActive(true);
    }

    // Muestra el panel para registrar agente LLM
    public void OpenPanelLLM()
    {
        // Ocultar el panel contrario
        if (panelHuman != null) panelHuman.SetActive(false);

        // Limpiar los campos
        if (nameInput != null) nameInput.text = "";
        if (promptText != null) promptText.text = "";
        if (questionModel != null) questionModel.value = 0;
        if (answerModel != null) answerModel.value = 0;

        // Mostrar el panel seleccionado
        if (panelLLM != null) panelLLM.SetActive(true);
    }

    // Muestra un mensaje temporal en la interfaz
    // Para avisos breves, como errores de vadilacion o 
    // confirmaciones de registro
    private void ShowMessage (string message)
    {
        if (confirmText != null)
        {
            // Detiene cualquier corrutina anterior para evitar que otro mensaje
            // o temporizador interfiera con el nuevo
            StopAllCoroutines();

            // Asigna el mensaje recibido y activa la visibilidad
            confirmText.text = message;
            confirmText.gameObject.SetActive(true);

            // Inicia una corrutina que ocultara el mensaje despues de 1.5 segundos
            StartCoroutine(HideText(1.5f));
        }
    }

    // Muestra un mensaje de carga en la interfaz
    // No se oculta automaticamente, permanece visible hasta que otro metodo lo oculte
    private void ShowLoadingMessage(string message)
    {
        if (confirmText != null)
        {
            // Detiene cualquier corrutina anterior para evitar el texto se oculte
            // mientras todavia se esta esperando una respuesta
            StopAllCoroutines();

            // Asigna el mensaje recibido y activa la visibilidad
            confirmText.text = message;
            confirmText.gameObject.SetActive(true);
        }
    }
    // Corrutina para desactivar el texto despues de X segundos
    private IEnumerator HideText(float time)
    {
        yield return new WaitForSeconds(time);
        if (confirmText != null)
        {
            confirmText.gameObject.SetActive(false);
        }
    }
    
    // Gestiona el avance de turno
    public void NextTurn()
    {
        // Avanza turno
        turn++;

        if (turn >= playersCount)
        {
            turn = 0;
            currentRound++;
        }

        // Resetear estados del tablero y movimiento para el nuevo turno
        diceThrew = false;
        CleanDstBoard();
        selectedMove = false;

        DiceThrow dice = FindFirstObjectByType<DiceThrow>();
        // Flujo normal
        if(dice != null)
        {
            if (playerDescriptor[turn].isHuman)
            {
                // Si es humano, activar boton de dados
                dice.ActiveThrowButton();
            }
            else
            {
                if (dice.throwText != null)
                {
                    dice.throwText.SetActive(false);
                }
            }
        }
    }

    public void WasteMovement()
    {
        diceThrew = false;
    }
    public void ShowPosibleDestinations()
    {
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
    public void ReceiveSelectedNode(SquareNode node)
    {
        selectedMove = true;
        selectedNode = node;
    }
    public void RecieveSelectedTransform(Transform trf)
    {
        selectedMove = true;
        foreach (SquareNode node in posdst)
        {
            if(node.transform == trf)
            {
                selectedNode = node;
                break; 
            }
        }
    }
    public void AddToPosibleDestination(SquareNode node)
    {
        posdst.Add(node);
    }
    public void CleanDstBoard()
    {
        foreach(GameObject gobj in physPlaceToMove)
        {
            Destroy(gobj);
        }
        physPlaceToMove.Clear();
        posdst.Clear();

    }
}
