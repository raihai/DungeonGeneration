using UnityEngine;
public class RoomNode : Node
{
    public RoomNode(Vector2Int bottomLeft, Vector2Int topRight, Node parentNode, int index) : base(parentNode)
    {
        this.BotLeft = bottomLeft;
        this.TopRight = topRight;
        this.BotRight = new Vector2Int(topRight.x, bottomLeft.y);
        this.TopLeft = new Vector2Int(bottomLeft.x, TopRight.y);
        this.TreeLayerInt = index;
    }

    public int Width { get => (int)(TopRight.x - BotLeft.x); }
    public int Length { get => (int)(TopRight.y - BotLeft.y); }
}