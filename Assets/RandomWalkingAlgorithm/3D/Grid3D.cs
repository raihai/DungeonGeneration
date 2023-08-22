using UnityEngine;

public class Grid3D<T>
{
    private T[,,] dungeonData;

    public Vector3Int Size { get; private set; }
    public Vector3Int Offset { get; set; }

    public Grid3D(Vector3Int size, Vector3Int offset)
    {
        Size = size;
        Offset = offset;

        dungeonData = new T[size.x, size.y, size.z];
    }

    public bool InBounds(Vector3Int position)
    {
        return new BoundsInt(Vector3Int.zero, Size).Contains(position + Offset);
    }

    public T this[int x, int y, int z]
    {
        get { return this[new Vector3Int(x, y, z)]; }
        set { this[new Vector3Int(x, y, z)] = value; }
    }

    public T this[Vector3Int position]
    {
        get { return dungeonData[position.x + Offset.x, position.y + Offset.y, position.z + Offset.z]; }
        set { dungeonData[position.x + Offset.x, position.y + Offset.y, position.z + Offset.z] = value; }
    }
}