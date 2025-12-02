using UnityEngine;

public static class LocalizationManager
{
    public static string CurrentLanguage = "ES";

    public static string GetDayText(int day)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"DAY {day}";
            case "PT": return $"DIA {day}";
            default: return $"DÍA {day}";
        }
    }

    public static string GetCallText(int current, int total)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"CALL {current}/{total}";
            case "PT": return $"CHAMADA {current}/{total}";
            default: return $"LLAMADA {current}/{total}";
        }
    }

    public static string GetLockedText()
    {
        switch (CurrentLanguage)
        {
            case "EN": return " [LOCKED]";
            case "PT": return " [BLOQUEADO]";
            default: return " [BLOQUEADO]";
        }
    }

    public static string GetTotalCallsText(int total)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"Total Game Calls: {total}";
            case "PT": return $"Total de Chamadas: {total}";
            default: return $"Llamadas totales del juego: {total}";
        }
    }

    public static string GetCorrectAnswersText(int correct)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"Correct Suggestions: {correct}";
            case "PT": return $"Sugestões Corretas: {correct}";
            default: return $"Sugerencias correctas: {correct}";
        }
    }

    public static string GetWrongAnswersText(int wrong)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"Wrong Suggestions: {wrong}";
            case "PT": return $"Sugestões Incorretas: {wrong}";
            default: return $"Sugerencias incorrectas: {wrong}";
        }
    }

    public static string GetScoreText(float percentage)
    {
        switch (CurrentLanguage)
        {
            case "EN": return $"Score: {percentage:F0}%";
            case "PT": return $"Pontuação: {percentage:F0}%";
            default: return $"Puntuación: {percentage:F0}%";
        }
    }

    public static string GetSubmitButtonText()
    {
        switch (CurrentLanguage)
        {
            case "EN": return "SEND";
            case "PT": return "ENVIAR";
            default: return "ENVIAR";
        }
    }
}
