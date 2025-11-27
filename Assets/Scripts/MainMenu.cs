using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias")]
    public GameManager gameManager; // Arrastraremos el GameManager aquí
    public GameObject panelMenu;    // El panel visual para apagarlo al jugar
    public Button btnIniciar;

    [Header("Toggles Temática")]
    public Toggle toggleArgentina;
    public Toggle toggleUrbanas;
    public Toggle toggleEspacio;

    [Header("Toggles Idioma")]
    public Toggle toggleES;
    public Toggle toggleEN;
    public Toggle togglePT;

    void Start()
    {
        // Escuchamos el clic del botón
        btnIniciar.onClick.AddListener(IniciarConSeleccion);
    }

    void IniciarConSeleccion()
    {
        // 1. Detectar Temática
        string tematicaElegida = "Argentina"; // Default
        if (toggleUrbanas.isOn) tematicaElegida = "Urbana";
        if (toggleEspacio.isOn) tematicaElegida = "Espacio";

        // 2. Detectar Idioma (Por ahora solo pasamos el dato)
        string idiomaElegido = "ES";
        if (toggleEN.isOn) idiomaElegido = "EN";
        if (togglePT.isOn) idiomaElegido = "PT";

        Debug.Log($"Iniciando: {tematicaElegida} en {idiomaElegido}");

        // 3. Avisar al GameManager y cerrar menú
        gameManager.IniciarJuego(tematicaElegida, idiomaElegido);
        panelMenu.SetActive(false);
    }
}