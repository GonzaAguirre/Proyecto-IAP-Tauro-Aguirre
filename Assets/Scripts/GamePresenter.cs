using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePresenter
{
    private readonly IGameView _view;
    private readonly DataManager _model;

    private int _currentDay = 1;
    private Queue<CallData> _todaysCalls;
    private CallData _currentCall;
    private bool _isWaitingForAnswer;
    
    private int _correctAnswers = 0;
    private int _totalAnswers = 0;
    
    private int _totalCorrectAnswers = 0;
    private int _totalGameAnswers = 0;
    
    private int _totalCallsInDay = 0;
    private int _currentCallNumber = 0;

    public GamePresenter(IGameView view, DataManager model)
    {
        _view = view;
        _model = model;

        _view.OnSubmitAnswer += HandleSubmitAnswer;
        _view.OnPlagueSelected += HandlePlagueSelected;
    }

    public void StartGame()
    {
        _currentDay = 1;
        SetupDay(_currentDay);
    }

    private void SetupDay(int day)
    {
        // Día 1: 5 Normales, 7 Llamadas
        // Día 2: +Extrañas, 8 Llamadas
        // Día 3: +Especiales, 10 Llamadas
        
        List<string> unlockedTypes = new List<string>();
        int callsCount = 0;

        switch (day)
        {
            case 1:
                unlockedTypes.Add("Normal");
                callsCount = 7;
                break;
            case 2:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño");
                callsCount = 8;
                break;
            case 3:
                unlockedTypes.Add("Normal");
                unlockedTypes.Add("Extraño");
                unlockedTypes.Add("Especial");
                callsCount = 10;
                break;
            default:
                Debug.Log("Juego Terminado");
                return;
        }

        _view.SetUnlockedTypes(unlockedTypes);
        
        var allPests = _model.GetAllPests();
        _view.PopulateEntriesList(allPests);
        GenerateDailyCalls(day, callsCount);
        
        _view.UpdateDayInfo(day);
        _currentCallNumber = 0;
        _totalCallsInDay = _todaysCalls.Count(c => c.callType == "Consejo");

        ProcessNextCall();
    }

    private void GenerateDailyCalls(int day, int count)
    {
        _todaysCalls = new Queue<CallData>();

        var calls = _model.GetCallsForDay(day);
        
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
        
        var consejosCalls = calls.Where(call => 
        {
            if (call.callType != "Consejo") return false;
            var pest = _model.GetPestByID(call.correctPestID);
            return pest != null && unlockedTypes.Contains(pest.type);
        }).ToList();
        
        var extraCalls = calls.Where(c => c.callType == "Extra").ToList();
        var confirmacionCalls = calls.Where(c => c.callType == "Confirmacion").ToList();
        
        var random = new System.Random();
        consejosCalls = consejosCalls.OrderBy(x => random.Next()).ToList();
        extraCalls = extraCalls.OrderBy(x => random.Next()).ToList();
        confirmacionCalls = confirmacionCalls.OrderBy(x => random.Next()).ToList();
        
        var selectedConsejos = consejosCalls.Take(count).ToList();
        
        int consejoIndex = 0;
        int extraIndex = 0;
        int confirmacionIndex = 0;
        
        foreach (var consejo in selectedConsejos)
        {
            _todaysCalls.Enqueue(consejo);
            consejoIndex++;
            
            if (consejoIndex % 3 == 0 && extraIndex < extraCalls.Count)
            {
                _todaysCalls.Enqueue(extraCalls[extraIndex]);
                extraIndex++;
            }
        }
        
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
        
        if (_currentCall.callType == "Consejo")
        {
            _currentCallNumber++;
            _view.UpdateCallCounter(_currentCallNumber, _totalCallsInDay);
        }
        
        if (GameManager.IsToonsMode)
        {
            SetupCallUI(null);
        }
        else
        {
            _model.RequestImage(_currentCall.callerImageURL, (sprite) => 
            {
                SetupCallUI(sprite);
            });
        }
    }

    private void SetupCallUI(Sprite sprite)
    {
        _view.NewCallPopUp(_currentCall.callerName, sprite, _currentCall.audio);
        _view.UpdateCallerInfo(_currentCall.callerName, _currentCall.message, sprite);
        
        _view.HideWaitingScreen();
        
        if (_currentCall.callType == "Consejo")
        {
            _isWaitingForAnswer = true;
            _view.EnableSubmitButton(true);
        }
        else
        {
            _isWaitingForAnswer = false;
            _view.EnableSubmitButton(false);
            _view.StartCoroutine(AutoCloseSpecialCall());
        }
    }

    private string _selectedPlagueID;
    private void HandlePlagueSelected(string plagueID)
    {
        _selectedPlagueID = plagueID;
        var pest = _model.GetPestByID(plagueID);
        
        if (GameManager.IsToonsMode)
        {
            _view.UpdateEntryInfo(pest.name, pest.description, pest.danger, pest.solution, null);
        }
        else
        {
            _model.RequestImage(pest.imageURL, (sprite) =>
            {
                _view.UpdateEntryInfo(pest.name, pest.description, pest.danger, pest.solution, sprite);
            });
        }
    }

    private void HandleSubmitAnswer()
    {
        if (!_isWaitingForAnswer || _currentCall == null) return;
        bool isCorrect = (_selectedPlagueID == _currentCall.correctPestID);
        
        _totalAnswers++;
        if (isCorrect)
        {
            _correctAnswers++;
        }
        
        _view.ShowWaitingScreen();
        _isWaitingForAnswer = false;

        _view.StartCoroutine(WaitAndNextCall());
    }

    private System.Collections.IEnumerator WaitAndNextCall()
    {
        yield return new WaitForSeconds(3f);
        
        ProcessNextCall();
    }

    private System.Collections.IEnumerator AutoCloseSpecialCall()
    {
        yield return new WaitForSeconds(14f);
        
        _view.ShowWaitingScreen();
        
        yield return new WaitForSeconds(3f);
        
        ProcessNextCall();
    }

    private void EndDay()
    {
        Debug.Log($"Día {_currentDay} Terminado.");
        
        _totalCorrectAnswers += _correctAnswers;
        _totalGameAnswers += _totalAnswers;
        
        _view.ShowDayComplete(_currentDay, _correctAnswers, _totalAnswers);
        
        _correctAnswers = 0;
        _totalAnswers = 0;
        
        _currentDay++;
        
        if (_currentDay <= 3)
        {
            _view.StartCoroutine(WaitAndStartNextDay());
        }
        else
        {
            Debug.Log("¡Juego Completado!");
            _view.ShowGameComplete(_totalCorrectAnswers, _totalGameAnswers);
        }
    }

    private System.Collections.IEnumerator WaitAndStartNextDay()
    {
        yield return new WaitForSeconds(4f);
        SetupDay(_currentDay);
    }
}