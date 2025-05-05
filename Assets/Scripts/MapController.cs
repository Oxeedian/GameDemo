using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Net.Http.Headers;
using System;
using System.Linq;

public class MapController : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private GameObject gameCubePrefab;
    [SerializeField] private Transform parentMapTransform;
    [SerializeField] private PlayerUnit player;
    [Header("Map Dimensions")]
    [SerializeField] private int width = 100;
    [SerializeField] private int length = 100;
    [Header("Noise Settings")]
    [SerializeField] private float noiseZoom = 2;
    [SerializeField] private float noiseOffsetX = 100;
    [SerializeField] private float noiseOffsetZ = 100;
    [SerializeField] private float noiseMultiplier = 1;
    [SerializeField] private float noiseOffsetSkip = 0;
    [Header("Map Levels")]
    [SerializeField] private float waterLevelPercentage = 0;
    [SerializeField] private float groundLevelPercentage = 0.4f;
    [SerializeField] private float levelOnePercentage = 0.6f;
    [SerializeField] private float levelTwoPercentage = 0.8f;

    [Header("Debugging")]
    //Debugging. Want to somehow adapt the map colors to their height.
    [SerializeField] private float lowestValue = float.MaxValue;
    [SerializeField] private float highestValue = float.MinValue;
    [SerializeField] private bool constantlyGenerateMap = false;

    [SerializeField] Camera playerCamera;
    [SerializeField] PlayerUnit playerChar;

    [Header("Flat Area Settings")]
    [SerializeField] private Vector2Int flatAreaStart = new Vector2Int(40, 40); // bottom-left corner of flat area
    [SerializeField] private int flatAreaSize = 10;
    [SerializeField] private FlatLevel flatLevel = 0; // Optional: you can make this a level like Ground1, etc.
    [SerializeField] private float flatHeight = 0; // Optional: you can make this a level like Ground1, etc.


    public static Func<MapController> onRequestMapController;
    //List<GameObject> gameCubeNodes = new List<GameObject>();
    List<Portal> debugPortals = new List<Portal>();
    List<Vector3> debugPath = new List<Vector3>();
    public GameObject[,] nodeGrid;

    public struct Portal
    {
        public Vector3 left;
        public Vector3 right;

        public Portal(Vector3 left, Vector3 right)
        {
            this.left = left;
            this.right = right;
        }
    }

    enum FlatLevel
    {
        Ground1,
        Level2,
        Level3,
        Level4,
    }

    public void RandomGenerateMap()
    {
       noiseZoom = UnityEngine.Random.Range(1, 3);
        Debug.Log("zoom " + noiseZoom);
       noiseOffsetX = UnityEngine.Random.Range(1, 1000);
        Debug.Log("X " + noiseOffsetX);
        noiseOffsetZ = UnityEngine.Random.Range(1, 1000);
        Debug.Log("Z " + noiseOffsetZ);


        GenerateMap();
    }
    private void Awake()
    {
        onRequestMapController = () => this;
    }

    void OnDestroy()
    {
        if (onRequestMapController != null && onRequestMapController() == this)
            onRequestMapController = null;
    }
    void Start()
    {
    }

    public void Initialize()
    {
        switch (flatLevel)
        {
            case FlatLevel.Ground1:
                {
                    flatHeight = groundLevelPercentage - 0.01f;//waterLevelPercentage + 0.01f;
                    break;
                }
            case FlatLevel.Level2:
                {
                    flatHeight = levelOnePercentage - 0.01f;//groundLevelPercentage + 0.01f;
                    break;
                }
            case FlatLevel.Level3:
                {
                    flatHeight = levelTwoPercentage - 0.01f;//levelOnePercentage + 0.01f;
                    break;
                }
            default:
                {
                    flatHeight = 1; // levelTwoPercentage + 0.01f;
                    break;
                }
        }

                GenerateMap();
    }

    void GenerateMap()
    {
        if (nodeGrid != null && nodeGrid.Length > 0)
        {
            int rows = nodeGrid.GetLength(0);
            int cols = nodeGrid.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    nodeGrid[i, j] = null;
                }
            }

            foreach (Transform child in parentMapTransform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
       
        
        nodeGrid = new GameObject[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject instance = Instantiate(gameCubePrefab);
                instance.transform.SetParent(parentMapTransform);
                GameCubeNode cubeNode = instance.GetComponent<GameCubeNode>();

                Vector3 position;
                position.x = x * 2; // Since each cube is 2 units wide
                position.z = z * 2;
                position.y = CalculateNoisePos(position.x, position.z, cubeNode);

                if (position.y < 0)
                {
                    cubeNode.activated = false;
                }

                instance.transform.position = position;

                cubeNode.index.x = x;
                cubeNode.index.y = z;

                nodeGrid[x, z] = instance;
            }
        }

        ConnectNodes();
    }
    void ConnectNodes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameCubeNode node = nodeGrid[x, z].GetComponent<GameCubeNode>();

                if (node.activated)
                {
                    // Check all 4 directions (or 8 if you want diagonals too)
                    TryAddNeighbor(node, x - 1, z); // left
                    TryAddNeighbor(node, x + 1, z); // right
                    TryAddNeighbor(node, x, z - 1); // down
                    TryAddNeighbor(node, x, z + 1); // up

                    //TryAddNeighbor(node, x - 1, z + 1); // left up
                    //TryAddNeighbor(node, x + 1, z - 1); // right down
                    //TryAddNeighbor(node, x + 1, z + 1); // right up 
                    //TryAddNeighbor(node, x - 1, z - 1); // left down
                }

            }
        }
    }
    void TryAddNeighbor(GameCubeNode node, int x, int z)
    {
        if (x >= 0 && x < width && z >= 0 && z < length)
        {
            if (nodeGrid[x, z].GetComponent<GameCubeNode>().activated)
            {
                node.neighbors.Add(nodeGrid[x, z]);

            }
            if (node.levelType > nodeGrid[x, z].GetComponent<GameCubeNode>().levelType)
            {
               // node.SetLevelColor(0);
                node.isLedge = true;
            }
        }
    }

    void Update()
    {

        DebugRenderWalkPathLines();

    }

    void DebugRenderWalkPathLines()
    {
        for (int x = 0; x < debugPortals.Count; x++)
        {
            Vector3 start = debugPortals[x].left;
            Vector3 end = debugPortals[x].left + new Vector3(0, 10, 0);
            Debug.DrawLine(start, end, Color.red);
            start = debugPortals[x].right;
            end = debugPortals[x].right + new Vector3(0, 10, 0);
            Debug.DrawLine(start, end, Color.blue);
        }
        for (int x = 0; x < debugPath.Count - 1; x++)
        {
            Vector3 start = debugPath[x] + new Vector3(0, 3, 0);
            Vector3 end = debugPath[x + 1] + new Vector3(0, 3, 0);
            Debug.DrawLine(start, end, Color.white);
        }
    }
    void DebugReGenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                Vector3 position = nodeGrid[x, z].transform.position;
                position.y = CalculateNoisePos(position.x, position.z, nodeGrid[x, z].GetComponent<GameCubeNode>());

                if (position.y < 0)
                {
                    nodeGrid[x, z].GetComponent<GameCubeNode>().activated = false;
                }
                else
                {
                    nodeGrid[x, z].GetComponent<GameCubeNode>().activated |= true;
                }

                nodeGrid[x, z].transform.position = position;
            }
        }
    }

    float CalculateNoisePos(float x, float z, GameCubeNode node)
    {
        float xCoord = x / width * noiseZoom + noiseOffsetX + (x * noiseOffsetSkip);
        float zCoord = z / length * noiseZoom + noiseOffsetZ + (z * noiseOffsetSkip);

        int gridX = Mathf.RoundToInt(x / 2); // Convert world pos back to grid index
        int gridZ = Mathf.RoundToInt(z / 2);

        float yCoord = Mathf.PerlinNoise(xCoord, zCoord) * noiseMultiplier;

        // Check if the current tile is within the flat area
        if (gridX >= flatAreaStart.x && gridX < flatAreaStart.x + flatAreaSize &&
            gridZ >= flatAreaStart.y && gridZ < flatAreaStart.y + flatAreaSize)
        {
            switch (flatLevel)
            {
                case FlatLevel.Ground1:
                    {
                        yCoord = 0;
                        flatHeight = groundLevelPercentage - 0.01f;//waterLevelPercentage + 0.01f;
                        node.levelType = GameCubeNode.LevelType.Ground1;
                        break;
                    }
                case FlatLevel.Level2:
                    {
                        yCoord = 2;
                        flatHeight = levelOnePercentage - 0.01f;//groundLevelPercentage + 0.01f;
                        node.levelType = GameCubeNode.LevelType.Level2;
                        break;
                    }
                case FlatLevel.Level3:
                    {
                        yCoord = 4;
                        flatHeight = levelTwoPercentage - 0.01f;//levelOnePercentage + 0.01f;
                        node.levelType = GameCubeNode.LevelType.Level3;
                        break;
                    }
                default:
                    {
                        yCoord = 6;
                        flatHeight = 1; // levelTwoPercentage + 0.01f;
                        node.levelType = GameCubeNode.LevelType.Level4;
                        break;
                    }
            }
            //if (yCoord < groundLevelPercentage)
            //{
            //    yCoord = 0;
            //    node.levelType = GameCubeNode.LevelType.Ground1;
            //}
            //else if (yCoord < levelOnePercentage)
            //{
            //    yCoord = 2;
            //    node.levelType = GameCubeNode.LevelType.Level2;
            //}
            //else if (yCoord < levelTwoPercentage)
            //{
            //    yCoord = 4;
            //    node.levelType = GameCubeNode.LevelType.Level3;
            //}
            //else
            //{
            //    yCoord = 6;
            //    node.levelType = GameCubeNode.LevelType.Level4;
            //}
            //node.SetColorBlue();
            return yCoord;
        }
        // Apply blend effect based on the distance from the flat area
        Vector2 flatCenter = new Vector2(flatAreaStart.x + flatAreaSize / 2f, flatAreaStart.y + flatAreaSize / 2f);
        float distance = Vector2.Distance(new Vector2(gridX, gridZ), flatCenter);

        // Adjust the transition range with blendWidth
        float blendWidth = 10f; // You can tweak this value
        float t = Mathf.InverseLerp(flatAreaSize / 2f, flatAreaSize / 2f + blendWidth, distance);

        // Blend between flatHeight and noiseHeight based on the distance
        yCoord = Mathf.Lerp(flatHeight, yCoord, t);

        // Apply further height adjustments based on the noise thresholds for levels
        if (yCoord < lowestValue)
        {
            lowestValue = yCoord;
        }
        else if (yCoord > highestValue)
        {
            highestValue = yCoord;
        }

        if (yCoord < waterLevelPercentage)
        {
            yCoord = -1000;
            node.levelType = GameCubeNode.LevelType.Water;
            //node.SetLevelColor(0);
        }
        else if (yCoord < groundLevelPercentage)
        {
            yCoord = 0;
            node.levelType = GameCubeNode.LevelType.Ground1;
            //node.SetLevelColor(0.1f);
        }
        else if (yCoord < levelOnePercentage)
        {
            yCoord = 2;
            node.levelType = GameCubeNode.LevelType.Level2;
            //node.SetLevelColor(0.4f);
        }
        else if (yCoord < levelTwoPercentage)
        {
            yCoord = 4;
            node.levelType = GameCubeNode.LevelType.Level3;
            //node.SetLevelColor(0.75f);
        }
        else
        {
            yCoord = 6;
            node.levelType = GameCubeNode.LevelType.Level4;
            //node.SetLevelColor(2);
        }

        return yCoord;
    }

    public bool IntersectCube(Ray ray, out GameCubeNode node)
    {
        RaycastHit hit;
        node = null;
        foreach (GameObject cube in nodeGrid)
        {
            if (cube.GetComponent<BoxCollider>().Raycast(ray, out hit, 500.0f))
            {
                node = cube.GetComponent<GameCubeNode>();
                return true;
            }
        }

        return false;
    }
    GameCubeNode GetNearestNode(Vector3 position, out bool isActive)
    {
        int x = Mathf.RoundToInt(position.x / 2); // 2 = cube size
        int z = Mathf.RoundToInt(position.z / 2);

        x = Mathf.Clamp(x, 0, width - 1);
        z = Mathf.Clamp(z, 0, length - 1);

        isActive = nodeGrid[x, z].GetComponent<GameCubeNode>().activated;

        return nodeGrid[x, z].GetComponent<GameCubeNode>();
    }
    List<GameCubeNode> RetracePath(GameCubeNode startNode, GameCubeNode endNode)
    {
        List<GameCubeNode> path = new List<GameCubeNode>();
        GameCubeNode currentNode = endNode;

        while (currentNode != startNode)
        {

            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }

    public List<GameCubeNode> FindPath(Vector3 startPos, Vector3 endPos)
    {
        bool isActive = true;
        GameCubeNode startNode = GetNearestNode(startPos, out isActive);
        if (!isActive) return null;
        GameCubeNode endNode = GetNearestNode(endPos, out isActive);
        if (!isActive) return null;

        List<GameCubeNode> openSet = new List<GameCubeNode>();
        HashSet<GameCubeNode> closedSet = new HashSet<GameCubeNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            GameCubeNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            foreach (GameObject neighbor in currentNode.neighbors)
            {
                if (closedSet.Contains(neighbor.GetComponent<GameCubeNode>())) continue;

                if (neighbor.GetComponent<GameCubeNode>().activated == false) continue;

                if (neighbor.GetComponent<GameCubeNode>().GetInhabitantType() != GameCubeNode.InhabitantType.None) continue;

                //if (neighbor.GetComponent<GameCubeNode>().GetInhabitantType() != GameCubeNode.InhabitantType.None) continue;


                float climbLedgeGcost = 0;

                //if (currentNode.levelType > neighbor.GetComponent<GameCubeNode>().levelType)
                //{
                //    climbLedgeGcost += 8;
                //}

                if (startNode.levelType < neighbor.GetComponent<GameCubeNode>().levelType && neighbor.GetComponent<GameCubeNode>().isLedge && currentNode.levelType < neighbor.GetComponent<GameCubeNode>().levelType) //
                {
                    // neighbor.GetComponent<GameCubeNode>().SetColor();
                    neighbor.GetComponent<GameCubeNode>().isLedge = true;
                    climbLedgeGcost += 5;
                }

                float newGCost = currentNode.gCost + Vector3.Distance(currentNode.transform.position, neighbor.transform.position) + climbLedgeGcost;
                if (newGCost < neighbor.GetComponent<GameCubeNode>().gCost || !openSet.Contains(neighbor.GetComponent<GameCubeNode>()))
                {
                    neighbor.GetComponent<GameCubeNode>().gCost = newGCost;
                    neighbor.GetComponent<GameCubeNode>().hCost = Vector3.Distance(neighbor.transform.position, endNode.transform.position);
                    neighbor.GetComponent<GameCubeNode>().parent = currentNode;

                    if (!openSet.Contains(neighbor.GetComponent<GameCubeNode>()))
                        openSet.Add(neighbor.GetComponent<GameCubeNode>());
                }
            }
        }

        return null; // No path found
    }

    public List<GameCubeNode> GetNeigbhours(Vector3 position)
    {
        bool isActive = true;
        List<GameCubeNode> neighbours = new List<GameCubeNode>();
        GameCubeNode currentNode = GetNearestNode(position, out isActive);

        neighbours.Add(nodeGrid[currentNode.index.x - 1, currentNode.index.y].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x + 1, currentNode.index.y].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x, currentNode.index.y - 1].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x, currentNode.index.y + 1].GetComponent<GameCubeNode>());

        neighbours.Add(nodeGrid[currentNode.index.x - 1, currentNode.index.y + 1].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x + 1, currentNode.index.y - 1].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x + 1, currentNode.index.y + 1].GetComponent<GameCubeNode>());
        neighbours.Add(nodeGrid[currentNode.index.x - 1, currentNode.index.y - 1].GetComponent<GameCubeNode>());

        //foreach (GameCubeNode node in neighbours)
        //{
        //    node.SetColor();
        //}

        return neighbours;
    }

    public List<Portal> BuildPortals(List<GameCubeNode> path)
    {
        List<Portal> portals = new List<Portal>();
        debugPortals.Clear();

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 from = path[i - 1].transform.position;
            Vector3 to = path[i].transform.position;

            Vector3 dir = (to - from).normalized;
            Vector3 perp = Vector3.Cross(Vector3.up, dir) * 1.0f; // Half width of node

            Vector3 center = (from + to) * 0.5f;
            Vector3 left = center - (perp * 0.55f);
            Vector3 right = center + (perp * 0.55f);

            portals.Add(new Portal(left, right));
        }
        portals.Add(new Portal(path[path.Count - 1].transform.position, path[path.Count - 1].transform.position));
        debugPortals = portals;
        return portals;
    }

    public List<Vector3> StringPull(List<Portal> portals, Vector3 startPos, Vector3 endPos)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(startPos);

        int portalCount = portals.Count;

        // Funnel state
        Vector3 portalApex = startPos;
        Vector3 portalLeft = portals[0].right;
        Vector3 portalRight = portals[0].left;

        int apexIndex = 0;
        int leftIndex = 0;
        int rightIndex = 0;

        for (int i = 1; i < portalCount; i++)
        {
            Vector3 left = portals[i].right;
            Vector3 right = portals[i].left;

            // --- RIGHT Check ---
            if (TriangleArea2(portalApex, portalRight, right) <= 0)
            {
                if (portalApex == portalRight || TriangleArea2(portalApex, portalLeft, right) > 0)
                {
                    // Tighten the right
                    portalRight = right;
                    rightIndex = i;
                }
                else
                {
                    // Crossed over the left leg → insert left and reset
                    path.Add(portalLeft);
                    portalApex = portalLeft;
                    apexIndex = leftIndex;

                    // Reset portal legs
                    portalLeft = portalApex;
                    portalRight = portalApex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    i = apexIndex;
                    continue;
                }
            }

            // --- LEFT Check ---
            if (TriangleArea2(portalApex, portalLeft, left) >= 0)
            {
                if (portalApex == portalLeft || TriangleArea2(portalApex, portalRight, left) < 0)
                {
                    // Tighten the left
                    portalLeft = left;
                    leftIndex = i;
                }
                else
                {
                    // Crossed over the right leg → insert right and reset
                    path.Add(portalRight);
                    portalApex = portalRight;
                    apexIndex = rightIndex;

                    // Reset portal legs
                    portalLeft = portalApex;
                    portalRight = portalApex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    i = apexIndex;
                    continue;
                }
            }
        }

        // Finally, add the end position
        path.Add(endPos);
        debugPath = path;
        return path;
    }

    float TriangleArea2(Vector3 a, Vector3 b, Vector3 c)
    {
        return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
    }


    public GameCubeNode RandomSpawn()
    {
        GameCubeNode spawnNode = null;
        bool loop = true;
        while (loop)
        {
            int randomX = UnityEngine.Random.Range(0, width - 1);
            int randomY = UnityEngine.Random.Range(0, length - 1);

            if (nodeGrid[randomX, randomY].GetComponent<GameCubeNode>().activated && nodeGrid[randomX, randomY].GetComponent<GameCubeNode>().GetInhabitantType() == GameCubeNode.InhabitantType.None)
            {
                loop = false;
                spawnNode = nodeGrid[randomX, randomY].GetComponent<GameCubeNode>();
            }
        }

        return spawnNode;

    }


    public List<GameCubeNode> CreateGroupRandomSpawn()
    {
        while (true)
        {
            List<GameCubeNode> group = new List<GameCubeNode>();
            GameCubeNode spawnNode = null;

            int randomX = UnityEngine.Random.Range(0, width);
            int randomY = UnityEngine.Random.Range(0, length);

            var candidate = nodeGrid[randomX, randomY].GetComponent<GameCubeNode>();
            if (!candidate.activated || candidate.GetInhabitantType() != GameCubeNode.InhabitantType.None)
                continue;

            spawnNode = candidate;
            group.Add(spawnNode);

            // Define directions for adjacency (up, down, left, right, and diagonals)
            Vector2Int[] directions = new Vector2Int[]
            {
            new Vector2Int(3, 0),
            new Vector2Int(-3, 0),
            new Vector2Int(0, 3),
            new Vector2Int(0, -3),
            new Vector2Int(3, 3),
            new Vector2Int(-3, -3),
            new Vector2Int(3, -3),
            new Vector2Int(-3, 3),
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2),
            new Vector2Int(2, 2),
            new Vector2Int(-2, -2),
            new Vector2Int(2, -2),
            new Vector2Int(-2, 2),
            new Vector2Int(2, 1),
            new Vector2Int(-2, 1),
            new Vector2Int(1, 2),
            new Vector2Int(1, -2),
            };

            List<GameCubeNode> neighbors = new List<GameCubeNode>();

            // Shuffle directions to get random neighbors
            directions = directions.OrderBy(d => UnityEngine.Random.value).ToArray();

            foreach (var dir in directions)
            {
                int newX = randomX + dir.x;
                int newY = randomY + dir.y;

                if (newX >= 0 && newX < width && newY >= 0 && newY < length)
                {
                    var neighbor = nodeGrid[newX, newY].GetComponent<GameCubeNode>();
                    if (neighbor.activated && neighbor.GetInhabitantType() == GameCubeNode.InhabitantType.None)
                    {
                        neighbors.Add(neighbor);
                        if (neighbors.Count == 6)
                            break;
                    }
                }
            }

            // If not enough valid neighbors, restart
            if (neighbors.Count < 6)
                continue;

            group.AddRange(neighbors);
            return group;
        }
    }

    public List<GameCubeNode> GetReachableNodes(Vector3 startPos, float maxDistance)
    {
        List<GameCubeNode> reachableNodes = new List<GameCubeNode>();
        List<GameCubeNode> unreachableNodes = new List<GameCubeNode>();
        Queue<(GameCubeNode node, float cost)> frontier = new Queue<(GameCubeNode, float)>();
        HashSet<GameCubeNode> visited = new HashSet<GameCubeNode>();

        bool isActive;
        GameCubeNode startNode = GetNearestNode(startPos, out isActive);
        if (!isActive) return reachableNodes;

        frontier.Enqueue((startNode, 0));
        visited.Add(startNode);

        float totalCost = 0;
        
        while (frontier.Count > 0)
        {
            var (currentNode, currentCost) = frontier.Dequeue();
            currentNode.reachableValue = currentCost;
            reachableNodes.Add(currentNode);

            foreach (GameObject neighborObj in currentNode.neighbors)
            {
                GameCubeNode neighbor = neighborObj.GetComponent<GameCubeNode>();

                // Skip if not active or already visited
                if (!neighbor.activated || visited.Contains(neighbor) || neighbor.GetInhabitantType() != GameCubeNode.InhabitantType.None) continue;

                float movementCost = 1;

                if (startNode.levelType < neighbor.levelType && neighbor.isLedge && currentNode.levelType < neighbor.levelType)
                {
                    //neighbor.SetColor();
                    neighbor.isLedge = true;
                    movementCost += 3;
                }

                totalCost = currentCost + movementCost;

                if (totalCost <= maxDistance)
                {
                    frontier.Enqueue((neighbor, totalCost));
                    visited.Add(neighbor);
                }
                else
                {
                    unreachableNodes.Add(neighbor);
                }
            }
        }

        return reachableNodes;
    }
}
