using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RWGeneration2D : MonoBehaviour
{
    [Header("Dungeon Generation Values")]
    public int dungeonWidth;
    public int dungeonHeight;
    public int maxIteration;
    public int walkerNum;
    public float dirChangeChance;

    [Header("Room Values")]
    public int minSize;
    public int maxSize;
    public int minDistance;
    public int roomAmount;

    [Header("Dungeon Asset")]
    public GameObject roomObject;
    public GameObject floorObject;

    private List<Vector2Int> walkerPos;
    private List<Direction> walkerDir;
    private HashSet<Vector2Int> visitedPos;
    private List<RectInt> roomBounds;

    private List<GameObject> floorGameObjects;

    private enum Direction { Up, Down, Left, Right }

    private void Start()
    {

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
   
        Initialize();
        CreateWalkers();
        GenerateDungeon();
        PlaceRooms(); 
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken: " + stopwatch.Elapsed.TotalMilliseconds + " ms");
       
    }

    private void Initialize()
    {
        visitedPos = new HashSet<Vector2Int>();
        walkerPos = new List<Vector2Int>();
        walkerDir = new List<Direction>();
        roomBounds = new List<RectInt>();
        floorGameObjects = new List<GameObject>();
    }

    private void CreateWalkers()
    {
        for (int i = 0; i < walkerNum; i++)
        {
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, dungeonWidth), Random.Range(0, dungeonHeight));
            walkerPos.Add(randomPosition);
            walkerDir.Add((Direction)Random.Range(0, 4));
        }
    }

    private void PlaceRoom(Vector2Int position, Vector2Int size)
    {
        Vector3 pos = new Vector3(position.x + size.x / 2f, 0, position.y + size.y / 2f);
        Vector3 roomScale = new Vector3(size.x, 1, size.y);
        GameObject room = Instantiate(roomObject, pos, Quaternion.identity, transform);
        room.transform.localScale = roomScale;
    }

    private void PlaceRooms()
    {
        List<Vector2Int> visitedPositionsList = visitedPos.ToList();

        for (int i = 0; i < roomAmount; i++)
        {
            Vector2Int roomPos;
            RectInt roomDimension;
            bool hasAdjacentPath, roomOverlap;
            int roomGenAttempt= 0;

            do{
                roomPos = visitedPositionsList[Random.Range(0, visitedPositionsList.Count)];

                int roomWidth = Random.Range(minSize, maxSize + 1);
                int roomHeight = Random.Range(minSize, maxSize + 1);

                int posX = Random.Range(minDistance, dungeonWidth - minDistance - roomWidth);
                int posY = Random.Range(minDistance, dungeonHeight - minDistance - roomHeight);

                roomDimension = new RectInt(posX, posY, roomWidth, roomHeight);
                hasAdjacentPath = AdjacentPath(roomDimension);
                roomOverlap = OverlapRoom(roomDimension);
                roomGenAttempt++;

            } while ((!hasAdjacentPath || roomOverlap) && roomGenAttempt < 100);

            if (hasAdjacentPath && !roomOverlap)
            {
                roomBounds.Add(roomDimension);

                Vector2Int roomSize = new Vector2Int(roomDimension.width, roomDimension.height);
                Vector2Int roomPosition = new Vector2Int(roomDimension.x, roomDimension.y);
                PlaceRoom(roomPosition, roomSize);

                RemoveFloorWithinRoom(roomDimension);
            }
        }
    }

    private bool AdjacentPath(RectInt roomRect)
    {
        for (int x = roomRect.x - 1; x <= roomRect.x + roomRect.width; x++)
        {
            for (int y = roomRect.y - 1; y <= roomRect.y + roomRect.height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (visitedPos.Contains(pos))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void GenerateDungeon()
    {
        for (int i = 0; i < maxIteration; i++)
        {
            for (int j = 0; j < walkerPos.Count; j++)
            {
                Vector2Int currentPosition = walkerPos[j];
                Direction currentDirection = walkerDir[j];

                if (!visitedPos.Contains(currentPosition))
                {
                    visitedPos.Add(currentPosition);
                    PlaceFloorInWorld(currentPosition);
                }
                if (Random.value < dirChangeChance)
                {
                    currentDirection = (Direction)Random.Range(0, 4);
                }

                Vector2Int nextPosition = MoveWalker(currentPosition, currentDirection);

                if (InBounds(nextPosition))
                {
                    walkerPos[j] = nextPosition;
                    walkerDir[j] = currentDirection;
                }
            }
        }
    }

    private bool OverlapRoom(RectInt roomRect)
    {
        foreach (RectInt existingRoom in roomBounds)
        {
            if (roomRect.Overlaps(existingRoom))
            {
                return true;
            }
        }

        return false;
    }

    private void PlaceFloorInWorld(Vector2Int position)
    {
        Vector3 pos = new Vector3(position.x, 0, position.y);
        GameObject floorInstance = Instantiate(floorObject, pos, Quaternion.identity, transform);
        floorGameObjects.Add(floorInstance); 
    }

    private Vector2Int MoveWalker(Vector2Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Vector2Int(position.x, position.y + 1);
            case Direction.Down:
                return new Vector2Int(position.x, position.y - 1);
            case Direction.Left:
                return new Vector2Int(position.x - 1, position.y);
            case Direction.Right:
                return new Vector2Int(position.x + 1, position.y);
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    private bool InBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < dungeonWidth && position.y >= 0 && position.y < dungeonHeight;
    }

    private void RemoveFloorWithinRoom(RectInt roomRect)
    {
        for (int i = floorGameObjects.Count - 1; i >= 0; i--)
        {
            GameObject floorInstance = floorGameObjects[i];
            Vector2Int floorPosition = new Vector2Int(Mathf.FloorToInt(floorInstance.transform.position.x), Mathf.FloorToInt(floorInstance.transform.position.z));
            if (roomRect.Contains(floorPosition))
            {
                floorGameObjects.RemoveAt(i);
                Destroy(floorInstance);
            }
        }
    }

}