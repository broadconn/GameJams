﻿using UnityEngine;

namespace GameStates {
    public class NormalGameplayProcessor : GameplayProcessor {
        public NormalGameplayProcessor(GameplayStateContext ctx) : base(ctx) { }

        public override void OnEnterState() {
            // if this remains unused consider composition over inheritance
        }

        public override void Update() {
            // if this remains unused consider composition over inheritance

            // debug
            if (Input.GetKey(KeyCode.RightArrow)) {
                Ctx.PathController.StepSearch();
                Ctx.PathController.DrawAllDebug();
            }
        }

        public override void OnExitState() {
            // if this remains unused consider composition over inheritance
        }
    }
}