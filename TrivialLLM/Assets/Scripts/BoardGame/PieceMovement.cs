using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Se encarga del movimiento de las fichas en el tablero
/// </summary>
public class PathInfo
{
    public SquareNode destination;
    public List<SquareNode> path;
}
public class PieceMovement : MonoBehaviour
{
    public SquareNode currentSquare;    // Casilla actual donde se encuentra la ficha
    [SerializeField]
    private AIService aiService;        // Referencia a AIService

    [SerializeField]    
    private float speed = 30f;          // Velocidad del movimiento

    public int indexTurn;               // Indice del turno
    private bool edgeReached = false;   // Indica si ha alcanzdo la circunferencia exterior del tablero
    private bool isMoving = false;      // Indica si se esta moviendo

    private bool dstShown = false;
    List<SquareNode> posibilities = new List<SquareNode>();
    List<PathInfo> posiblePaths= new List<PathInfo>();

    TrivialPiece trivialPiece;          // Referencia a la pieza

    void Start()
    {
        // Colocar la ficha exactamente en la casilla inicial al darle a Play
        if(currentSquare != null)
        {
            Vector3 iniPos = new Vector3(currentSquare.transform.position.x, transform.position.y, currentSquare.transform.position.z);
            transform.position = iniPos;
        }

        trivialPiece = GetComponent<TrivialPiece>();
        aiService = FindFirstObjectByType<AIService>();
    }
    void Update()
    {
        // Si no es el turno de esta ficha, se ignora todo lo demas
        if (GameManager.GetInstance().GetIndexTurn() != indexTurn) return;
        if(isMoving) {return; }

        if(!dstShown&&GameManager.GetInstance().IsDiceThrown()) {
            int movesLeft = GameManager.GetInstance().GetRemainingMoves();
            GetPossibleDestinations(currentSquare, movesLeft);
            
            SaveDestinations(posibilities);
            GameManager.GetInstance().ShowPosibleDestinations();
            dstShown = true;
        }

        if (GameManager.GetInstance().GetSelectedStatus() && !isMoving)
        {
            isMoving = true;
            SquareNode target = GameManager.GetInstance().GetSelectedDst();
            
            GameManager.GetInstance().SetSelectedStatus(false);
            GameManager.GetInstance().CleanDstBoard(); //borrar las listas y objetos del tablero

            List<SquareNode> selectedPath = null;

            // Buscar la ruta de forma segura
            foreach (PathInfo p in posiblePaths)
            {
                if (p.destination == target)
                {
                    selectedPath = p.path;
                    break;
                }
            }

            if (selectedPath == null)
            {
                if (posiblePaths.Count > 0)
                {
                    // Si no encuentra la exacta por desincronizacion, pilla la primera
                    selectedPath = posiblePaths[0].path;
                }
                else
                {
                    // Si la lista esta totalmente vacia por la rapidez de la IA, 
                    // abortamos el movimiento de este fotograma para que recalcule en el siguiente.
                    isMoving = false;
                    return;
                }
            }

            // Mueve la ficha
            StartCoroutine(MovePiece(selectedPath));
            return;
        }
    }

    // Mueve la ficha
    IEnumerator MovePiece(List<SquareNode> targetPath)
    {
        foreach (SquareNode actNod in targetPath)
        {
            // Calcular la posicion destino manteniendo la altura (Y) original de la ficha
            Vector3 targetPos = new Vector3(actNod.transform.position.x, transform.position.y, actNod.transform.position.z);

            // Mover la ficha poco a poco
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null; // Esperar al siguiente frame
            }

            // Ajustar al final 
            transform.position = targetPos;
            currentSquare = actNod;
        }
        isMoving = false;
        dstShown = false;
        ClearPaths();
        GameManager.GetInstance().WasteMovement();

        // Comprobar si se ha llegado al radio exterior por primera vez desde la casilla de salida central
        // Si la casilla tiene conexiones a los lados
        if (currentSquare.left != null || currentSquare.right != null)
        {
            edgeReached = true;
        }

        // Si ha caido en la casilla de los dados
        if (currentSquare.category == TrivialCategories.Dados)
        {
            DiceThrow diceUI = FindFirstObjectByType<DiceThrow>();
            if (diceUI != null)
            {
                // Se vuelve a lanzar los dados de forma automatica
                diceUI.SquareThrowAgain();
            }

            // Avisar AITurnManager de que puede volver a elegir destino
            if (!GameManager.GetInstance().GetPlayerCurrentTurn().isHuman)
            {
                AITurnManager aiManager = FindFirstObjectByType<AITurnManager>();
                if(aiManager != null)
                {
                    aiManager.AllowNewMovement();
                }
            }
            // Abortar corrutina y no pedir pregunta
            yield break;
        }

        // Enviar peticion de la pregunta dependiendo de la casilla
        if (aiService != null)
        {
            // Categoria de la casilla actual
            string questionCategory = currentSquare.GetCategoryString();

            // Si ha caido en el centro
            if (currentSquare.category == TrivialCategories.Final)
            {
                // Si tiene todos los quesitos, empieza la ronda final
                if (trivialPiece != null && trivialPiece.HaveAllWedges())
                {
                    UIController uiController = FindFirstObjectByType<UIController>();
                    if(uiController != null)
                    {
                        uiController.StartFinalRound();
                    }
                    yield break;
                }
                else
                {
                    // Si aun no tiene todos, se hace una pregunta aleatoria
                    string[] categories = { "Ciencias", "Geografia", "Historia", "Arte y Literatura", "Deportes y Pasatiempos", "Entretenimiento" };
                    questionCategory = categories[UnityEngine.Random.Range(0, categories.Length)];
                    // Debug.Log("Tema aleatorio elegido: " + questionCategory);
                }
            }

            // Obtener datos del jugador actual
            PlayerDescriptor currentPlayer = GameManager.GetInstance().GetPlayerCurrentTurn();

            // Obtener el modelo que pregunta y el que responde
            AIService.Models questionModel = currentPlayer.questionModel;
            AIService.Models answerModel = currentPlayer.answerModel;

            // Debug.Log($"La ficha de {currentPlayer.name} ha caido en {currentSquare.category}. Solicitando pregunta a {questionModel}...");
           
            // Elegir dificultad de la pregunta de forma aleatoria
            string[] difficulties = { "Facil", "Media", "Dificil"};
            string questionDifficulty = difficulties[UnityEngine.Random.Range(0, difficulties.Length)];
            // Debug.Log("Dificultad aleatoria elegida: " + questionDifficulty);

            UIController ui = FindFirstObjectByType<UIController>();
            // Mostrar texto de cargando
            if (ui != null )
            {
                ui.ShowTextLoading();
            }

            // Pedir la pregunta al modelo correspondiente, con la categoria y dificultad correspondientes
            aiService.RequestQuestion(questionModel, answerModel, questionCategory, questionDifficulty);
        }
        else
        {
            Debug.LogError("AIService no esta asignado en el script PieceMovement.");
        }
    }

    void SaveDestinations(List<SquareNode> posdst)
    {
       foreach (SquareNode dst in posdst) {
            GameManager.GetInstance().AddToPosibleDestination(dst);
       }
    }

    //Funciones para el movimiento imaginario 
    //BFS para que busque las casillas a x distancia
    public List<SquareNode> GetPossibleDestinations(SquareNode initialNode, int moves)
    {
        posibilities.Clear();
        posiblePaths.Clear();

        List<SquareNode> results = new List<SquareNode>();
        HashSet<SquareNode> visited = new HashSet<SquareNode>();

        // El primer movimiento solo te deja salir hacia el radio desde la casilla central
        // Comprobar que se esta en un nodo de inicio (no tiene centro asignado y no es el final)
        if (initialNode.centre == null && initialNode.category != TrivialCategories.Final)
        {
            // Comprobar centro verdadero a traves de la primera casilla del radio
            if (initialNode.outwards != null && initialNode.outwards.centre != null)
            {
                // Agregar a la lista de visitados antes de empezar a moverse
                visited.Add(initialNode.outwards.centre);
            }
        }
        SearchDestinations(initialNode, moves, visited, results);
        foreach (PathInfo path in posiblePaths)
        {
            posibilities.Add(path.destination);
        }
        return posibilities;
    }

    void SearchDestinations(SquareNode current, int movesLeft, HashSet<SquareNode> visited, List<SquareNode> actPath)
    {
        visited.Add(current);
        List<SquareNode> pathAtm = new List<SquareNode>(actPath);
        pathAtm.Add(current);

        // Caso base
        if (movesLeft == 0)
        {
            //if (!results.Contains(current)) results.Add(current);
            posiblePaths.Add(new PathInfo { destination = current, path = pathAtm });
            visited.Remove(current); // Limpiamos al salir para otras ramas
            return;
        }

        ////Posibles casillas adyacentes (a falta de la casilla central)
        //SquareNode[] neighbors = { current.centre, current.outwards, current.left, current.right };
        // Obtiene todos los vecinos
        List<SquareNode> neighbors = current.GetNeighbours();
        foreach (SquareNode nei in neighbors)
        {

            if (nei != null && !visited.Contains(nei))
            {
                // Bloquear ir hacia atras en la primera salida desde el centro hasta el radio exterior
                // Si aun no se ha llegado al exterior y el vecino es una casilla que va hacia el centro se ignora
                if (!edgeReached && nei == current.centre)
                {
                    continue;
                }
                SearchDestinations(nei, movesLeft - 1, visited, pathAtm);
            }
        }

        visited.Remove(current);
    }

    void ClearPaths()
    {
        posiblePaths.Clear();
        posibilities.Clear();
    }
}
