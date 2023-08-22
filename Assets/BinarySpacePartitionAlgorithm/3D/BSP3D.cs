using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BSP3D : MonoBehaviour
{
    public int dungeonWidth, dungeonHeight, dungeonLength;
    public int roomWidthMin, roomHeightMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public int corridorDepth;

    public int roomHeightMax;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;
    public GameObject roomPrefab;
    public GameObject corridorPrefab;

    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        CreateDungeon();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken: " + stopwatch.Elapsed.TotalMilliseconds + " ms");
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        DungeonGenerator3D generator = new DungeonGenerator3D(dungeonWidth, dungeonHeight, dungeonLength);
        float corridorHeight = 4.0f;
        var listOfRooms = generator.CalculateDungeon(maxIterations, 
            roomWidthMin, 
            roomLengthMin,
            roomHeightMin,
            roomBottomCornerModifier,
            roomTopCornerMidifier, 
            roomOffset, 
            corridorWidth, 
            corridorHeight, 
            corridorDepth);

        foreach (var node in listOfRooms)
        {
            if (node is RoomNode3D)
            {
                InstantiatePrefab(roomPrefab, node.BotLeft, node.TopRight);
            }
            else if (node is CorridorNode3D)
            {
                InstantiatePrefab(corridorPrefab, node.BotLeft, node.TopRight);
            }
        }
    }

    private void InstantiatePrefab(GameObject prefab, Vector3 bottomLeftCorner, Vector3 topRightCorner)
    {
        Vector3 position = new Vector3((bottomLeftCorner.x + topRightCorner.x) / 2, (bottomLeftCorner.y + topRightCorner.y) / 2, (bottomLeftCorner.z + topRightCorner.z) / 2);
        Vector3 scale = new Vector3(Mathf.Abs(topRightCorner.x - bottomLeftCorner.x), Mathf.Abs(topRightCorner.y - bottomLeftCorner.y), Mathf.Abs(topRightCorner.z - bottomLeftCorner.z));

        float offset = 0.01f;
        scale += new Vector3(offset, offset, offset);

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, scale);
        instance.transform.parent = transform;
    }

    private void DestroyAllChildren()
    {
        while (transform.childCount != 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}