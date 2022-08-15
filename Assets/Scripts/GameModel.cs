using System.Collections;
using System.Collections.Generic;
using GameFoundation.State;
using UnityEngine;

public enum GameStateID
{
    Loading,
    Playing,
    Paused,
    GameOver
}

public class GameModel : StateModel<GameStateID>
{
}
