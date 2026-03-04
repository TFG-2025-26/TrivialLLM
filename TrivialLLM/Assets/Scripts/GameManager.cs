using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    // Modos de juego posible
    public enum GameMode { HumanGame, AIGame }

    // Modo de juego actual
    public GameMode currentMode;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    // Establece el modo de juego desde los botones del menu principal
    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;
        Debug.Log("Modo de juego actual: " + currentMode);
    }

    // Metodos para los botones de StartScene
    public void PlayHumanMode()
    {
        SetGameMode(GameMode.HumanGame);
        SceneManager.LoadScene("HumanGameScene");
    }

    public void PlayAIMode()
    {
        SetGameMode(GameMode.AIGame);
        SceneManager.LoadScene("IAGameScene");
    }
}
