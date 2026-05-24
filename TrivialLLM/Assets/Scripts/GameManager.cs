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

    [Header ("Variables de jugadores")]
    private int turn;
    private int playersCount;
    private int humansCount;
    private int LLMsCount;

    public List<PlayerDescriptor> playerDescriptor;

    [Header("Fichas")]
    public Button[] piecesButtons;
    public GameObject[] piecesPrefabs;
    private int currentSelectedPiece;       // boton seleccionado sin haberlo confirmado
    private bool[] takenPieces;          // true si ya se ha seleccionado

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
        if (startGameButton != null)
        {
            // El boton solo es interactuable cuando el num de jugadores total es mayor o igual que 2
            startGameButton.interactable = (playersCount >= 2);
        }
    }
    public void ClickOnPiece(int index)
    {
        if (takenPieces[index]) return; // si la ha seleccionado otro jugador

        currentSelectedPiece = index;
        UpdateVisualPiecesButtons();
    }

    private void UpdateVisualPiecesButtons()
    {
        for (int i = 0; i < piecesButtons.Length; i++)
        {
            if (piecesButtons[i] == null) continue;

            Image img = piecesButtons[i].GetComponent<Image>();

            if (takenPieces[i])
            {
                // Escogida por alguien, oscura y no pulsable
                piecesButtons[i].interactable = false;
                img.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gris oscuro
                piecesButtons[i].transform.localScale = Vector3.one;
            }
            else if (i == currentSelectedPiece)
            {
                piecesButtons[i].interactable = true;
                img.color = Color.white;
                piecesButtons[i].transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            }
            else
            {
                // Libre pero no seleccionada
                piecesButtons[i].interactable = true;
                img.color = Color.white;
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
       // Debug.Log("Todas las fichas asignadas correctamente.");
    }

    // metodo para validar el nombre
    private bool CheckName(out string nameResult)
    {
        nameResult = nameInput.text;

        if(string.IsNullOrEmpty(nameResult))
        {
            ShowMessage("Introduce un nombre primero");
            return false;
        }

        return true;
    }
    public void AddHuman()
    {
        if (playersCount >= 6)
        {
           // Debug.Log("No se pueden añadir más de 6 jugadores");
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            return;
        }

        currentSelectedPiece = -1;
        UpdateVisualPiecesButtons();
        gameObject.GetComponent<AudioSource>().Play();

        if (!panelHuman.activeSelf)
        {
            OpenPanelHuman();
            // panelHumano.SetActive(true);
        }
        if (!nameInput.gameObject.activeSelf)
        {
            nameInput.gameObject.SetActive(true);
        }
        if (questionModel != null)
        {
            questionModel.value = 0;
            questionModel.RefreshShownValue();
            questionModel.gameObject.SetActive(true);
        }
    }

    public void RegisterHuman()
    {
        if (playersCount >= 6)
        {
           // Debug.Log("No se pueden añadir más de 6 jugadores");
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            if (panelHuman != null)
            {
                panelHuman.SetActive(false);
            }
            return;
        }

        //  Comprobar que ha elegido una ficha
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
        if (CheckName(out string nombreValido))
        {
            playerDescriptor.Add(new PlayerDescriptor { name = nombreValido, isHuman = true, indexPiece = currentSelectedPiece, questionModel = whoAsk});
            humansCount++;
            playersCount++;
            numHumansText.text = humansCount.ToString();

            CheckStartButton();

            takenPieces[currentSelectedPiece] = true;
            currentSelectedPiece = -1;
            UpdateVisualPiecesButtons();

            gameObject.GetComponent<AudioSource>().Play();
            ShowMessage(nombreValido + " registrado correctamente");
            nameInput.text = "";
            if (panelHuman != null) panelHuman.SetActive(false);
            if (nameInput != null) nameInput.gameObject.SetActive(false);
            if (questionModel != null) questionModel.gameObject.SetActive(false);

        }
    }
    public void AddLLM()
    {
        if (playersCount >= 6)
        {
            //Debug.Log("No se pueden añadir más de 6 jugadores");
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            return;
        }

        gameObject.GetComponent<AudioSource>().Play();
        
        if (!panelLLM.activeSelf)
        {
            OpenPanelLLM();
            //panelLLM.SetActive(true);
        }
        if (!nameInput.gameObject.activeSelf)
        {
            nameInput.gameObject.SetActive(true);
        }
        if (questionModel != null)
        {
            questionModel.value = 0;
            questionModel.RefreshShownValue();
            questionModel.gameObject.SetActive(true);
        }
        if(answerModel != null)
        {
            answerModel.value = 0;
            answerModel.RefreshShownValue();
        }

    }

    public void RegisterLLM()
    {
        if (playersCount >= 6)
        {
           // Debug.Log("No se pueden añadir más de 6 jugadores");
            if (limitText != null && !limitText.gameObject.activeSelf)
            {
                limitText.gameObject.SetActive(true);
            }
            if (panelLLM != null) panelLLM.SetActive(false);
            return;
        }

        AIService.Models whoAsk = AIService.Models.Gemini; // Por defecto
        if (questionModel != null)
            whoAsk = (AIService.Models)questionModel.value;

        if (!CheckName(out string validName))
            return;

        AIService.Models whoReply = AIService.Models.Gemini; // Por defecto
        if (answerModel != null)
            whoReply = (AIService.Models)answerModel.value;

        string role = promptText.text;

        ShowLoadingMessage("Registrando jugador. Puede tardar unos segundos.");

        StartCoroutine(
            GameObject.Find("AIService")
            .GetComponent<AIService>()
            .RequestProfile(role, (profile) =>
            {
                if(profile == null)
                {
                    Debug.LogError("Perfil nulo, LLM no registrado");
                    ShowMessage("Error al registrar LLM");
                    return;
                }

                //Debug.Log("Llega");
                playerDescriptor.Add(new PlayerDescriptor
                {
                    name = validName,
                    isHuman = false,
                    answerModel = whoReply,
                    questionModel = whoAsk,
                    profile = profile,
                    indexPiece = -1
                });

                LLMsCount++;
                playersCount++;
                if (numLLMsText != null) numLLMsText.text = LLMsCount.ToString();

                CheckStartButton();

                gameObject.GetComponent<AudioSource>().Play();
                ShowMessage(validName + " registrado correctamente");
                //Debug.Log("LLM registrado correctamente: " + nombreValido);

                if (nameInput != null) nameInput.text = "";
                if (promptText != null) promptText.text = "";

                if(panelLLM != null) panelLLM.SetActive(false);
                if(nameInput != null) nameInput.gameObject.SetActive(false);
                if (questionModel != null) questionModel.gameObject.SetActive(false);
            })
        );
    }

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

    private void ShowMessage (string message)
    {
        if (confirmText != null)
        {
            StopAllCoroutines();
            confirmText.text = message;
            confirmText.gameObject.SetActive(true);
            StartCoroutine(HideText(1.5f));
        }
    }

    private void ShowLoadingMessage(string message)
    {
        if (confirmText != null)
        {
            StopAllCoroutines();
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
    //public void RemoveHuman()
    //{
    //    if (humansCount > 0)
    //    {
    //        int i = playerDescriptor.Count - 1;
    //        bool enc = false;
    //        while(!enc && i >= 0) {
    //            if (playerDescriptor[i].isHuman)
    //            {
    //                enc = true;
    //                playerDescriptor.RemoveAt(i);
    //                gameObject.GetComponent<AudioSource>().Play();
    //                humansCount--;
    //                playersCount--;
    //                numHumansText.text= humansCount.ToString();

    //                CheckStartButton();
    //            }
    //            else
    //            {
    //                i--;
    //            }
    //        }
    //    }
    //}
    //public void RemoveLLM()
    //{
    //    if (LLMsCount > 0)
    //    {
    //        int i = playerDescriptor.Count - 1;
    //        bool enc = false;
    //        while (!enc && i >= 0)
    //        {
    //            if (!playerDescriptor[i].isHuman)
    //            {
    //                enc = true;
    //                playerDescriptor.RemoveAt(i);
    //                gameObject.GetComponent<AudioSource>().Play();
    //                LLMsCount--;
    //                playersCount--;
    //                numLLMsText.text= LLMsCount.ToString();
    //                if (LLMsCount <= 0)
    //                {
    //                    panelLLM.SetActive(false);
    //                }

    //                CheckStartButton();
    //            }
    //            else
    //            {
    //                i--;
    //            }
    //        }
    //        if(!enc &&  LLMsCount > 0)
    //        {
    //            LLMsCount = 0;
    //            numLLMsText.text = LLMsCount.ToString();
    //            panelLLM.SetActive(false);
    //        }
    //    }
    //}

    public int GetHumansCount()
    {
        return humansCount;
    }

    public int GetLLMsCount()
    {
        return LLMsCount;
    }

    public PlayerDescriptor GetPlayerCurrentTurn()
    {
        if (playersCount == 0 || playerDescriptor.Count == 0) return null;
        return playerDescriptor[turn];
        //return descriptorJug[(turno - 1) % numTotalJugadores];

        //// Por seguridad, si la lista esta vacia devuelve un humano generico
        //if (descriptorJug.Count == 0)
        //{
        //    return new DescriptorJugador { esHumano = true };
        //}
        //return descriptorJug[GetTurnoIndex()];
    }

    public void NextTurn()
    {
        // Avanza turno
        turn++;

        if (turn >= playersCount)
        {
            turn = 0;
            currentRound++;
        }

        //Debug.Log("Siguiente turno: " + getJugTurnoActual().nombre);

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
        
        //FichaTrivial nextPiece = piecesList[(turno - 1) % numTotalJugadores];
        //uiController.setCurrentPiece(nextPiece);
    }

    public int GetIndexTurn()
    {
        if(playersCount == 0) return 0;
        return turn;
    }

    public int GetAbsTurn()
    {
        return turn;
    }

    public void SetTurnMoves(int moves)
    {
        turnMoves= moves;
        actMoves = moves;
        diceThrew = true;

    }
    public void WasteMovement()
    {
        //actMoves--;
        diceThrew = false;
    }

    public int GetRemainingMoves()
    {
        return actMoves;
    }

    public void ShowPosibleDestinations()
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
    public SquareNode GetSelectedDst()
    {
        return selectedNode;
    }

    public bool IsDiceThrown()
    {
        return diceThrew;
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

    public bool GetSelectedStatus()
    {
        return selectedMove;
    }

    public void SetSelectedStatus(bool status)
    {
        selectedMove = status;
    }
}
