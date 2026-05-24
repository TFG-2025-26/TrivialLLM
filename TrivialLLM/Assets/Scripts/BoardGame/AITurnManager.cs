using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Se encarga de gestionar el turno completo de la IA 
/// para automatizar las acciones
/// </summary>
public class AITurnManager : MonoBehaviour
{
    private GameManager gameManager;
    private PieceMovement currentAIPiece;
    private DiceThrow diceController;

    // private bool isAITurnActive = false;
    private bool diceThrown = false;
    private bool isChoosingDestination = false;
    private int currentTurnTicket = -1;

    [SerializeField] private SquareNode centralNode;

    void Start()
    {
        gameManager = GameManager.GetInstance();
        diceController = FindFirstObjectByType<DiceThrow>();
    }

    void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        int currentTurnIndex = gameManager.GetAbsTurn();
        PlayerDescriptor currentPlayer = gameManager.GetPlayerCurrentTurn();

        // Si el turno cambia en el GameManager, resetear todos los bloqueos de la IA
        if (currentTurnTicket != currentTurnIndex)
        {
            currentTurnTicket = currentTurnIndex;
            diceThrown = false;
            isChoosingDestination = false;
        }

        // Si es el turno de la IA
        if (!currentPlayer.isHuman)
        {
            // Lanzar dado al principio del turno
            // Solo entra si no ha empezado el turno y el dado no se ha lanzado
            if(!diceThrown && !gameManager.IsDiceThrown())
            {
                diceThrown = true;
                StartCoroutine(StartAITurn(currentPlayer));
            }

            // Elegir el destino
            // Entra cuando ya se ha tirado el dado pero no ha seleccionado una casilla a la que moverse
            if (diceThrown && gameManager.IsDiceThrown() && !gameManager.GetSelectedStatus() && !isChoosingDestination)
            {
                isChoosingDestination = true;
                StartCoroutine(ChooseDestination(currentPlayer));
            }
        }
    }

    IEnumerator StartAITurn(PlayerDescriptor jugIA)
    {
        //Debug.Log($"Iniciando turno de IA: {jugIA.nombre}");

        // Bloquear boton del dado
        if (diceController != null && diceController.throwButton != null)
        {
            diceController.throwButton.interactable = false;
        }
        // Esperar unos segundos para dar fluidez visual al cambio de turno
        yield return new WaitForSeconds(1.5f);

        // Lanzar el dado automaticamente
        if (diceController != null)
        {
            diceController.ReleaseNumberAutomatic();
        }
    }

    IEnumerator ChooseDestination(PlayerDescriptor jugIA)
    {
        // Esperar a que PieceMovement calcule las casillas posibles
        yield return new WaitForSeconds(1.5f);

        // Pieza de la IA actual en la escena
        currentAIPiece = FindAIPiece(gameManager.GetIndexTurn());

        if (currentAIPiece != null)
        {
            List<SquareNode> possibleDestinations = currentAIPiece.GetPossibleDestinations(currentAIPiece.actualSquare, gameManager.GetRemainingMoves());

            if (possibleDestinations.Count > 0)
            {
                // Logica inteligente para elegir destino
                SquareNode bestDestination = ChooseSmartDestination(currentAIPiece.actualSquare, possibleDestinations, currentAIPiece.GetComponent<TrivialPiece>());
                gameManager.ReceiveSelectedNode(bestDestination);
            }
            else
            {
                Debug.LogWarning("La IA no tiene movimientos posibles");
                gameManager.WasteMovement();
                gameManager.SetSelectedStatus(false);
                gameManager.CleanDstBoard();
                gameManager.NextTurn();
            }
        }

       // isChoosingDestination = false;
    }
    private SquareNode ChooseSmartDestination(SquareNode currentSquare, List<SquareNode> options, TrivialPiece fichaStatus)
    {
        // Al principio salir del centro al radio exterior
        if (currentSquare.category == TrivialCategories.Final && currentSquare.centre == null)
        {
            return options[0];
        }

        // Si la IA ya tiene todos los quesitos, su objetivo es ir hacia el centro
        if (fichaStatus != null && fichaStatus.HaveAllWedges())
        {
            SquareNode nearestNode = null;
            float minDistance = float.MaxValue;

            // Comprobacion de seguridad
            if(centralNode == null)
            {
                Debug.LogError("Falta asignar la casilla centran en AITurnManager");
                return options[0];
            }

            // Evaluar todas las casillas a las que puede ir en este turno
            foreach (var node in options)
            {
                // Si alguna opcion es la casilla final (centro), ir directamente
                if (node.category == TrivialCategories.Final)
                {
                    Debug.Log("La IA tiene todos los quesitos y llega EXACTA al centro.");
                    return node; // Se queda con la primera casilla que le falte
                }

                // Logica para acercarse al centro si no llega en este turno
                float currentDistance = Vector3.Distance(node.transform.position, centralNode.transform.position);

                // Si esta opcion esta mas cerca que las anteriores
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    nearestNode = node;
                }
                
            }

            // Una vez evaluadas todas las opciones, devolver la mas cercana
            if (nearestNode != null)
            {
                return nearestNode; 
            }
        }

        SquareNode selectedNode = options[0]; // Por defecto, coger el primero

        // Priorizar las casillas las que aun no se tiene el quesito
        if (fichaStatus != null)
        {
            // Lista para guardar destinos validos (que no sean dados)
            List<SquareNode> validDestinations = new List<SquareNode>();
            SquareNode squareDice = null;   

            // Buscar quesitos que falten o casilla de dados
            foreach (var node in options)
            {
                if (node.category == TrivialCategories.Dados)
                {
                    squareDice = node;
                    continue;
                }

                // Si no es centro ni dados, comprobar si le falta el quesito
                if (node.category != TrivialCategories.Final && node.category != TrivialCategories.Dados)
                {
                    validDestinations.Add(node);

                    string topic = node.getTopicString();
                    if(!fichaStatus.HaveWedge(topic))
                    {
                        // Prioridad si no tiene el quesito
                        return node;
                    }
                    
                }
            }

            // Si llega aqui es porque ya tiene los quesitos de todos los destinos posibles
            // o todos los destinos son dados

            // Si hay casilla de dados disponible, la escoge
            if (squareDice != null)
            {
                return squareDice;
            }
            // Escoger uno al azar para otorgar variedad
            if (validDestinations.Count > 0)
            {
                selectedNode = validDestinations[UnityEngine.Random.Range(0, validDestinations.Count)];
            }
        }
        return selectedNode;
    }

    private PieceMovement FindAIPiece(int index)
    {
        // Encontrar la pieza del turno actual en la escena
        PieceMovement[] allPieces = FindObjectsByType<PieceMovement>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece.indexTurn == index) return piece;
        }
        return null;
    }

    // Permite un nuevo movimiento dentro del mismo turno si ha caido en dados y "tira otra vez"
    public void AllowNewMovement()
    {
        isChoosingDestination = false;
        Debug.Log("La IA ha caido en dados. Desbloqueando nueva eleccion de destino");
    }
}
