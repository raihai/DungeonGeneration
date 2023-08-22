using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BinarySpacePartitioner3D
{
    RoomNode3D rootNode;

    public RoomNode3D RootNode { get => rootNode; }
    public BinarySpacePartitioner3D(int dungeonWidth, int dungeonLength, int dungeonHeight)
    {
        this.rootNode = new RoomNode3D(new Vector3Int(0, 0, 0), new Vector3Int(dungeonWidth, dungeonHeight, dungeonLength), null, 0);
    }

public List<RoomNode3D> PrepareNodesCollection(int maxIterations, int roomWidthMin, int roomLengthMin, int roomHeightMin)    {
        Queue<RoomNode3D> graph = new Queue<RoomNode3D>();
        List<RoomNode3D> listToReturn = new List<RoomNode3D>();
        graph.Enqueue(this.rootNode);
        listToReturn.Add(this.rootNode);
        int iterations = 0;
        while (iterations < maxIterations && graph.Count > 0)
        {
            iterations++;
            RoomNode3D currentNode = graph.Dequeue();
            if (currentNode.Width >= roomWidthMin * 2 || currentNode.Length >= roomLengthMin * 2 || currentNode.Height >= roomHeightMin * 2)
            {
                SplitTheSpace(currentNode, listToReturn, roomLengthMin, roomWidthMin, roomHeightMin, graph);
            }
        }
        return listToReturn;
    }

    private void SplitTheSpace(RoomNode3D currentNode, List<RoomNode3D> listToReturn, int roomLengthMin, int roomWidthMin, int roomHeightMin, Queue<RoomNode3D> graph)
    {
        Axis axis = GetAxisToSplit(currentNode, roomWidthMin, roomLengthMin, roomHeightMin);
        int splitPosition = GetSplitPosition(currentNode, axis, roomWidthMin, roomLengthMin, roomHeightMin);

        RoomNode3D node1, node2;
        Vector3Int splitPoint = Vector3Int.zero;
        switch (axis)
        {
            case Axis.X:
                splitPoint = new Vector3Int(splitPosition, currentNode.BotLeft.y, currentNode.BotLeft.z);
                node1 = new RoomNode3D(currentNode.BotLeft, new Vector3Int(splitPoint.x, currentNode.TopRight.y, currentNode.TopRight.z), currentNode, currentNode.TreeLayerInt + 1);
                node2 = new RoomNode3D(new Vector3Int(splitPoint.x, currentNode.BotLeft.y, currentNode.BotLeft.z), currentNode.TopRight, currentNode, currentNode.TreeLayerInt + 1);
                break;
            case Axis.Y:
                splitPoint = new Vector3Int(currentNode.BotLeft.x, splitPosition, currentNode.BotLeft.z);
                node1 = new RoomNode3D(currentNode.BotLeft, new Vector3Int(currentNode.TopRight.x, splitPoint.y, currentNode.TopRight.z), currentNode, currentNode.TreeLayerInt + 1);
                node2 = new RoomNode3D(new Vector3Int(currentNode.BotLeft.x, splitPoint.y, currentNode.BotLeft.z), currentNode.TopRight, currentNode, currentNode.TreeLayerInt + 1);
                break;
            default: 
                splitPoint = new Vector3Int(currentNode.BotLeft.x, currentNode.BotLeft.y, splitPosition);
                node1 = new RoomNode3D(currentNode.BotLeft, new Vector3Int(currentNode.TopRight.x, currentNode.TopRight.y, splitPoint.z), currentNode, currentNode.TreeLayerInt + 1);

                node2 = new RoomNode3D(new Vector3Int(currentNode.BotLeft.x, currentNode.BotLeft.y, splitPoint.z), currentNode.TopRight, currentNode, currentNode.TreeLayerInt + 1);
                break;
        }

        AddNewNodeToCollections(listToReturn, graph, node1);
        AddNewNodeToCollections(listToReturn, graph, node2);
    }

    private void AddNewNodeToCollections(List<RoomNode3D> listToReturn, Queue<RoomNode3D> graph, RoomNode3D node)
    {
        listToReturn.Add(node);
        graph.Enqueue(node);
    }

    private Axis GetAxisToSplit(RoomNode3D currentNode, int roomWidthMin, int roomLengthMin, int roomHeightMin)
    {
        List<Axis> validAxes = new List<Axis>();

        if (currentNode.Width >= roomWidthMin * 2)
        {
            validAxes.Add(Axis.X);
        }

        if (currentNode.Height >= roomHeightMin * 2)
        {
            validAxes.Add(Axis.Y);
        }

        if (currentNode.Length >= roomLengthMin * 2)
        {
            validAxes.Add(Axis.Z);
        }

        return validAxes[Random.Range(0, validAxes.Count)];
    }

    private int GetSplitPosition(RoomNode3D currentNode, Axis axis, int roomWidthMin, int roomLengthMin, int roomHeightMin)
    {
        int splitPosition = 0;

        switch (axis)
        {
            case Axis.X:
                splitPosition = Random.Range(currentNode.BotLeft.x + roomWidthMin, currentNode.TopRight.x - roomWidthMin);
                break;
            case Axis.Y:
                splitPosition = Random.Range(currentNode.BotLeft.y + roomHeightMin, currentNode.TopRight.y - roomHeightMin);
                break;
            case Axis.Z:
                splitPosition = Random.Range(currentNode.BotLeft.z + roomLengthMin, currentNode.TopRight.z - roomLengthMin);
                break;
        }

        return splitPosition;
    }

}

public enum Axis
{
    X,
    Y,
    Z
}