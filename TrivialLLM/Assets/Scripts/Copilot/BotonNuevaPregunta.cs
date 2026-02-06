using UnityEngine;

public class BotonNuevaPregunta :MonoBehaviour 
{
    public CopilotService copilot;

    public void NuevaPregunta()
    {
        copilot.PedirPregunta("ciencia", "media");
    }
}
