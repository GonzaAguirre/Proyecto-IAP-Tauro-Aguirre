using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameView : MonoBehaviour, IGameView
{
    [Header("Módulos (Sub-Vistas)")]
    [SerializeField] private EntriesListView entriesListView;   
    [SerializeField] private NewCallPopupView popupView;        

    [Header("Calling Screen (Principal)")]
    [SerializeField] private TextMeshProUGUI callerNameText;
    [SerializeField] private TextMeshProUGUI callMessageText;
    [SerializeField] private Image callerImage;
    [SerializeField] private Button submitAnswerButton;

    [Header("Entry Info Screen")]
    [SerializeField] private TextMeshProUGUI entryInfoTitleText;
    [SerializeField] private TextMeshProUGUI entryInfoDescriptionText;
    [SerializeField] private TextMeshProUGUI entryInfoDangerText;
    [SerializeField] private TextMeshProUGUI entryInfoSolutionText;
    [SerializeField] private Image entryInfoImage;

    [Header("General")]
    [SerializeField] private AudioSource voiceAudioSource; 

    [Header("Feedback")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private TextMeshProUGUI feedbackText;

    // Eventos de la Interfaz
    public event Action OnSubmitAnswer;
    public event Action<string> OnPlagueSelected;
    public event Action OnCallAnswered;

    private GamePresenter presenter;

    void Start()
    {
        if (submitAnswerButton != null)
            submitAnswerButton.onClick.AddListener(() => OnSubmitAnswer?.Invoke());

        if (entriesListView != null)
            entriesListView.OnPlagueClicked += (id) => OnPlagueSelected?.Invoke(id);

        if (popupView != null)
            popupView.OnCallAnswered += HandleCallAnswered;

        entryInfoImage.gameObject.SetActive(false);
        if (popupView != null) popupView.Hide();
    }


    // --- Métodos de IGameView (Delegación) ---

    public void PopulateEntriesList(List<PestData> plagues)
    {
        if (entriesListView != null) 
            entriesListView.PopulateList(plagues);
    }

    public void SetUnlockedTypes(List<string> types)
    {
        if (entriesListView != null) 
            entriesListView.SetUnlockedTypes(types);
    }

    public void NewCallPopUp(string callerName, Sprite callerImage, string audioPath)
    {
        submitAnswerButton.interactable = false; 
        currentAudioPath = audioPath; 

        if (popupView != null) 
            popupView.Show(callerName, callerImage);
    }

    private string currentAudioPath;

    private void HandleCallAnswered()
    {
        OnCallAnswered?.Invoke();
        PlayCallAudio(currentAudioPath);
        submitAnswerButton.interactable = true;
    }

    // --- Métodos de UI Estándar ---

    public void UpdateCallerInfo(string name, string message, Sprite image)
    {
        callerNameText.text = name;
        callMessageText.text = message;
        callerImage.sprite = image;
        
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
        feedbackText.gameObject.SetActive(true); 
        feedbackCanvas.gameObject.SetActive(true);
        submitAnswerButton.interactable = false;
        
        UpdateCallerInfo("", "", null); 

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

        Invoke("HideFeedback", 2f);
    }

    private void HideFeedback()
    {
        feedbackCanvas.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }

    private void PlayCallAudio(string audioPath)
    {
        if (string.IsNullOrEmpty(audioPath)) return;
        if (voiceAudioSource == null) return;

        string resourcePath = audioPath;
        if (System.IO.Path.HasExtension(resourcePath))
            resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
        
        resourcePath = resourcePath.Replace("Assets/Resources/", "").Replace("Assets/", "");

        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            voiceAudioSource.clip = clip;
            voiceAudioSource.Play();
        }
    }
}