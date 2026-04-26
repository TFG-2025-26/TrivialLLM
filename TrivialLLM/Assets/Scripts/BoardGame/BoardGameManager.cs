using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BoardGameManager : MonoBehaviour
{
    public UIController uiController;

    [Header("Marcadores UI")]
    public FichaTrivial[] marcadores;

    [Header("Nodos de inicio (en la escena)")]
    [Tooltip("Arrastra los 6 nodos centrales de la escena en el mismo orden que los prefabs de las fichas")]
    public SquareNode[] nodesSalida;

    void Start()
    {
        if (GameManager.GetInstance() != null)
        {
            InicializarPartida();
        }
    }

    void InicializarPartida()
    {
        GameManager gm = GameManager.GetInstance();
        int totalJugadores = gm.descriptorJug.Count;

        List<FichaTrivial> fichasInstanciadas = new List<FichaTrivial>();
        List<FichaTrivial> marcadoresActivos = new List<FichaTrivial>();

        // Desactivar todos los marcadores por seguridad
        foreach (var marcador in marcadores)
        {
            if (marcador != null) marcador.gameObject.SetActive(false);
        }

        for (int i = 0; i < totalJugadores; i++)
        {
            DescriptorJugador datos = gm.descriptorJug[i];

            // Prefab
            GameObject prefabFicha = gm.prefabsFichas[datos.fichaIndex];

            // Obtener nodo real de la escena basandose en el indice de la ficha
            SquareNode nodeInicio = nodesSalida[datos.fichaIndex];

            if (nodeInicio != null)
            {
                // Posicion de salida
                Vector3 posSalida = nodeInicio.transform.position;
                posSalida.y = prefabFicha.transform.position.y;

                // Instanciar la ficha en la casilla de salida
                Quaternion iniRot = Quaternion.Euler(90f, 0f, 0f);
                GameObject nuevaFicha = Instantiate(prefabFicha, posSalida, iniRot);
                nuevaFicha.name = "Ficha_" + datos.nombre;

                // Inyectar a la ficha instanciada el nodo real
                PieceMovement pmInstacia = nuevaFicha.GetComponent<PieceMovement>();
                if(pmInstacia != null)
                {
                    pmInstacia.actualSquare = nodeInicio;
                    pmInstacia.turnoIndex = i;
                }

                // Guardar la referencia para el UIController
                FichaTrivial fichaT = nuevaFicha.GetComponent<FichaTrivial>();
                if (fichaT != null)
                {
                    fichasInstanciadas.Add(fichaT);
                }
            }
            else
            {
                Debug.LogError($"Falta asignar el nodo de salida en el indice {datos.fichaIndex} del BoardGameManager.");
            }

            // Marcadores interfaz
            if (datos.fichaIndex >= 0 && datos.fichaIndex < marcadores.Length)
            {
                // Buscar marcador que corresponde a esta ficha
                FichaTrivial marcadorUI = marcadores[datos.fichaIndex];

                if (marcadorUI != null)
                {
                    // Activar en la jerarquia
                    marcadorUI.gameObject.SetActive(true);

                    // Añadir a la lista ordanada para el UIController
                    marcadoresActivos.Add(marcadorUI);

                    // Añadir nombre del jugador
                    TextMeshProUGUI textoNombre = marcadorUI.GetComponentInChildren<TextMeshProUGUI>();
                    if ( textoNombre != null )
                    {
                        textoNombre.text = datos.nombre;
                    }
                }
            }
        }

        // Pasar las fichas instanciadas y los marcadores activos ordenados al UIController
        if (uiController != null)
        {
            uiController.fichasTablero = fichasInstanciadas.ToArray();
            uiController.fichasMarcadores = marcadoresActivos.ToArray();

            // Resaltar primer turno al empezar
            uiController.ActualizarIndicadoresTurno();
        }
    }
}
