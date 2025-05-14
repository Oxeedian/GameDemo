using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [SerializeField] MapController mapController;
    //[SerializeField] PlayerUnit playerController;
    //[SerializeField] PlayerUnit playerController2;
    [SerializeField] Camera gameCamera;
    [SerializeField] CameraController gameCameraController;
    [SerializeField] TurnManager turnManager;
    [SerializeField] UiController uiController;
    [SerializeField] Enemy enemy;
    [SerializeField] Enemy enemy2;
    [SerializeField] Attack attack;
    [SerializeField] PlayerController playerControlleractual;
    [SerializeField] PlayerUnitManager playerUnitManager;
    [SerializeField] GameObject playerPrefab;
        

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
        mapController.Initialize();
        uiController.Initialize(turnManager);

        foreach(PlayerData savedData in PlayerTransfer.GetUnits() )
        {
            GameObject newChar = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newChar.GetComponent<PlayerUnit>().Initialize(mapController, gameCameraController);
            newChar.GetComponent<PlayerUnit>().charName = savedData.charName;
            playerList.Add(newChar.GetComponent<PlayerUnit>());
        }

        //playerController.Initialize(mapController, gameCameraController);
        //playerController2.Initialize(mapController, gameCameraController);
        //playerList.Add(playerController);  
        //playerList.Add(playerController2);

        List<GameCubeNode> spawnNodes = mapController.CreateGroupRandomSpawn();

        //playerController.SetPlayerPos(mapController.RandomSpawn());
        //playerController2.SetPlayerPos(mapController.RandomSpawn());

        int index = 0;
        foreach (PlayerUnit player in playerList)
        {
            player.SetPlayerPos(spawnNodes[index]);
            index++;
        }


        enemyList.Add(enemy);
        enemyList.Add(enemy2);

        turnManager.Initialize(playerList, enemyList);

        enemy.Initialize(playerList, mapController, turnManager);
        enemy.SetPos(mapController.RandomSpawn());
        enemy2.Initialize(playerList, mapController, turnManager);
        enemy2.SetPos(mapController.RandomSpawn());

        playerUnitManager.Initialize(playerList);
        PostMaster.manager = playerUnitManager;
        playerUnitManager.CullOutOfRange();
    }

    private void Update()
    {
        
        GameTurnUpdateLoop();
        gameCameraController.UpdateLoop(playerList[0].GetComponent<PlayerUnit>());

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

                playerUnitManager.ReGatherRenderers();
            }
        }
    }

    void GameTurnUpdateLoop()
    {
        switch (turnManager.GetWhosTurn())
        {
            case TurnManager.CurrentTurn.Player:
                    turnManager.PlayCurrentTurn(playerList, gameCameraController, mapController, attack, enemyList, playerControlleractual);
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
