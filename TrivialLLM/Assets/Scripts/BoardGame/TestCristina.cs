using UnityEngine;

public class TestCristina : MonoBehaviour
{
    public AIService ai;
    public void BotonSimularPregunta()
    {
        // Obtenemos qué IA debe generar la pregunta (Copilot por ejemplo)
        // Y le pedimos una pregunta de un tema aleatorio
        string tema = "Historia";

        DescriptorJugador jugActual = GameManager.GetInstance().getJugTurnoActual();

        Debug.Log("Simulando llegada a casilla para: " + jugActual.nombre);

        // Pedimos la pregunta. 
        // Usamos Copilot para generar y el modelo del jugador para responder
        ai.PedirPregunta(jugActual.modeloPreguntas, jugActual.modelo, tema, "Media");
    }
}
