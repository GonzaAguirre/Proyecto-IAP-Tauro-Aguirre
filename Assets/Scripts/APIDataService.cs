using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // Necesario para UnityWebRequest

public class DataService
{
    // Esta clase NO es MonoBehaviour, es una clase pura (Helper).
    
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
}