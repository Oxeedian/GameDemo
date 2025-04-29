using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour
{
    TurnManager turnManager;
    public void Initialize(TurnManager aTurnManager)
    {
        turnManager = aTurnManager;
    }

    public void EndTurnButton()
    {
        turnManager.EndTurn();
    }

}
