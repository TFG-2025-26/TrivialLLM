using UnityEngine;
using System.Collections.Generic;

public class BoardGameManager : MonoBehaviour
{
    public UIController uiController;
    
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
        }

        // Pasar las fichas instanciadas al UIController
        if (uiController != null)
        {
            uiController.fichasTablero = fichasInstanciadas.ToArray();
        }
    }
}
