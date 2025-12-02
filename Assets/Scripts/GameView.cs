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
    [SerializeField] private TextMeshProUGUI submitButtonText; // Referencia al texto del botón

    [Header("Entry Info Screen")]
    [SerializeField] private TextMeshProUGUI entryInfoTitleText;
    [SerializeField] private TextMeshProUGUI entryInfoDescriptionText;
    [SerializeField] private TextMeshProUGUI entryInfoDangerText;
    [SerializeField] private TextMeshProUGUI entryInfoSolutionText;
    [SerializeField] private Image entryInfoImage;

    [Header("General")]
    [SerializeField] private GameObject gamePanel; // Panel principal del juego
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
    [SerializeField] private GameObject waitingPanel; // Panel "Espera Llamada"

    [Header("Estadísticas Panel")]
    [SerializeField] private GameObject estadisticasPanel; // Panel de estadísticas finales
    [SerializeField] private TextMeshProUGUI totalCallsText; // "Llamadas totales del juego: N"
    [SerializeField] private TextMeshProUGUI correctAnswersText; // "Sugerencias correctas: N"
    [SerializeField] private TextMeshProUGUI wrongAnswersText; // "Sugerencias malas: N"
    [SerializeField] private TextMeshProUGUI scoreText; // "Puntuación: N%"

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
        if (waitingPanel != null) waitingPanel.SetActive(false); // Ocultar pantalla de espera
        if (estadisticasPanel != null) estadisticasPanel.SetActive(false); // Ocultar panel de estadísticas
        
        // Iniciar actualización del reloj cada segundo
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
        // Detener cualquier audio anterior
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
        // Detener el audio de la llamada
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
        // Solo reproducir audio si estamos en Español Y NO estamos en modo Toons
        if (LocalizationManager.CurrentLanguage != "ES" || GameManager.IsToonsMode) return;

        if (string.IsNullOrEmpty(audioPath)) return;
        if (voiceAudioSource == null) return;

        // Detener cualquier audio anterior
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
        
        // No mostramos feedback aquí - se mostrará todo en el resumen final
        // El resumen completo se mostrará en ShowGameComplete() al finalizar el día 3
    }

    public void ShowGameComplete(int totalCorrectAnswers, int totalAnswers)
    {
        Debug.Log("¡Juego Completado!");
        
        // Calcular estadísticas
        int wrongAnswers = totalAnswers - totalCorrectAnswers;
        float percentage = totalAnswers > 0 ? ((float)totalCorrectAnswers / totalAnswers) * 100f : 0f;
        
        // Ocultar el juego y mostrar el panel de estadísticas
        if (gamePanel != null) gamePanel.SetActive(false);
        if (estadisticasPanel != null) estadisticasPanel.SetActive(true);
        
        // Actualizar los textos del panel de estadísticas
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
            // Obtener hora actual del sistema
            System.DateTime now = System.DateTime.Now;
            
            // Formatear como HH:MM:SS (24 horas)
            clockText.text = now.ToString("HH:mm:ss");
            // Alternativa 12 horas con AM/PM: clockText.text = now.ToString("hh:mm:ss tt");
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
        if (gamePanel != null) gamePanel.SetActive(false); // Ocultar el juego
        
        // Limpiar la pantalla por seguridad
        if (popupView != null) popupView.Hide();
        if (entryInfoImage != null) entryInfoImage.gameObject.SetActive(false);
        if (feedbackCanvas != null) feedbackCanvas.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void HideWaitingScreen()
    {
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true); // Mostrar el juego
    }

    public void UpdateLocalization()
    {
        if (submitButtonText != null)
        {
            submitButtonText.text = LocalizationManager.GetSubmitButtonText();
        }
    }
}