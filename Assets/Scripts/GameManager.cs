using UnityEngine;

[System.Serializable] // <--- Esto hace que aparezca la "cajita" en el Inspector
public class ArchivosPorIdioma
{
    public TextAsset español;
    public TextAsset ingles;
    public TextAsset portugues;
}

public class GameManager : MonoBehaviour
{
    [Header("Referencias Clave")]
    public DataManager dataManager;
    public GameView gameView;
    public GameObject panelDeJuego;

    [Header("Bases de Datos (Temáticas + Idiomas)")]
    // En lugar de un archivo suelto, ahora pedimos el paquete de 3 idiomas
    public ArchivosPorIdioma archivosArgentina;
    public ArchivosPorIdioma archivosUrbanas;
    public ArchivosPorIdioma archivosEspacio;

    public void IniciarJuego(string tematica, string idioma)
    {
        // 1. Primero elegimos el PAQUETE de temática correcto
        ArchivosPorIdioma paqueteSeleccionado = archivosArgentina; // Default

        switch (tematica)
        {
            case "Urbana": paqueteSeleccionado = archivosUrbanas; break;
            case "Espacio": paqueteSeleccionado = archivosEspacio; break;
            case "Argentina": paqueteSeleccionado = archivosArgentina; break;
        }

        // 2. Dentro del paquete, elegimos el ARCHIVO de idioma correcto
        TextAsset jsonFinal = paqueteSeleccionado.español; // Default

        switch (idioma)
        {
            case "EN": 
                jsonFinal = paqueteSeleccionado.ingles; 
                break;
            case "PT": 
                jsonFinal = paqueteSeleccionado.portugues; 
                break;
            case "ES": 
                jsonFinal = paqueteSeleccionado.español; 
                break;
        }

        // Verificación de seguridad
        if (jsonFinal == null)
        {
            Debug.LogError($"¡Falta el archivo JSON para {tematica} en {idioma}! Usando español por defecto.");
            jsonFinal = paqueteSeleccionado.español;
        }

        // 3. Cargar y Arrancar
        dataManager.jsonFile = jsonFinal;
        dataManager.LoadLocalData();

        if (panelDeJuego != null) panelDeJuego.SetActive(true);
        
        gameView.IniciarJuegoManual();
    }
}