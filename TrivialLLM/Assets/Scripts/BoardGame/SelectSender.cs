using UnityEngine;

public class SelectSender : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Me he creado");
        Debug.Log(this.gameObject.transform.position);
    }

    private void OnDestroy()
    {
        Debug.Log("WHY??????");
    }
    public void sendSigToMove()
    {
        Debug.Log("WHY??????");
        GameManager.GetInstance().recieveSelectedTransform(this.gameObject.transform);
    }

}
