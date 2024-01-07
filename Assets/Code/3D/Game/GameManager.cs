using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance =>
        _instance ? _instance : new GameObject("Game Manager").AddComponent<GameManager>();

    [SerializeField] private MarkerSelection _xSelection;
    [SerializeField] private MarkerSelection _oSelection;

    private int _rows;
    private int _turn;
    private int _match = 3;
    private bool _gameEnd = false;
    private bool _devMode = true;
    private Board _board;
    private Dictionary<string, HitBox> _fields = new Dictionary<string, HitBox>();
    private List<HitBox> _matchedPattern = new List<HitBox>();

    public int Turn => _turn % 2;
    public int Rows => _rows;
    public int Match => _match;
    public bool GameEnd => _gameEnd;

    public bool DevMode => _devMode;
    public Board Board => _board;
    public List<HitBox> Pattern => _matchedPattern;
    /*
        public Marker GetSelectedMarker => Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;*/


    public Marker GetSelectedMarker
    {
        get
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == "2dGame")
            {

                return Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;
            }
            else if (currentScene.name == "3dGame")
            {
                return Turn == 0 ? _xSelection.SelectedMarker : _oSelection.SelectedMarker;
            }

            // Standardfall, sollte nicht erreicht werden
            return null;
        }
    }

    private int _maxMoves => _rows * _rows;

    public event Action<bool, int> OnGameEnd;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _devMode = false;
        InitializeGame();
    }

    private void InitializeGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "2dGame")
        {
            // Initialisierung für das normale TicTacToe
            _xSelection.SetTurn(0);
            _oSelection.SetTurn(1);
        }
        else if (currentScene.name == "3dGame")
        {
            // Initialisierung für das Gobblet TicTacToe
            // Hier könntest du zusätzliche Initialisierungen für das Gobblet TicTacToe vornehmen
            _xSelection.SetTurn(0);
            _oSelection.SetTurn(1);
        }
    }

    public void Set(int rows, int match = 3)
    {
        _rows = rows;
        _match = match;
    }

    public void AddHitBox(HitBox hitBox, int x, int y)
    {
        // store hit box with x y row as key
        _fields.Add($"{x},{y}", hitBox);

        hitBox.Initialize(this, _xSelection, _oSelection);
    }

    public void Clear()
    {
        _fields.Clear();
        _matchedPattern?.Clear();
        _gameEnd = false;
        _turn = 0;
        OnGameEnd?.Invoke(_gameEnd, -1);
    }

    public void MoveMade()
    {
        _turn++;

        _matchedPattern = PatternFinder.CheckWin(_fields);
        if (_matchedPattern != null)
        {
            // WINNER
            _gameEnd = true;
            OnGameEnd?.Invoke(_gameEnd, _matchedPattern[0].Type);
        }
/*        else if (_turn >= _maxMoves)
        {
            // TIE
            _gameEnd = true;
            OnGameEnd?.Invoke(_gameEnd, -1);
        }*/

        _xSelection.UpdateMarkers();
        _oSelection.UpdateMarkers();

/*        PrintDictionaryContents();*/
        GetBoardStateAsJson();
    }
    public bool ToggleDevMode()
    {
        _devMode = !_devMode;
        return _devMode;
    }
    public void SetBoard(Board board)
    {
        _board = board;
    }

    public void Reset()
    {
        _board.Reset();
        _xSelection.Reset();
        _oSelection.Reset();
    }

    public string GetBoardStateAsJson()
    {
        if (_board != null)
        {
            List<List<Dictionary<string, object>>> boardState = new List<List<Dictionary<string, object>>>();

            for (int i = 0; i < 3; i++) // Assuming 3 rows
            {
                List<Dictionary<string, object>> row = new List<Dictionary<string, object>>();

                for (int j = 0; j < 3; j++) // Assuming 3 columns
                {
                    HitBox hitBox;
                    if (_fields.TryGetValue($"{i},{j}", out hitBox))
                    {
                        Dictionary<string, object> cell = new Dictionary<string, object>
                    {
                        { "type", hitBox.Type == 0 ? "X" : hitBox.Type == 1 ? "O" : "-1" },
                        { "size", hitBox.GetMarkerSize()}
                    };
                        row.Add(cell);
                    }
                    else
                    {
                        row.Add(new Dictionary<string, object> { { "type", " " }, { "size", 0 } });
                    }
                }
                boardState.Add(row);
            }

            // Convert the list to JSON using Newtonsoft.Json
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { board = boardState });

            Debug.Log(json);

            return json;
        }

        return null;
    }


    public void PrintDictionaryContents()
    {
        foreach (var kvp in _fields)
        {
            string key = kvp.Key;
            HitBox value = kvp.Value;
            Debug.Log($"Key: {key}, Value: {value}");
        }
    }


}