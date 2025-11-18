using System.Collections.Generic;
using UnityEngine;

public interface IGameView
{
    // Métodos para actualizar la UI
    void UpdateCallerInfo(string name, string message, Sprite image);
    void UpdateEntryInfo(string title, string desc, string extra, Sprite image);
    
    // Método para poblar la lista de entradas (plagas)
    void PopulateEntriesList(List<PlagueData> plagues);

    // Eventos
    event System.Action OnSubmitAnswer;
    event System.Action<string> OnPlagueSelected; 
}