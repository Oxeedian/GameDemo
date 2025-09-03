using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.AI;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerUnit : Unit
{
    public PlayerData playerData = new PlayerData();
    public string charName = "Empty";

    private GameCubeNode hoveredNode = new GameCubeNode();
    private bool isPerformingAction = false; //Ändra till Enum "PerformedAction"
    private List<Enemy> enemiesInRange;
    private Vector3 rotateToDirection = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;
    public void Initialize(MapController map, CameraController playerCamer)
    {
        pathCoordinates = new List<Vector3>();
        inhabitantType = GameCubeNode.InhabitantType.Player;
    }

    public void UpdateLoop(CameraController cameraController, MapController mapController, Attack attack, List<Enemy> allEnemies
        , PlayerController playerController)
    {
        RotateToDirection(100.0f);

        if (isPerformingAction)
        {
            AimMode(enemiesInRange, cameraController, attack); //Ändra till funktion "Performing Action
            return;
        }

        WalkPath();
        rotateToDirection = (transform.position - lastPosition).normalized;
        HandleAction(playerController, attack, mapController, cameraController, allEnemies);
        playerController.RenderWalkableNodes(isMoving, mapController, actionsAmount, reachableNodes, walkRange);
        lastPosition = transform.position;
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

        if (currentNode != null)
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

    private void HandleAction(PlayerController playerController, Attack attack, MapController mapController, CameraController cameraController, List<Enemy> allEnemies)
    {
        if (!isMoving)
        {
            Vector3 intersectPos = Vector3.zero;
            reachableNodes = mapController.GetReachableNodes(transform.position, walkRange);
            attack.SetRangeIndicatorPosition(transform.position, attackRange);


            GameCubeNode clickedNode;
            GameCubeNode hoveredNode;
            clickedNode = playerController.HandleMouseInput(cameraController, mapController, actionsAmount, out hoveredNode);

            if (hoveredNode != null && currentNode != null)
            {
                List<GameCubeNode> hypotheticalRawPath = mapController.FindPath(currentNode.transform.position, hoveredNode.transform.position);
                PostMaster.uiController.DrawPath(hypotheticalRawPath, walkRange, actionsAmount);
            }





            if (clickedNode != null)
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
                            //if(actionsAmount > 0)
                            //{
                            //    if (attack.AttackUnit(transform.position, attackRange, clickedNode.GetInhabitant(), 1))
                            //    {
                            //        actionsAmount = 0;
                            //        Debug.Log("Attacking Enemy!");
                            //        Debug.Log("PEW PEW!");
                            //    }
                            //}

                            break;
                        }
                    case GameCubeNode.InhabitantType.None:
                        {

                            if (CanNodeBeReachead(clickedNode))
                            {
                                Debug.Log("Walk");

                                float walkPointsReq = walkRange * 0.5f;

                                if (clickedNode.reachableValue > walkPointsReq && actionsAmount >= 2)
                                {
                                    actionsAmount -= 2;
                                }
                                else if (clickedNode.reachableValue <= walkPointsReq && actionsAmount >= 1)
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
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //List<Enemy> enemies = new List<Enemy>();
                //foreach (Enemy enemy in allEnemies)
                //{
                //    if (Vector3.Distance(enemy.transform.position, transform.position) < attackRange)
                //    {
                //        enemies.Add(enemy);
                //    }
                //}


                enemiesInRange = attack.GatherAllEnemiesInRange(transform.position, attackRange, allEnemies);
                if (enemiesInRange.Count <= 0 || actionsAmount <= 0)
                {
                    return;
                }

                cameraController.SaveCameraMatrix();
                currentAimedEnemy = 0;
                isPerformingAction = true;
                once = true;
            }
        }
    }

    bool once = true;
    int currentAimedEnemy = 0;
    private void AimMode(List<Enemy> allEnemies, CameraController cameraController, Attack attack)
    {
        int maxListSize = allEnemies.Count;


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAimedEnemy--;
            once = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAimedEnemy++;
            once = true;
        }

        if (currentAimedEnemy >= maxListSize)
        {
            currentAimedEnemy = 0;
        }
        else if (currentAimedEnemy < 0)
        {
            currentAimedEnemy += maxListSize;
        }

        if (once)
        {
            once = false;
            cameraController.AimCamera(transform.position, allEnemies[currentAimedEnemy].transform.position);
            attack.BuildGraph(attackRange, 0.75f); //TODO: Byt 1 mot shootefficiency nör det kommer in.
        }

        int hitChance = attack.GetHitChance(transform.position, allEnemies[currentAimedEnemy].transform.position, attackRange, 0.75f); //TODO: Ändra 0.76 till medlemsvariable i Unit klassen
        PostMaster.uiController.SetTargetIndicatiorPosition(cameraController.GetGameCamera(), allEnemies[currentAimedEnemy].transform.position, hitChance);


        if (PostMaster.uiController.IsShootButtonPressed())
        {
            attack.AttackUnit(transform.position, attackRange, allEnemies[currentAimedEnemy], 1, hitChance); //TODO: Ändra 1 till någon form utav damage senare.
            actionsAmount = 0;
            isPerformingAction = false;
            cameraController.ResetCamera();
            PostMaster.uiController.HideTargetIndicator();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPerformingAction = false;
            cameraController.ResetCamera();
            PostMaster.uiController.HideTargetIndicator();
        }



    }


    public void RotateToDirection(float rotationSpeed)
    {
        if (rotateToDirection == Vector3.zero) return; 

        Quaternion targetRotation = Quaternion.LookRotation(rotateToDirection);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

}
