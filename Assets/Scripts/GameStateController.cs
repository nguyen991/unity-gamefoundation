using System.Collections;
using System.Collections.Generic;
using GameFoundation.State;
using UnityEngine;

public abstract class GameState : StateScript<GameModel, GameStateID>
{
}

public class GameStateController : StateController<GameState, GameModel, GameStateID>
{
}
