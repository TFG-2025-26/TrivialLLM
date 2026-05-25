using System;
using System.Collections.Generic;
/// <summary>
/// Define el perfil de conocimiento y comportamiento de un jugador controlado por IA
/// Se genera a partir del rol introducido por el usuario
/// </summary>
[Serializable]
public class PlayerProfile
{
    public float accuracyBase;              // Nivel base de acierto del jugador (entre 0.0 y 1-0)
    public int knowledgeStart;              // Año inicial desde que el jugador empieza a tener conocimientos
    public int knowledgeCutoff;             // Año maximo hasta que el jugador tiene conocimientos
    public List<string> strongCategories;   // Categorias que domina el jugador
    public List<string> weakCategories;     // Categorias debiles para el jugador
    public float randomness;                // Nivel de aleatoriedad del comportamiento del jugador
}

/// <summary>
/// Clase utilizada para enviar al backend el rol escrito por el usuario
/// A partir de este texto se genera un PlayerProfile
/// </summary>
[System.Serializable]
public class RoleRequest
{
    public string role; // Descipcion del rol asignado al agente
}