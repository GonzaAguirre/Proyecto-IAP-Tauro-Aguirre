using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    [Header("Archivo JSON Local")]
    public TextAsset jsonFile;

    private List<PestData> allPests = new List<PestData>();
    private List<CallData> allCalls = new List<CallData>();
    private DataService dataService = new DataService();

    public bool IsDataLoaded { get; private set; } = false;
    public event System.Action OnDataReady;

    public void Initialize()
    {
        if (jsonFile != null) LoadLocalData();
    }

    void Start()
    {

    }

    public void LoadLocalData(TextAsset customJsonFile = null)
    {
        Debug.Log("Cargando datos desde archivo local...");

        try
        {
            TextAsset activeJsonFile = customJsonFile != null ? customJsonFile : jsonFile;
            
            if (activeJsonFile == null)
            {
                Debug.LogError("El archivo JSON es NULL en el Inspector.");
                return;
            }

            string jsonText = activeJsonFile.text;

            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError("El archivo JSON está vacío.");
                return;
            }

            Debug.Log("Contenido del JSON:");
            Debug.Log(jsonText); 

            GameDataCollection data = JsonUtility.FromJson<GameDataCollection>(jsonText);

            if (data != null)
            {
                allPests = data.pests ?? new List<PestData>();
                allCalls = data.calls ?? new List<CallData>();

                IsDataLoaded = true;

                Debug.Log($"✅ ÉXITO LOCAL: Cargadas {allPests.Count} plagas y {allCalls.Count} llamadas.");
                Debug.Log($"Temática: {data.themeName}");

                Debug.Log("Invocando OnDataReady...");
                OnDataReady?.Invoke();
            }
            else
            {
                Debug.LogError("El JSON parece estar vacío o mal formado (data es null). Verifica el formato del archivo.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error crítico leyendo el JSON local: {e.Message}");
        }
    }

    public List<PestData> GetAllPests() => allPests;

    public CallData GetFirstCall() => allCalls.Count > 0 ? allCalls[0] : null;

    public PestData GetPestByID(string id)
    {
        return allPests.FirstOrDefault(p => p.id == id);
    }

    public List<CallData> GetCallsForDay(int day)
    {
        return allCalls.Where(c => c.day == day).ToList();
    }

    public CallData GetCallByID(string id)
    {
        return allCalls.FirstOrDefault(c => c.id == id);
    }

    public void RequestImage(string url, System.Action<Sprite> callback)
    {
        if (url.StartsWith("http") || url.StartsWith("https"))
        {
            StartCoroutine(dataService.DownloadImage(url, callback));
        }
        else
        {
            Sprite localSprite = Resources.Load<Sprite>(url);

            if (localSprite != null)
            {
                callback?.Invoke(localSprite);
            }
            else
            {
                Debug.LogError($"❌ No se encontró la imagen en Resources: {url}");
                callback?.Invoke(null);
            }
        }
    }
}