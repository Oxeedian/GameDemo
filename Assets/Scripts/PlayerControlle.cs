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

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameCubeNode currentNode;
    [SerializeField] private GameObject twoStepQuad;
    [SerializeField] private GameObject oneStepQuad;

    public bool isMoving = false;

    private List<Vector3> pathCoordinates;
    private int targetPosIndex = 0;
    private int targetRawPathIndex = 0;
    private List<GameCubeNode> rawPath;
    private Vector3 endPos = Vector3.zero;
    private float groundLevel = 0;
    private GameCubeNode hoveredNode = new GameCubeNode();
    private MapController mapCon;
    public bool unitReady = false;
    List<GameCubeNode> reachableNodes = new List<GameCubeNode>();
    public CameraController playerCamera;

    public int health = 1;
    private int actionsAmount = 2;


    public void Initialize(MapController map, CameraController playerCamer)
    {
        mapCon = map;
        pathCoordinates = new List<Vector3>();

        playerCamera = playerCamer;
    }
    public void UpdateLoop(CameraController playerCamera, MapController mapController, Attack attack, List<Enemy> allEnemies)
    {
        WalkPath(mapController);
        MousePointer(playerCamera, mapController, attack, allEnemies);
    }

    void MousePointer(CameraController playerCamera, MapController mapController, Attack attack, List<Enemy> allEnemies)
    {
        Ray ray = playerCamera.camera.ScreenPointToRay(Input.mousePosition);
        Vector3 intersectPos = Vector3.zero;
        List<Enemy> enemiesInRange = attack.GatherAllEnemiesInRange(transform.position, 50, allEnemies);


        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (mapController.IntersectCube(ray, out intersectPos, out hoveredNode))
            {
                mapController.intersectionVFX.transform.position = intersectPos;

                if (Input.GetMouseButtonDown(0))
                {
                    foreach (Enemy enemy in enemiesInRange)
                    {
                        if (hoveredNode.inhabitant == enemy.gameObject)
                        {
                            attack.AttackUnit(enemy, 1);
                            Debug.Log("PEW PEW!");
                        }
                    }

                    if (CanNodeBeReachead(hoveredNode))
                    {
                        if(hoveredNode.reachableValue > 7 && actionsAmount >= 2)
                        {
                            actionsAmount -= 2;
                        }
                        else if(hoveredNode.reachableValue <= 7 && actionsAmount >= 1)
                        {
                            actionsAmount -= 1;
                        }
                        else
                        {
                            return;
                        }
                        endPos = intersectPos;
                        Debug.Log("ETT");

                        Debug.Log("NU?");
                        List<GameCubeNode> rawPath = mapController.FindPath(transform.position, endPos);
                        List<MapController.Portal> portals = mapController.BuildPortals(rawPath);
                        List<Vector3> finalSmoothPath = mapController.StringPull(portals, transform.position, endPos);

                        foreach (GameCubeNode node in reachableNodes)
                        {
                            node.SetLevelColor(0.7f);
                        }

                        FollowPath(finalSmoothPath, rawPath);
                    }

                }
            }

            if(!isMoving)
            {
                reachableNodes = mapCon.GetReachableNodes(transform.position, 12);


                List<Matrix4x4> matricesTwoStep = new List<Matrix4x4>();
                List<Matrix4x4> matricesOneStep = new List<Matrix4x4>();
                foreach (GameCubeNode node in reachableNodes)
                {
                    if(node.reachableValue > 7)
                    {
                        Vector3 pos = node.transform.position;
                        pos.y += 1.2f;
                        matricesTwoStep.Add(Matrix4x4.TRS(pos, twoStepQuad.transform.rotation, twoStepQuad.transform.lossyScale));
                    }
                    else
                    {
                        Vector3 pos = node.transform.position;
                        pos.y += 1.2f;
                        matricesOneStep.Add(Matrix4x4.TRS(pos, oneStepQuad.transform.rotation, oneStepQuad.transform.lossyScale));
                    }
                }
                if(actionsAmount > 1)
                {
                    Mesh meshTwoStep = twoStepQuad.GetComponent<MeshFilter>().mesh;
                    Material matTwoStep = twoStepQuad.GetComponent<Renderer>().material;
                    Graphics.DrawMeshInstanced(meshTwoStep, 0, matTwoStep, matricesTwoStep);
                }
                if(actionsAmount > 0)
                {
                    Mesh meshOneStep = oneStepQuad.GetComponent<MeshFilter>().mesh;
                    Material matOneStep = oneStepQuad.GetComponent<Renderer>().material;
                    Graphics.DrawMeshInstanced(meshOneStep, 0, matOneStep, matricesOneStep);
                }
            }
        }
    }
    public void StartOfTurn()
    {
        unitReady = false;
        actionsAmount = 2;
    }
    public void FollowPath(List<Vector3> path, List<GameCubeNode> rawWalkPath)
    {
        pathCoordinates = path;
        targetPosIndex = 0;
        targetRawPathIndex = 0;
        rawPath = rawWalkPath;
        currentNode = rawPath[targetRawPathIndex];
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
    public void SetPlayerPos(Vector3 pos)
    {
        transform.position = pos;
        groundLevel = pos.y;


        HasStoppedWalking();
    }

    void HasStoppedWalking()
    {
        reachableNodes = mapCon.GetReachableNodes(transform.position, 10);
    }

    void WalkPath(MapController mapController)
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


                    if (targetRawPathIndex < rawPath.Count)
                    {
                        currentNode.LeaveNode();
                        currentNode = rawPath[targetRawPathIndex];

                        rawPath[targetRawPathIndex].EnterNode(gameObject, GameCubeNode.InhabitantType.Player);
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
