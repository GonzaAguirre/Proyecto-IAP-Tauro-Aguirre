using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // Necesario para IEnumerator
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

    [Header("New Call PopUp")]
    [SerializeField] private Image newCallCanvas;
    [SerializeField] private TextMeshProUGUI newCallText;
    [SerializeField] private Button newCallButton;
    [SerializeField] private Image newCallCallerImage;
    [SerializeField] private AudioSource callAudioSource;

    // Eventos de la Interfaz
    public event Action OnSubmitAnswer;
    public event Action<string> OnPlagueSelected;
    public event Action OnCallAnswered;

    private GamePresenter presenter;

void Start()
    {
        if (submitAnswerButton != null)
            submitAnswerButton.onClick.AddListener(() => OnSubmitAnswer?.Invoke());

        // --- BORRAR O COMENTAR ESTA LÍNEA ---
        // presenter = new GamePresenter(this, dataManager); 
        // -------------------------------------

        entryInfoImage.gameObject.SetActive(false);
        newCallCanvas.gameObject.SetActive(false);
    }

    // --- AGREGAR ESTE MÉTODO NUEVO ---
    public void IniciarJuegoManual()
    {
        presenter = new GamePresenter(this, dataManager);
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

        // Opcional: Ocultar el texto después de 2 segundos
        Invoke("HideFeedback", 2f);
    }

    private void HideFeedback()
    {
        feedbackCanvas.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
    }

    private List<string> unlockedTypes = new List<string>();

    public void SetUnlockedTypes(List<string> types)
    {
        unlockedTypes = types;
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
                // LÓGICA DE BLOQUEO
                bool isUnlocked = unlockedTypes.Contains(plague.type);
                
                if (isUnlocked)
                {
                    button.interactable = true;
                    button.onClick.AddListener(() => OnPlagueSelected?.Invoke(plague.id));
                }
                else
                {
                    button.interactable = false;
                    // Opcional: Cambiar color o texto para indicar que está bloqueado
                    tmpComponent.text += " (Bloqueado)";
                    tmpComponent.color = Color.gray;
                }
            }
            else
            {
                Debug.LogError("El Prefab no tiene un componente Button.");
            }
        }
    }

    public void NewCallPopUp(string callerName, Sprite callerImage, string audioPath)
    {
        newCallCanvas.gameObject.SetActive(true);
        newCallText.text = $"¡Nueva llamada de {callerName}!";
        newCallCallerImage.sprite = callerImage;
        
        submitAnswerButton.interactable = false;

        // Limpiamos listeners previos para evitar duplicados
        newCallButton.onClick.RemoveAllListeners();
        
        newCallButton.onClick.AddListener(() => 
        {
            newCallCanvas.gameObject.SetActive(false);
            OnCallAnswered?.Invoke(); 
            PlayCallAudio(audioPath);
            submitAnswerButton.interactable = true;
        });
    }

    private void PlayCallAudio(string audioPath)
    {
        if (string.IsNullOrEmpty(audioPath)) return;

        string resourcePath = audioPath;
        
        // Quitar extensión
        if (System.IO.Path.HasExtension(resourcePath))
        {
            resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
        }

        // Quitar "Assets/Resources/" o "Assets/" si existen, para dejar solo la ruta relativa
        if (resourcePath.StartsWith("Assets/Resources/"))
        {
            resourcePath = resourcePath.Replace("Assets/Resources/", "");
        }
        else if (resourcePath.StartsWith("Assets/"))
        {
            resourcePath = resourcePath.Replace("Assets/", "");
        }

        AudioClip clip = Resources.Load<AudioClip>(resourcePath);

        if (clip != null)
        {
            callAudioSource.clip = clip;
            callAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"[GameView] No se pudo cargar el audio desde Resources: '{resourcePath}'.");
        }
    }
}