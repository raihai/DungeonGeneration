using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BSP2D : MonoBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
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
        DugeonGenerator generator = new DugeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerMidifier,
            roomOffset,
            corridorWidth);

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            if (listOfRooms[i] is RoomNode)
            {
                InstantiatePrefab(roomPrefab, listOfRooms[i].BotLeft, listOfRooms[i].TopRight);
            }
            else if (listOfRooms[i] is CorridorNode2D)
            {
                InstantiatePrefab(corridorPrefab, listOfRooms[i].BotLeft, listOfRooms[i].TopRight);
            }
        }
    }

    private void InstantiatePrefab(GameObject prefab, Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Vector3 position = new Vector3((bottomLeftCorner.x + topRightCorner.x) / 2, 0, (bottomLeftCorner.y + topRightCorner.y) / 2);
        Vector3 scale = new Vector3(Mathf.Abs(topRightCorner.x - bottomLeftCorner.x), 1, Mathf.Abs(topRightCorner.y - bottomLeftCorner.y));
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.localScale = scale;
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