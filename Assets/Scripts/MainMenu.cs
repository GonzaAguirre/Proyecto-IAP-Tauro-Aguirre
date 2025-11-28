using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias Clave")]
    public DataManager dataManager;
    public GameView gameView;
    public GameObject panelDeJuego;

    [Header("Bases de Datos (Temáticas + Idiomas)")]
    public ArchivosPorIdioma archivosArgentina;
    public ArchivosPorIdioma archivosUrbanas;
    public ArchivosPorIdioma archivosEspacio;
    private GamePresenter presenter;
    
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    public GameObject panelMenu;
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
        btnIniciar.onClick.AddListener(IniciarConSeleccion);
    }

    void IniciarConSeleccion()
    {
        // 1. Detectar Temática
        string tematicaElegida = "Argentina";
        if (toggleUrbanas.isOn) tematicaElegida = "Urbana";
        if (toggleEspacio.isOn) tematicaElegida = "Espacio";

        // 2. Detectar Idioma (Por ahora solo pasamos el dato)
        string idiomaElegido = "ES";
        if (toggleEN.isOn) idiomaElegido = "EN";
        if (togglePT.isOn) idiomaElegido = "PT";

        // 3. Iniciar GameManager
        gameManager = new GameManager(dataManager, gameView, panelDeJuego, archivosArgentina, archivosUrbanas, archivosEspacio);
        
        Debug.Log($"Iniciando: {tematicaElegida} en {idiomaElegido}");

        // 4. Avisar al GameManager y cerrar menú
        gameManager.IniciarJuego(tematicaElegida, idiomaElegido);
        panelMenu.SetActive(false);
    }
}