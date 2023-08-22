using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class Node
{
    private List<Node> childNodes;

    public List<Node> NodeChildList { get => childNodes; }

    public bool Visted { get; set; }
    public Vector2Int BotLeft { get; set; }
    public Vector2Int BotRight { get; set; }
    public Vector2Int TopRight { get; set; }
    public Vector2Int TopLeft { get; set; }

    public Node Parent { get; set; }


    public int TreeLayerInt { get; set; }

    public Node(Node parentNode)
    {
        childNodes = new List<Node>();
        this.Parent = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node node)
    {
        childNodes.Add(node);

    }

    public void RemoveChild(Node node)
    {
        childNodes.Remove(node);
    }
}