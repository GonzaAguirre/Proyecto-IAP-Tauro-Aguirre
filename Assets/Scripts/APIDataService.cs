using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking; 

public class DataService
{    
    public IEnumerator FetchJsonFromURL(string url, Action<string> onSuccess, Action<string> onFailure)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                onFailure?.Invoke(webRequest.error);
            }
            else
            {
                string jsonText = webRequest.downloadHandler.text;
                onSuccess?.Invoke(jsonText);
            }
        }
    }

    public IEnumerator DownloadImage(string url, Action<Sprite> onSuccess)
    {
        if (string.IsNullOrEmpty(url)) yield break;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                onSuccess?.Invoke(sprite);
            }
            else
            {
                Debug.LogError($"Error descargando imagen {url}: {request.error}");
            }
        }
    }
}