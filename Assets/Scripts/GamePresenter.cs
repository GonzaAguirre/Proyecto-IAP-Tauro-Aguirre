using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private List<PestData> allPlagues;
     private CallData currentCall;
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
          allPlagues = new List<PestData>();
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
               // Try to load a Sprite from Resources using the stored image path; if not found, pass null.
               Sprite sprite = LoadSpriteFromPath(plague.imageURL);
               view.UpdateEntryInfo(plague.name, plague.description, plague.danger, sprite);
          }
     }

     // Helper to load a Sprite from a Resources path; returns null if not found or path is empty.
     private Sprite LoadSpriteFromPath(string path)
     {
          if (string.IsNullOrEmpty(path))
               return null;

          // Assumes image paths are stored as Resources-compatible paths (without file extension).
          Sprite s = Resources.Load<Sprite>(path);
          return s;
     }

     private void HandleSubmit()
     {
          // Lógica de validación
          if (currentCall != null && selectedPlagueId == currentCall.correctPestID)
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