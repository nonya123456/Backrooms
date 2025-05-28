using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;

    private enum NodeType
    {
        Unvisited,
        Frontier,
        Visited,
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        var frontier = new List<Vector2Int>();
        if (frontier == null) throw new ArgumentNullException(nameof(frontier));

        var nodes = new NodeType[width, height];
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));

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

        var count = 0;
        while (frontier.Count > 0)
        {
            var index = Random.Range(0, frontier.Count);
            var node = frontier[index];
            frontier.RemoveAt(index);

            var neighbors = GetVisitedNeighbors(node.x, node.y, nodes);
            if (neighbors.Count > 0)
            {
                var selectedNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                Debug.Log($"Moving from {node} to {selectedNeighbor}");
            }

            nodes[node.x, node.y] = NodeType.Visited;
            AddFrontier(node.x - 1, node.y, frontier, nodes);
            AddFrontier(node.x + 1, node.y, frontier, nodes);
            AddFrontier(node.x, node.y - 1, frontier, nodes);
            AddFrontier(node.x, node.y + 1, frontier, nodes);
            count++;
        }

        Debug.Log($"Generated map with {count} nodes");
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
