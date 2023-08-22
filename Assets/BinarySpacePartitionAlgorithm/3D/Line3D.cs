using UnityEngine;

public class Line3D
{
    Orientation3D orientation;
    Vector3Int coordinates;

    public Line3D(Orientation3D orientation, Vector3Int coordinates)
    {
        this.orientation = orientation;
        this.coordinates = coordinates;
    }

    public Orientation3D Orientation { get => orientation; set => orientation = value; }
    public Vector3Int Coordinates { get => coordinates; set => coordinates = value; }
}

public enum Orientation3D
{
    X = 0,
    Y = 1,
    Z = 2
}