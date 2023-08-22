using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Helper2D
{
    public static List<Node> ExtractLowestLeafNodes(Node parentNode)
    {
        Queue<Node> nodesToCheck = new Queue<Node>();
        List<Node> leafNodes = new List<Node>();

        if (parentNode.NodeChildList.Count == 0)
        {
            return new List<Node>() { parentNode };
        }

        foreach (var child in parentNode.NodeChildList)
        {
            nodesToCheck.Enqueue(child);
        }

        while (nodesToCheck.Count > 0)
        {
            var currentNode = nodesToCheck.Dequeue();

            if (currentNode.NodeChildList.Count == 0)
            {
                leafNodes.Add(currentNode);
            }
            else
            {
                foreach (var child in currentNode.NodeChildList)
                {
                    nodesToCheck.Enqueue(child);
                }
            }
        }

        return leafNodes;
    }

    public static Vector2Int GenerateBotLeft(Vector2Int leftBoundary, Vector2Int rightBoundary, float pointModifier, int offset)
    {
        int minX = leftBoundary.x + offset;
        int maxX = rightBoundary.x - offset;
        int minY = leftBoundary.y + offset;
        int maxY = rightBoundary.y - offset;

        return new Vector2Int(
            Random.Range(minX, (int)(minX + (maxX - minX) * pointModifier)),
            Random.Range(minY, (int)(minY + (maxY - minY) * pointModifier))
        );
    }

    public static Vector2Int GenerateTopRight(Vector2Int leftBoundary, Vector2Int rightBoundary, float pointModifier, int offset)
    {
        int minX = leftBoundary.x + offset;
        int maxX = rightBoundary.x - offset;
        int minY = leftBoundary.y + offset;
        int maxY = rightBoundary.y - offset;

        return new Vector2Int(
            Random.Range((int)(minX + (maxX - minX) * pointModifier), maxX),
            Random.Range((int)(minY + (maxY - minY) * pointModifier), maxY)
        );
    }

    public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2)
    {
        Vector2 sum = v1 + v2;
        Vector2 tempVector = sum / 2;

        return new Vector2Int((int)tempVector.x, (int)tempVector.y);
    }
}

public enum RelativePosition
{
    Up,
    Down,
    Right,
    Left,
    Forward,
    Backward
}