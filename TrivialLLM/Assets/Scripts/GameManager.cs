using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public TextMeshProUGUI textoNumHumanos;
    public TextMeshProUGUI textoNumLLMS;

    public GameObject panel;
    public TMP_Dropdown model;
    public TMP_InputField promptText;

    public int numMaxJugadores=1;
    public int numMaxLLMS = 3;
    private int numTotalJugadores;
    private int turno;

    private int numJug;
    private int numLLMS;

    public List<DescriptorJugador> descriptorJug;

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
            numJug = 0;
            numLLMS = 0;
            numTotalJugadores = 0;
            turno = 1;
            descriptorJug = new List<DescriptorJugador>();
           // currentMode = GameMode.AIGame;
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

    public void addHuman()
    {
        if (numJug < numMaxJugadores)
        {
            descriptorJug.Add(new DescriptorJugador { esHumano = true });
            numJug++;
            numTotalJugadores++;
            textoNumHumanos.text = numJug.ToString();
        }
    }
    public void addLLM()
    {
        if (numLLMS < numMaxLLMS)
        {
            //descriptorJug.Add(new DescriptorJugador { esHumano = false, modelo=model, prompt=promptDes });
            numLLMS++;
            numTotalJugadores++;
            textoNumLLMS.text = numLLMS.ToString();
            if(!panel.activeSelf && numLLMS > 0)
            {
                panel.SetActive(true);
            }
          
        }
    }

    public void registrarLLM()
    {
        if (numLLMS < numMaxLLMS)
        {
            string modelo = model.options[model.value].text;
            AIService.Models modeloSeleccionado;
            switch (modelo)
            {
                case "Gemini":
                    modeloSeleccionado = AIService.Models.Gemini; 
                    break;
                case "Copilot":
                    modeloSeleccionado = AIService.Models.Copilot;
                    break;
                case "ChatGPT":
                    modeloSeleccionado = AIService.Models.ChatGPT;
                    break;
                case "Azure":
                    modeloSeleccionado = AIService.Models.Azure;
                    break;
                default:
                    modeloSeleccionado = AIService.Models.Copilot;
                    break;
                    
            }
            descriptorJug.Add(new DescriptorJugador { esHumano = false, modelo =modeloSeleccionado, prompt=promptText.text });
            // Debug del último jugador agregado
            DescriptorJugador ultimo = descriptorJug[descriptorJug.Count - 1];
            Debug.Log("Último LLM agregado -> Modelo: " + ultimo.modelo
                      + ", Prompt: " + ultimo.prompt
                      + ", EsHumano: " + ultimo.esHumano);

        }
    }

    public void quitarHum()
    {
        if (numJug > 0)
        {
            int i = descriptorJug.Count - 1;
            bool enc = false;
            while(!enc && i >= 0) {
                if (descriptorJug[i].esHumano)
                {
                    enc = true;
                    descriptorJug.RemoveAt(i);
                    numJug--;
                    numTotalJugadores--;
                    textoNumHumanos.text= numJug.ToString();
                }
                else
                {
                    i--;
                }
            }
        }
    }
    public void quitarLLM()
    {
        if (numLLMS > 0)
        {
            int i = descriptorJug.Count - 1;
            bool enc = false;
            while (!enc && i >= 0)
            {
                if (!descriptorJug[i].esHumano)
                {
                    enc = true;
                    descriptorJug.RemoveAt(i);
                    numLLMS--;
                    numTotalJugadores--;
                    textoNumLLMS.text= numLLMS.ToString();
                    if (numLLMS <= 0)
                    {
                        panel.SetActive(false);
                    }
                }
                else
                {
                    i--;
                }
            }
            if(!enc &&  numLLMS > 0)
            {
                numLLMS = 0;
                textoNumLLMS.text = numLLMS.ToString();
                panel.SetActive(false);
            }
        }
    }

    public int getNumHuman()
    {
        return numJug;
    }

    public int getNumLLMS()
    {
        return numLLMS;
    }

    public DescriptorJugador getJugTurnoActual()
    {
        return descriptorJug[(turno - 1) % numTotalJugadores];
    }

    public void sigTurno()
    {
        turno++;
      
    }
    
}
