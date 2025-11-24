using System.Collections.Generic;
using UnityEngine;

public class GamePresenter
{
     private IGameView view;
     private DataManager dataManager;

     // Listas y Estado
     private List<PestData> allPlagues;
     private List<CallData> dailyCalls; // Las llamadas del día actual
     private CallData currentCall;

     private string selectedPlagueId;
     private int currentCallIndex = 0;

     // --- NUEVA VARIABLE: Controla el día actual (1, 2 o 3) ---
     private int currentDay = 1;

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
          Debug.Log("🚀 PRESENTER: Iniciando juego...");

          allPlagues = dataManager.GetAllPests();
          currentDay = 1; // Aseguramos empezar en el Día 1

          LoadDayData(); // Función auxiliar para cargar el día
     }

     // --- LÓGICA DE CARGA POR DÍA ---
     private void LoadDayData()
     {
          // 1. Pedimos al Manager las llamadas del día actual
          dailyCalls = dataManager.GetCallsForDay(currentDay);

          Debug.Log($"🌞 INICIANDO DÍA {currentDay} | Llamadas: {dailyCalls.Count}");

          // 2. Llenar la lista visual de plagas (si quisieras filtrar plagas por día, sería aquí)
          view.PopulateEntriesList(allPlagues);

          // 3. Resetear índice y cargar primera llamada
          currentCallIndex = 0;
          LoadCallByIndex(currentCallIndex);
     }

     private void LoadCallByIndex(int index)
     {
          // Seguridad: Si no hay llamadas hoy
          if (dailyCalls == null || dailyCalls.Count == 0)
          {
               view.UpdateCallerInfo($"DÍA {currentDay}", "No hay llamadas programadas para hoy.", null);
               return;
          }

          // --- DETECCIÓN DE FIN DE DÍA (Automática) ---
          if (index >= dailyCalls.Count)
          {
               Debug.Log("🏁 FIN DEL TURNO ACTUAL.");
               StartNextDay(); // <--- Saltamos al siguiente día
               return;
          }
          // --------------------------------------------

          currentCall = dailyCalls[index];
          selectedPlagueId = "";

          view.UpdateCallerInfo(currentCall.callerName, currentCall.message, null);
          view.NewCallPopUp(currentCall.callerName, null, currentCall.audio);

          dataManager.RequestImage(currentCall.callerImageURL, (sprite) =>
          {
               if (currentCall == dailyCalls[index])
               {
                    view.UpdateCallerInfo(currentCall.callerName, currentCall.message, sprite);
                    view.NewCallPopUp(currentCall.callerName, sprite, currentCall.audio);
               }
          });
     }

     // --- LÓGICA PARA AVANZAR AL SIGUIENTE DÍA ---
     private void StartNextDay()
     {
          currentDay++; // Avanzamos (1 -> 2, 2 -> 3)

          // Chequeo de Final del Juego (Después del día 3)
          if (currentDay > 3)
          {
               Debug.Log("🏆 JUEGO COMPLETADO");
               view.UpdateCallerInfo("FIN DEL CONTRATO", "¡Felicidades! Has completado los 3 días de prueba.", null);
               return;
          }

          // Si seguimos jugando, cargamos los datos del nuevo día
          LoadDayData();
     }

     private void HandlePlagueSelection(string plagueId)
     {
          selectedPlagueId = plagueId;
          var plague = allPlagues.Find(p => p.id == plagueId);

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

          if (string.IsNullOrEmpty(selectedPlagueId))
          {
               Debug.Log("⚠️ Selecciona una plaga primero.");
               return;
          }

          bool isCorrect = (selectedPlagueId == currentCall.correctPestID);

          if (isCorrect)
          {
               view.ShowFeedback(true);
               AdvanceToNextCall();
          }
          else
          {
               view.ShowFeedback(false);
               // Avanzamos igual (podrías cambiar esto para obligar a reintentar)
               AdvanceToNextCall();
          }
     }

     private void AdvanceToNextCall()
     {
          currentCallIndex++;
          LoadCallByIndex(currentCallIndex);
     }
}