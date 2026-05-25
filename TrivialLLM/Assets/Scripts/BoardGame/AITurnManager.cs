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

    private GameManager gameManager;                    // Referencia al GameManager
    private PieceMovement currentAIPiece;               // Referencia al PieceMovement de la ficha
    private DiceThrow diceController;                   // Referencia al controlador del dado

    private bool diceThrown = false;                    // Indica si se ha lanzado el dedo
    private bool isChoosingDestination = false;         // Indica si se ha seleccionado una casilla de destino
    private int currentTurnTicket = -1;                 // Almacena el ID del turno absoluto para detectar cuando cambia y reiniciar la IA

    [SerializeField] private SquareNode centralNode;    // Referencia al Nodo del centro del tablero

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
                StartCoroutine(StartAITurn());
            }

            // Elegir el destino
            // Entra cuando ya se ha tirado el dado pero no ha seleccionado una casilla a la que moverse
            if (diceThrown && gameManager.IsDiceThrown() && !gameManager.GetSelectedStatus() && !isChoosingDestination)
            {
                isChoosingDestination = true;
                StartCoroutine(ChooseDestination());
            }
        }
    }

    // Inicia el turno de la IA
    IEnumerator StartAITurn()
    {
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

    // Elige destino entre las casillas posibles 
    IEnumerator ChooseDestination()
    {
        // Esperar a que PieceMovement calcule las casillas posibles
        yield return new WaitForSeconds(1.5f);

        // Pieza de la IA actual en la escena
        currentAIPiece = FindAIPiece(gameManager.GetIndexTurn());

        if (currentAIPiece != null)
        {
            // Lista con los posibles destinos
            List<SquareNode> possibleDestinations = currentAIPiece.GetPossibleDestinations(currentAIPiece.currentSquare, gameManager.GetRemainingMoves());

            if (possibleDestinations.Count > 0)
            {
                // Logica inteligente para elegir destino
                SquareNode bestDestination = ChooseSmartDestination(currentAIPiece.currentSquare, possibleDestinations, currentAIPiece.GetComponent<TrivialPiece>());
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
    }

    // Logica inteligente para elegir destino
    private SquareNode ChooseSmartDestination(SquareNode currentSquare, List<SquareNode> options, TrivialPiece pieceStatus)
    {
        // Al principio salir del centro al radio exterior
        if (currentSquare.category == TrivialCategories.Final && currentSquare.centre == null)
        {
            return options[0];
        }

        // Si la IA ya tiene todos los quesitos, su objetivo es ir hacia el centro
        if (pieceStatus != null && pieceStatus.HaveAllWedges())
        {
            SquareNode nearestNode = null;
            float minDistance = float.MaxValue;

            // Comprobacion de seguridad
            if(centralNode == null)
            {
                Debug.LogError("Falta asignar la casilla central en AITurnManager");
                return options[0];
            }

            // Evaluar todas las casillas a las que puede ir en este turno
            foreach (var node in options)
            {
                // Si alguna opcion es la casilla final (centro), ir directamente
                if (node.category == TrivialCategories.Final)
                {
                    //Debug.Log("La IA tiene todos los quesitos y llega EXACTA al centro.");
                    return node; // Se queda con la primera casilla que le falte
                }

                // Logica para acercarse al centro si no llega en este turno
                float currentDistance = Vector3.Distance(node.transform.position, centralNode.transform.position);

                // Si esta opcion esta mas cerca que las anteriores se elige
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

        // Si le faltan quesitos
        SquareNode selectedNode = options[0]; // Por defecto, coger el primero

        // Priorizar las casillas que aun no tiene el quesito
        if (pieceStatus != null)
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

                    string topic = node.GetCategoryString();
                    if(!pieceStatus.HaveWedge(topic))
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

    // Encontrar la pieza del turno actual en la escena
    private PieceMovement FindAIPiece(int index)
    {
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
        //Debug.Log("La IA ha caido en dados. Desbloqueando nueva eleccion de destino");
    }
}
