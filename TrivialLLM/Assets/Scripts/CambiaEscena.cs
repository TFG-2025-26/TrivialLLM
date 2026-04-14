using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiaEscena : MonoBehaviour
{
    public string sceneName; 

    public void onClick()
    {
        gameObject.GetComponent<AudioSource>().Play();
        Invoke("cambiaEscena", 0.2f);
    }

    private void cambiaEscena()
    {
        SceneManager.LoadScene(sceneName);
    }
}
