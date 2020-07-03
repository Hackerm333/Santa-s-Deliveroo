using UnityEngine;
using UnityEngine.UI;

public class GameStats : Singleton<GameStats>
{
    [SerializeField] private Text unitsAvailable;
    [SerializeField] private Text itemsToDeliver;
    [SerializeField] private Text itemsDelivered;

    private void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameManager.GameState arg0, GameManager.GameState arg1)
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Running)
            UpdateGameStatsUi();    
    }

    public void UpdateGameStatsUi()
    {
        unitsAvailable.text = "Units available: " + "<color=red>" + RTSController.Instance.AvailableUnits.Count + "</color>";
        itemsToDeliver.text = "Gifts to deliver: " + "<color=red>" + GameManager.Instance.MinItemsToDeliver + "</color>";
        itemsDelivered.text = "Gifts delivered: " + "<color=red>" + GameManager.Instance.ItemsDelivered + "</color>";
    }
}