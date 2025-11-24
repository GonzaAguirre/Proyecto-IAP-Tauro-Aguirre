using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private DataManager dataManager;

     // Listas y Estado
     private List<PestData> allPlagues;
     private List<CallData> dailyCalls; // La lista de llamadas de hoy
     private CallData currentCall;      // La llamada actual en pantalla

     private string selectedPlagueId;
     private int currentCallIndex = 0;  // Para saber por cuál vamos (0, 1, 2...)

     public GamePresenter(IGameView view, DataManager model)
     {
          this.view = view;
          this.dataManager = model;

          this.view.OnPlagueSelected += HandlePlagueSelection;
          this.view.OnSubmitAnswer += HandleSubmit;

          if (dataManager.IsDataLoaded) StartGame();
          else dataManager.OnDataReady += StartGame;
     }

     private void StartGame()
     {
          Debug.Log("🚀 PRESENTER: Iniciando ciclo de juego...");

          allPlagues = dataManager.GetAllPests();

          // 1. Cargamos TODAS las llamadas del Día 1
          dailyCalls = dataManager.GetCallsForDay(1);

          Debug.Log($"📊 Datos: {allPlagues.Count} plagas | {dailyCalls.Count} llamadas para hoy.");

          // 2. Llenar la UI
          view.PopulateEntriesList(allPlagues);

          // 3. Empezar por la primera llamada (Índice 0)
          currentCallIndex = 0;
          LoadCallByIndex(currentCallIndex);
     }

     private void LoadCallByIndex(int index)
     {
          // Chequeo de seguridad: ¿Existen llamadas?
          if (dailyCalls == null || dailyCalls.Count == 0)
          {
               Debug.LogError("❌ No hay llamadas cargadas para este día.");
               return;
          }

          // Chequeo de fin de juego: ¿Ya no hay más llamadas?
          if (index >= dailyCalls.Count)
          {
               Debug.Log("🏁 FIN DEL TURNO");
               view.UpdateCallerInfo("FIN DEL DÍA", "¡Has completado todas las llamadas! Buen trabajo.", null);
               return;
          }

          // Cargar la llamada actual
          currentCall = dailyCalls[index];
          selectedPlagueId = ""; // Reseteamos la selección del jugador

          // Mostrar info básica (mientras carga la foto)
          view.UpdateCallerInfo(currentCall.callerName, currentCall.message, null);

          // Descargar foto del cliente
          dataManager.RequestImage(currentCall.callerImageURL, (sprite) =>
          {
               // Verificar que seguimos en la misma llamada (por si tardó mucho)
               if (currentCall == dailyCalls[index])
                    view.UpdateCallerInfo(currentCall.callerName, currentCall.message, sprite);
          });
     }

     private void HandlePlagueSelection(string plagueId)
     {
          selectedPlagueId = plagueId;
          var plague = allPlagues.Find(p => p.id == plagueId); // minúsculas 'id'

          if (plague != null)
          {
               view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.solution, null);

               dataManager.RequestImage(plague.imageURL, (sprite) =>
               {
                    if (selectedPlagueId == plagueId)
                         view.UpdateEntryInfo(plague.name, plague.description, plague.danger, plague.solution, sprite);
               });
          }
     }

     private void HandleSubmit()
     {
          if (currentCall == null) return;

          // 1. Validar si el jugador seleccionó algo
          if (string.IsNullOrEmpty(selectedPlagueId))
          {
               Debug.Log("⚠️ Selecciona una plaga primero.");
               return;
          }

          Debug.Log($"📝 Respuesta: {selectedPlagueId} | Correcta: {currentCall.correctPestID}");

          // 2. Verificar respuesta
          bool isCorrect = (selectedPlagueId == currentCall.correctPestID);

          if (isCorrect)
          {
               view.ShowFeedback(true); // VERDE
               AdvanceToNextCall();
          }
          else
          {
               view.ShowFeedback(false); // ROJO
               // Opcional: ¿Quieres que avance igual aunque falle? 
               // Por ahora digamos que sí para que el juego fluya:
               AdvanceToNextCall();
          }
     }

     private void AdvanceToNextCall()
     {
          currentCallIndex++;
          // Cargamos la siguiente
          LoadCallByIndex(currentCallIndex);
     }
}