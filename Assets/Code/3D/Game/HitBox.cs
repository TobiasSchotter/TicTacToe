using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;

    private int _type = -1;
    public int Type => _type;

    private bool _markerPlaced;
    private readonly List<Marker> _markers = new List<Marker>();
    private Marker CurrentMarker => _markers.Count > 0 ? _markers.Last() : null;
    private GameManager _gameManager; // Reference to the GameManager

    private MarkerSelection _xSelection;
    private MarkerSelection _oSelection;

    private void Start()
    {
        _renderer.enabled = false;
    }

    public void Initialize(GameManager gameManager, MarkerSelection xSelection, MarkerSelection oSelection)
    {
        _gameManager = gameManager;
        _xSelection = xSelection;
        _oSelection = oSelection;
    }


    private bool CheckAvailableToTrigger()
    {
        if (GameManager.Instance.GameEnd || GameManager.Instance.DevMode ||
            GameManager.Instance.GetSelectedMarker == null)
        {
            return false;
        }

        if (_markerPlaced && GameManager.Instance.GetSelectedMarker != null && CurrentMarker != null)
        {
            return GameManager.Instance.GetSelectedMarker.Size > CurrentMarker.Size;
        }

        return true;
    }

    private void OnMouseOver()
    {
        if (!CheckAvailableToTrigger())
        {
            return;
        }

        _renderer.enabled = true;
    }

    private void OnMouseExit()
    {
        _renderer.enabled = false;
    }


    public void LogMarkersPool(List<Marker> _markersPool)
    {
        foreach (var marker in _markersPool)
        {
            Debug.Log($"Marker in pool: {marker}");
        }
    }

    private void MakeMove(Marker marker)
    {
        //TODO check if marker of its size exists in MarkerPool
        if (CurrentMarker != null)
        {
            CurrentMarker.OverRuled(true);

            CurrentMarker.Remove();
        }

        marker.SetPosition(transform.position, transform, this);
        _markers.Add(marker);

        _renderer.enabled = false;
        _markerPlaced = true;
        marker.SetIsPlaced(true);
        _type = GameManager.Instance.Turn;

        if (_type == 0)
        {
            Debug.Log("Marker in pool X");
            _xSelection.MarkersPoolX.Remove(marker);
            LogMarkersPool(_xSelection.MarkersPoolX);
        }
        else if (_type == 1)
        {
            Debug.Log("Marker in pool O");
            _oSelection.MarkersPoolO.Remove(marker);
            LogMarkersPool(_oSelection.MarkersPoolO);
        }

        GameManager.Instance.MoveMade();
    }

    private void OnMouseUpAsButton()
    {
        if (!CheckAvailableToTrigger())
        {
            return;
        }

        var marker = GameManager.Instance.GetSelectedMarker;
        if (marker == null)
        {
            return;
        }

        MakeMove(marker);
    }

    public void RemoveMarker(Marker marker)
    {
        _markers.Remove(marker);

        if (CurrentMarker == null)
        {
            _type = -1;
            return;
        }

        _type = CurrentMarker.Type;
    }

    public void PrintMarkers()
    {
        foreach (var marker in _markers)
        {
            Debug.Log($"Marker Type: {marker.Type}, Size: {marker.Size}");
        }
    }

    public int GetMarkerSize()
    {
        if (_markerPlaced)
        {
            return CurrentMarker.Size;
        }
        return -1;
    }
}