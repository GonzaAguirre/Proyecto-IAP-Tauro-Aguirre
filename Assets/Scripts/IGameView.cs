using System.Collections.Generic;
using UnityEngine;

public interface IGameView
{
    // Métodos para actualizar la UI
    void UpdateCallerInfo(string name, string message, Sprite image);
    void UpdateEntryInfo(string title, string desc, string danger, string solution, Sprite image);
    void NewCallPopUp(string callerName, Sprite callerImage, string audioPath);

    // Método para poblar la lista de entradas (plagas)
    void PopulateEntriesList(List<PestData> plagues);
    void ShowFeedback(bool isCorrect);
    
    // Eventos
    event System.Action OnSubmitAnswer;
    event System.Action<string> OnPlagueSelected;
    event System.Action OnCallAnswered;

    // Métodos nuevos para lógica de juego
    void SetUnlockedTypes(List<string> types);
    Coroutine StartCoroutine(System.Collections.IEnumerator routine);
}