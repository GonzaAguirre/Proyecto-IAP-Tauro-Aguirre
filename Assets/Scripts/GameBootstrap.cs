using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private GameView gameView;
    [SerializeField] private DataManager dataManager;

    void Start()
    {
        // 1. Inicializar Datos
        dataManager.Initialize();

        // 2. Crear Presentador conectando Vista y Datos
        GamePresenter presenter = new GamePresenter(gameView);
    }
}