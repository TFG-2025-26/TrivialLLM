using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiaEscena : MonoBehaviour
{
    public string sceneName; 

    public void onClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
