using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid2D<T>
{
    private T[,] dungeonData;

    public Vector2Int Size { get; private set; }
    public Vector2Int Offset { get; set; }

    public Grid2D(Vector2Int size, Vector2Int offset)
    {
        Size = size;
        Offset = offset;

        dungeonData = new T[size.x, size.y];
    }

    public bool InBounds(Vector2Int position)
    {
        return new RectInt(Vector2Int.zero, Size).Contains(position + Offset);
    }

    public T this[int x, int y]
    {
        get { return this[new Vector2Int(x, y)]; }
        set { this[new Vector2Int(x, y)] = value; }
    }

    public T this[Vector2Int position]
    {
        get { return dungeonData[position.x + Offset.x, position.y + Offset.y]; }
        set { dungeonData[position.x + Offset.x, position.y + Offset.y] = value; }
    }
}