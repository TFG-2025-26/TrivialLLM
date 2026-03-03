using System;
using UnityEngine;

[Serializable]
public class PromptGPTData : PromptRequest
{
    public int max_tokens;
    public int temperature;
}
