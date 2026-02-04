using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService : MonoBehaviour
{
    [SerializeField] private string apiKey;

    private const string endPoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key=";

    // Enviar prompt con callback
    public void sendPrompt(string prompt, System.Action<string> callback)
    {
        StartCoroutine(requestGemini(prompt, callback));
    }

    private IEnumerator requestGemini(string prompt, System.Action<string> callback)
    {
        string url = endPoint + apiKey;

        string json = $@"
        {{
          ""contents"": [
            {{
              ""parts"": [
                {{ ""text"": ""{escape(prompt)}"" }}
              ]
            }}
          ]
        }}";

        byte[] body = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Gemini error: " + request.error + "\nResponse: " + request.downloadHandler.text);
            callback?.Invoke("Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            string responseText = extractText(request.downloadHandler.text);
            callback?.Invoke(responseText);
        }
    }

    private string escape(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private string extractText(string json)
    {
        string result = "";

        int index = 0;
        while (true)
        {
            // Busca "text" dentro del JSON
            int textIndex = json.IndexOf("\"text\"", index);
            if (textIndex < 0) break;

            int colon = json.IndexOf(":", textIndex);
            int startQuote = json.IndexOf("\"", colon) + 1;
            int endQuote = json.IndexOf("\"", startQuote);
            if (colon < 0 || startQuote < 0 || endQuote < 0) break;

            string text = json.Substring(startQuote, endQuote - startQuote);
            text = text.Replace("\\n", "\n").Replace("\\\"", "\"").Trim();

            if (!string.IsNullOrEmpty(text))
                result += text + "\n";

            index = endQuote + 1;
        }

        if (string.IsNullOrEmpty(result)) return "Respuesta vacía";
        return result.Trim();
    }

}
