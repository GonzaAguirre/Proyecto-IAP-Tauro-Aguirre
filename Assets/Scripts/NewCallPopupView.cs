using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NewCallPopupView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupCanvas; // El objeto padre que activamos/desactivamos
    [SerializeField] private TextMeshProUGUI newCallText;
    [SerializeField] private Image callerImage;
    [SerializeField] private Button answerButton;

    [Header("Audio Ringtone")]
    [SerializeField] private AudioSource ringtoneSource; // El AudioSource del Panel
    [SerializeField] private AudioClip ringtoneClip;     // El archivo de sonido

    // Evento para avisar al padre que atendimos
    public event Action OnCallAnswered;

    public void Show(string callerName, Sprite image)
    {
        popupCanvas.SetActive(true);
        newCallText.text = $"¡Nueva llamada de {callerName}!";
        callerImage.sprite = image;

        // --- RINGTONE ---
        if (ringtoneSource != null && ringtoneClip != null)
        {
            ringtoneSource.clip = ringtoneClip;
            ringtoneSource.loop = true;
            ringtoneSource.volume = 0.5f;
            ringtoneSource.Play();
        }

        // Configurar botón
        answerButton.onClick.RemoveAllListeners();
        answerButton.onClick.AddListener(AnswerCall);
    }

    private void AnswerCall()
    {
        // Cortar Ringtone
        if (ringtoneSource != null) ringtoneSource.Stop();

        popupCanvas.SetActive(false);
        OnCallAnswered?.Invoke();
    }

    public void Hide()
    {
        if (ringtoneSource != null) ringtoneSource.Stop();
        popupCanvas.SetActive(false);
    }
}