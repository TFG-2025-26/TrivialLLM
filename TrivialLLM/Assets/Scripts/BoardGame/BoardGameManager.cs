using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Se encarga de gestionar las fichas, el tablero  y los marcadores
/// </summary>
public class BoardGameManager : MonoBehaviour
{
    public UIController uiController;   // Referencia al controlador de la UI

    [Header("Marcadores UI")]
    public TrivialPiece[] scoreboards;  // Referencia a los marcadores de la UI

    [Header("Nodos de inicio (en la escena)")]
    [Tooltip("Arrastra los 6 nodos centrales de la escena en el mismo orden que los prefabs de las fichas")]
    public SquareNode[] outgoingNodes;  // Referncia a los nodos de salida del tablero

    void Start()
    {
        if (GameManager.GetInstance() != null)
        {
            StartGame();
        }
    }

    // Gestiona el inicio de la partida
    void StartGame()
    {
        GameManager gm = GameManager.GetInstance();
        int totalPlayers = gm.playerDescriptor.Count;

        // Almacena las piezas instanciadas
        List<TrivialPiece> instantiatedPieces = new List<TrivialPiece>();
        // Almacena los marcadores que estaran activos
        List<TrivialPiece> activeScoreboards = new List<TrivialPiece>();

        // Desactivar todos los marcadores por seguridad
        foreach (var scoreboard in scoreboards)
        {
            if (scoreboard != null) scoreboard.gameObject.SetActive(false);
        }

        // Para cada jugador
        for (int i = 0; i < totalPlayers; i++)
        {
            // Obtener sus datos
            PlayerDescriptor data = gm.playerDescriptor[i];

            // Prefab de la ficha que se le ha asignado
            GameObject piecePrefab = gm.piecesPrefabs[data.indexPiece];

            // Obtener nodo real de la escena basandose en el indice de la ficha
            SquareNode startNode = outgoingNodes[data.indexPiece];

            if (startNode != null)
            {
                // Posicion de salida
                Vector3 iniPos = startNode.transform.position;
                iniPos.y = piecePrefab.transform.position.y;

                // Instanciar la ficha en la casilla de salida
                Quaternion iniRot = Quaternion.Euler(90f, 0f, 0f);
                GameObject newPiece = Instantiate(piecePrefab, iniPos, iniRot);
                newPiece.name = "Ficha_" + data.name;

                // Inyectar a la ficha instanciada el nodo real
                PieceMovement pmInstance = newPiece.GetComponent<PieceMovement>();
                if(pmInstance != null)
                {
                    pmInstance.currentSquare = startNode;
                    pmInstance.indexTurn = i;
                }

                // Guardar la referencia para el UIController
                TrivialPiece trivialPiece = newPiece.GetComponent<TrivialPiece>();
                if (trivialPiece != null)
                {
                    instantiatedPieces.Add(trivialPiece);
                }
            }
            else
            {
                Debug.LogError($"Falta asignar el nodo de salida en el indice {data.indexPiece} del BoardGameManager.");
            }

            // Marcadores de la interfaz
            if (data.indexPiece >= 0 && data.indexPiece < scoreboards.Length)
            {
                // Buscar marcador que corresponde a esta ficha
                TrivialPiece scoreboardUI = scoreboards[data.indexPiece];

                if (scoreboardUI != null)
                {
                    // Activar en la jerarquia
                    scoreboardUI.gameObject.SetActive(true);

                    // Añadir a la lista ordanada para el UIController
                    activeScoreboards.Add(scoreboardUI);

                    // Añadir nombre del jugador
                    TextMeshProUGUI textName = scoreboardUI.GetComponentInChildren<TextMeshProUGUI>();
                    if ( textName != null )
                    {
                        textName.text = data.name;
                    }
                }
            }
        }

        // Pasar las fichas instanciadas y los marcadores activos ordenados al UIController
        if (uiController != null)
        {
            uiController.boardgamePieces = instantiatedPieces.ToArray();
            uiController.scoreboardPieces = activeScoreboards.ToArray();

            // Resaltar primer turno al empezar
            uiController.UpdateTurnSigns();
        }
    }
}
