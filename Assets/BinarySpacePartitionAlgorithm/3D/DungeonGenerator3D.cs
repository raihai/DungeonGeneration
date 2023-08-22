using System;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator3D
{
    List<RoomNode3D> nodeCollection = new List<RoomNode3D>();
    private int dungeonWidth;
    private int dungeonLength;
    private int dungeonHeight;

    public DungeonGenerator3D(int dungeonWidth, int dungeonLength, int dungeonHeight)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
        this.dungeonHeight = dungeonHeight;
    }

    public List<Node3D> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, int roomHeightMin, float roomBotMod, float roomTopMod, int roomOffset, int corridorWidth, float corridorHeight, float corridorDepth)
    {
        BinarySpacePartitioner3D bsp = new BinarySpacePartitioner3D(dungeonWidth, dungeonLength, dungeonHeight);
        nodeCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin, roomHeightMin);
        List<Node3D> roomSpaces = StructureHelper3D.ExtractLowestLeafNodes(bsp.RootNode);

        RoomGenerator3D roomGenerator = new RoomGenerator3D(maxIterations, roomLengthMin, roomWidthMin, roomHeightMin);
        List < RoomNode3D>roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBotMod, roomTopMod, roomOffset);
        CorridorGen3D corridorGenerator = new CorridorGen3D();
        var corridorList = corridorGenerator.CreateCorridor(nodeCollection, corridorWidth, corridorHeight, corridorDepth);

        return new List<Node3D>(roomList).Concat(corridorList).ToList();
    }

   
}