---
config:
  layout: elk
---

classDiagram
%% --- DATA STRUCTURES ---
    class PestData {
        <<Serializable>>
        +string id
        +string name
        +string imageURL
        +string description
        +string danger
        +string solution
        +string type
    }

    class CallData {
        <<Serializable>>
        +string id
        +string callerName
        +string callerImageURL
        +string message
        +string correctPestID
        +int day
        +string callType
        +string audio
    }

    class GameDataCollection {
        <<Serializable>>
        +string themeName
        +List~PestData~ pests
        +List~CallData~ calls
    }

    class ArchivosPorIdioma {
        <<Serializable>>
    }

    %% --- CORE LOGIC ---
    class GamePresenter {
        <<Logic>>
        +IGameView view
        +DataManager dataManager
        +List~PestData~ allPlagues
        +List~CallData~ dailyCalls
        +CallData currentCall
        +string selectedPlagueId
        +int currentCallIndex
        +int currentDay
        -GamePresenter(IGameView v, DataManager m)
        -StartGame()
        -LoadDayData()
        -LoadCallByIndex(int index)
        -StartNextDay()
        -HandlePlagueSelection(string plagueId)
        -HandleSubmit()
        -AdvanceToNextCall()
        -ShuffleCalls(List~CallData~ calls)
        -WaitAndLoadCall(float delay)
    }

    class DataManager {
        <<MonoBehaviour>>
        +TextAsset jsonFile
        +List~PestData~ allPests
        +List~CallData~ allCalls
        +DataService dataService
        +bool IsDataLoaded
        +event Action OnDataReady
        -Initialize()
        -Start()
        -LoadLocalData()
        -GetAllPests() 
        -GetFirstCall() 
        -GetPestByID(string id) 
        -GetCallsForDay(int day)
        -GetCallByID(string id) 
        -RequestImage()
    }

    class DataService {
        <<Helper>>
        -FetchJsonFromURL()
        -DownloadImage()
    }

    class GameManager {
        +DataManager dataManager
        +GameView gameView
        +GameObject panelDeJuego
        +ArchivosPorIdioma archivos
        -IniciarJuego()
    }

    %% --- VIEW / UI ---
    class IGameView {
        <<Interface>>
        -UpdateCallerInfo()
        -UpdateEntryInfo()
        -NewCallPopUp()
        -PopulateEntriesList()
        -ShowFeedback()
        -SetUnlockedTypes()
    }

    class GameView {
        <<MonoBehaviour>>
        +EntriesListView entriesListView
        +NewCallPopupView popupView
        +DataManager dataManager
        +GamePresenter presenter
        -Start()
        -IniciarJuegoManual()
        -PopulateEntriesList()
        -SetUnlockedTypes()
        -NewCallPopUp()
        -HandleCallAnswered()
        -UpdateCallerInfo()
        -UpdateEntryInfo()
        -Show/HideFeedback()
        -PlayCallAudio()
    }

    class EntriesListView {
        <<MonoBehaviour>>
        -SetUnlockedTypes(List~string~ types)
        -PopulateList(List~PestData~ plagues)
    }

    class NewCallPopupView {
        <<MonoBehaviour>>
        +event Action OnCallAnswered
        -Show(callerName, image)
        -AnswerCall()
        -Hide()
    }

    class MainMenu {
        <<MonoBehaviour>>
        +GameManager gameManager
        +GameObject panelMenu
        +Button
        +Toggles
        -Start()
        -IniciarConSeleccion()
    }

    %% --- RELATIONSHIPS ---

    %% Inheritance / Implementation
    GameView ..|> IGameView : Implements

    %% Composition / Aggregation
    GameDataCollection *-- PestData : Contains List
    GameDataCollection *-- CallData : Contains List

    %% Dependencies & Associations
    MainMenu --> GameManager : Calls IniciarJuego
    GameManager --> DataManager : Configures & Loads
    GameManager --> GameView : Initializes
    GameManager ..> ArchivosPorIdioma : Uses

    GamePresenter --> IGameView : Controls via Interface
    GamePresenter --> DataManager : Fetches Data
    GamePresenter ..> PestData : Uses
    GamePresenter ..> CallData : Uses

    DataManager --> DataService : Uses for Downloads
    DataManager ..> GameDataCollection : Deserializes

    GameView --> GamePresenter : Owns & Creates
    GameView *-- EntriesListView : Manages
    GameView *-- NewCallPopupView : Manages

    EntriesListView ..> PestData : Displays
