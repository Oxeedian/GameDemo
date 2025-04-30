using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected GameCubeNode currentNode;
    protected int actionsAmount = 2;
    protected List<Vector3> pathCoordinates;
    protected int targetPosIndex = 0;
    protected int targetRawPathIndex = 0;
    protected List<GameCubeNode> rawPath;
    protected Vector3 endPos = Vector3.zero;
    protected float groundLevel = 0;
    protected List<GameCubeNode> reachableNodes = new List<GameCubeNode>();

    public int health = 1;
    public bool isMoving = false;
    public bool unitReady = false;

    public virtual void Test()
    {
        // Base implementation (optional)
        Debug.Log("Unit base UpdateLoop");
    }

    public virtual void FollowPath(List<Vector3> path, List<GameCubeNode> rawWalkPath)
    {
        pathCoordinates = path;
        targetPosIndex = 0;
        targetRawPathIndex = 0;
        rawPath = rawWalkPath;
        currentNode = rawPath[targetRawPathIndex];
    }
}
