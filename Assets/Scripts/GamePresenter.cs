using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private DataManager dataManager;

     private List<PestData> allPlagues;
     private CallData currentCall;
     private string selectedPlagueId;

// GamePresenter.cs
     public GamePresenter(IGameView view, DataManager model) // <--- Agregamos el parámetro
     {
     this.view = view;
     this.dataManager = model; // <--- Lo asignamos, NO hacemos new()

     this.view.OnPlagueSelected += HandlePlagueSelection;
     this.view.OnSubmitAnswer += HandleSubmit;

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
 // REEMPLAZAR ESTE MÉTODO EN GamePresenter.cs

     private void HandlePlagueSelection(string plagueId)
     {
     selectedPlagueId = plagueId;
     var plague = allPlagues.Find(p => p.id == plagueId);

     if (plague != null)
     {
          // 1. Mostramos el texto inmediatamente (con imagen null/vacía por ahora)
          //    Esto hace que la interfaz se sienta rápida.
          view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.solution, null);

          // 2. Pedimos descargar la imagen de internet
          dataManager.RequestImage(plague.imageURL, (sprite) => 
          {
               // 3. Verificamos si el usuario TODAVÍA tiene seleccionada esta plaga
               //    (Por si cambió rápido a otra mientras descargaba)
               if (selectedPlagueId == plagueId)
               {
                    view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.solution, sprite);
               }
          });
     }
     }

// Puedes borrar el método 'LoadSpriteFromPath' ya que no lo usaremos.

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