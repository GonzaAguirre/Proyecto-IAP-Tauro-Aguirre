using System;
using System.Collections.Generic;

// [Serializable] es OBLIGATORIO para que Unity pueda leer esto desde un JSON
[Serializable]
public class PestData
{
    public string ID;           // Ej: "pest_raton"
    public string Name;         // Ej: "Ratones"
    public string ImageURL;     // URL de la imagen en internet
    public string Description;  // Descripción física
    public string Danger;       // Por qué es peligroso
    public string Solution;     // Cómo solucionarlo
    public string Type;         // "Normal", "Extraño", "Especial"
}

[Serializable]
public class CallData
{
    public string ID;               // Ej: "call_001"
    public string CallerName;       // Nombre del cliente
    public string CallerImageURL;   // Foto del cliente
    public string Message;          // Lo que dice (transcripción)
    public string CorrectPestID;    // ID de la plaga correcta (Ej: "pest_raton")
    public int Day;                 // Día en que aparece (1, 2 o 3)
    public string CallType;         // "Consejo", "Confirmacion", "Extra"
}

// Esta clase "Root" es necesaria porque el JsonUtility de Unity
// no puede leer listas sueltas, necesita un objeto padre.
[Serializable]
public class GameDataCollection
{
    public string themeName;
    public List<PestData> pests;
    public List<CallData> calls;
}