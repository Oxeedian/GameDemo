using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float movePoints = 7f;

    private List<PlayerController> allPlayers;
    private List<Vector3> pathCoordinates;
    private int targetPosIndex = 0;
    private int targetRawPathIndex = 0;
    private List<GameCubeNode> rawPath;
    private Vector3 endPos = Vector3.zero;
    private bool isMoving = false;
    private GameCubeNode currentNode;
    private bool pathIsChosen = false;
    private MapController mapController;
    private TurnManager turnManager;
    private float groundLevel = 0;

    public int health = 1;
    public bool isAlive = true;
    public bool isReady = false;

    public void Initialize(List<PlayerController> players, MapController mapControll, TurnManager turnmanager)
    {
        allPlayers = players;
        mapController = mapControll;
        turnManager = turnmanager;
    }

    public void StartOfTurn()
    {
        WalkTowardsPlayer(mapController);
        isReady = false;
    }
    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
        groundLevel = pos.y;
    }
    public void Kill()
    {
        currentNode.LeaveNode();
    }
    void WalkTowardsPlayer(MapController mapController)
    {
        Vector3 closestPlayer = Vector3.zero;
        Vector3 newPos = Vector3.zero;
        float closestDistance = float.MaxValue;

        foreach (PlayerController player in allPlayers)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < closestDistance)
            {
                closestDistance = Vector3.Distance(transform.position, player.transform.position);
                closestPlayer = player.transform.position;
            }
        }

        List<GameCubeNode> rawPath = mapController.FindPath(transform.position, closestPlayer);


        if (rawPath[rawPath.Count - 1].GetInhabitantType() == GameCubeNode.InhabitantType.Player)
        {
            rawPath.Remove(rawPath[rawPath.Count - 1]);
        }

        newPos = FindClosestPathToGoal(rawPath);
        rawPath = mapController.FindPath(transform.position, newPos);


        List<MapController.Portal> portals = mapController.BuildPortals(rawPath);
        List<Vector3> finalSmoothPath = mapController.StringPull(portals, transform.position, newPos);

        FollowPath(finalSmoothPath, rawPath);
    }
    public void FollowPath(List<Vector3> path, List<GameCubeNode> rawWalkPath)
    {
        pathCoordinates = path;
        targetPosIndex = 0;
        targetRawPathIndex = 0;
        rawPath = rawWalkPath;
        currentNode = rawPath[targetRawPathIndex];
    }

    public void UpdateLoop(MapController mapController)
    {
        WalkPath();
    }

    Vector3 FindClosestPathToGoal(List<GameCubeNode> rawPath)
    {
        int currentWalkPointsSpent = 0;
        int index = 0;
        for (; index < rawPath.Count - 1; index++)
        {
            if (currentWalkPointsSpent == movePoints)
            {
                break;
            }

            if (index + 1 < rawPath.Count)
            {
                if (rawPath[index].levelType < rawPath[index + 1].levelType && rawPath[index + 1].isLedge)
                {
                    Debug.Log("Ledge identified!");
                    if (currentWalkPointsSpent + 6 > movePoints)
                    {
                        break;
                    }
                    currentWalkPointsSpent += 6;
                }
                else
                {
                    if (currentWalkPointsSpent + 1 > movePoints)
                    {
                        break;
                    }
                    currentWalkPointsSpent++;
                }
            }
        }
        Debug.Log(currentWalkPointsSpent);
        //rawPath[index].SetColorBlue();
        return rawPath[index].transform.position;
    }

    void WalkPath()
    {
        if (pathCoordinates.Count > 0)
        {
            if (pathCoordinates.Count == targetPosIndex) //|| targetRawPathIndex == movePoints
            {
                pathCoordinates.Clear();
                isMoving = false;
                //turnManager.EndTurnEnemy();
                isReady = true;
                return;
            }
            isMoving = true;
            Vector2 targetPos = new Vector2(pathCoordinates[targetPosIndex].x, pathCoordinates[targetPosIndex].z);
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);

            // Face the direction (optional)
            //Vector3 dir = (target - transform.position).normalized;
            //if (dir != Vector3.zero)
            //{
            //    Quaternion lookRot = Quaternion.LookRotation(dir);
            //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            //}

            if (Vector2.Distance(targetPos, currentPos) < 0.01f)
            {
                targetPosIndex++;
            }
            if (targetRawPathIndex < rawPath.Count)
            {
                Vector3 center = rawPath[targetRawPathIndex].transform.position;
                float halfX = rawPath[targetRawPathIndex].transform.localScale.x * 0.5f;
                float halfZ = rawPath[targetRawPathIndex].transform.localScale.z * 0.5f;

                float minX = center.x - halfX;
                float maxX = center.x + halfX;
                float minZ = center.z - halfZ;
                float maxZ = center.z + halfZ;

                float px = transform.position.x;
                float pz = transform.position.z;

                bool insideXZ =
                    px >= minX && px <= maxX &&
                    pz >= minZ && pz <= maxZ;

                if (insideXZ)
                {
                    //currentNode.SetColor();

                    

                    if (targetRawPathIndex < rawPath.Count)
                    {
                        currentNode.LeaveNode();
                        currentNode = rawPath[targetRawPathIndex];

                        rawPath[targetRawPathIndex].EnterNode(gameObject, GameCubeNode.InhabitantType.Zombie);
                        groundLevel = rawPath[targetRawPathIndex].transform.position.y;
                    }
                    targetRawPathIndex++;
                }
            }

            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, groundLevel, newPos.y);
        }

    }
}
