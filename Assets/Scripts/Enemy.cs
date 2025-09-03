using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Enemy : Unit
{
    [SerializeField] private float movePoints = 10f;

    private List<Unit> allPlayers;
    private bool pathIsChosen = false;
    private MapController mapController;
    private TurnManager turnManager;

    public Renderer renderer;
    public bool isReady = false;
    public bool cullChecked = false;

    public void Initialize(List<Unit> players, MapController mapControll, TurnManager turnmanager)
    {
        allPlayers = players;
        mapController = mapControll;
        turnManager = turnmanager;
        inhabitantType = GameCubeNode.InhabitantType.Zombie;
    }

    public override void StartOfTurn()
    {
        WalkTowardsPlayer(mapController);
        isReady = false;
    }

    public void SetPos(GameCubeNode node)
    {

        transform.position = node.transform.position;
        groundLevel = node.transform.position.y;

        if (currentNode != null)
        {
            currentNode.LeaveNode();

        }

        currentNode = node;
        node.EnterNode(this, inhabitantType);
    }

    public void Kill()
    {
        currentNode.LeaveNode();
        isAlive = false;
        transform.position = new Vector3( 0,1000,0);
    }

    void WalkTowardsPlayer(MapController mapController)
    {
        Vector3 closestPlayer = Vector3.zero;
        //Vector3 newPos = Vector3.zero;
        PlayerUnit playerUnit = null;
        float closestDistance = float.MaxValue;

        // Find the closest player to chase
        foreach (PlayerUnit player in allPlayers)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < closestDistance)
            {
                closestDistance = Vector3.Distance(transform.position, player.transform.position);
                closestPlayer = player.transform.position;
                playerUnit = player;
            }
        }

        closestDistance = float.MaxValue;

        //Find the closest neighbouring node
        foreach (GameObject neighbors in playerUnit.GetInhabitatedNode().neighbors)
        {
            if (Vector3.Distance(transform.position, neighbors.transform.position) < closestDistance && neighbors.GetComponent<GameCubeNode>().GetInhabitantType() == GameCubeNode.InhabitantType.None)
            {
                closestDistance = Vector3.Distance(transform.position, neighbors.transform.position);
                closestPlayer = neighbors.transform.position;
            }
        }

        //Create the full path to the player
        List<GameCubeNode> rawPath = mapController.FindPath(transform.position, closestPlayer);


        actionsAmount -= CalculateAndCutOffPath(rawPath);
       // rawPath = mapController.FindPath(transform.position, newPos);


        List<MapController.Portal> portals = mapController.BuildPortals(rawPath);
        List<Vector3> finalSmoothPath = mapController.StringPull(portals, transform.position, rawPath[rawPath.Count - 1].transform.position);

        FollowPath(finalSmoothPath, rawPath);
    }

    public void UpdateLoop(MapController mapController)
    {
        WalkPath();
    }

    int CalculateAndCutOffPath(List<GameCubeNode> rawPath)
    {
        int currentWalkPointsSpent = 0;
        int cutOffIndex = 0;
        int actionValue = 0;

        for (; cutOffIndex < rawPath.Count - 1; cutOffIndex++)
        {
            if (currentWalkPointsSpent == movePoints)
            {
                break;
            }

            if (cutOffIndex + 1 < rawPath.Count)
            {
                if (rawPath[cutOffIndex].levelType < rawPath[cutOffIndex + 1].levelType && rawPath[cutOffIndex + 1].isLedge)
                {
                    Debug.Log("Ledge identified!");
                    if (currentWalkPointsSpent + 4 > movePoints)
                    {
                        break;
                    }
                    currentWalkPointsSpent += 4;
                }
                else
                {
                    if (currentWalkPointsSpent + 1 > movePoints)
                    {
                        break;
                    }
                    currentWalkPointsSpent += 1;
                }
            }
        }

        if (cutOffIndex >= 0 && cutOffIndex < rawPath.Count)
        {
            rawPath.RemoveRange(cutOffIndex + 1, rawPath.Count - (cutOffIndex + 1));
        }


        return actionValue;
    }

    public override void HasStoppedWalking()
    {
        isReady = true;
    }




    public override void NodeEntered()
    {
        PostMaster.manager.CullEnemyOutOfRange(this);
    }
}
