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

    [Header("Feedback")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private TextMeshProUGUI feedbackText;

    // Eventos de la Interfaz
    public event Action OnSubmitAnswer;
    public event Action<string> OnPlagueSelected;

    private GamePresenter presenter;

    void Start()
    {
        // 1. CONECTAR EL BOTÓN (Esto faltaba)
        if (submitAnswerButton != null)
        {
            submitAnswerButton.onClick.AddListener(() => OnSubmitAnswer?.Invoke());
        }
        else
        {
            Debug.LogError("⚠️ ¡El botón Submit no está asignado en el Inspector!");
        }

        // 2. INICIALIZAR PRESENTER
        presenter = new GamePresenter(this, dataManager);

        entryInfoImage.gameObject.SetActive(false);
    }
    // --- Implementación de la Interfaz ---

    public void UpdateCallerInfo(string name, string message, Sprite image)
    {
        callerNameText.text = name;
        callMessageText.text = message;
        callerImage.sprite = image;
        callMessageText.text = message;

        // Ante una nueva llamada, ocultamos la información de la entrada
        entryInfoImage.gameObject.SetActive(false);
        entryInfoTitleText.text = "";
        entryInfoDescriptionText.text = "";
        entryInfoDangerText.text = "";
        entryInfoSolutionText.text = "";
    }

    public void UpdateEntryInfo(string title, string desc, string danger, string solution, Sprite image)
    {
        entryInfoTitleText.text = title;
        entryInfoDescriptionText.text = desc;
        entryInfoDangerText.text = danger;
        entryInfoSolutionText.text = solution;
        entryInfoImage.sprite = image;
        entryInfoImage.gameObject.SetActive(true);
    }
    public void ShowFeedback(bool isCorrect)
    {
        feedbackText.gameObject.SetActive(true); // Nos aseguramos que se vea
        feedbackCanvas.gameObject.SetActive(true);

        if (isCorrect)
        {
            feedbackText.text = "¡CORRECTO!\nENVIANDO EQUIPO...";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "¡INCORRECTO!\nINTENTA DE NUEVO.";
            feedbackText.color = Color.red;
        }

        // Opcional: Ocultar el texto después de 2 segundos
        Invoke("HideFeedback", 2f);
    }

    private void HideFeedback()
    {
        feedbackCanvas.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }
    public void PopulateEntriesList(List<PestData> plagues)
    {
        Debug.Log($"INTENTANDO CREAR {plagues.Count} BOTONES..."); // <--- Agrega esto

        // Limpiar lista
        foreach (Transform child in entriesContainer) Destroy(child.gameObject);

        if (plagues == null)
        {
            Debug.LogError("LA LISTA DE PLAGAS ES NULL");
            return;
        }

        if (entryButtonPrefab == null)
        {
            Debug.LogError("El prefab 'entryButtonPrefab' no está asignado en el Inspector.");
            return;
        }

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
            btnObj.transform.localScale = Vector3.one;

            // Chequeo 2: ¿Encontró el componente?
            var tmpComponent = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (tmpComponent == null)
            {
                Debug.LogError("El Prefab no tiene un componente TextMeshProUGUI.");
                continue;
            }

            tmpComponent.text = plague.name;

            // Asignar evento al botón
            var button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnPlagueSelected?.Invoke(plague.id));
            }
            else
            {
                Debug.LogError("El Prefab no tiene un componente Button.");
            }
        }
    }
}