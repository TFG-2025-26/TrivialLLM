using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PieceMovement : MonoBehaviour
{

    public SquareNode actualSquare;
    [SerializeField]
    private AIService aiService;

    [SerializeField]
    private float speed = 30f;

    private bool isMoving = false;

    private bool dstShown = false;
    private bool dstSelected = false;
    List<SquareNode> posibilities = new List<SquareNode>();

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
    }
    void Update()
    {
        if(isMoving) {return; }

        if(!dstShown&&GameManager.GetInstance().IsDiceThrown()) {
            int movesLeft = GameManager.GetInstance().getRemainingMoves();
            posibilities = GetPossibleDestinations(actualSquare, movesLeft);
            saveDestinations(posibilities);
            GameManager.GetInstance().showPosibleDestinations();
            dstShown = true;
        }

        if (GameManager.GetInstance().getSelectedStatus())
        {
            StartCoroutine(MovePiece(GameManager.GetInstance().GetSelectedDst()));
            GameManager.GetInstance().setSelectedStatus(false);
            GameManager.GetInstance().cleanDstBoard(); //borrar las listas y objetos del tablero
        }

        if(posibilities.Count>=1&& Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(MovePiece(posibilities[0]));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 2 && Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(MovePiece(posibilities[1]));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 3 && Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(MovePiece(posibilities[2]));
            GameManager.GetInstance().cleanDstBoard();
        }
        else if (posibilities.Count >= 4 && Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(MovePiece(posibilities[3]));
            GameManager.GetInstance().cleanDstBoard();
        }
        // Pruebas con input

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
        if (Input.GetKeyDown(KeyCode.C))
        {
            if(ficha != null)
            {
                ficha.GanarQuesito("ciencias");
            }
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            if (ficha != null)
            {
                ficha.GanarQuesito("historia");
            }
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            if (ficha != null)
            {
                ficha.GanarQuesito("geografía");
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (ficha != null)
            {
                ficha.GanarQuesito("entretenimiento");
            }
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (ficha != null)
            {
                ficha.GanarQuesito("deportes y pasatiempos");
            }
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (ficha != null)
            {
                ficha.GanarQuesito("arte y literatura");
            }
        }

    }

    IEnumerator MovePiece(SquareNode targetSquare)
    {

       
        //if (movesLeft > 0)
        //{
        //GameManager.GetInstance().wasteMovement();
        //GameManager.GetInstance().showPosibleDestinations();
        
            isMoving = true;

            // Calcular la posicion destino manteniendo la altura (Y) original de la ficha
            Vector3 targetPos = new Vector3(targetSquare.transform.position.x, transform.position.y, targetSquare.transform.position.z);

            // Mover la ficha poco a poco
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null; // Esperar al siguiente frame
            }

            // Ajustar al final 
            transform.position = targetPos;
            actualSquare = targetSquare;
            isMoving = false;
            dstShown = false;
            GameManager.GetInstance().wasteMovement();

            // Enviar peticion de la pregunta dependiendo de la casilla
            /*if (aiService != null)
            {
                Debug.Log($"La ficha ha caido en {actualSquare.topic}. Solicitando pregunta...");

                // Copilot por defecto para probar
                AIService.Models modeloPregunta = AIService.Models.Copilot;
                AIService.Models modeloRespuesta = AIService.Models.Copilot;

                // Si hay GameManager, se obtienen el modelo de respuesta del jugador actual
                if (GameManager.GetInstance() != null && GameManager.GetInstance().descriptorJug.Count > 0)
                {
                    modeloPregunta = GameManager.GetInstance().getJugTurnoActual().modelo;
                }

                aiService.PedirPregunta(modeloPregunta, modeloRespuesta, actualSquare.getTopicString(), "Media");
            }
            else
            {
                Debug.LogError("AIService no esta asignado en el script PieceMovement.");
            }*/
        //}
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
        SearchDestinations(initialNode, moves, visited, results);

        return results;
    }

    void SearchDestinations(SquareNode current, int movesLeft, HashSet<SquareNode> visited, List<SquareNode> results)
    {
       // Debug.Log($"Visitando: {current.gameObject.name} | Pasos restantes: {movesLeft}");
        visited.Add(current);

        // Caso base
        if (movesLeft == 0)
        {
            if (!results.Contains(current)) results.Add(current);
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
                SearchDestinations(nei, movesLeft - 1, visited, results);
            }
        }

        visited.Remove(current);
    }
}
