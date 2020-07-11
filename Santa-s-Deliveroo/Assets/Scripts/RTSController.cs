using System.Collections.Generic;
using UnityEngine;

public class RTSController : Singleton<RTSController>
{
    private UnitRTS _currentUnit;
    private bool _isTacticalView;
    private IOutlineable _selectedObj;
    private GameObject _hittedObj;

    [Header("Cameras")] [SerializeField] private Camera rtsCamera;
    [SerializeField] private FlyCam flyCamera;

    [Header("Player units")] [SerializeField]
    private List<UnitRTS> availableUnits = new List<UnitRTS>();

    public List<UnitRTS> AvailableUnits => availableUnits;

    public Material WaypointPathMaterial => waypointPathMaterial;

    public UnitRTS CurrentUnit => _currentUnit;

    public GameObject HittedObj => _hittedObj;

    [SerializeField] private Material waypointPathMaterial;

    private void Start()
    {
        GameManager.Instance.onGameStateChanged.AddListener(HandleGameStateChanged);

        if (availableUnits.Count == 0)
        {
            foreach (var o in FindObjectsOfType(typeof(UnitRTS)))
            {
                var unit = (UnitRTS) o;
                availableUnits.Add(unit);
            }

            if (availableUnits.Count == 0)
                Debug.LogWarning("The current scene is missing units");
        }

        foreach (var unit in AvailableUnits)
        {
            unit.onBeingCaptured.AddListener(HandleUnitCaptured);
        }
    }

    private void HandleGameStateChanged(GameManager.GameState arg0, GameManager.GameState arg1)
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.Paused)
        {
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.UpdateCursor(true);
        }

        else if (GameManager.Instance.currentGameState == GameManager.GameState.Running)
        {
            if (_isTacticalView)
                Cursor.lockState = CursorLockMode.None;
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.EndGame)
            return;

        if (Input.GetKeyUp(KeyCode.Space))
            ManageView();

        if (!_isTacticalView)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000,
                GameManager.Instance.SelectionLayer))
            {
                _hittedObj = rayHit.collider.gameObject;

                var newSelectedObj = HittedObj.GetComponent<IOutlineable>();

                _selectedObj = HittedObj.GetComponent<IOutlineable>();

                if (_selectedObj != null && _selectedObj != newSelectedObj)
                    _selectedObj.Selection.SetActive(false);

                if (HittedObj.CompareTag("UnitRTS"))
                {
                    var selectedPlayer = HittedObj.GetComponent<UnitRTS>();
                    if (selectedPlayer != null && CurrentUnit != selectedPlayer)
                        _currentUnit = selectedPlayer;

                    else
                        _currentUnit = null;
                }

                if (_selectedObj != null)
                {
                    OutlineEffect(_selectedObj);

                    if (_selectedObj.Selection.activeInHierarchy)
                        AudioManager.Instance.PlayAudio(AudioManager.Instance.selectionClip);
                    else
                    {
                        AudioManager.Instance.PlayAudio(AudioManager.Instance.deselection);
                        _selectedObj = null;
                    }
                }
            }

            else
            {
                if (_selectedObj != null)
                {
                    _selectedObj.ManageSelection();
                    _selectedObj = null;

                    if (HittedObj != null)
                    {
                        if (_hittedObj == _currentUnit.gameObject)
                            _currentUnit = null;
                    }

                    AudioManager.Instance.PlayAudio(AudioManager.Instance.deselection);
                }
            }
        }

        else if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000))
                    CurrentUnit.AddWaypoint(rayHit.point);
            }
        }

        else if (Input.GetMouseButtonDown(1))
        {
            if (!CurrentUnit) return; // Check if a player unit is selected 

            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000))
                CurrentUnit.SetTargetPosition(rayHit.point);

            AudioManager.Instance.PlayAudio(AudioManager.Instance.newDestination);
        }
    }

    private static void OutlineEffect(IOutlineable go)
    {
        go.ManageSelection();
    }

    private void ManageView()
    {
        if (!flyCamera || !rtsCamera)
        {
            Debug.LogWarning("You have to assign flyCamera or rtsCamera in the inspector");
            return;
        }

        _isTacticalView = !_isTacticalView;

        flyCamera.gameObject.SetActive(!_isTacticalView);
        rtsCamera.gameObject.SetActive(_isTacticalView);
        Cursor.lockState = _isTacticalView ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleUnitCaptured(UnitRTS unit)
    {
        if (unit == _currentUnit)
            _currentUnit = null;

        AvailableUnits.Remove(unit);
        Debug.Log("<color=red>" + "You loose a unit!" + "</color>");

        if (AvailableUnits.Count <= 0)
            GameManager.Instance.GameOver();
    }
}