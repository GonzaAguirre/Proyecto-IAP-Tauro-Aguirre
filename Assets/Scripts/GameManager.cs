using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referencias Clave")]
    public DataManager dataManager;
    public GameView gameView;
    
    // --- AGREGA ESTA LÍNEA ---
    public GameObject panelDeJuego; // Referencia al panel que contiene TODA la UI del juego
    // -------------------------

    [Header("Bases de Datos (JSONs)")]
    public TextAsset jsonArgentina;
    public TextAsset jsonUrbanas;
    public TextAsset jsonEspacio;

    public void IniciarJuego(string tematica, string idioma)
    {
        // 1. Lógica de elegir JSON (igual que antes)
        TextAsset jsonAUsar = jsonArgentina;
        switch (tematica)
        {
            case "Urbana": jsonAUsar = jsonUrbanas; break;
            case "Espacio": jsonAUsar = jsonEspacio; break;
        }

        // 2. Cargar datos
        dataManager.jsonFile = jsonAUsar;
        dataManager.LoadLocalData();

        // --- AGREGA ESTA LÍNEA ---
        // Encendemos el panel del juego antes de arrancar la lógica
        if (panelDeJuego != null) panelDeJuego.SetActive(true);
        // -------------------------

        // 3. Arrancar lógica
        gameView.IniciarJuegoManual();
    }
}