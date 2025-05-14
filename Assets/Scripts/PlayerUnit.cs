using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.AI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.Experimental.GraphView;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerUnit : Unit
{
    private GameCubeNode hoveredNode = new GameCubeNode();
    public PlayerData playerData = new PlayerData();
    public string charName = "Empty";

    public void Initialize(MapController map, CameraController playerCamer)
    {
        pathCoordinates = new List<Vector3>();
        inhabitantType = GameCubeNode.InhabitantType.Player;
    }

    public void UpdateLoop(CameraController playerCamera, MapController mapController, Attack attack, List<Enemy> allEnemies
        , PlayerController playerController)
    {
        WalkPath();
        HandleAction(playerController, attack, mapController, playerCamera);
        playerController.RenderWalkableNodes(isMoving, mapController, actionsAmount, reachableNodes);
    }

    public override void StartOfTurn()
    {
        unitReady = false;
        actionsAmount = 2;
    }

    private bool CanNodeBeReachead(GameCubeNode node)
    {
        foreach (GameCubeNode reachableNode in reachableNodes)
        {
            if (reachableNode == node)
            {
                return true;
            }
        }
        return false;
    }

    public void SetPlayerPos(GameCubeNode node)
    {
        transform.position = node.transform.position;
        groundLevel = node.transform.position.y;

        if(currentNode != null)
        {
            currentNode.LeaveNode();

        }

        currentNode = node;
        node.EnterNode(this, inhabitantType);
        //HasStoppedWalking();
    }



    public override void NodeEntered()
    {
        PostMaster.manager.CullOutOfRange();
    }
    public override void HasStoppedWalking()
    {
        if (MapController.onRequestMapController != null)
        {
            MapController mapControllers = MapController.onRequestMapController.Invoke();
            reachableNodes = mapControllers.GetReachableNodes(transform.position, walkRange);
        }
    }

    private void HandleAction(PlayerController playerController, Attack attack, MapController mapController, CameraController cameraController)
    {
        if (!isMoving)
        {
            Vector3 intersectPos = Vector3.zero;
            reachableNodes = mapController.GetReachableNodes(transform.position, walkRange);
            attack.SetRangeIndicatorPosition(transform.position, attackRange);
            GameCubeNode clickedNode;
            if (clickedNode = playerController.HandleMouseInput(cameraController, mapController, actionsAmount))
            {
                switch (clickedNode.GetInhabitantType())
                {
                    case GameCubeNode.InhabitantType.Player:
                        {
                            Debug.Log("player");
                            //Switch to selected player
                            break;
                        }
                    case GameCubeNode.InhabitantType.Zombie:
                        {
                            if(actionsAmount > 0)
                            {
                                if (attack.AttackUnit(transform.position, attackRange, clickedNode.GetInhabitant(), 1))
                                {
                                    actionsAmount = 0;
                                    Debug.Log("Attacking Enemy!");
                                    Debug.Log("PEW PEW!");
                                }
                            }

                            break;
                        }
                    case GameCubeNode.InhabitantType.None:
                        {

                            if (CanNodeBeReachead(clickedNode))
                            {
                                Debug.Log("Walk");
                                if (clickedNode.reachableValue > 7 && actionsAmount >= 2)
                                {
                                    actionsAmount -= 2;
                                }
                                else if (clickedNode.reachableValue <= 7 && actionsAmount >= 1)
                                {
                                    actionsAmount -= 1;
                                }
                                else
                                {
                                    return;
                                }

                                endPos = clickedNode.transform.position;

                                List<GameCubeNode> rawPath = mapController.FindPath(transform.position, endPos);
                                List<MapController.Portal> portals = mapController.BuildPortals(rawPath);
                                List<Vector3> finalSmoothPath = mapController.StringPull(portals, transform.position, endPos);

                                FollowPath(finalSmoothPath, rawPath);
                            }
                            break;
                        }
                }



            }
        }
    }


}
