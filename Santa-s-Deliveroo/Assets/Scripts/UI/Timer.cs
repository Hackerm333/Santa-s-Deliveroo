using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private float timeRemaining;
    private float _timeAvailable;
    [SerializeField] private Text timeText;
    private static bool _timerIsRunning;

    public static UnityEvent OnTimerEnded;

    private void Start()
    {
        _timeAvailable = timeRemaining;
        GameManager.Instance.onGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameManager.GameState previousGameState, GameManager.GameState currentGameState)
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Pregame)
            timeRemaining = _timeAvailable;
    }

    public static void StartTimer()
    {
        if (OnTimerEnded == null)
            OnTimerEnded = new UnityEvent();

        _timerIsRunning = true;
    }


    private void Update()
    {
        if (!_timerIsRunning) return;
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
        else
        {
            OnTimerEnded.Invoke();
            timeRemaining = 0;
            _timerIsRunning = false;
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}