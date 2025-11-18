using UnityEngine;

public class TestLoader : MonoBehaviour
{
    // CAMBIO CLAVE: De 'string' a 'TextAsset'
    [Header("Arrastra aquí tu archivo JSON")]
    public TextAsset jsonFile; 

    void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("¡Te olvidaste de arrastrar el archivo JSON al inspector!");
            return;
        }

        Debug.Log("Contenido del archivo: " + jsonFile.text);

        // Aquí simularíamos pasarle este texto al DataManager...
        // DataManager.LoadData(jsonFile.text);
    }
}