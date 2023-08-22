using UnityEngine;

public class RoomNode3D : Node3D
{
    public RoomNode3D(Vector3Int bottomLeft, Vector3Int topRight, Node3D parentNode, int index) : base(parentNode)
    {
        this.BotLeft = bottomLeft;
        this.TopRight = topRight;
        this.BotRight = new Vector3Int(topRight.x, bottomLeft.y, bottomLeft.z);
        this.TopLeft = new Vector3Int(bottomLeft.x, topRight.y, bottomLeft.z);
        this.TreeLayerInt = index;
    }

    public int Width { get => (int)(TopRight.x - BotLeft.x); }
    public int Length { get => (int)(TopRight.y - BotLeft.y); }
    public int Height { get => (int)(TopRight.z - BotLeft.z); }

    public int Size
    {
        get { return Width * Length * Height; }
    }
}