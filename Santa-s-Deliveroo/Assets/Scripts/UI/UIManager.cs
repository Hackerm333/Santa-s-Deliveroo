using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private GameObject gameStats;
    [SerializeField] private Camera dummyCamera;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Text endGameText;

    public Events.EventFadeComplete onMainMenuFadeComplete;

    private void Start()
    {
        mainMenu.onMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
        GameManager.Instance.onGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleMainMenuFadeComplete(bool fadeOut)
    {
        onMainMenuFadeComplete.Invoke(fadeOut);
    }

    private void HandleGameStateChanged(GameManager.GameState currentState, GameManager.GameState previousState)
    {
        pauseMenu.gameObject.SetActive(currentState == GameManager.GameState.Paused);
        gameStats.gameObject.SetActive(currentState != GameManager.GameState.Pregame);
        endGamePanel.gameObject.SetActive(currentState == GameManager.GameState.Paused && previousState == GameManager.GameState.EndGame);
    }

    private void Update()
    {
        if (GameManager.Instance.currentGameState != GameManager.GameState.Pregame)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            GameManager.Instance.StartGame();
    }

    public void SetDummyCameraActive(bool active)
    {
        dummyCamera.gameObject.SetActive(active);
    }

    public void EndGame(bool win)
    {
        if (endGamePanel)
            endGamePanel.SetActive(true);

        if (endGameText)
        {
            var winString = win ? "WIN!" : "LOOSE!";
            endGameText.text = "YOU " + winString;
        }
    }
}