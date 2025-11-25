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

          // MEZCLAR LLAMADAS (Randomize)
          ShuffleCalls(dailyCalls);

          Debug.Log($"🌞 INICIANDO DÍA {currentDay} | Llamadas: {dailyCalls.Count}");

          // 2. Llenar la lista visual de plagas FILTRANDO por día
          // Día 1: Solo "Normal"
          // Día 2: "Normal" + "Extraño"
          // Día 3: Todas ("Normal", "Extraño", "Especial")
          
          List<string> unlockedTypes = new List<string>();
          unlockedTypes.Add("Normal");
          
          if (currentDay >= 2) unlockedTypes.Add("Extraño");
          if (currentDay >= 3) unlockedTypes.Add("Especial");

          // PRIMERO configuramos los tipos desbloqueados en la Vista
          if (view is GameView gameView)
          {
               gameView.SetUnlockedTypes(unlockedTypes);
          }

          // LUEGO poblamos la lista (ahora la vista ya sabe qué bloquear)
          view.PopulateEntriesList(allPlagues);

          // 3. Resetear índice y cargar primera llamada con DELAY inicial
          currentCallIndex = 0;
          
          // Pequeño delay inicial antes de la primera llamada
          view.StartCoroutine(WaitAndLoadCall(2.0f)); 
     }

     private System.Collections.IEnumerator WaitAndLoadCall(float delay)
     {
          yield return new WaitForSeconds(delay);
          LoadCallByIndex(currentCallIndex);
     }

     private void ShuffleCalls(List<CallData> calls)
     {
          for (int i = 0; i < calls.Count; i++)
          {
               CallData temp = calls[i];
               int randomIndex = Random.Range(i, calls.Count);
               calls[i] = calls[randomIndex];
               calls[randomIndex] = temp;
          }
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
          // Esperar entre 5 y 10 segundos antes de la siguiente llamada
          float randomDelay = Random.Range(5.0f, 10.0f);
          Debug.Log($"⏳ Esperando {randomDelay:F1} segundos para la próxima llamada...");
          view.StartCoroutine(WaitAndLoadCall(randomDelay));
     }
}