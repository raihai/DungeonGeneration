using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorGen3D
{
    public List<Node3D> CreateCorridor(List<RoomNode3D> allNodesCollection, float corridorWidth, float corridorHeight, float corridorDepth)
    {
        List<Node3D> corridorList = new List<Node3D>();
        HashSet<Tuple<RoomNode3D, RoomNode3D>> connectedNodes = new HashSet<Tuple<RoomNode3D, RoomNode3D>>();

        Queue<RoomNode3D> structuresToCheck = new Queue<RoomNode3D>(
        allNodesCollection.OrderByDescending(node => node.Size).ToList());

        while (structuresToCheck.Count > 0)
        {
            var node = structuresToCheck.Dequeue();
            if (node.NodeChildList.Count == 0)
            {
                continue;
            }

            RoomNode3D child1 = (RoomNode3D)node.NodeChildList[0];
            RoomNode3D child2 = (RoomNode3D)node.NodeChildList[1];

            // Ensure corridors connect at the center of room edges.
            Vector3 connectionPoint1 = new Vector3(child1.TopRight.x, (child1.BotLeft.y + child1.TopRight.y) / 2, child1.TopRight.z);
            Vector3 connectionPoint2 = new Vector3(child2.BotLeft.x, (child2.BotLeft.y + child2.TopRight.y) / 2, child2.BotLeft.z);

            // Check if a corridor already exists between the two nodes.
            var nodesTuple = Tuple.Create(child1, child2);
            if (!connectedNodes.Contains(nodesTuple))
            {
                CorridorNode3D corridor = new CorridorNode3D(child1, child2, (int)corridorWidth, (int)corridorHeight, (int)corridorDepth);
                corridorList.Add(corridor);
                connectedNodes.Add(nodesTuple);
            }
        }
        return corridorList;
    }
}