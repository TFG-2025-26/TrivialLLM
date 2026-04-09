using UnityEngine;
using System.Collections;

public class PieceMovement : MonoBehaviour
{

    [SerializeField]
    private SquareNode actualSquare;
    [SerializeField]
    private AIService aiService;

    [SerializeField]
    private float speed = 15f;

    private bool isMoving = false;

    void Start()
    {
        // Colocar la ficha exactamente en la casilla inicial al darle a Play
        if(actualSquare != null)
        {
            Vector3 iniPos = new Vector3(actualSquare.transform.position.x, transform.position.y, actualSquare.transform.position.z);
            transform.position = iniPos;
        }
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

    }

    IEnumerator MovePiece(SquareNode targetSquare)
    {
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
            aiService.PedirPregunta(aiService.modeloPregunta, aiService.modeloRespuesta, actualSquare.getTopicString(), "Media");
        }
        else
        {
            Debug.LogError("AIService no esta asignado en el script PieceMovement.");
        }
    }
}
