using UnityEngine;

public class SelectSender : MonoBehaviour
{
    public SquareNode nodoDestino;
    private void Start()
    {
        //Debug.Log("Me he creado");
        //Debug.Log(this.gameObject.transform.position);
    }

    private void OnDestroy()
    {
        //Debug.Log("WHY??????");
    }
    public void sendSigToMove()
    {
        //Debug.Log("WHY??????");
        GameManager.GetInstance().recieveSelectedTransform(this.gameObject.transform);
    }

    private void OnMouseDown()
    {
        //Debug.Log("Destino cliclado");
        GameManager.GetInstance().receiveSelectedNode(nodoDestino);
    }
}
