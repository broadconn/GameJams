using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using GameStates;
using UnityEngine;

public class GameController : MonoBehaviour {
    private static GameController _instance;
    public static GameController Instance => _instance;

    [SerializeField] private GridHighlighter gridHighlighter; 
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform enemySpawnTransform;
    [SerializeField] private Transform enemyGoalTransform;
    [SerializeField] private Transform debugPathDotsParent;

    private GameplayProcessor _gameplayProcessor;
    private Dictionary<GameplayState, GameplayProcessor> _gameplayStateToProcessorMappings;

    public Vector3 EnemySpawnPos => enemySpawnTransform.position;
    public Vector3 EnemyGoalPos => enemyGoalTransform.position;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }
            
        var ctx = new GameplayStateContext {
            MainCamera = Camera.main,
            GridHighlighter = gridHighlighter,
            PathController = new PathController(enemySpawnTransform.position.WorldXZToV2Int(), enemyGoalTransform.position.WorldXZToV2Int(), debugPathDotsParent),
            CameraController = cameraController
        };
        _gameplayStateToProcessorMappings = new Dictionary<GameplayState, GameplayProcessor> {
            { GameplayState.Normal, new NormalGameplayProcessor(ctx) },
            { GameplayState.PlacingBuilding, new PlacingBuildingProcessorProcessor(ctx) }
        };
    }

    private void Start() {
        SetGameplayState(GameplayState.Normal);
    }

    private void Update() {
        cameraController.DoUpdate();
        _gameplayProcessor.Update();
        
        if (_gameplayProcessor.StateChanged) 
            SetGameplayState(_gameplayProcessor.NextState); 
    }

    private void SetGameplayState(GameplayState gameplayState, GameObject referenceObject = null) {
        _gameplayProcessor?.OnExitState();
        
        _gameplayProcessor = _gameplayStateToProcessorMappings[gameplayState]; // TODO: check mapping exists
        if(_gameplayProcessor is IGameplayProcessorReferencingGameObject processor)
            processor.SetReferenceObject(referenceObject);
        
        _gameplayProcessor.OnEnterState();
    }

    public void ClickedBuildBuildingButton(GameObject buildingPrefab) {
        // TODO: check if the player can afford this building
        SetGameplayState(GameplayState.PlacingBuilding, buildingPrefab);
    }

    public void OnEnemyReachedGoal()
    {
        
    }

    private void OnDestroy() { if (this == _instance) { _instance = null; } }
}

public static class UtilityExtensions {
    public static Vector2Int WorldXZToV2Int(this Vector3 pos) {
        return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
    }
}