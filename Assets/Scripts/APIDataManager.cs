using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Nos ayuda a buscar en listas fácilmente

public class DataManager : MonoBehaviour
{
    // Dependencia: El servicio de descarga
    private DataService dataService;

    // Almacenamiento local de los datos cargados
    private List<PestData> allPests = new List<PestData>();
    private List<CallData> allCalls = new List<CallData>();

    // Evento para avisar cuando los datos estén listos
    public bool IsDataLoaded { get; private set; } = false;

    public void Initialize(DataService service)
    {
        this.dataService = service;
    }

    // Método para iniciar la carga
    public void LoadAllData(string url)
    {
        if (dataService == null)
        {
            Debug.LogError("DataService no inicializado en DataManager.");
            return;
        }

        Debug.Log("Iniciando descarga de datos...");
        // Como DataService no es MonoBehaviour, quien corre la Corrutina soy YO (DataManager)
        StartCoroutine(dataService.FetchJsonFromURL(url, OnDataLoadSuccess, OnDataLoadError));
    }

    // Se ejecuta si la descarga fue exitosa
    private void OnDataLoadSuccess(string jsonText)
    {
        Debug.Log("Datos descargados correctamente. Procesando...");

        try
        {
            // 1. Convertir el texto JSON en objetos C#
            GameDataCollection data = JsonUtility.FromJson<GameDataCollection>(jsonText);

            // 2. Guardar en nuestras listas
            allPests = data.pests;
            allCalls = data.calls;
            
            IsDataLoaded = true;
            Debug.Log($"¡Éxito! Cargadas {allPests.Count} plagas y {allCalls.Count} llamadas. Temática: {data.themeName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al leer el JSON: {e.Message}");
        }
    }

    // Se ejecuta si la descarga falló
    private void OnDataLoadError(string error)
    {
        Debug.LogError($"Error crítico descargando datos: {error}");
    }

    // --- MÉTODOS PÚBLICOS PARA EL PRESENTER ---

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
}   