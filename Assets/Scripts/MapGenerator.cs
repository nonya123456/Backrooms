using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellHeight;
    [Range(0, 1)] [SerializeField] private float wallSpawnChance = 1f;
    [SerializeField] private int orbCount;
    [SerializeField] private float orbY;
    [SerializeField] private int waypointCount;

    [field: ReadOnly] [field: SerializeField] public GameObject Map { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject ceilingPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject orbPrefab;

    [Header("References")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private MonsterAI monsterAI;

    private enum NodeType
    {
        Unvisited,
        Frontier,
        Visited,
    }

    public void ResetConfig(int seed0, int width0, int height0, int orbCount0, int waypointCount0)
    {
        seed = seed0;
        width = width0;
        height = height0;
        orbCount = orbCount0;
        waypointCount = waypointCount0;

        if (Map)
        {
            Destroy(Map);
        }
    }

    public void GenerateMap()
    {
        Random.InitState(seed);

        var edges = new HashSet<EdgeData>();
        var frontier = new List<Vector2Int>();
        var nodes = new NodeType[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                nodes[x, y] = NodeType.Unvisited;
            }
        }

        var startX = Random.Range(0, width);
        var startY = Random.Range(0, height);
        AddFrontier(startX, startY, frontier, nodes);

        while (frontier.Count > 0)
        {
            var index = Random.Range(0, frontier.Count);
            var node = frontier[index];
            frontier.RemoveAt(index);

            var neighbors = GetVisitedNeighbors(node.x, node.y, nodes);
            if (neighbors.Count > 0)
            {
                var selectedNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                if (node.x < selectedNeighbor.x || (node.x == selectedNeighbor.x && node.y < selectedNeighbor.y))
                {
                    edges.Add(new EdgeData { From = node, To = selectedNeighbor });
                }
                else
                {
                    edges.Add(new EdgeData { From = selectedNeighbor, To = node });
                }
            }

            nodes[node.x, node.y] = NodeType.Visited;
            AddFrontier(node.x - 1, node.y, frontier, nodes);
            AddFrontier(node.x + 1, node.y, frontier, nodes);
            AddFrontier(node.x, node.y - 1, frontier, nodes);
            AddFrontier(node.x, node.y + 1, frontier, nodes);
        }

        InstantiateMap(edges);

        var orbGridPositions = SampleRange(0, width * height, orbCount);
        InstantiateOrbs(orbGridPositions);
        SetPlayerPosition(orbGridPositions);

        var waypointGridPositions = SampleRange(0, width * height, waypointCount);
        var waypoints = InstantiateWaypoints(waypointGridPositions);
        monsterAI.SetWaypoints(waypoints.transform);
    }

    public void BuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

    private void InstantiateMap(HashSet<EdgeData> edges)
    {
        Map = new GameObject("Map");
        var mapWidth = width + 1 + width * cellSize;
        var mapHeight = height + 1 + height * cellSize;

        var floor = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity);
        floor.transform.parent = Map.transform;
        floor.transform.localScale = new Vector3(mapWidth, 1, mapHeight);

        var ceiling = Instantiate(ceilingPrefab, new Vector3(0, cellHeight, 0), Quaternion.identity);
        ceiling.transform.parent = Map.transform;
        ceiling.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
        ceiling.layer = LayerMask.NameToLayer("Ceiling");

        var outerWallUp = Instantiate(wallPrefab, new Vector3(0, cellHeight / 2, (mapHeight - 1) / 2),
            Quaternion.identity);
        outerWallUp.transform.parent = Map.transform;
        outerWallUp.transform.localScale = new Vector3(mapWidth, cellHeight + 1, 1);

        var outerWallDown = Instantiate(wallPrefab, new Vector3(0, cellHeight / 2, -(mapHeight - 1) / 2),
            Quaternion.identity);
        outerWallDown.transform.parent = Map.transform;
        outerWallDown.transform.localScale = new Vector3(mapWidth, cellHeight + 1, 1);

        var outerWallLeft = Instantiate(wallPrefab, new Vector3(-(mapWidth - 1) / 2, cellHeight / 2, 0),
            Quaternion.identity);
        outerWallLeft.transform.parent = Map.transform;
        outerWallLeft.transform.localScale = new Vector3(1, cellHeight + 1, mapHeight);

        var outerWallRight = Instantiate(wallPrefab, new Vector3((mapWidth - 1) / 2, cellHeight / 2, 0),
            Quaternion.identity);
        outerWallRight.transform.parent = Map.transform;
        outerWallRight.transform.localScale = new Vector3(1, cellHeight + 1, mapHeight);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (y < height - 1 &&
                    !edges.Contains(new EdgeData() { From = new Vector2Int(x, y), To = new Vector2Int(x, y + 1) }) &&
                    Random.value < wallSpawnChance)
                {
                    var posX = (cellSize + 1) * (x + 1) - cellSize / 2 - mapWidth / 2;
                    var posY = (cellSize + 1) * (y + 1) + 0.5f - mapHeight / 2;
                    var innerWall = Instantiate(wallPrefab, new Vector3(posX, cellHeight / 2, posY),
                        Quaternion.identity);
                    innerWall.transform.parent = Map.transform;
                    innerWall.transform.localScale = new Vector3(cellSize + 2, cellHeight + 1, 1);
                }

                if (x < width - 1 &&
                    !edges.Contains(new EdgeData() { From = new Vector2Int(x, y), To = new Vector2Int(x + 1, y) }) &&
                    Random.value < wallSpawnChance)
                {
                    var posX = (cellSize + 1) * (x + 1) + 0.5f - mapWidth / 2;
                    var posY = (cellSize + 1) * (y + 1) - cellSize / 2 - mapHeight / 2;
                    var innerWall = Instantiate(wallPrefab, new Vector3(posX, cellHeight / 2, posY),
                        Quaternion.identity);
                    innerWall.transform.parent = Map.transform;
                    innerWall.transform.localScale = new Vector3(1, cellHeight + 1, cellSize + 2);
                }
            }
        }
    }

    private void InstantiateOrbs(int[] gridPositions)
    {
        var mapWidth = width + 1 + width * cellSize;
        var mapHeight = height + 1 + height * cellSize;

        foreach (var gridPosition in gridPositions)
        {
            var x = gridPosition % width;
            var y = gridPosition / width;
            var posX = (cellSize + 1) * (x + 1) - cellSize / 2 - mapWidth / 2;
            var posY = (cellSize + 1) * (y + 1) - cellSize / 2 - mapHeight / 2;
            var orb = Instantiate(orbPrefab, new Vector3(posX, orbY, posY), Quaternion.identity);
            orb.transform.parent = Map.transform;
        }
    }

    private GameObject InstantiateWaypoints(int[] gridPositions)
    {
        var waypoints = new GameObject("Waypoints")
        {
            transform =
            {
                parent = Map.transform
            }
        };

        var mapWidth = width + 1 + width * cellSize;
        var mapHeight = height + 1 + height * cellSize;

        foreach (var gridPosition in gridPositions)
        {
            var x = gridPosition % width;
            var y = gridPosition / width;
            var posX = (cellSize + 1) * (x + 1) - cellSize / 2 - mapWidth / 2;
            var posY = (cellSize + 1) * (y + 1) - cellSize / 2 - mapHeight / 2;
            _ = new GameObject("Waypoint")
            {
                transform =
                {
                    parent = waypoints.transform,
                    localPosition = new Vector3(posX, 0.5f, posY)
                }
            };
        }

        return waypoints;
    }

    private void SetPlayerPosition(int[] orbGridPositions)
    {
        var positions = new HashSet<int>();
        positions.AddRange(orbGridPositions);

        while (true)
        {
            var randomValue = Random.Range(0, width * height);
            if (positions.Contains(randomValue))
            {
                continue;
            }

            var mapWidth = width + 1 + width * cellSize;
            var mapHeight = height + 1 + height * cellSize;

            var x = randomValue % width;
            var y = randomValue / width;
            var posX = (cellSize + 1) * (x + 1) - cellSize / 2 - mapWidth / 2;
            var posY = (cellSize + 1) * (y + 1) - cellSize / 2 - mapHeight / 2;
            playerTransform.position = new Vector3(posX, 1.0f, posY);
            cameraController.Reset();
            return;
        }
    }

    private static int[] SampleRange(int minInclusive, int maxExclusive, int count)
    {
        var selected = new HashSet<int>();
        while (selected.Count < count)
        {
            var randomValue = Random.Range(minInclusive, maxExclusive);
            selected.Add(randomValue);
        }

        var result = new int[count];
        selected.CopyTo(result);
        return result;
    }

    private void AddFrontier(int x, int y, List<Vector2Int> frontier, NodeType[,] nodes)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }

        if (nodes[x, y] != NodeType.Unvisited)
        {
            return;
        }

        frontier.Add(new Vector2Int(x, y));
        nodes[x, y] = NodeType.Frontier;
    }

    private List<Vector2Int> GetVisitedNeighbors(int x, int y, NodeType[,] nodes)
    {
        var neighbors = new List<Vector2Int>();
        if (x > 0 && nodes[x - 1, y] == NodeType.Visited)
        {
            neighbors.Add(new Vector2Int(x - 1, y));
        }

        if (x < width - 1 && nodes[x + 1, y] == NodeType.Visited)
        {
            neighbors.Add(new Vector2Int(x + 1, y));
        }

        if (y > 0 && nodes[x, y - 1] == NodeType.Visited)
        {
            neighbors.Add(new Vector2Int(x, y - 1));
        }

        if (y < height - 1 && nodes[x, y + 1] == NodeType.Visited)
        {
            neighbors.Add(new Vector2Int(x, y + 1));
        }

        return neighbors;
    }
}

public struct EdgeData : IEquatable<EdgeData>
{
    public Vector2Int From;
    public Vector2Int To;

    public bool Equals(EdgeData other)
    {
        return From.Equals(other.From) && To.Equals(other.To);
    }

    public override bool Equals(object obj)
    {
        return obj is EdgeData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }
}
