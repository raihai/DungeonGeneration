using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class StructureHelper3D
{
    public static List<Node3D> ExtractLowestLeafNodes(Node3D parentNode)
    {
        Queue<Node3D> nodesToCheck = new Queue<Node3D>();
        List<Node3D> leafNodes = new List<Node3D>();

        if (parentNode.NodeChildList.Count == 0)
        {
            return new List<Node3D>() { parentNode };
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

    public static Vector3Int GenerateBotLeft(Vector3Int leftBoundary, Vector3Int rightBoundary, float pointModifier, int offset)
    {
        int minX = leftBoundary.x + offset;
        int maxX = rightBoundary.x - offset;
        int minY = leftBoundary.y + offset;
        int maxY = rightBoundary.y - offset;
        int minZ = leftBoundary.z + offset;
        int maxZ = rightBoundary.z - offset;

        return new Vector3Int(
            Random.Range(minX, (int)(minX + (maxX - minX) * pointModifier)),
            Random.Range(minY, (int)(minY + (maxY - minY) * pointModifier)),
            Random.Range(minZ, (int)(minZ + (maxZ - minZ) * pointModifier))
        );
    }

    public static Vector3Int GenerateTopRight(Vector3Int leftBoundary, Vector3Int rightBoundary, float pointModifier, int offset)
    {
        int minX = leftBoundary.x + offset;
        int maxX = rightBoundary.x - offset;
        int minY = leftBoundary.y + offset;
        int maxY = rightBoundary.y - offset;
        int minZ = leftBoundary.z + offset;
        int maxZ = rightBoundary.z - offset;

        return new Vector3Int(
            Random.Range((int)(minX + (maxX - minX) * pointModifier), maxX),
            Random.Range((int)(minY + (maxY - minY) * pointModifier), maxY),
            Random.Range((int)(minZ + (maxZ - minZ) * pointModifier), maxZ)
        );
    }

    public static Vector3Int CalculateMiddlePoint(Vector3Int v1, Vector3Int v2)
    {
        Vector3 sum = v1 + v2;
        Vector3 tempVector = sum / 2;

        return new Vector3Int((int)tempVector.x, (int)tempVector.y, (int)tempVector.z);
    }
}

public enum RelativePosition3D
{
    Up,
    Down,
    Right,
    Left,
    Forward,
    Backward
}