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
        +TextAsset español
        +TextAsset ingles
        +TextAsset portugues
    }

    %% --- CORE LOGIC ---
    class GamePresenter {
        <<Logic>>
        -IGameView view
        -DataManager dataManager
        -List~PestData~ allPlagues
        -List~CallData~ dailyCalls
        -CallData currentCall
        -string selectedPlagueId
        -int currentCallIndex
        -int currentDay
        +GamePresenter(IGameView view, DataManager model)
        +StartGame()
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
        -List~PestData~ allPests
        -List~CallData~ allCalls
        -DataService dataService
        +bool IsDataLoaded
        +event Action OnDataReady
        +Initialize()
        -Start()
        +LoadLocalData()
        +GetAllPests() 
        +GetFirstCall() 
        +GetPestByID(string id) 
        +GetCallsForDay(int day)
        +GetCallByID(string id) 
        +RequestImage()
    }

    class DataService {
        <<Helper>>
        +FetchJsonFromURL()
        +DownloadImage()
    }

    class GameManager {
        <<MonoBehaviour>>
        +DataManager dataManager
        +GameView gameView
        +GameObject panelDeJuego
        +ArchivosPorIdioma archivosArgentina
        +ArchivosPorIdioma archivosUrbanas
        +ArchivosPorIdioma archivosEspacio
        -GamePresenter presenter
        +IniciarJuego(string tematica, string idioma)
    }

    %% --- VIEW / UI ---
    class IGameView {
        <<Interface>>
        +UpdateCallerInfo()
        +UpdateEntryInfo()
        +NewCallPopUp()
        +PopulateEntriesList()
        +ShowFeedback()
        +SetUnlockedTypes()
        +StartCoroutine()
        +event Action OnSubmitAnswer
        +event Action~string~ OnPlagueSelected
        +event Action OnCallAnswered
    }

    class GameView {
        <<MonoBehaviour>>
        -EntriesListView entriesListView
        -NewCallPopupView popupView
        -GamePresenter presenter
        -Start()
        +PopulateEntriesList()
        +SetUnlockedTypes()
        +NewCallPopUp()
        -HandleCallAnswered()
        +UpdateCallerInfo()
        +UpdateEntryInfo()
        +ShowFeedback()
        -HideFeedback()
        -PlayCallAudio()
    }

    class EntriesListView {
        <<MonoBehaviour>>
        +event Action~string~ OnPlagueClicked
        +SetUnlockedTypes(List~string~ types)
        +PopulateList(List~PestData~ plagues)
    }

    class NewCallPopupView {
        <<MonoBehaviour>>
        +event Action OnCallAnswered
        +Show(callerName, image)
        -AnswerCall()
        +Hide()
    }

    class MainMenu {
        <<MonoBehaviour>>
        +GameManager gameManager
        +GameObject panelMenu
        +Button btnIniciar
        +Toggle toggleArgentina
        +Toggle toggleUrbanas
        +Toggle toggleEspacio
        +Toggle toggleES
        +Toggle toggleEN
        +Toggle togglePT
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
    GameManager --> GamePresenter : Creates & Owns
    GameManager ..> ArchivosPorIdioma : Uses

    GamePresenter --> IGameView : Controls via Interface
    GamePresenter --> DataManager : Fetches Data
    GamePresenter ..> PestData : Uses
    GamePresenter ..> CallData : Uses

    DataManager --> DataService : Uses for Downloads
    DataManager ..> GameDataCollection : Deserializes

    GameView --> GamePresenter : Has Field (Unused)
    GameView *-- EntriesListView : Manages
    GameView *-- NewCallPopupView : Manages

    EntriesListView ..> PestData : Displays
