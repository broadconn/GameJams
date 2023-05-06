using System;
using System.Collections;
using System.Collections.Generic;
using GameStates;
using UnityEngine;

public class GameController : MonoBehaviour {
    [SerializeField] private GridHighlighter gridHighlighter; 

    private ActionStateProcessor _actionStateProcessor;
    private Dictionary<ActionState, ActionStateProcessor> _actionStateMappings;

    private void Awake() {
        var ctx = new ActionStateContext {
            MainCamera = Camera.main,
            GridHighlighter = gridHighlighter
        };
        _actionStateMappings = new Dictionary<ActionState, ActionStateProcessor> {
            { ActionState.Normal, new NormalActionStateProcessor(ctx) },
            { ActionState.PlacingBuilding, new PlacingBuildingActionStateProcessor(ctx) }
        };
    }

    private void Start() {
        SetActionState(ActionState.Normal);
    }

    private void Update() {
        _actionStateProcessor.HandleKeyboardInput();
        _actionStateProcessor.Update();
        
        if (_actionStateProcessor.StateChanged) 
            SetActionState(_actionStateProcessor.NextState); 
    }

    private void SetActionState(ActionState actionState) {
        _actionStateProcessor = _actionStateMappings[actionState]; // TODO: check mapping exists
        _actionStateProcessor.OnEnterState();
    }

    public void ClickedBuildBuildingButton(string buildingId) {
        // TODO: check if the player can afford this building
        SetActionState(ActionState.PlacingBuilding);
    }
}
