using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Pregame,
        Running,
        Paused,
        EndGame
    }

    public GameObject[] systemPrefabs;
    public Events.EventGameState onGameStateChanged;

    private List<GameObject> _instancedSystemPrefabs;
    private List<AsyncOperation> _loadOperations = new List<AsyncOperation>();
    private GameState _currentGameState = GameState.Pregame;

    private string _currentLevelName = string.Empty;

    [SerializeField] private LayerMask selectionLayer;

    public LayerMask SelectionLayer => selectionLayer;

    [SerializeField] private int minItemsToDeliver;

    private UnitRTS _lastUnitCaptured;

    public GameState currentGameState
    {
        get => _currentGameState;
        private set { _currentGameState = value; }
    }

    public int MinItemsToDeliver => minItemsToDeliver;

    public int ItemsDelivered => itemsDelivered;

    // Cursor
    [SerializeField] private Texture2D cursorOverSelectable;
    [SerializeField] private int itemsDelivered;

    public void UpdateItems()
    {
        itemsDelivered = ItemsDelivered + 1;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _instancedSystemPrefabs = new List<GameObject>();
        _loadOperations = new List<AsyncOperation>();
        InstantiateSystemPrefabs();
        UIManager.Instance.onMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
    }

    private void SetupLevel()
    {
        Timer.StartTimer();
        Timer.OnTimerEnded.AddListener(HandleTimerEnded);
        AudioManager.Instance.PlayMusic();
    }

    private void HandleTimerEnded()
    {
        UpdateState(GameState.EndGame);
        if (ItemsDelivered < MinItemsToDeliver)
            GameOver();

        UIManager.Instance.EndGame(itemsDelivered > minItemsToDeliver);
    }

    public void GameOver()
    {
        Debug.Log("<color=red>" + "YOU LOOSE!" + "</color>");
        UpdateState(GameState.EndGame);
    }

    private void HandleMainMenuFadeComplete(bool fadeOut)
    {
        if (!fadeOut)
            UnloadLevel(_currentLevelName);
    }

    private void Update()
    {
        if (currentGameState == GameState.Pregame)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnLoadOperationComplete(AsyncOperation ao)
    {
        if (_loadOperations.Contains(ao))
            _loadOperations.Remove(ao);

        if (_loadOperations.Count == 0)
        {
            UpdateState(GameState.Running);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Main"));
        }
    }

    private void UnloadOperationComplete(AsyncOperation obj)
    {
        Debug.Log("Unload Complete.");
    }

    private void UpdateState(GameState state)
    {
        var previousGameState = _currentGameState;
        _currentGameState = state;

        switch (_currentGameState)
        {
            case GameState.Pregame:
                Time.timeScale = 1.0f;
                break;

            case GameState.Running:
                Time.timeScale = 1.0f;
                if (previousGameState == GameState.Pregame && currentGameState == GameState.Running)
                    SetupLevel();
                break;

            case GameState.Paused:
                Time.timeScale = 0.0f;
                break;
            
            case GameState.EndGame:
                UpdateState(GameState.Paused);
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
        }

        onGameStateChanged.Invoke(_currentGameState, previousGameState);
        Debug.LogWarning("GameState changed to " + currentGameState);
    }

    private void InstantiateSystemPrefabs()
    {
        GameObject prefabInstance;
        for (int i = 0; i < systemPrefabs.Length; i++)
        {
            prefabInstance = Instantiate(systemPrefabs[i]);
            _instancedSystemPrefabs.Add(prefabInstance);
        }
    }

    public void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        ao.completed += OnLoadOperationComplete;
        _currentLevelName = levelName;
    }

    public void UnloadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }

        ao.completed += UnloadOperationComplete;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        for (int i = 0; i < _instancedSystemPrefabs.Count; ++i)
        {
            Destroy(_instancedSystemPrefabs[i]);
        }

        _instancedSystemPrefabs.Clear();
    }

    public void StartGame()
    {
        LoadLevel("Main");
    }

    public void TogglePause()
    {
        UpdateState(_currentGameState == GameState.Running ? GameState.Paused : GameState.Running);
    }

    public void RestartGame()
    {
        UnloadLevel("Main");
        AudioManager.Instance.StopSounds();
        UpdateState(GameState.Pregame);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void UpdateCursor(bool defaultCursor)
    {
        if (!defaultCursor)
            Cursor.SetCursor(cursorOverSelectable, Vector2.zero, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}