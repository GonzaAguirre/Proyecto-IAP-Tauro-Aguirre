using UnityEngine;

public class APITestLoader : MonoBehaviour
{
    public string jsonUrl = "https://gist.githubusercontent.com/GonzaAguirre/fb4553ed38196a068e57d5160b568a8b/raw/f1a3b05f15d655458bf32c15fad6887ac18805a6/tematica1_urbanas.json"; 

    void Start()
    {
        // 1. Crear los componentes
        GameObject managerGO = new GameObject("DataManager");
        DataManager dataManager = managerGO.AddComponent<DataManager>();
        DataService dataService = new DataService();

        // 2. Inicializar
        dataManager.Initialize(dataService);

        // 3. Cargar
        dataManager.LoadAllData(jsonUrl);
    }
}