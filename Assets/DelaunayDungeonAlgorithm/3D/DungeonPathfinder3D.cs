using System;
using System.Collections.Generic;
using UnityEngine;
using BlueRaja;

public class DungeonPathfinder3D
{
    public class Node
    {
        public Vector3Int Position { get; private set; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector3Int position)
        {
            Position = position;
        }
    }

    public struct PathCost
    {
        public bool traversable;
        public float cost;
    }

    static readonly Vector3Int[] neighbors = {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
    };

    Grid3D<Node> grid;
    SimplePriorityQueue<Node, float> queue;
    HashSet<Node> closed;

    public DungeonPathfinder3D(Vector3Int size)
    {
        grid = new Grid3D<Node>(size, Vector3Int.zero);

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    grid[x, y, z] = new Node(new Vector3Int(x, y, z));
                }
            }
        }
    }

    void ResetNodes()
    {
        var size = grid.Size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    var node = grid[x, y, z];
                    node.Previous = null;
                    node.Cost = float.PositiveInfinity;
                }
            }
        }
    }

    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, Func<Node, Node, PathCost> costFunction)
    {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        grid[start].Cost = 0;
        queue.Enqueue(grid[start], 0);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            closed.Add(node);

            if (node.Position == end)
            {
                return ReconstructPath(node);
            }

            foreach (var offset in neighbors)
            {
                if (!grid.InBounds(node.Position + offset)) continue;
                var neighbor = grid[node.Position + offset];
                if (closed.Contains(neighbor)) continue;

                var pathCost = costFunction(node, neighbor);
                if (!pathCost.traversable) continue;

                float newCost = node.Cost + pathCost.cost;

                if (newCost < neighbor.Cost)
                {
                    neighbor.Previous = node;
                    neighbor.Cost = newCost;

                    if (queue.TryGetPriority(node, out float existingPriority))
                    {
                        queue.UpdatePriority(node, newCost);
                    }
                    else
                    {
                        queue.Enqueue(neighbor, neighbor.Cost);
                    }
                }
            }
        }

        return null;
    }
    List<Vector3Int> ReconstructPath(Node node)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        Stack<Vector3Int> stack = new Stack<Vector3Int>();

        while (node != null)
        {
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0)
        {
            result.Add(stack.Pop());
        }

        return result;
    }
}