using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator3D
{
    private int maxIterations;
    private int roomLengthMin;
    private int roomWidthMin;
    private int roomHeightMin;

    public RoomGenerator3D(int maxIterations, int roomLengthMin, int roomWidthMin, int roomHeightMin)
    {
        this.maxIterations = maxIterations;
        this.roomLengthMin = roomLengthMin;
        this.roomWidthMin = roomWidthMin;
        this.roomHeightMin = roomHeightMin;
    }

    public List<RoomNode3D> GenerateRoomsInGivenSpaces(List<Node3D> roomSpaces, float roomBotMod, float roomTopMod, int roomOffset)
    {
        List<RoomNode3D> listToReturn = new List<RoomNode3D>();
        foreach (var space in roomSpaces)
        {
            Vector3Int newBottomLeftPoint = StructureHelper3D.GenerateBotLeft(
                space.BotLeft, space.TopRight, roomBotMod, roomOffset);

            Vector3Int newTopRightPoint = StructureHelper3D.GenerateTopRight(
                space.BotLeft, space.TopRight, roomTopMod, roomOffset);
            space.BotLeft = newBottomLeftPoint;
            space.TopRight = newTopRightPoint;
            space.BotRight = new Vector3Int(newTopRightPoint.x, newBottomLeftPoint.y, newBottomLeftPoint.z);
            space.TopLeft = new Vector3Int(newBottomLeftPoint.x, newTopRightPoint.y, newBottomLeftPoint.z);
            listToReturn.Add((RoomNode3D)space);
        }
        return listToReturn;
    }
}