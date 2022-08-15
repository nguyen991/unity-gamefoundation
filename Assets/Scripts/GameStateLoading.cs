using System.Collections;
using System.Collections.Generic;
using GameFoundation.State;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStateLoading", menuName = "GameFoundation/State/GameStateLoading")]
public class GameStateLoading : GameState
{
    public override GameStateID ID => GameStateID.Loading;

    public override void OnStateEnter(Controller<GameModel, GameStateID> controller)
    {
    }
}
