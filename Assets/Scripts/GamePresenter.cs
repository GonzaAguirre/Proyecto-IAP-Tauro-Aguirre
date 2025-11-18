using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private List<PlagueData> allPlagues;
     private CallerData currentCall;
     private string selectedPlagueId;

     public GamePresenter(IGameView view)
     {
          this.view = view;

          view.OnPlagueSelected += HandlePlagueSelection;
          view.OnSubmitAnswer += HandleSubmit;

          LoadDummyData(); // Aca se cargan los datos reales 
     }

     private void LoadDummyData()
     {
          // TODO: Aca se implementa la carga (JSON o LLM)
          // Por ahora creamos datos falsos para probar
          allPlagues = new List<PlagueData>();
          // Agregar plagas a la lista 

          // Mandamos la lista a la vista para que genere los botones
          view.PopulateEntriesList(allPlagues);
     }

     // Lógica cuando el usuario selecciona una plaga de la lista
     private void HandlePlagueSelection(string plagueId)
     {
          selectedPlagueId = plagueId;
          var plague = allPlagues.Find(p => p.id == plagueId);

          if (plague != null)
          {
               view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.image);
          }
     }

     private void HandleSubmit()
     {
          // Lógica de validación
          if (currentCall != null && selectedPlagueId == currentCall.correctPlagueId)
          {
               Debug.Log("¡Respuesta Correcta!");
               // Lógica para pasar al siguiente día o llamada
          }
          else
          {
               Debug.Log("Respuesta Incorrecta...");
          }
     }
}