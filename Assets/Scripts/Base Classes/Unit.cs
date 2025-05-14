using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected GameCubeNode currentNode = null;
    protected float attackRange = 30;
    [SerializeField] protected float walkRange = 10;
    protected int actionsAmount = 2;
    protected List<Vector3> pathCoordinates;
    protected int targetPosIndex = 0;
    protected int targetRawPathIndex = 0;
    protected List<GameCubeNode> rawPath;
    protected Vector3 endPos = Vector3.zero;
    protected float groundLevel = 0;
    protected List<GameCubeNode> reachableNodes = new List<GameCubeNode>();
    protected GameCubeNode.InhabitantType inhabitantType;
    public bool isAlive = true;
    public int health = 1;
    public bool isMoving = false;
    public bool unitReady = false;


    public virtual void FollowPath(List<Vector3> path, List<GameCubeNode> rawWalkPath)
    {
        pathCoordinates = path;
        targetPosIndex = 0;
        targetRawPathIndex = 0;
        rawPath = rawWalkPath;
        currentNode = rawPath[targetRawPathIndex];
    }

    public virtual void StartOfTurn()
    {
        unitReady = false;
        actionsAmount = 2;
    }

    public virtual void InhabitNode()
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
            if (targetRawPathIndex < rawPath.Count)
            {
                currentNode.LeaveNode();
                currentNode = rawPath[targetRawPathIndex];

                rawPath[targetRawPathIndex].EnterNode(this, inhabitantType);
                NodeEntered();
                groundLevel = rawPath[targetRawPathIndex].transform.position.y;
            }
            targetRawPathIndex++;
        }
    }

    public virtual void NodeEntered()
    {

    }

    public virtual void WalkPath()
    {
        if (pathCoordinates.Count > 0)
        {
            if (pathCoordinates.Count == targetPosIndex)
            {
                pathCoordinates.Clear();
                isMoving = false;
                HasStoppedWalking();
                return;
            }

            isMoving = true;
            Vector2 targetPos = new Vector2(pathCoordinates[targetPosIndex].x, pathCoordinates[targetPosIndex].z);
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);

            if (Vector2.Distance(targetPos, currentPos) < 0.01f)
            {
                targetPosIndex++;
            }
            if (targetRawPathIndex < rawPath.Count)
            {
                InhabitNode();
            }

            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, groundLevel, newPos.y);
        }

    }

    public virtual void HasStoppedWalking()
    {

        
    }

    public virtual GameCubeNode GetInhabitatedNode()
    {
        return currentNode;
    }
}
