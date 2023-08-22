using System;
using System.Collections.Generic;
using System.Linq;

public class CorridorsGenerator
{
    public List<Node> CreateCorridor(List<RoomNode> allNodesCollection, int corridorWidth)
    {
        List<Node> corridorList = new List<Node>();
        Queue<RoomNode> structuresToCheck = new Queue<RoomNode>(
            allNodesCollection.OrderByDescending(node => node.TreeLayerInt).ToList());
        while (structuresToCheck.Count > 0)
        {
            var node = structuresToCheck.Dequeue();
            if (node.NodeChildList.Count == 0)
            {
                continue;
            }
            CorridorNode2D corridor = new CorridorNode2D(node.NodeChildList[0], node.NodeChildList[1], corridorWidth);
            corridorList.Add(corridor);
        }
        return corridorList;
    }
}