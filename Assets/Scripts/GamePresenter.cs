using UnityEngine;

/// <summary>
/// Basic Unity script for GamePresenter.
/// Attach this to a GameObject to implement presentation/gameflow logic.
/// </summary>
public class GamePresenter 
{
     private GameView view;

     public GamePresenter(GameView view)
     {
          this.view = view;
     }
     
     // Start is called before the first frame update
     private void Start()
     {
          // Initialization that may rely on other objects
     }

     // Update is called once per frame
     private void Update()
     {
          // Per-frame logic goes here
     }
}