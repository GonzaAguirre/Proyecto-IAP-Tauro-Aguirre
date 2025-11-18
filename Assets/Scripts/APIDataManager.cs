using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    // EN LUGAR DE URL, AHORA USAMOS UN ARCHIVO LOCAL
    [Header("Archivo JSON Local")]
    public TextAsset jsonFile; 

    // Almacenamiento interno
    private List<PestData> allPests = new List<PestData>();
    private List<CallData> allCalls = new List<CallData>();

    public bool IsDataLoaded { get; private set; } = false;

    // Ya no necesitamos Initialize ni DataService por ahora
    
    // Cambiamos Start o un método público para cargar al iniciar
    void Start()
    {
        if (jsonFile != null)
        {
            LoadLocalData();
        }
        else
        {
            Debug.LogError("¡Falta asignar el archivo JSON en el Inspector!");
        }
    }

    public void LoadLocalData()
    {
        Debug.Log("Cargando datos desde archivo local...");

        try
        {
            // Leemos el texto directamente del archivo
            string jsonText = jsonFile.text;

            // Parseamos (Convertimos texto a objetos)
            GameDataCollection data = JsonUtility.FromJson<GameDataCollection>(jsonText);

            if (data != null)
            {
                allPests = data.pests;
                allCalls = data.calls;
                IsDataLoaded = true;

                Debug.Log($"✅ ÉXITO LOCAL: Cargadas {allPests.Count} plagas y {allCalls.Count} llamadas.");
                Debug.Log($"Temática: {data.themeName}");
            }
            else
            {
                Debug.LogError("El JSON parece estar vacío o mal formado.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error crítico leyendo el JSON local: {e.Message}");
        }
    }

    // --- GETTERS (Igual que antes) ---
    public PestData GetPestByID(string id)
    {
        // Busca en la lista la plaga con ese ID
        return allPests.FirstOrDefault(p => p.id == id);
    }

    public List<CallData> GetCallsForDay(int day)
    {
        // Devuelve todas las llamadas asignadas a ese día
        return allCalls.Where(c => c.day == day).ToList();
    }
    
    public CallData GetCallByID(string id)
    {
         return allCalls.FirstOrDefault(c => c.id == id);
    }
}