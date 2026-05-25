using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se encarga de cambiar de escenas
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public string sceneName; 

    public void onClick()
    {
        // Suena un sonido
        gameObject.GetComponent<AudioSource>().Play();
        Invoke("ChangeScene", 0.2f);
    }

    // Cambia a la escena correspondiente segun el nombre
    private void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
