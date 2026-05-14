using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProfile
{
    public float accuracyBase;
    public int knowledgeStart;
    public int knowledgeCutoff;
    public List<string> strongCategories;
    public List<string> weakCategories;
    public float randomness;
}

[System.Serializable]
public class RoleRequest
{
    public string role;
}