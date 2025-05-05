using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public enum CurrentTurn
    {
        Player,
        Zombies,
        //Bandits,
        //Animals,
        Count
    }

    [SerializeField] Image imageTurn;
    [SerializeField] Sprite playerTurnsSprite;
    [SerializeField] Sprite zombieTurnsSprite;

    private List<Unit> allPlayers;
    private List<Enemy> allEnemies;
    private int amountOfReadyPlayers = 0;
    private CurrentTurn currentTurn = CurrentTurn.Player;
    private int currentPlayerIndex = 0;
    private PlayerUnit currentPlayer;
    private int currentTurnIndex = 0;


    public void Initialize(List<Unit> allPlayer, List<Enemy> allEnemie)
    {
        allPlayers = allPlayer;
        allEnemies = allEnemie;
    }

    void AddPlayer(PlayerUnit player)
    {
        allPlayers.Add(player);
    }

    bool AllPlayersReady()
    {

        foreach (PlayerUnit player in allPlayers)
        {
            if (player.isMoving)
            {
                return false;
            }
        }

        return true;
    }

    public void EndTurn()
    {
        if (AllPlayersReady())
        {
            currentTurn += 1;
            if (currentTurn == CurrentTurn.Count)
            {
                currentTurn = CurrentTurn.Player;
            }

            currentPlayerIndex = 0;

            switch (currentTurn)
            {
                case TurnManager.CurrentTurn.Player:
                    imageTurn.sprite = playerTurnsSprite;
                    foreach (PlayerUnit player in allPlayers)
                    {
                        player.StartOfTurn();
                    }
                    break;

                case TurnManager.CurrentTurn.Zombies:
                    imageTurn.sprite = zombieTurnsSprite;
                    foreach (Enemy enemy in allEnemies)
                    {
                        enemy.StartOfTurn();
                    }


                    if (allEnemies.Count <= 0)
                    {
                        EndTurn();
                    }
                    break;
            }
        }
    }
    public void EndTurnEnemy()
    {
        currentTurn += 1;
        if (currentTurn == CurrentTurn.Count)
        {
            currentTurn = CurrentTurn.Player;
        }
        currentPlayerIndex = 0;
        switch (currentTurn)
        {
            case TurnManager.CurrentTurn.Player:
                imageTurn.sprite = playerTurnsSprite;
                foreach (PlayerUnit player in allPlayers)
                {
                    player.StartOfTurn();
                }
                //allPlayers[currentPlayerIndex].GetComponent<PlayerUnit>().playerCamera.SetCameraPosition(allPlayers[currentPlayerIndex].transform.position);
                break;

            case TurnManager.CurrentTurn.Zombies:
                imageTurn.sprite = zombieTurnsSprite;
                foreach (Enemy enemy in allEnemies)
                {
                    enemy.StartOfTurn();
                }
                if (allEnemies.Count <= 0)
                {
                    EndTurn();
                }
                break;
        }
    }
    void AutoEndTurn()
    {

    }




    public CurrentTurn GetWhosTurn()
    {
        return currentTurn;
    }


    public void PlayCurrentTurn(List<Unit> units, CameraController playerCamera, MapController mapController, Attack attack, List<Enemy> allEnemies, 
        PlayerController playerController)
    {
        if (units.Count > 0)
        {
            units[currentPlayerIndex].GetComponent<PlayerUnit>().UpdateLoop(playerCamera, mapController, attack, allEnemies, playerController);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            int amountOfReadyUnits = 0;

            foreach (PlayerUnit unit in allPlayers)
            {
                if (unit.unitReady)
                {
                    amountOfReadyUnits++;
                }
            }
            if(amountOfReadyUnits >= allPlayers.Count)
            {
                return;
            }
            bool loop = true;
            while (loop)
            {
                currentPlayerIndex++;
                if (currentPlayerIndex >= units.Count)
                {
                    currentPlayerIndex = 0;
                }
                if(!units[currentPlayerIndex].GetComponent<PlayerUnit>().unitReady)
                {
                    loop = false;
                    break;
                }
            }
            playerCamera.SetCameraPosition(units[currentPlayerIndex].transform.position);
            currentPlayer = units[currentPlayerIndex].GetComponent<PlayerUnit>();
        }
    }

    public void PlayCurrentTurnEnemy(List<Enemy> units, CameraController playerCamera, MapController mapController, Attack attack, List<Enemy> allEnemies)
    {
        if (units.Count > 0)
        {
            units[currentPlayerIndex].UpdateLoop(mapController);

            if(units[currentPlayerIndex].isReady)
            {
                currentPlayerIndex++;
                if (currentPlayerIndex >= units.Count)
                {
                    EndTurnEnemy();
                }
                units[currentPlayerIndex].StartOfTurn();

            }
        }
        else
        {
            EndTurnEnemy();
        }
    }

    public void EndCurrentUnitsTurnButton()
    {
        allPlayers[currentPlayerIndex].GetComponent<PlayerUnit>().unitReady = true;

        int amountOfReadyUnits = 0;

        foreach (PlayerUnit unit in allPlayers)
        {
            if (unit.unitReady)
            {
                amountOfReadyUnits++;
            }
        }
        if(amountOfReadyUnits == allPlayers.Count)
        {
            EndTurn();
            return;
        }

        bool loop = true;
        while (loop)
        {
            currentPlayerIndex++;
            if (currentPlayerIndex >= allPlayers.Count)
            {
                currentPlayerIndex = 0;
            }
            if (!allPlayers[currentPlayerIndex].GetComponent<PlayerUnit>().unitReady)
            {
                loop = false;
                break;
            }
        }
       // allPlayers[currentPlayerIndex].GetComponent<PlayerUnit>().playerCamera.SetCameraPosition(allPlayers[currentPlayerIndex].transform.position);
    }
}