using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService : MonoBehaviour
{
    private const string url = "http://127.0.0.1:8000/trivial";
   


    public void PedirPregunta(string prompt,string model, System.Action<string> callback)
    {
        StartCoroutine(RequestPregunta(prompt,model, callback));
    }

    private IEnumerator RequestPregunta(string prompt,string model, System.Action<string> callback)
    {
        string json = $"{{\"prompt\":\"{Escape(prompt)}\", \"modelo\":\"{model}\"}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Backend error: " + request.error);
            Debug.LogError(request.downloadHandler.text);
            callback?.Invoke(null);
        }
        else
        {
            Debug.Log("Respuesta backend:");
            Debug.Log(request.downloadHandler.text);
            callback?.Invoke(request.downloadHandler.text);
        }
    }

    private string Escape(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
