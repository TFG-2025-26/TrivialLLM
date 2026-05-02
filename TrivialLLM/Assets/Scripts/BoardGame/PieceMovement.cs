using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class PathInfo
{
    public SquareNode destination;
    public List<SquareNode> path;
}
public class PieceMovement : MonoBehaviour
{

    public SquareNode actualSquare;
    [SerializeField]
    private AIService aiService;

    [SerializeField]
    private float speed = 30f;

    public int turnoIndex;
    private bool bordeAlcanzado = false;
    private bool isMoving = false;

    private bool dstShown = false;
    private bool dstSelected = false;
    List<SquareNode> posibilities = new List<SquareNode>();
    List<PathInfo> posiblePaths= new List<PathInfo>();

    FichaTrivial ficha;



    void Start()
    {
        // Colocar la ficha exactamente en la casilla inicial al darle a Play
        if(actualSquare != null)
        {
            Vector3 iniPos = new Vector3(actualSquare.transform.position.x, transform.position.y, actualSquare.transform.position.z);
            transform.position = iniPos;
        }

        ficha = GetComponent<FichaTrivial>();
        aiService = FindFirstObjectByType<AIService>();
    }
    void Update()
    {
        // Si no es el turno de esta ficha, se ignora todo lo demas
        if (GameManager.GetInstance().GetTurnoIndex() != turnoIndex) return;
        if(isMoving) {return; }

        if(!dstShown&&GameManager.GetInstance().IsDiceThrown()) {
            int movesLeft = GameManager.GetInstance().getRemainingMoves();
            GetPossibleDestinations(actualSquare, movesLeft);
            
            saveDestinations(posibilities);
            GameManager.GetInstance().showPosibleDestinations();
            dstShown = true;
        }

        if (GameManager.GetInstance().getSelectedStatus() && !isMoving)
        {
            isMoving = true;
            SquareNode target = GameManager.GetInstance().GetSelectedDst();
            
            GameManager.GetInstance().setSelectedStatus(false);
            GameManager.GetInstance().cleanDstBoard(); //borrar las listas y objetos del tablero
            int i = 0;
            while (i<posiblePaths.Count && target != posiblePaths[i].destination)
            {
                i++;
            }
            StartCoroutine(MovePiece(posiblePaths[i].path));
            return;
        }

        if(posibilities.Count>=1&& Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(MovePiece(posiblePaths[0].path));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 2 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(MovePiece(posiblePaths[1].path));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 3 && Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(MovePiece(posiblePaths[2].path));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 4 && Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(MovePiece(posiblePaths[3].path));
            GameManager.GetInstance().cleanDstBoard();
        }
        // Pruebas con input

        if(Input.GetKeyDown(KeyCode.F))
        {
            UIController uiController = FindFirstObjectByType<UIController>();
            if (uiController != null)
            {
                uiController.StartFinalRound();
            }
        }
        // Comprobar el input y si la casilla actual tiene una conexion en esa direccion
        /*if(Input.GetKeyDown(KeyCode.W) && actualSquare.centre != null)
        {
            StartCoroutine(MovePiece(actualSquare.centre));
        }
        else if (Input.GetKeyDown(KeyCode.S) && actualSquare.outwards != null)
        {
            StartCoroutine(MovePiece(actualSquare.outwards));
        }
        else if(Input.GetKeyDown(KeyCode.A) && actualSquare.left != null)
        {
            StartCoroutine(MovePiece(actualSquare.left));
        }
        else if(Input.GetKeyDown(KeyCode.D) && actualSquare.right != null)
        {
            StartCoroutine(MovePiece(actualSquare.right));
        }*/

    }

    IEnumerator MovePiece(List<SquareNode> targetPath)
    {


        //if (movesLeft > 0)
        //{
        //GameManager.GetInstance().wasteMovement();
        //GameManager.GetInstance().showPosibleDestinations();
        foreach (SquareNode actNod in targetPath)
        {
            //isMoving = true;

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
            actualSquare = actNod;
        }
        isMoving = false;
        dstShown = false;
        clearPaths();
        GameManager.GetInstance().wasteMovement();

        //borrar lista de caminos

        // Comprobar si se ha llegado al radio exterior por primera vez desde la casilla de salida central
        // Si la casilla tiene conexiones a los lados
        if (actualSquare.left != null || actualSquare.right != null)
        {
            bordeAlcanzado = true;
        }

        // Si ha caido en la casilla de los dados
        if (actualSquare.topic == TrivialTopic.Dados)
        {
            DiceTrows dadoUI = FindFirstObjectByType<DiceTrows>();
            if (dadoUI != null)
            {
                dadoUI.squareThrowAgain();
            }

            // Avisar AITurnManager de que puede volver a elegir destino
            if (!GameManager.GetInstance().getJugTurnoActual().esHumano)
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
            
            string temaPregunta = actualSquare.getTopicString();

            // Si ha caido en el centro
            if (actualSquare.topic == TrivialTopic.FinalCentro)
            {
                // Si tiene todos los quesitos, empieza la ronda final
                if (ficha != null && ficha.HaveAllWedges())
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
                    string[] temas = { "Ciencias", "Geografia", "Historia", "Arte y Literatura", "Deportes y Pasatiempos", "Entretenimiento" };
                    temaPregunta = temas[UnityEngine.Random.Range(0, temas.Length)];
                    Debug.Log("Tema aleatorio elegido: " + temaPregunta);
                }

            }

            // Obtener datos del jugador actual
            DescriptorJugador jugActual = GameManager.GetInstance().getJugTurnoActual();

            AIService.Models modeloPregunta = jugActual.modeloPreguntas;
            AIService.Models modeloRespuesta = jugActual.modelo;

            Debug.Log($"La ficha de {jugActual.nombre} ha caido en {actualSquare.topic}. Solicitando pregunta a {modeloPregunta}...");
            string[] dificultades = { "Facil", "Media", "Dificil"};
            string dificultadPregunta = dificultades[UnityEngine.Random.Range(0, dificultades.Length)];
            Debug.Log("Dificultad aleatoria elegida: " + dificultadPregunta);

            UIController ui = FindFirstObjectByType<UIController>();
            if (ui != null )
            {
                ui.ShowTextLoading();
            }
            aiService.PedirPregunta(modeloPregunta, modeloRespuesta, temaPregunta, dificultadPregunta);
        }
        else
        {
            Debug.LogError("AIService no esta asignado en el script PieceMovement.");
        }
    }

    void saveDestinations(List<SquareNode> posdst)
    {
       foreach (SquareNode dst in posdst) {
            GameManager.GetInstance().addToPosibleDestination(dst);
       }
    }

   

    //Funciones para el movimiento imaginario 
    //BFS para que busque las casillas a x distancia
    public List<SquareNode> GetPossibleDestinations(SquareNode initialNode, int moves)
    {
        List<SquareNode> results = new List<SquareNode>();
        HashSet<SquareNode> visited = new HashSet<SquareNode>();

        // El primer movimiento solo te deja salir hacia el radio desde la casilla central
        // Comprobar que se esta en un nodo de inicio (no tiene centro asignado y no es el final)
        if (initialNode.centre == null && initialNode.topic != TrivialTopic.FinalCentro)
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
       // Debug.Log($"Visitando: {current.gameObject.name} | Pasos restantes: {movesLeft}");
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
        List<SquareNode> neighbors = current.ObtenerVecinos();
        foreach (SquareNode nei in neighbors)
        {

            if (nei != null && !visited.Contains(nei))
            {
                // Bloquear ir hacia atras en la primera salida desde el centro hasta el radio exterior
                // Si aun no se ha llegado al exterior y el vecino es una casilla que va hacia el centro se ignora
                if (!bordeAlcanzado && nei == current.centre)
                {
                    continue;
                }
                SearchDestinations(nei, movesLeft - 1, visited, pathAtm);
            }
        }

        visited.Remove(current);
    }

    void clearPaths()
    {
        posiblePaths.Clear();
    }
}
