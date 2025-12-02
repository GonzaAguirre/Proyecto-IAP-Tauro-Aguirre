using UnityEngine;

public class GameManager
{
    private DataManager dataManager;
    private GameView gameView;
    private GameObject panelDeJuego;

    private ArchivosPorIdioma archivosArgentina;
    private ArchivosPorIdioma archivosUrbanas;
    
    private GamePresenter presenter;

    public GameManager(DataManager dm, GameView gv, GameObject panel, ArchivosPorIdioma argentina, ArchivosPorIdioma urbanas)
    {
        dataManager = dm;
        gameView = gv;
        panelDeJuego = panel;
        archivosArgentina = argentina;
        archivosUrbanas = urbanas;
    }
    
    public static bool IsToonsMode = false;

    public void IniciarJuego(string tematica, string idioma)
    {
        // Detectar si es modo Toons (Urbana)
        IsToonsMode = (tematica == "Urbana");

        // Setear idioma global
        LocalizationManager.CurrentLanguage = idioma;

        // 1. Primero elegimos el PAQUETE de temática correcto
        ArchivosPorIdioma paqueteSeleccionado = archivosArgentina; // Default

        switch (tematica)
        {
            case "Urbana": paqueteSeleccionado = archivosUrbanas; break;
            case "Argentina": paqueteSeleccionado = archivosArgentina; break;
        }

        // 2. Dentro del paquete, elegimos el ARCHIVO de idioma correcto
        TextAsset jsonFinal = paqueteSeleccionado.español; // Default

        switch (idioma)
        {
            case "EN": 
                jsonFinal = paqueteSeleccionado.ingles;
                Debug.Log($"📖 Idioma seleccionado: INGLÉS");
                break;
            case "PT": 
                jsonFinal = paqueteSeleccionado.portugues;
                Debug.Log($"📖 Idioma seleccionado: PORTUGUÉS");
                break;
            case "ES": 
                jsonFinal = paqueteSeleccionado.español;
                Debug.Log($"📖 Idioma seleccionado: ESPAÑOL");
                break;
        }

        // Verificación de seguridad
        if (jsonFinal == null)
        {
            Debug.LogError($"❌ ¡Falta el archivo JSON para {tematica} en {idioma}! Usando español por defecto.");
            jsonFinal = paqueteSeleccionado.español;
        }
        else
        {
            Debug.Log($"✅ Archivo JSON cargado: {jsonFinal.name} para {tematica} en {idioma}");
        }

        // 3. Cargar y Arrancar - Pasamos el JSON directamente sin sobrescribir la referencia
        dataManager.LoadLocalData(jsonFinal);

        if (panelDeJuego != null) panelDeJuego.SetActive(true);
        
        // Actualizar textos localizados estáticos (como botones)
        gameView.UpdateLocalization();

        presenter = new GamePresenter(gameView, dataManager);

        presenter.StartGame();
    }
}