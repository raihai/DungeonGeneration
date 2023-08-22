using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class CorridorNode3D : Node3D
{
    private Node3D room1;
    private Node3D room2;
    private int corridorWidth;
    private int corridorHeight;
    private int corridorDepth;
    private int distFromWall = 1;

    public CorridorNode3D(Node3D node1, Node3D node2, int corridorWidth, int corridorHeight, int corridorDepth) : base(null)
    {
        this.room1 = node1;
        this.room2 = node2;
        this.corridorWidth = corridorWidth;
        this.corridorHeight = corridorHeight;
        this.corridorDepth = corridorDepth;
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
            case RelativePosition.Forward:
                ProcessRoomInRelationForwardOrBackward(this.room1, this.room2);
                break;
            case RelativePosition.Backward:
                ProcessRoomInRelationForwardOrBackward(this.room2, this.room1);
                break;
            default:
                break;
        }
    }

    private void ProcessRoomInRelationRightOrLeft(Node3D room1, Node3D room2)
    {
        Node3D leftRoom = null;
        List<Node3D> leftRoomChildren = StructureHelper3D.ExtractLowestLeafNodes(room1);
        Node3D rightRoom = null;
        List<Node3D> rightRoomChildren = StructureHelper3D.ExtractLowestLeafNodes(room2);

        var sortedleftRoom = leftRoomChildren.OrderByDescending(child => child.TopRight.x).ToList();
        if (sortedleftRoom.Count == 1)
        {
            leftRoom = sortedleftRoom[0];
        }
        else if (sortedleftRoom.Count > 1)
        {
            int maxX = sortedleftRoom[0].TopRight.x;
            sortedleftRoom = sortedleftRoom.Where(children => Math.Abs(maxX - children.TopRight.x) < 10).ToList();
            if (sortedleftRoom.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, sortedleftRoom.Count);
                leftRoom = sortedleftRoom[index];
            }
            else
            {
                // Handle the case when no valid left room is found
                leftRoom = sortedleftRoom[0]; // Assign the first room as a fallback
            }
        }
        else
        {
            // Handle the case when no left room children are present
            leftRoom = room1; // Assign the input room as a fallback
        }

        var possibleNeighboursInrightRoomList = rightRoomChildren.Where(
            child => GetValidYForNeighourLeftRight(
                leftRoom.TopRight,
                leftRoom.BotRight,
                child.TopLeft,
                child.BotLeft
            ) != -1
        ).OrderBy(child => child.BotRight.x).ToList();

        if (possibleNeighboursInrightRoomList.Count > 0)
        {
            rightRoom = possibleNeighboursInrightRoomList[0];
        }
        else
        {
            // Handle the case when no valid right room is found
            rightRoom = room2; // Assign the input room as a fallback
        }

        int y = GetValidYForNeighourLeftRight(leftRoom.TopLeft, leftRoom.BotRight,
            rightRoom.TopLeft,
            rightRoom.BotLeft);

        while (y == -1 && sortedleftRoom.Count > 1)
        {
            sortedleftRoom = sortedleftRoom.Where(
                child => child.TopLeft.y != leftRoom.TopLeft.y).ToList();
            if (sortedleftRoom.Count > 0)
            {
                leftRoom = sortedleftRoom[0];
                y = GetValidYForNeighourLeftRight(leftRoom.TopLeft, leftRoom.BotRight,
                    rightRoom.TopLeft,
                    rightRoom.BotLeft);
            }
            else
            {
                // Handle the case when no valid left room is found
                leftRoom = sortedleftRoom[0]; // Assign the first room as a fallback
                y = GetValidYForNeighourLeftRight(leftRoom.TopLeft, leftRoom.BotRight,
                    rightRoom.TopLeft,
                    rightRoom.BotLeft);
            }
        }

        BotLeft = new Vector3Int(leftRoom.BotRight.x, y, leftRoom.BotRight.z);
        TopRight = new Vector3Int(rightRoom.TopLeft.x, y + this.corridorWidth, rightRoom.TopLeft.z);
    }

    private int GetValidYForNeighourLeftRight(Vector3Int leftNodeUp, Vector3Int leftNodeDown, Vector3Int rightNodeUp, Vector3Int rightNodeDown)
    {
        if (rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                leftNodeDown + new Vector3Int(0, distFromWall, 0),
                leftNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
                ).y;
        }
        if (rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                rightNodeDown + new Vector3Int(0, distFromWall, 0),
                rightNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
                ).y;
        }
        if (leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                rightNodeDown + new Vector3Int(0, distFromWall, 0),
                leftNodeUp - new Vector3Int(0, distFromWall, 0)
                ).y;
        }
        if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                leftNodeDown + new Vector3Int(0, distFromWall, 0),
                rightNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
                ).y;
        }
        return -1;
    }

    private void ProcessRoomInRelationUpOrDown(Node3D room1, Node3D room2)
    {
        Node3D bottomStructure = null;
        List<Node3D> structureBottmChildren = StructureHelper3D.ExtractLowestLeafNodes(room1);
        Node3D topStructure = null;
        List<Node3D> structureAboveChildren = StructureHelper3D.ExtractLowestLeafNodes(room2);

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
        while (x == -1 && sortedBottomStructure.Count > 1)
        {
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeft.x != topStructure.TopLeft.x).ToList();
            bottomStructure = sortedBottomStructure[0];
            x = GetValidXForNeighbourUpDown(
                bottomStructure.TopLeft,
                bottomStructure.TopRight,
                topStructure.BotLeft,
                topStructure.BotRight);
        }
        BotLeft = new Vector3Int(x, bottomStructure.TopLeft.y, bottomStructure.TopLeft.z);
        TopRight = new Vector3Int(x + this.corridorWidth, topStructure.BotLeft.y, topStructure.BotLeft.z);
    }

    private int GetValidXForNeighbourUpDown(Vector3Int bottomNodeLeft,
        Vector3Int bottomNodeRight, Vector3Int topNodeLeft, Vector3Int topNodeRight)
    {
        if (topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight.x < topNodeRight.x)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                bottomNodeLeft + new Vector3Int(distFromWall, 0, 0),
                bottomNodeRight - new Vector3Int(this.corridorWidth + distFromWall, 0, 0)
                ).x;
        }
        if (topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                topNodeLeft + new Vector3Int(distFromWall, 0, 0),
                topNodeRight - new Vector3Int(this.corridorWidth + distFromWall, 0, 0)
                ).x;
        }
        if (bottomNodeLeft.x >= (topNodeLeft.x) && bottomNodeLeft.x <= topNodeRight.x)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                bottomNodeLeft + new Vector3Int(distFromWall, 0, 0),
                topNodeRight - new Vector3Int(this.corridorWidth + distFromWall, 0, 0)
                ).x;
        }
        if (bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                topNodeLeft + new Vector3Int(distFromWall, 0, 0),
                bottomNodeRight - new Vector3Int(this.corridorWidth + distFromWall, 0, 0)
                ).x;
        }
        return -1;
    }

    private void ProcessRoomInRelationForwardOrBackward(Node3D room1, Node3D room2)
    {
        Node3D backRoom = null;
        List<Node3D> backRoomChildren = StructureHelper3D.ExtractLowestLeafNodes(room1);
        Node3D forwardRoom = null;
        List<Node3D> forwardRoomChildren = StructureHelper3D.ExtractLowestLeafNodes(room2);

        var sortedBackRoom = backRoomChildren.OrderByDescending(child => child.TopRight.z).ToList();
        if (sortedBackRoom.Count == 1)
        {
            backRoom = sortedBackRoom[0];
        }
        else
        {
            int maxZ = sortedBackRoom[0].TopRight.z;
            sortedBackRoom = sortedBackRoom.Where(children => Math.Abs(maxZ - children.TopRight.z) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedBackRoom.Count);
            backRoom = sortedBackRoom[index];
        }

        var possibleNeighboursInForwardRoomList = forwardRoomChildren.Where(
            child => GetValidYForNeighourBackForward(
                backRoom.TopRight,
                backRoom.BotRight,
                child.TopLeft,
                child.BotLeft
                ) != -1
            ).OrderBy(child => child.BotRight.z).ToList();

        if (possibleNeighboursInForwardRoomList.Count <= 0)
        {
            forwardRoom = room2;
        }
        else
        {
            forwardRoom = possibleNeighboursInForwardRoomList[0];
        }
        int y = GetValidYForNeighourBackForward(backRoom.TopRight, backRoom.BotRight,
            forwardRoom.TopLeft,
            forwardRoom.BotLeft);
        while (y == -1 && sortedBackRoom.Count > 1)
        {
            sortedBackRoom = sortedBackRoom.Where(
                child => child.TopLeft.y != backRoom.TopLeft.y).ToList();
            backRoom = sortedBackRoom[0];
            y = GetValidYForNeighourBackForward(backRoom.TopRight, backRoom.BotRight,
            forwardRoom.TopLeft,
            forwardRoom.BotLeft);
        }
        BotLeft = new Vector3Int(backRoom.BotRight.x, y, backRoom.BotRight.z);
        TopRight = new Vector3Int(forwardRoom.TopLeft.x, y + this.corridorWidth, forwardRoom.TopLeft.z);
    }

    private int GetValidYForNeighourBackForward(Vector3Int backNodeUp, Vector3Int backNodeDown, Vector3Int forwardNodeUp, Vector3Int forwardNodeDown)
    {
        if (forwardNodeUp.y >= backNodeUp.y && backNodeDown.y >= forwardNodeDown.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                backNodeDown + new Vector3Int(0, distFromWall, 0),
                backNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
                ).y;
        }
        if (forwardNodeUp.y <= backNodeUp.y && backNodeDown.y <= forwardNodeDown.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
                forwardNodeDown + new Vector3Int(0, distFromWall, 0),
                forwardNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
                ).y;
        }
        if (backNodeUp.y >= forwardNodeDown.y && backNodeUp.y <= forwardNodeUp.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
 forwardNodeDown + new Vector3Int(0, distFromWall, 0),
            forwardNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
            ).y;
        }
        if (forwardNodeUp.y >= backNodeDown.y && forwardNodeUp.y <= backNodeUp.y)
        {
            return StructureHelper3D.CalculateMiddlePoint(
            backNodeDown + new Vector3Int(0, distFromWall, 0),
            backNodeUp - new Vector3Int(0, distFromWall + this.corridorWidth, 0)
            ).y;
        }
        return -1;
    }

    private RelativePosition CheckPositionroom2Againstroom1()
    {
        Vector3Int middlePointroom1Temp = (room1.TopRight + room1.BotLeft) / 2;
        Vector3Int middlePointroom2Temp = (room2.TopRight + room2.BotLeft) / 2;
        float angle = CalculateAngle(middlePointroom1Temp, middlePointroom2Temp);

        float angleZ = CalculateAngleZ(middlePointroom1Temp, middlePointroom2Temp);

        if ((angle < 45 && angle >= 0) || (angle > -45 && angle < 0))
        {
            return RelativePosition.Right;
        }
        else if (angle > 45 && angle < 135)
        {
            return RelativePosition.Up;
        }
        else if (angle > -135 && angle < -45)
        {
            return RelativePosition.Down;
        }
        else if (angleZ > 0)
        {
            return RelativePosition.Forward;
        }
        else
        {
            return RelativePosition.Backward;
        }
    }

    private float CalculateAngle(Vector3Int middlePointroom1Temp, Vector3Int middlePointroom2Temp)
    {
        return Mathf.Atan2(middlePointroom2Temp.y - middlePointroom1Temp.y,
        middlePointroom2Temp.x - middlePointroom1Temp.x) * Mathf.Rad2Deg;
    }

    private float CalculateAngleZ(Vector3Int middlePointroom1Temp, Vector3Int middlePointroom2Temp)
    {
        return Mathf.Atan2(middlePointroom2Temp.z - middlePointroom1Temp.z,
        middlePointroom2Temp.x - middlePointroom1Temp.x) * Mathf.Rad2Deg;
    }

}

