using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class GameManager : MonoBehaviour
{
    [SerializeField] MapController mapController;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerController playerController2;
    [SerializeField] Camera gameCamera;
    [SerializeField] CameraController gameCameraController;
    [SerializeField] TurnManager turnManager;
    [SerializeField] UiController uiController;
    [SerializeField] Enemy enemy;
    [SerializeField] Enemy enemy2;
    [SerializeField] Attack attack;
        

    List<Enemy> enemyList = new List<Enemy>();
    List<Unit> playerList = new List<Unit>();
    public event Action AllPlayersReadyEvent;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    void Start()
    {
        playerController.Test();

        mapController.Initialize();
        uiController.Initialize(turnManager);

        playerController.Initialize(mapController, gameCameraController);
        playerController.SetPlayerPos(mapController.RandomSpawn());

        playerController2.Initialize(mapController, gameCameraController);
        playerController2.SetPlayerPos(mapController.RandomSpawn());

        playerList.Add(playerController);  
        playerList.Add(playerController2);
        playerList[0].Test();


        enemyList.Add(enemy);
        enemyList.Add(enemy2);

        turnManager.Initialize(playerList, enemyList);

        enemy.Initialize(playerList, mapController, turnManager);
        enemy.SetPos(mapController.RandomSpawn());
        enemy2.Initialize(playerList, mapController, turnManager);
        enemy2.SetPos(mapController.RandomSpawn());
    }

    private void Update()
    {
        GameTurnUpdateLoop();
        gameCameraController.UpdateLoop(playerController);

        HandleDeath();
    }

    private void HandleDeath()
    {
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemyList[i];
            if (enemy.isAlive == false)
            {
                enemy.Kill();
                GameObject.Destroy(enemy.gameObject); // or DestroyImmediate(obj) in editor
                enemyList.RemoveAt(i);
            }
        }
    }

    void GameTurnUpdateLoop()
    {
        switch (turnManager.GetWhosTurn())
        {
            case TurnManager.CurrentTurn.Player:
                    turnManager.PlayCurrentTurn(playerList, gameCameraController, mapController, attack, enemyList);
                break;

            case TurnManager.CurrentTurn.Zombies:
                turnManager.PlayCurrentTurnEnemy(enemyList, gameCameraController, mapController, attack, enemyList);
                //foreach (Enemy item in enemyList)
                //{
                //    item.UpdateLoop(mapController);
                //}
                break;
        }
    }

}
