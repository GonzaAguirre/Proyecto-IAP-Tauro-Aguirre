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
        -StartGame()
        -LoadDayData()
        -LoadCallByIndex(int index)
        -StartNextDay()
        -HandlePlagueSelection(string plagueId)
        -HandleSubmit()
        -AdvanceToNextCall()
        -ShuffleCalls(List~CallData~ calls)
        -WaitAndLoadCall(float delay) IEnumerator
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
        +GetAllPests() List~PestData~
        +GetFirstCall() CallData
        +GetPestByID(string id) PestData
        +GetCallsForDay(int day) List~CallData~
        +GetCallByID(string id) CallData
        +RequestImage(string url, Action~Sprite~ callback)
    }

    class DataService {
        <<Helper>>
        +FetchJsonFromURL(string url, Action~string~ onSuccess, Action~string~ onFailure) IEnumerator
        +DownloadImage(string url, Action~Sprite~ onSuccess) IEnumerator
    }

    class GameManager {
        <<MonoBehaviour>>
        +DataManager dataManager
        +GameView gameView
        +GameObject panelDeJuego
        +ArchivosPorIdioma archivosArgentina
        +ArchivosPorIdioma archivosUrbanas
        +ArchivosPorIdioma archivosEspacio
        +IniciarJuego(string tematica, string idioma)
    }

    %% --- VIEW / UI ---
    class IGameView {
        <<Interface>>
        +UpdateCallerInfo(string name, string message, Sprite image)
        +UpdateEntryInfo(string title, string desc, string danger, string solution, Sprite image)
        +NewCallPopUp(string callerName, Sprite callerImage, string audioPath)
        +PopulateEntriesList(List~PestData~ plagues)
        +ShowFeedback(bool isCorrect)
        +SetUnlockedTypes(List~string~ types)
        +StartCoroutine(IEnumerator routine) Coroutine
        +event Action OnSubmitAnswer
        +event Action~string~ OnPlagueSelected
        +event Action OnCallAnswered
    }

    class GameView {
        <<MonoBehaviour>>
        -EntriesListView entriesListView
        -NewCallPopupView popupView
        -TextMeshProUGUI callerNameText
        -TextMeshProUGUI callMessageText
        -Image callerImage
        -Button submitAnswerButton
        -TextMeshProUGUI entryInfoTitleText
        -TextMeshProUGUI entryInfoDescriptionText
        -TextMeshProUGUI entryInfoDangerText
        -TextMeshProUGUI entryInfoSolutionText
        -Image entryInfoImage
        -DataManager dataManager
        -AudioSource voiceAudioSource
        -Canvas feedbackCanvas
        -TextMeshProUGUI feedbackText
        -GamePresenter presenter
        -string currentAudioPath
        +event Action OnSubmitAnswer
        +event Action~string~ OnPlagueSelected
        +event Action OnCallAnswered
        -Start()
        +IniciarJuegoManual()
        +PopulateEntriesList(List~PestData~ plagues)
        +SetUnlockedTypes(List~string~ types)
        +NewCallPopUp(string callerName, Sprite callerImage, string audioPath)
        -HandleCallAnswered()
        +UpdateCallerInfo(string name, string message, Sprite image)
        +UpdateEntryInfo(string title, string desc, string danger, string solution, Sprite image)
        +ShowFeedback(bool isCorrect)
        -HideFeedback()
        -PlayCallAudio(string audioPath)
    }

    class EntriesListView {
        <<MonoBehaviour>>
        -Transform entriesContainer
        -GameObject entryButtonPrefab
        -List~string~ unlockedTypes
        +event Action~string~ OnPlagueClicked
        +SetUnlockedTypes(List~string~ types)
        +PopulateList(List~PestData~ plagues)
    }

    class NewCallPopupView {
        <<MonoBehaviour>>
        -GameObject popupCanvas
        -TextMeshProUGUI newCallText
        -Image callerImage
        -Button answerButton
        -AudioSource ringtoneSource
        -AudioClip ringtoneClip
        +event Action OnCallAnswered
        +Show(string callerName, Sprite image)
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
    GameManager ..> ArchivosPorIdioma : Uses

    GamePresenter --> IGameView : Controls via Interface
    GamePresenter --> DataManager : Fetches Data
    GamePresenter ..> PestData : Uses
    GamePresenter ..> CallData : Uses

    DataManager --> DataService : Uses for Downloads
    DataManager ..> GameDataCollection : Deserializes

    GameView --> GamePresenter : Owns & Creates
    GameView --> DataManager : Passes to Presenter
    GameView *-- EntriesListView : Manages
    GameView *-- NewCallPopupView : Manages

    EntriesListView ..> PestData : Displays
