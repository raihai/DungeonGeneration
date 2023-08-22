using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorNode2D : Node
{
    private Node room1;
    private Node room2;
    private int corridorWidth;
    private int distFromWall=1;

    public CorridorNode2D(Node node1, Node node2, int corridorWidth) : base(null)
    {
        this.room1 = node1;
        this.room2 = node2;
        this.corridorWidth = corridorWidth;
        GenerateCorridor();
    }


    private void GenerateCorridor()
    {
        var relativePositionOfroom2 = CheckPositionroom2Againstroom1();
        switch (relativePositionOfroom2)
        {
            case RelativePosition.Up:
                ProcessRoomInRelationUpOrDown(this.room1, this.room2);
                break;
            case RelativePosition.Down:
                ProcessRoomInRelationUpOrDown(this.room2, this.room1);
                break;
            case RelativePosition.Right:
                ProcessRoomInRelationRightOrLeft(this.room1, this.room2);
                break;
            case RelativePosition.Left:
                ProcessRoomInRelationRightOrLeft(this.room2, this.room1);
                break;
            default:
                break;
        }
    }

    private void ProcessRoomInRelationRightOrLeft(Node room1, Node room2)
    {
        Node leftRoom = null;
        List<Node> leftRoomChildren = Helper2D.ExtractLowestLeafNodes(room1);
        Node rightRoom = null;
        List<Node> rightRoomChildren = Helper2D.ExtractLowestLeafNodes(room2);

        var sortedleftRoom = leftRoomChildren.OrderByDescending(child => child.TopRight.x).ToList();
        if (sortedleftRoom.Count == 1)
        {
            leftRoom = sortedleftRoom[0];
        }
        else
        {
            int maxX = sortedleftRoom[0].TopRight.x;
            sortedleftRoom = sortedleftRoom.Where(children => Math.Abs(maxX - children.TopRight.x) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedleftRoom.Count);
            leftRoom = sortedleftRoom[index];
        }

        var possibleNeighboursInrightRoomList = rightRoomChildren.Where(
            child => GetValidYForNeighourLeftRight(
                leftRoom.TopRight,
                leftRoom.BotRight,
                child.TopLeft,
                child.BotLeft
                ) != -1
            ).OrderBy(child => child.BotRight.x).ToList();

        if (possibleNeighboursInrightRoomList.Count <= 0)
        {
            rightRoom = room2;
        }
        else
        {
            rightRoom = possibleNeighboursInrightRoomList[0];
        }
        int y = GetValidYForNeighourLeftRight(leftRoom.TopLeft, leftRoom.BotRight,
            rightRoom.TopLeft,
            rightRoom.BotLeft);
        while(y==-1 && sortedleftRoom.Count > 1)
        {
            sortedleftRoom = sortedleftRoom.Where(
                child => child.TopLeft.y != leftRoom.TopLeft.y).ToList();
            leftRoom = sortedleftRoom[0];
            y = GetValidYForNeighourLeftRight(leftRoom.TopLeft, leftRoom.BotRight,
            rightRoom.TopLeft,
            rightRoom.BotLeft);
        }
        BotLeft = new Vector2Int(leftRoom.BotRight.x, y);
        TopRight = new Vector2Int(rightRoom.TopLeft.x, y + this.corridorWidth);
    }

    private int GetValidYForNeighourLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
    {
        if(rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
        {
            return Helper2D.CalculateMiddlePoint(
                leftNodeDown + new Vector2Int(0, distFromWall),
                leftNodeUp - new Vector2Int(0, distFromWall + this.corridorWidth)
                ).y;
        }
        if(rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y)
        {
            return Helper2D.CalculateMiddlePoint(
                rightNodeDown+new Vector2Int(0,distFromWall),
                rightNodeUp - new Vector2Int(0, distFromWall+this.corridorWidth)
                ).y;
        }
        if(leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
        {
            return Helper2D.CalculateMiddlePoint(
                rightNodeDown+new Vector2Int(0,distFromWall),
                leftNodeUp-new Vector2Int(0,distFromWall)
                ).y;
        }
        if(leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
        {
            return Helper2D.CalculateMiddlePoint(
                leftNodeDown+new Vector2Int(0,distFromWall),
                rightNodeUp-new Vector2Int(0,distFromWall+this.corridorWidth)
                ).y;
        }
        return- 1;
    }

    private void ProcessRoomInRelationUpOrDown(Node room1, Node room2)
    {
        Node bottomStructure = null;
        List<Node> structureBottmChildren = Helper2D.ExtractLowestLeafNodes(room1);
        Node topStructure = null;
        List<Node> structureAboveChildren = Helper2D.ExtractLowestLeafNodes(room2);

        var sortedBottomStructure = structureBottmChildren.OrderByDescending(child => child.TopRight.y).ToList();

        if (sortedBottomStructure.Count == 1)
        {
            bottomStructure = structureBottmChildren[0];
        }
        else
        {
            int maxY = sortedBottomStructure[0].TopLeft.y;
            sortedBottomStructure = sortedBottomStructure.Where(child => Mathf.Abs(maxY - child.TopLeft.y) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedBottomStructure.Count);
            bottomStructure = sortedBottomStructure[index];
        }

        var possibleNeighboursInTopStructure = structureAboveChildren.Where(
            child => GetValidXForNeighbourUpDown(
                bottomStructure.TopLeft,
                bottomStructure.TopRight,
                child.BotLeft,
                child.BotRight)
            != -1).OrderBy(child => child.BotRight.y).ToList();
        if (possibleNeighboursInTopStructure.Count == 0)
        {
            topStructure = room2;
        }
        else
        {
            topStructure = possibleNeighboursInTopStructure[0];
        }
        int x = GetValidXForNeighbourUpDown(
                bottomStructure.TopLeft,
                bottomStructure.TopRight,
                topStructure.BotLeft,
                topStructure.BotRight);
        while(x==-1 && sortedBottomStructure.Count > 1)
        {
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeft.x != topStructure.TopLeft.x).ToList();
            bottomStructure = sortedBottomStructure[0];
            x = GetValidXForNeighbourUpDown(
                bottomStructure.TopLeft,
                bottomStructure.TopRight,
                topStructure.BotLeft,
                topStructure.BotRight);
        }
        BotLeft = new Vector2Int(x, bottomStructure.TopLeft.y);
        TopRight = new Vector2Int(x + this.corridorWidth, topStructure.BotLeft.y);
    }

    private int GetValidXForNeighbourUpDown(Vector2Int bottomNodeLeft, 
        Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
    {
        if(topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight.x < topNodeRight.x)
        {
            return Helper2D.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(distFromWall, 0),
                bottomNodeRight - new Vector2Int(this.corridorWidth + distFromWall, 0)
                ).x;
        }
        if(topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
        {
            return Helper2D.CalculateMiddlePoint(
                topNodeLeft+new Vector2Int(distFromWall,0),
                topNodeRight - new Vector2Int(this.corridorWidth+distFromWall,0)
                ).x;
        }
        if(bottomNodeLeft.x >= (topNodeLeft.x) && bottomNodeLeft.x <= topNodeRight.x)
        {
            return Helper2D.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(distFromWall,0),
                topNodeRight - new Vector2Int(this.corridorWidth+distFromWall,0)

                ).x;
        }
        if(bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
        {
            return Helper2D.CalculateMiddlePoint(
                topNodeLeft + new Vector2Int(distFromWall, 0),
                bottomNodeRight - new Vector2Int(this.corridorWidth + distFromWall, 0)

                ).x;
        }
        return -1;
    }

    private RelativePosition CheckPositionroom2Againstroom1()
    {
        Vector2 middlePointroom1Temp = ((Vector2)room1.TopRight + room1.BotLeft) / 2;
        Vector2 middlePointroom2Temp = ((Vector2)room2.TopRight + room2.BotLeft) / 2;
        float angle = CalculateAngle(middlePointroom1Temp, middlePointroom2Temp);
        if ((angle < 45 && angle >= 0) || (angle > -45 && angle < 0))
        {
            return RelativePosition.Right;
        }
        else if(angle > 45 && angle < 135)
        {
            return RelativePosition.Up;
        }
        else if(angle > -135 && angle < -45)
        {
            return RelativePosition.Down;
        }
        else
        {
            return RelativePosition.Left;
        }
    }

    private float CalculateAngle(Vector2 middlePointroom1Temp, Vector2 middlePointroom2Temp)
    {
        return Mathf.Atan2(middlePointroom2Temp.y - middlePointroom1Temp.y,
            middlePointroom2Temp.x - middlePointroom1Temp.x)*Mathf.Rad2Deg;
    }
}