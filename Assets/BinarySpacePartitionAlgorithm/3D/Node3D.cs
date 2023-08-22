using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node3D
{
    private List<Node3D> childNodes;

    public List<Node3D> NodeChildList { get => childNodes; }

    public bool Visited { get; set; }
    public Vector3Int BotLeft { get; set; }
    public Vector3Int BotRight { get; set; }
    public Vector3Int TopRight { get; set; }
    public Vector3Int TopLeft { get; set; }



    public Node3D Parent { get; set; }

    public int TreeLayerInt { get; set; }

    public Node3D(Node3D parentNode)
    {
        childNodes = new List<Node3D>();
        this.Parent = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node3D node)
    {
        childNodes.Add(node);
    }

    public void RemoveChild(Node3D node)
    {
        childNodes.Remove(node);
    }
}