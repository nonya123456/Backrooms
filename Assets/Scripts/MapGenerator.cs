using System;
using System.Collections.Generic;
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

    [field: ReadOnly] [field: SerializeField] public GameObject Map { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject ceilingPrefab;
    [SerializeField] private GameObject floorPrefab;

    private enum NodeType
    {
        Unvisited,
        Frontier,
        Visited,
    }

    private void Start()
    {
        if (!Map)
        {
            GenerateMap();
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
