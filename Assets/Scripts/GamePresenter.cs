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
    
    // Estadísticas totales del juego (acumuladas)
    private int _totalCorrectAnswers = 0;
    private int _totalGameAnswers = 0;
    
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
        // Contar solo las llamadas de Consejo para el contador de UI
        _totalCallsInDay = _todaysCalls.Count(c => c.callType == "Consejo");

        // 6. Iniciar la primera llamada
        ProcessNextCall();
    }

    private void GenerateDailyCalls(int day, int count)
    {
        _todaysCalls = new Queue<CallData>();

        // Obtener las llamadas del día desde el DataManager
        var calls = _model.GetCallsForDay(day);
        
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
        
        // Separar llamadas por tipo
        var consejosCalls = calls.Where(call => 
        {
            if (call.callType != "Consejo") return false;
            var pest = _model.GetPestByID(call.correctPestID);
            return pest != null && unlockedTypes.Contains(pest.type);
        }).ToList();
        
        var extraCalls = calls.Where(c => c.callType == "Extra").ToList();
        var confirmacionCalls = calls.Where(c => c.callType == "Confirmacion").ToList();
        
        // Mezclar aleatoriamente cada tipo
        var random = new System.Random();
        consejosCalls = consejosCalls.OrderBy(x => random.Next()).ToList();
        extraCalls = extraCalls.OrderBy(x => random.Next()).ToList();
        confirmacionCalls = confirmacionCalls.OrderBy(x => random.Next()).ToList();
        
        // Tomar las llamadas de Consejo necesarias
        var selectedConsejos = consejosCalls.Take(count).ToList();
        
        // Intercalar llamadas especiales de forma controlada
        int consejoIndex = 0;
        int extraIndex = 0;
        int confirmacionIndex = 0;
        
        foreach (var consejo in selectedConsejos)
        {
            // Agregar llamada de Consejo
            _todaysCalls.Enqueue(consejo);
            consejoIndex++;
            
            // Cada 2-3 llamadas de Consejo, agregar una Extra (si hay disponibles)
            if (consejoIndex % 3 == 0 && extraIndex < extraCalls.Count)
            {
                _todaysCalls.Enqueue(extraCalls[extraIndex]);
                extraIndex++;
            }
        }
        
        // Agregar Confirmaciones al final del día (si hay disponibles)
        while (confirmacionIndex < confirmacionCalls.Count && confirmacionIndex < 2)
        {
            _todaysCalls.Enqueue(confirmacionCalls[confirmacionIndex]);
            confirmacionIndex++;
        }
        
        Debug.Log($"Día {day}: {_todaysCalls.Count} llamadas encoladas ({selectedConsejos.Count} Consejo, {extraIndex} Extra, {confirmacionIndex} Confirmación).");
    }

    private void ProcessNextCall()
    {
        if (_todaysCalls.Count == 0)
        {
            EndDay();
            return;
        }

        _currentCall = _todaysCalls.Dequeue();
        
        // Solo incrementar contador para llamadas de Consejo
        if (_currentCall.callType == "Consejo")
        {
            _currentCallNumber++;
            _view.UpdateCallCounter(_currentCallNumber, _totalCallsInDay);
        }
        
        // Pedir imagen (si es URL o local, el DataManager lo resuelve)
        _model.RequestImage(_currentCall.callerImageURL, (sprite) => 
        {
            // 1. PRIMERO actualizamos los datos (mientras sigue oculto)
            _view.NewCallPopUp(_currentCall.callerName, sprite, _currentCall.audio);
            _view.UpdateCallerInfo(_currentCall.callerName, _currentCall.message, sprite);
            
            // 2. LUEGO mostramos el juego (ya actualizado)
            _view.HideWaitingScreen();
            
            // Diferenciar entre llamadas de Consejo y Especiales
            if (_currentCall.callType == "Consejo")
            {
                // Llamada normal - esperar respuesta del jugador
                _isWaitingForAnswer = true;
                _view.EnableSubmitButton(true);
            }
            else
            {
                // Llamada especial (Extra o Confirmación) - auto-cerrar
                _isWaitingForAnswer = false;
                _view.EnableSubmitButton(false);
                _view.StartCoroutine(AutoCloseSpecialCall());
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
        if (!_isWaitingForAnswer || _currentCall == null) return;

        // Verificar si es correcta
        bool isCorrect = (_selectedPlagueID == _currentCall.correctPestID);
        
        // Actualizar estadísticas
        _totalAnswers++;
        if (isCorrect)
        {
            _correctAnswers++;
        }
        
        // Mostrar directamente la pantalla de espera (sin feedback)
        _view.ShowWaitingScreen();
        _isWaitingForAnswer = false;

        // Esperar unos segundos y pasar a la siguiente
        _view.StartCoroutine(WaitAndNextCall());
    }

    private System.Collections.IEnumerator WaitAndNextCall()
    {
        // Esperar tiempo fijo de 3 segundos en la pantalla de espera
        yield return new WaitForSeconds(3f);
        
        ProcessNextCall();
    }

    private System.Collections.IEnumerator AutoCloseSpecialCall()
    {
        yield return new WaitForSeconds(14f); // 14 segundos para leer el mensaje
        
        // Mostrar pantalla de espera al terminar la llamada especial
        _view.ShowWaitingScreen();
        
        // Esperar tiempo fijo de 3 segundos también aquí
        yield return new WaitForSeconds(3f);
        
        ProcessNextCall();
    }

    private void EndDay()
    {
        Debug.Log($"Día {_currentDay} Terminado.");
        
        // Acumular estadísticas del día en el total del juego
        _totalCorrectAnswers += _correctAnswers;
        _totalGameAnswers += _totalAnswers;
        
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
            // Pasar las estadísticas totales al resumen final
            _view.ShowGameComplete(_totalCorrectAnswers, _totalGameAnswers);
        }
    }

    private System.Collections.IEnumerator WaitAndStartNextDay()
    {
        yield return new WaitForSeconds(4f); // Esperar que se vea el resumen del día
        SetupDay(_currentDay);
    }
}