using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // Necesario para UnityWebRequest

public class DataService
{    
    // Action<string> callback es una función que ejecutaremos cuando terminemos
    public IEnumerator FetchJsonFromURL(string url, Action<string> onSuccess, Action<string> onFailure)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Enviamos la petición y esperamos
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Si falló, avisamos del error
                onFailure?.Invoke(webRequest.error);
            }
            else
            {
                // Si funcionó, devolvemos el texto descargado
                string jsonText = webRequest.downloadHandler.text;
                onSuccess?.Invoke(jsonText);
            }
        }
    }

    // NUEVO: Descargar Imagen desde URL
    public IEnumerator DownloadImage(string url, Action<Sprite> onSuccess)
    {
        if (string.IsNullOrEmpty(url)) yield break;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                // Crear un Sprite a partir de la textura descargada
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