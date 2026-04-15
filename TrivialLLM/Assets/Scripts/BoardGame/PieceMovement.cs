using UnityEngine;
using System.Collections;

public class PieceMovement : MonoBehaviour
{

    [SerializeField]
    private SquareNode actualSquare;
    [SerializeField]
    private AIService aiService;

    [SerializeField]
    private float speed = 30f;

    private bool isMoving = false;

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

        // Pruebas con input

        // Comprobar el input y si la casilla actual tiene una conexion en esa direccion
        if(Input.GetKeyDown(KeyCode.W) && actualSquare.centre != null)
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
        }
        else if (Input.GetKeyDown(KeyCode.C))
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

        int movesLeft = GameManager.GetInstance().getRemainingMoves();
        if (movesLeft > 0)
        {
            GameManager.GetInstance().wasteMovement();
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

            // Enviar peticion de la pregunta dependiendo de la casilla
            if (aiService != null)
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
            }
        }
    }
}
