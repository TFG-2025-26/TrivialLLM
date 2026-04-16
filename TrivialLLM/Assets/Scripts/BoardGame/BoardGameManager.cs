using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BoardGameManager : MonoBehaviour
{
    public UIController uiController;

    [Header("Marcadores UI")]
    public FichaTrivial[] marcadores;

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

            PieceMovement pmPrefab = prefabFicha.GetComponent<PieceMovement>();

            if(pmPrefab != null && pmPrefab.actualSquare != null)
            {
                // Posicion de salida
                Vector3 posSalida = pmPrefab.actualSquare.transform.position;
                posSalida.y = prefabFicha.transform.position.y;


                // Instanciar la ficha en la casilla de salida
                Quaternion iniRot = Quaternion.Euler(90f, 0f, 0f);
                GameObject nuevaFicha = Instantiate(prefabFicha, posSalida, iniRot );
                nuevaFicha.name = "Ficha_" + datos.nombre;

                // Guardar la referencia para el UIController
                FichaTrivial fichaT = nuevaFicha.GetComponent<FichaTrivial>();
                if (fichaT != null)
                {
                    fichasInstanciadas.Add(fichaT);
                }

            }
            else
            {
                Debug.LogError($"El prefab de la ficha índice {datos.fichaIndex} no tiene asignada una actualSquare en el Inspector.");
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
