using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameView : MonoBehaviour, IGameView
{
    [Header("Calling screen")]
    [SerializeField] private TextMeshProUGUI callerNameText;
    [SerializeField] private TextMeshProUGUI callMessageText;
    [SerializeField] private Image callerImage;
    [SerializeField] private Button submitAnswerButton;

    [Header("Entries List Config")]
    [SerializeField] private Transform entriesContainer; // El objeto "Content" del ScrollView
    [SerializeField] private GameObject entryButtonPrefab; // PREFAB de un botón

    [Header("Entry info screen")]
    [SerializeField] private TextMeshProUGUI entryInfoTitleText;
    [SerializeField] private TextMeshProUGUI entryInfoDescriptionText;
    [SerializeField] private TextMeshProUGUI entryInfoDangerText;
    [SerializeField] private TextMeshProUGUI entryInfoSolutionText;
    [SerializeField] private Image entryInfoImage;
    [SerializeField] private DataManager dataManager; // <--- Arrastra el DataManager aquí en el Inspector  

    // Eventos de la Interfaz
    public event Action OnSubmitAnswer;
    public event Action<string> OnPlagueSelected;

    private GamePresenter presenter;

    void Start()
    {
        // Pasamos 'this' (la vista) y 'dataManager' (el modelo)
        presenter = new GamePresenter(this, dataManager);
    }
    // --- Implementación de la Interfaz ---

    public void UpdateCallerInfo(string name, string message, Sprite image)
    {
        callerNameText.text = name;
        callMessageText.text = message;
        callerImage.sprite = image;
    }

    public void UpdateEntryInfo(string title, string desc, string danger, string solution, Sprite image)
    {
        entryInfoTitleText.text = title;
        entryInfoDescriptionText.text = desc;
        entryInfoDangerText.text = danger;
        entryInfoSolutionText.text = solution;
        entryInfoImage.sprite = image;
    }

    public void PopulateEntriesList(List<PestData> plagues)
    {
        // Limpiar lista
        foreach (Transform child in entriesContainer) Destroy(child.gameObject);

        if (plagues == null) { Debug.LogError("LA LISTA DE PLAGAS ES NULL"); return; }

        Debug.Log($"Generando {plagues.Count} botones...");

        foreach (var plague in plagues)
        {
            // Chequeo 1: ¿El objeto datos existe?
            if (plague == null)
            {
                Debug.LogError("¡ALERTA! Hay una 'plague' nula en la lista.");
                continue;
            }

            GameObject btnObj = Instantiate(entryButtonPrefab, entriesContainer);

            // Chequeo 2: ¿Encontró el componente?
            var tmpComponent = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (tmpComponent == null)
            {
                // Si entra acá, el Prefab NO tiene el componente, aunque creas que sí.
                Debug.LogError($"ERROR FATAL: El objeto '{btnObj.name}' NO tiene TextMeshProUGUI. Componentes encontrados:");
                foreach (var comp in btnObj.GetComponentsInChildren<Component>())
                    Debug.Log($"- {comp.GetType().Name}");

                continue; // Saltamos para no romper el juego
            }

            // Chequeo 3: ¿El nombre es nulo? (Problema de JSON)
            if (plague.name == null)
            {
                Debug.LogWarning($"OJO: El nombre de la plaga ID '{plague.id}' es NULL. Revisa mayúsculas/minúsculas en JSON vs Script.");
                tmpComponent.text = "SIN NOMBRE";
            }
            else
            {
                tmpComponent.text = plague.name;
            }

            // Click listener...
            Button btn = btnObj.GetComponent<Button>();
            string id = plague.id;
            if (btn) btn.onClick.AddListener(() => OnPlagueSelected?.Invoke(id));
        }
    }
}