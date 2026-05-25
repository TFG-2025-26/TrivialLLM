using UnityEngine;

public class SelectSender : MonoBehaviour
{
    public SquareNode nodoDestino;
    private void Start()
    {
    }

    private void OnDestroy()
    {
    }
    public void sendSigToMove()
    {
        GameManager.GetInstance().RecieveSelectedTransform(this.gameObject.transform);
    }

    private void OnMouseDown()
    {
        GameManager.GetInstance().ReceiveSelectedNode(nodoDestino);
    }
}
