using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameStateController))]
public class GameController : MonoBehaviour
{
    private GameStateController state;

    private void Start() {
        state = GetComponent<GameStateController>();
        state.Model = new GameModel();
        state.ChangeState(GameStateID.Loading);
    }
}
