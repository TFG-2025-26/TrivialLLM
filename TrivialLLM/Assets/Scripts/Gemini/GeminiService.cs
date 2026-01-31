
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService: MonoBehaviour
{
    [SerializeField] private string apiKey;

    private const string endPoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key="
;

    public void sendPrompt(string prompt)
    {
        StartCoroutine(RequestGemini(prompt));
    }

    private IEnumerator RequestGemini(string prompt)
    {
        string url = endPoint + apiKey;

        string json = $@"
        {{
          ""contents"": [
            {{
              ""parts"": [
                {{ ""text"": ""{Escape(prompt)}"" }}
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
            Debug.LogError("Gemini error: " + request.error);
        }
        else
        {
            Debug.Log("Gemini response: " + request.downloadHandler.text);
        }
    }

    private string Escape(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }


}
