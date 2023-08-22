using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator
{
    private int maxIterations;
    private int roomLengthMin;
    private int roomWidthMin;

    public RoomGenerator(int maxIterations, int roomLengthMin, int roomWidthMin)
    {
        this.maxIterations = maxIterations;
        this.roomLengthMin = roomLengthMin;
        this.roomWidthMin = roomWidthMin;
    }

    public List<RoomNode> GenerateRoomsInGivenSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCornerMidifier, int roomOffset)
    {
        List<RoomNode> listToReturn = new List<RoomNode>();
        foreach (var space in roomSpaces)
        {
            Vector2Int newBottomLeftPoint = Helper2D.GenerateBotLeft(
                space.BotLeft, space.TopRight, roomBottomCornerModifier, roomOffset);

            Vector2Int newTopRightPoint = Helper2D.GenerateTopRight(
                space.BotLeft, space.TopRight, roomTopCornerMidifier, roomOffset);
            space.BotLeft = newBottomLeftPoint;
            space.TopRight = newTopRightPoint;
            space.BotRight = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
            space.TopLeft = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);
            listToReturn.Add((RoomNode)space);
                
        }
        return listToReturn;
    }
}