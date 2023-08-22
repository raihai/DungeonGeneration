using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RWGeneration3D : MonoBehaviour
{

    [Header("Dungeon Generation Values")]
    public int dungeonWidth;
    public int dungeonHeight;
    public int dungeonDepth;
    public int maxIteration;
    public int walkerNum;
    public float dirChangeChance;

    [Header("Room Values")]
    public int minSize;
    public int maxSize;
    public int maxHight;
    public int minDistance;
    public int roomAmount;

    [Header("Prefabs")]
    public GameObject floorObject;
    public GameObject roomObject;

    private List<Vector3Int> walkerPos;
    private List<Direction> walkerDir;
    private HashSet<Vector3Int> visitedPos;

    private enum Direction { Up, Down, Left, Right, Forward, Backward }

    private List<RectInt> roomBounds;
    private List<Vector3Int> roomCentrePos;
    private List<GameObject> floorGameObjects;
   

    private void Start()
    {
   
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Initialize();
        GenerateDungeon();
        PlaceRooms();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken: " + stopwatch.Elapsed.TotalMilliseconds + " ms");

    }

    private void Initialize()
    {
        visitedPos = new HashSet<Vector3Int>();
        walkerPos = new List<Vector3Int>();
        walkerDir = new List<Direction>();
        roomBounds = new List<RectInt>();

        floorGameObjects = new List<GameObject>();

        for (int i = 0; i < walkerNum; i++)
        {
            walkerPos.Add(new Vector3Int(dungeonWidth / 2, dungeonHeight / 2, dungeonDepth / 2));
            walkerDir.Add((Direction)Random.Range(0, 6));
        }
    }

    private void PlaceRooms()
    {
        roomCentrePos = new List<Vector3Int>();
        List<Vector3Int> visitedPositionsList = visitedPos.ToList();

        for (int i = 0; i < roomAmount; i++)
        {
            bool roomGen= false;
            int roomPlacementAttempts = 0;
            const int roomGenAttempt = 100;

            while (!roomGen && roomPlacementAttempts < roomGenAttempt)
            {
                Vector3Int roomPos = visitedPositionsList[Random.Range(0, visitedPositionsList.Count)];

                int roomWidth = Random.Range(minSize, maxSize + 1);
                int roomHeight = Random.Range(minSize, Mathf.Min(maxSize + 1, maxHight + 1));
                int roomDepth = Random.Range(minSize, maxSize + 1);

                RectInt newRoomRect = new RectInt(roomPos.x, roomPos.y, roomWidth, roomHeight);

                bool overlapsExistingRoom = false;

               
                foreach (RectInt existingRoomRect in roomBounds)
                {
                    RectInt paddedRect = new RectInt(existingRoomRect.x - minDistance, existingRoomRect.y - minDistance, existingRoomRect.width + minDistance * 2, 
                        existingRoomRect.height + minDistance * 2);
                    if (paddedRect.Overlaps(newRoomRect))
                    {
                        overlapsExistingRoom = true;
                        break;
                    }
                }

                if (!overlapsExistingRoom)
                {
                    roomGen = true;

                    GameObject room = new GameObject("Room" + i);
                    room.transform.parent = transform;

                    Vector3Int roomCentre = new Vector3Int(roomPos.x + roomWidth / 2, roomPos.y + roomHeight / 2, roomPos.z + roomDepth / 2);
                    GameObject floor = Instantiate(roomObject, new Vector3(roomCentre.x, roomCentre.y, roomCentre.z), Quaternion.identity, room.transform);
                    floor.transform.localScale = new Vector3(roomWidth, roomHeight, roomDepth);

                    Vector2Int roomPosss = new Vector2Int(roomPos.x, roomPos.y);
                    newRoomRect.position = roomPosss;
                    roomBounds.Add(newRoomRect);
                    roomCentrePos.Add(roomCentre);

                    for (int j = floorGameObjects.Count - 1; j >= 0; j--)
                    {
                        GameObject path = floorGameObjects[j]; 
                        Vector3Int floorPos = Vector3Int.FloorToInt(path.transform.position);
                        if (newRoomRect.Contains(new Vector2Int(floorPos.x, floorPos.y)) && floorPos.z >= roomPos.z 
                            && floorPos.z < roomPos.z + roomDepth)
                        {
                            floorGameObjects.RemoveAt(j);
                            Destroy(path);
                        }
                    }
                }

                roomPlacementAttempts++;
            }
        }
    }

    private void GenerateDungeon()
    {
        for (int i = 0; i < maxIteration; i++)
        {
            for (int j = 0; j < walkerNum; j++)
            {
                Vector3Int currPos = walkerPos[j];
                Direction currDir = walkerDir[j];

                if (!visitedPos.Contains(currPos))
                {
                    visitedPos.Add(currPos);
                    PlaceFloorInWorld(currPos);
                }
                if (Random.value < dirChangeChance)
                {
                    currDir = (Direction)Random.Range(0, 6);
                }

                Vector3Int nextPosition = MoveWalker(currPos, currDir);

                if (InBounds(nextPosition))
                {
                    walkerPos[j] = nextPosition;
                    walkerDir[j] = currDir;
                }
            }
        }
    }

    private Vector3Int MoveWalker(Vector3Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return position + Vector3Int.up;
            case Direction.Down:
                return position + Vector3Int.down;
            case Direction.Left:
                return position + Vector3Int.left;
            case Direction.Right:
                return position + Vector3Int.right;
            case Direction.Forward:
                return position + Vector3Int.forward;
            case Direction.Backward:
                return position + Vector3Int.back;
            default:
                return position;
        }
    }

    private void PlaceFloorInWorld(Vector3Int position)
    {
        GameObject floorInstance = Instantiate(floorObject, new Vector3(position.x, position.y, position.z), Quaternion.identity, transform);
        floorGameObjects.Add(floorInstance);
    }

    private bool InBounds(Vector3Int position)
    {
        return position.x >= 0 && position.x < dungeonWidth &&
               position.y >= 0 && position.y < dungeonHeight &&
               position.z >= 0 && position.z < dungeonDepth;
    }
}