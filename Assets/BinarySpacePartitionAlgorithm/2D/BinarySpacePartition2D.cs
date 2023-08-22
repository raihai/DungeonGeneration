using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class BinarySpacePartition2D
{
    RoomNode rootNode;

    public RoomNode RootNode { get => rootNode; }
    public BinarySpacePartition2D(int dungeonWidth, int dungeonLength)
    {
        this.rootNode = new RoomNode(new Vector2Int(0, 0), new Vector2Int(dungeonWidth, dungeonLength), null, 0);
    }

    public List<RoomNode> PrepareNodesCollection(int maxIterations, int roomWidthMin, int roomLengthMin)
    {
        Queue<RoomNode> graph = new Queue<RoomNode>();
        List<RoomNode> listToReturn = new List<RoomNode>();
        graph.Enqueue(this.rootNode);
        listToReturn.Add(this.rootNode);
        int iterations = 0;
        while (iterations<maxIterations && graph.Count>0)
        {
            iterations++;
            RoomNode currentNode = graph.Dequeue();
            if(currentNode.Width>=roomWidthMin*2 || currentNode.Length >= roomLengthMin * 2)
            {
                SplitTheSpace(currentNode, listToReturn, roomLengthMin, roomWidthMin, graph);
            }
        }
        return listToReturn;
    }

    private void SplitTheSpace(RoomNode currentNode, List<RoomNode> listToReturn, int roomLengthMin, int roomWidthMin, Queue<RoomNode> graph)
    {
        Line line = GetLineDividingSpace(
            currentNode.BotLeft,
            currentNode.TopRight,
            roomWidthMin,
            roomLengthMin);
        RoomNode node1, node2;
        if(line.Orientation == Orientation.Horizontal)
        {
            node1 = new RoomNode(currentNode.BotLeft,
                new Vector2Int(currentNode.TopRight.x, line.Coordinates.y),
                currentNode,
                currentNode.TreeLayerInt + 1);
            node2 = new RoomNode(new Vector2Int(currentNode.BotLeft.x, line.Coordinates.y),
                currentNode.TopRight,
                currentNode,
                currentNode.TreeLayerInt + 1);
        }
        else
        {
            node1 = new RoomNode(currentNode.BotLeft,
                new Vector2Int(line.Coordinates.x,currentNode.TopRight.y),
                currentNode,
                currentNode.TreeLayerInt + 1);
            node2 = new RoomNode(new Vector2Int(line.Coordinates.x,currentNode.BotLeft.y),
                currentNode.TopRight,
                currentNode,
                currentNode.TreeLayerInt + 1);
        }
        AddNewNodeToCollections(listToReturn, graph, node1);
        AddNewNodeToCollections(listToReturn, graph, node2);
    }

    private void AddNewNodeToCollections(List<RoomNode> listToReturn, Queue<RoomNode> graph, RoomNode node)
    {
        listToReturn.Add(node);
        graph.Enqueue(node);
    }

    private Line GetLineDividingSpace(Vector2Int BotLeft, Vector2Int TopRight, int roomWidthMin, int roomLengthMin)
    {
        Orientation orientation;
        bool lengthStatus = (TopRight.y - BotLeft.y) >= 2 * roomLengthMin;
        bool widthStatus = (TopRight.x - BotLeft.x) >= 2*roomWidthMin;
        if (lengthStatus && widthStatus)
        {
            orientation = (Orientation)(Random.Range(0,2));
        }else if (widthStatus)
        {
            orientation = Orientation.Vertical;
        }
        else
        {
            orientation = Orientation.Horizontal;
        }
        return new Line(orientation, GetOrientCoords(
            orientation,
            BotLeft,
            TopRight,
            roomWidthMin,
            roomLengthMin));
    }

    private Vector2Int GetOrientCoords(Orientation orientation, Vector2Int BotLeft, Vector2Int TopRight, int roomWidthMin, int roomLengthMin)
    {
        Vector2Int coordinates = Vector2Int.zero;
        if (orientation == Orientation.Horizontal)
        {
            coordinates = new Vector2Int(
                0, 
                Random.Range(
                (BotLeft.y + roomLengthMin),
                (TopRight.y - roomLengthMin)));
        }
        else
        {
            coordinates = new Vector2Int(
                Random.Range(
                (BotLeft.x + roomWidthMin),
                (TopRight.x - roomWidthMin))
                ,0);
        }
        return coordinates;
    }
}