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
    [SerializeField] private TextMeshProUGUI submitButtonText;

    [Header("Entry Info Screen")]
    [SerializeField] private TextMeshProUGUI entryInfoTitleText;
    [SerializeField] private TextMeshProUGUI entryInfoDescriptionText;
    [SerializeField] private TextMeshProUGUI entryInfoDangerText;
    [SerializeField] private TextMeshProUGUI entryInfoSolutionText;
    [SerializeField] private Image entryInfoImage;

    [Header("General")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private AudioSource voiceAudioSource;
    
    [Header("Clock")]
    [SerializeField] private TextMeshProUGUI clockText;
    
    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI callCounterText; 

    [Header("Feedback")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Waiting Screen")]
    [SerializeField] private GameObject waitingPanel;

    [Header("Estadísticas Panel")]
    [SerializeField] private GameObject estadisticasPanel;
    [SerializeField] private TextMeshProUGUI totalCallsText;
    [SerializeField] private TextMeshProUGUI correctAnswersText;
    [SerializeField] private TextMeshProUGUI wrongAnswersText;
    [SerializeField] private TextMeshProUGUI scoreText;

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
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (estadisticasPanel != null) estadisticasPanel.SetActive(false);
        
        InvokeRepeating("UpdateClock", 0f, 1f);
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
        StopCallAudio();
        
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
        StopCallAudio();
        
        feedbackText.gameObject.SetActive(true); 
        feedbackCanvas.gameObject.SetActive(true);
        submitAnswerButton.interactable = false;
        
        UpdateCallerInfo("", "", null); 

        if (isCorrect)
        {
            feedbackText.text = "✅";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "❌";
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
        if (LocalizationManager.CurrentLanguage != "ES" || GameManager.IsToonsMode) return;

        if (string.IsNullOrEmpty(audioPath)) return;
        if (voiceAudioSource == null) return;

        StopCallAudio();

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

    private void StopCallAudio()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
    }

    // --- Métodos nuevos para lógica de juego ---

    public void EnableSubmitButton(bool enabled)
    {
        if (submitAnswerButton != null)
            submitAnswerButton.interactable = enabled;
    }

    public void ShowDayComplete(int day, int correctAnswers, int totalAnswers)
    {
        Debug.Log($"Día {day} Completado! Respuestas correctas: {correctAnswers}/{totalAnswers}");
    }

    public void ShowGameComplete(int totalCorrectAnswers, int totalAnswers)
    {
        Debug.Log("¡Juego Completado!");
        
        int wrongAnswers = totalAnswers - totalCorrectAnswers;
        float percentage = totalAnswers > 0 ? ((float)totalCorrectAnswers / totalAnswers) * 100f : 0f;
        
        if (gamePanel != null) gamePanel.SetActive(false);
        if (estadisticasPanel != null) estadisticasPanel.SetActive(true);
        
        if (totalCallsText != null)
            totalCallsText.text = LocalizationManager.GetTotalCallsText(totalAnswers);
        
        if (correctAnswersText != null)
            correctAnswersText.text = LocalizationManager.GetCorrectAnswersText(totalCorrectAnswers);
        
        if (wrongAnswersText != null)
            wrongAnswersText.text = LocalizationManager.GetWrongAnswersText(wrongAnswers);
        
        if (scoreText != null)
            scoreText.text = LocalizationManager.GetScoreText(percentage);
    }

    // --- Reloj Digital ---

    private void UpdateClock()
    {
        if (clockText != null)
        {
            System.DateTime now = System.DateTime.Now;
            clockText.text = now.ToString("HH:mm:ss");
        }
    }
    
    // --- Información del Juego ---
    
    public void UpdateDayInfo(int day)
    {
        if (dayText != null)
        {
            dayText.text = LocalizationManager.GetDayText(day);
        }
    }
    
    public void UpdateCallCounter(int current, int total)
    {
        if (callCounterText != null)
        {
            callCounterText.text = LocalizationManager.GetCallText(current, total);
        }
    }

    public void ShowWaitingScreen()
    {
        if (waitingPanel != null) waitingPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
        
        if (popupView != null) popupView.Hide();
        if (entryInfoImage != null) entryInfoImage.gameObject.SetActive(false);
        if (feedbackCanvas != null) feedbackCanvas.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void HideWaitingScreen()
    {
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
    }

    public void UpdateLocalization()
    {
        if (submitButtonText != null)
        {
            submitButtonText.text = LocalizationManager.GetSubmitButtonText();
        }
    }
}