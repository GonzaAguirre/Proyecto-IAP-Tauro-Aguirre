using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    [Header("Calling screen")]
    [SerializeField] private TextMeshProUGUI callerNameText;
    [SerializeField] private TextMeshProUGUI callMessageText;
    [SerializeField] private Image callerImage;
    [SerializeField] private Button submitAnswerButton;

    [Header("Entries (plagues) screen")]
    // HAY QUE VER COMO HACER PARA PONER LA LISTA SELECCIONABLE DE ENTRIES
    [SerializeField] private TextMeshProUGUI entryNamesText;

    [Header("Entry info screen")]
    [SerializeField] private TextMeshProUGUI entryInfoTitleText;
    [SerializeField] private TextMeshProUGUI entryInfoDescriptionText;
    [SerializeField] private TextMeshProUGUI entryInfoAdditionalInfoText;
    [SerializeField] private Image entryInfoImage;

    private GamePresenter presenter;

    void Start()
    {
        this.presenter = new GamePresenter(this);
    }

    void Update()
    {
        
    }
}
