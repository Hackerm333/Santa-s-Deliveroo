﻿using System.Collections.Generic;
using UnityEngine;

public class RTSController : Singleton<RTSController>
{
    private UnitRTS _currentUnit;
    private IOutlineable _currentSelected;

    [Header("Cameras")] [SerializeField] private Camera rtsCamera;
    [SerializeField] private FlyCam flyCamera;
    private bool _isTacticalView;

    [Header("Player units")] [SerializeField]
    private List<UnitRTS> availableUnits = new List<UnitRTS>();

    [SerializeField] private Material waypointPathMaterial;

    public List<UnitRTS> AvailableUnits => availableUnits;
    public Material WaypointPathMaterial => waypointPathMaterial;

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
            unit.onBeingCaptured.AddListener(HandleUnitCaptured);
    }

    private void HandleGameStateChanged(GameManager.GameState arg0, GameManager.GameState arg1)
    {
        switch (GameManager.Instance.currentGameState)
        {
            case GameManager.GameState.Paused:
                Cursor.lockState = CursorLockMode.None;
                GameManager.Instance.UpdateCursor(true);
                break;
            case GameManager.GameState.Running when _isTacticalView:
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameManager.GameState.Running:
                Cursor.lockState = CursorLockMode.Locked;
                break;
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
            #region SELECTION/DESELECTION

            // Check if the ray hits an object in the proper layer
            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000,
                GameManager.Instance.SelectionLayer))
            {
                // Check if the object has a component which implements the IOutlineable interface needed for selection/deselection
                if (!rayHit.collider.gameObject.TryGetComponent(out IOutlineable outlineableHit))
                    return;

                // Check if we previously selected an object
                if (_currentSelected != null)
                {
                    SetOutlineEffect(_currentSelected);
                    if (outlineableHit == _currentSelected)
                    {
                        Deselect();
                        return;
                    }
                }

                var objHit = rayHit.collider.gameObject;
                _currentSelected = outlineableHit;

                if (objHit.CompareTag("UnitRTS") && objHit.TryGetComponent(out UnitRTS unitHit))
                    _currentUnit = unitHit;

                else
                    _currentUnit = null;

                SetOutlineEffect(_currentSelected);

                AudioManager.Instance.PlayAudio(_currentSelected.Selection.activeInHierarchy
                    ? AudioManager.Instance.selectionClip
                    : AudioManager.Instance.deselection);
            }

            else
            {
                if (_currentSelected == null) return;
                _currentSelected?.ManageOutlineEffect();
                Deselect();
                _currentUnit = null;
            }

            #endregion
        }

        else if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!Input.GetMouseButtonUp(1)) return;
            if (Physics.Raycast(rtsCamera.ScreenPointToRay(Input.mousePosition), out var rayHit, 1000))
                _currentUnit.AddWaypoint(rayHit.point);
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
        _currentSelected = null;
        _currentUnit = null;
        AudioManager.Instance.PlayAudio(AudioManager.Instance.deselection);
    }

    private static void SetOutlineEffect(IOutlineable go) => go.ManageOutlineEffect();

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

        if ((IOutlineable) unit == _currentSelected)
            _currentSelected = null;

        AvailableUnits.Remove(unit);

        if (AvailableUnits.Count <= 0)
            GameManager.Instance.GameOver();
    }
}