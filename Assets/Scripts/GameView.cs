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
    [SerializeField] private TextMeshProUGUI entryInfoAdditionalInfoText;
    [SerializeField] private Image entryInfoImage;

    // Eventos de la Interfaz
    public event Action OnSubmitAnswer;
    public event Action<string> OnPlagueSelected;

    private GamePresenter presenter;

    void Start()
    {
        // Asignar listener al botón de enviar
        submitAnswerButton.onClick.AddListener(() => OnSubmitAnswer?.Invoke());
        
        // Iniciar el presentador
        presenter = new GamePresenter(this);
    }

    // --- Implementación de la Interfaz ---

    public void UpdateCallerInfo(string name, string message, Sprite image)
    {
        callerNameText.text = name;
        callMessageText.text = message;
        callerImage.sprite = image;
    }

    public void UpdateEntryInfo(string title, string desc, string extra, Sprite image)
    {
        entryInfoTitleText.text = title;
        entryInfoDescriptionText.text = desc;
        entryInfoAdditionalInfoText.text = extra;
        entryInfoImage.sprite = image;
    }

    public void PopulateEntriesList(List<PlagueData> plagues)
    {
        // 1. Limpiar lista actual
        foreach (Transform child in entriesContainer) Destroy(child.gameObject);

        // 2. Crear nuevos botones
        foreach (var plague in plagues)
        {
            GameObject btnObj = Instantiate(entryButtonPrefab, entriesContainer);
            // Asumimos que el prefab tiene un TextMeshProUGUI hijo
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = plague.name;
            
            // Configurar el click del botón
            Button btn = btnObj.GetComponent<Button>();
            string id = plague.id; // Capturar variable para la lambda
            btn.onClick.AddListener(() => OnPlagueSelected?.Invoke(id));
        }
    }
}