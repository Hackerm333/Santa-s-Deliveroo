using System.Collections.Generic;
using UnityEngine;

public class RTSController : Singleton<RTSController>
{
    private UnitRTS _currentUnit;
    private bool _isTacticalView;
    private IOutlineable _currentSelectedObj;

    [Header("Cameras")] [SerializeField] private Camera rtsCamera;
    [SerializeField] private FlyCam flyCamera;

    [Header("Player units")] [SerializeField]
    private List<UnitRTS> availableUnits = new List<UnitRTS>();

    public List<UnitRTS> AvailableUnits => availableUnits;

    public Material WaypointPathMaterial => waypointPathMaterial;

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
            #region SELECTION

            // Check if the ray hits an object in the proper layer
            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000,
                GameManager.Instance.SelectionLayer))
            {
                // Check if the object has a component which implements the IOutlineable interface
                // which is needed for selection/deselection
                if (!rayHit.collider.gameObject.TryGetComponent(out IOutlineable outlineableHit))
                    return;

                // Check if we previously selected an object
                if (_currentSelectedObj != null)
                {
                    SetOutlineEffect(_currentSelectedObj);
                    if (outlineableHit == _currentSelectedObj)
                    {
                        Deselect();
                        return;
                    }
                }

                var objHit = rayHit.collider.gameObject;
                _currentSelectedObj = outlineableHit;

                if (objHit.CompareTag("UnitRTS") && objHit.TryGetComponent(out UnitRTS unitHit))
                    _currentUnit = unitHit;

                SetOutlineEffect(_currentSelectedObj);

                AudioManager.Instance.PlayAudio(_currentSelectedObj.Selection.activeInHierarchy
                    ? AudioManager.Instance.selectionClip
                    : AudioManager.Instance.deselection);
            }

            #endregion

            #region DESELECTION

            // We clicked on a non selectable object
            else
            {
                _currentSelectedObj?.ManageOutlineEffect();
                Deselect();
            }

            #endregion
        }

        else if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000))
                    _currentUnit.AddWaypoint(rayHit.point);
            }
        }

        else if (Input.GetMouseButtonDown(1))
        {
            if (!_currentUnit) return; // Check if a player unit is selected 

            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000))
                _currentUnit.SetTargetPosition(rayHit.point);

            AudioManager.Instance.PlayAudio(AudioManager.Instance.newDestination);
        }
    }

    private void Deselect()
    {
        _currentSelectedObj = null;
        _currentUnit = null;
        AudioManager.Instance.PlayAudio(AudioManager.Instance.deselection);
    }

    private static void SetOutlineEffect(IOutlineable go)
    {
        go.ManageOutlineEffect();
    }

    private void ManageView()
    {
        if (!flyCamera || !rtsCamera)
            return;

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

        if (AvailableUnits.Count <= 0)
            GameManager.Instance.GameOver();
    }
}