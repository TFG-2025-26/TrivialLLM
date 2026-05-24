using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName; 

    public void onClick()
    {
        gameObject.GetComponent<AudioSource>().Play();
        Invoke("ChangeScene", 0.2f);
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
