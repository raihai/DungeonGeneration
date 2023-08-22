using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class DugeonGenerator
{
    List<RoomNode> nodeCollection = new List<RoomNode>();
    private int dungeonWidth;
    private int dungeonLength;

    public DugeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
    }

    public List<Node> CalculateDungeon(int maxIterations, 
    int roomWidthMin, int roomLengthMin, float roomBotMod, 
    float roomTopMod, int roomOffset, int corridorWidth)
    {
        BinarySpacePartition2D bsp = new BinarySpacePartition2D(dungeonWidth, dungeonLength);
        nodeCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        List<Node> roomSpaces = Helper2D.ExtractLowestLeafNodes(bsp.RootNode);

        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBotMod, roomTopMod, roomOffset);

        CorridorsGenerator corridorGenerator = new CorridorsGenerator();
        var corridorList = corridorGenerator.CreateCorridor(nodeCollection, corridorWidth);
        
        return new List<Node>(roomList).Concat(corridorList).ToList();
    }
}