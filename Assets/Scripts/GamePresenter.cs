using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private DataManager dataManager;

     private List<PestData> allPlagues;
     private CallData currentCall;
     private string selectedPlagueId;

     public GamePresenter(IGameView view)
     {
          this.view = view;
          dataManager = new DataManager();

          // Suscripciones
          this.view.OnPlagueSelected += HandlePlagueSelection;
          this.view.OnSubmitAnswer += HandleSubmit;

          // Si los datos ya están, iniciamos. Si no, esperamos el evento.
          if (dataManager.IsDataLoaded) StartGame();
          else dataManager.OnDataReady += StartGame;
     }

     private void StartGame()
     {
          Debug.Log("Presenter: Iniciando juego...");
          allPlagues = dataManager.GetAllPests();
          
          // 1. Llenar la lista visual
          view.PopulateEntriesList(allPlagues);

          // 2. Cargar la primera llamada (Para probar)
          LoadNewCall(dataManager.GetFirstCall());
     }

     private void LoadNewCall(CallData call)
     {
          if (call == null) return;
          currentCall = call;

          // Ponemos una imagen temporal o null mientras carga
          view.UpdateCallerInfo(call.callerName, call.message, null); 

          // Pedimos al Manager que descargue la foto del cliente
          dataManager.RequestImage(call.callerImageURL, (sprite) => {
               // Cuando termine, actualizamos solo la foto
               view.UpdateCallerInfo(call.callerName, call.message, sprite);
          });
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
               view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.solution, sprite);
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