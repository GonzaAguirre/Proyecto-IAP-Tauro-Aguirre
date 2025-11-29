using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePresenter
{
    private readonly IGameView _view;
    private readonly DataManager _model;

    // Estado de la sesión
    private int _currentDay = 1;
    private Queue<CallData> _todaysCalls;
    private CallData _currentCall;
    private bool _isWaitingForAnswer;
    
    // Estadísticas del día
    private int _correctAnswers = 0;
    private int _totalAnswers = 0;
    
    // Contador de llamadas
    private int _totalCallsInDay = 0;
    private int _currentCallNumber = 0;

    public GamePresenter(IGameView view, DataManager model)
    {
        _view = view;
        _model = model;

        // Suscribirse a eventos de la Vista (El usuario hizo algo)
        _view.OnSubmitAnswer += HandleSubmitAnswer;
        _view.OnPlagueSelected += HandlePlagueSelected;
        // _view.OnNextCallRequested += NextCall; // Si tuvieras un botón de "Siguiente"
    }

    public void StartGame()
    {
        _currentDay = 1;
        SetupDay(_currentDay);
    }

    private void SetupDay(int day)
    {
        // 1. Definir configuración del día según enunciado
        // Día 1: 5 Normales, 7 Llamadas
        // Día 2: +Extrañas, 8 Llamadas
        // Día 3: +Especiales, 10 Llamadas
        
        List<string> unlockedTypes = new List<string>();
        int callsCount = 0;

        switch (day)
        {
            case 1:
                unlockedTypes.Add("Normal");
                callsCount = 7; // [cite: 48]
                break;
            case 2:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño"); // [cite: 49]
                callsCount = 8; // [cite: 50]
                break;
            case 3:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño");
                unlockedTypes.Add("Especial"); // [cite: 51]
                callsCount = 10; // [cite: 52]
                break;
            default:
                Debug.Log("Juego Terminado");
                return;
        }

        // 2. Actualizar la Vista con las plagas desbloqueadas
        _view.SetUnlockedTypes(unlockedTypes);
        
        // 3. Obtener todas las plagas y mandarlas a la vista (ella se encarga de bloquear las grises)
        var allPests = _model.GetAllPests();
        _view.PopulateEntriesList(allPests);

        // 4. Preparar la cola de llamadas del día
        GenerateDailyCalls(day, callsCount);
        
        // 5. Actualizar información del día en la UI
        _view.UpdateDayInfo(day);
        _currentCallNumber = 0;
        _totalCallsInDay = _todaysCalls.Count;

        // 6. Iniciar la primera llamada
        ProcessNextCall();
    }

    private void GenerateDailyCalls(int day, int count)
    {
        _todaysCalls = new Queue<CallData>();

        // Obtener las llamadas del día desde el DataManager
        var calls = _model.GetCallsForDay(day);
        
        // Validar que las llamadas correspondan a plagas desbloqueadas
        // Obtener los tipos desbloqueados para este día
        List<string> unlockedTypes = new List<string>();
        switch (day)
        {
            case 1:
                unlockedTypes.Add("Normal");
                break;
            case 2:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño");
                break;
            case 3:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño");
                unlockedTypes.Add("Especial");
                break;
        }
        
        // Filtrar llamadas que correspondan a plagas desbloqueadas
        var validCalls = calls.Where(call => 
        {
            var pest = _model.GetPestByID(call.correctPestID);
            return pest != null && unlockedTypes.Contains(pest.type);
        }).ToList();
        
        // Safety check: Si no hay suficientes llamadas válidas
        if (validCalls.Count < count)
        {
            Debug.LogWarning($"No hay suficientes llamadas válidas para el día {day}. Se esperaban {count}, pero solo hay {validCalls.Count} disponibles.");
            count = validCalls.Count;
        }

        // Mezclar aleatoriamente para variedad (opcional)
        var random = new System.Random();
        validCalls = validCalls.OrderBy(x => random.Next()).ToList();

        // Encolar las llamadas
        foreach(var call in validCalls.Take(count))
        {
            _todaysCalls.Enqueue(call);
        }
        
        Debug.Log($"Día {day}: {_todaysCalls.Count} llamadas encoladas.");
    }

    private void ProcessNextCall()
    {
        if (_todaysCalls.Count == 0)
        {
            EndDay();
            return;
        }

        _currentCall = _todaysCalls.Dequeue();
        _currentCallNumber++;
        
        // Actualizar contador de llamadas en la UI
        _view.UpdateCallCounter(_currentCallNumber, _totalCallsInDay);
        
        // Pedir imagen (si es URL o local, el DataManager lo resuelve)
        _model.RequestImage(_currentCall.callerImageURL, (sprite) => 
        {
            // Lógica de Tipos de Llamada según el enunciado
            bool requiresAction = (_currentCall.callType == "Consejo"); // Solo consejos requieren respuesta
            
            // Mandar datos a la vista
            _view.NewCallPopUp(_currentCall.callerName, sprite, _currentCall.audio);
            _view.UpdateCallerInfo(_currentCall.callerName, _currentCall.message, sprite);
            
            // Configurar la vista según el tipo de llamada
            if (requiresAction)
            {
                // Llamada de Consejo: Esperar respuesta del jugador
                _isWaitingForAnswer = true;
                _view.EnableSubmitButton(true);
            }
            else
            {
                // Llamadas de Confirmación o Extra: Se cierran automáticamente
                _isWaitingForAnswer = false;
                _view.EnableSubmitButton(false);
                
                // Cerrar automáticamente después de unos segundos
                _view.StartCoroutine(WaitAndNextCall());
            }
        });
    }

    // El usuario seleccionó una plaga en la lista
    private string _selectedPlagueID;
    private void HandlePlagueSelected(string plagueID)
    {
        _selectedPlagueID = plagueID;
        var pest = _model.GetPestByID(plagueID);
        
        // Cargar imagen de la plaga
        _model.RequestImage(pest.imageURL, (sprite) =>
        {
            _view.UpdateEntryInfo(pest.name, pest.description, pest.danger, pest.solution, sprite);
        });
    }

    // El usuario presionó "Submit Answer"
    private void HandleSubmitAnswer()
    {
        // Solo permitir submit para llamadas de tipo "Consejo"
        if (!_isWaitingForAnswer || _currentCall == null) return;
        if (_currentCall.callType != "Consejo") return;

        // Verificar si es correcta
        bool isCorrect = (_selectedPlagueID == _currentCall.correctPestID);
        
        // Actualizar estadísticas
        _totalAnswers++;
        if (isCorrect)
        {
            _correctAnswers++;
        }
        
        _view.ShowFeedback(isCorrect);
        _isWaitingForAnswer = false;

        // Esperar unos segundos y pasar a la siguiente
        _view.StartCoroutine(WaitAndNextCall());
    }

    private System.Collections.IEnumerator WaitAndNextCall()
    {
        yield return new WaitForSeconds(3f); // Esperar feedback
        ProcessNextCall();
    }

    private void EndDay()
    {
        Debug.Log($"Día {_currentDay} Terminado.");
        
        // Mostrar resumen del día con estadísticas
        _view.ShowDayComplete(_currentDay, _correctAnswers, _totalAnswers);
        
        // Resetear contadores para el siguiente día
        _correctAnswers = 0;
        _totalAnswers = 0;
        
        _currentDay++;
        
        if (_currentDay <= 3)
        {
            // Esperar un momento antes de iniciar el siguiente día
            _view.StartCoroutine(WaitAndStartNextDay());
        }
        else
        {
            Debug.Log("¡Juego Completado!");
            _view.ShowGameComplete();
        }
    }

    private System.Collections.IEnumerator WaitAndStartNextDay()
    {
        yield return new WaitForSeconds(4f); // Esperar que se vea el resumen del día
        SetupDay(_currentDay);
    }
}