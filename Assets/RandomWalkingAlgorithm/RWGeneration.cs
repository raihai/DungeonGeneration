using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RWGeneration : MonoBehaviour
{

    private class Edge
    {
        public int Room1;
        public int Room2;
        public float Distance;

        public Edge(int room1, int room2, float distance)
        {
            Room1 = room1;
            Room2 = room2;
            Distance = distance;
        }
    }

    [Header("Dungeon Parameters")]
    public int width;
    public int height;
    public int maxIterations;
    public int numberOfWalkers;
    public float chanceToChangeDirection;

    [Header("Room Parameters")]
    public int minRoomSize;
    public int maxRoomSize;
    public int minRoomDistance;
    public int numberOfRooms;

    [Header("Corridor Parameters")]
    public int minCorridorLength;
    public int minCorridorWidth, maxCorridorWidth;
    public int corridorSmoothingIterations;

    [Header("Connectivity Parameters")]
    public int maxPathfindingAttempts;
    public int maxPathLength;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathPrefab;
    public GameObject[] roomPrefabs;

    private List<Vector2Int> walkerPositions;
    private List<Direction> walkerDirections;
    private HashSet<Vector2Int> visitedPositions;

    private enum Direction { Up, Down, Left, Right }
    private PathFind _pathFinder;
    private List<RectInt> rooms;

    private List<RectInt> roomBounds;

    private List<Vector2Int> roomCenters;
    private List<Edge> edges;

    private void Start()
    {
        Initialize();
        PlaceRooms();
        GenerateDungeon();
        SmoothCorridors();
        EnsureConnectivity();
    }

    private void Initialize()
    {
        _pathFinder = new PathFind(new Vector2Int(width, height));
        visitedPositions = new HashSet<Vector2Int>();
        walkerPositions = new List<Vector2Int>();
        walkerDirections = new List<Direction>();
        roomBounds = new List<RectInt>(); // Initialize the roomBounds list
      


        for (int i = 0; i < numberOfWalkers; i++)
        {
            walkerPositions.Add(new Vector2Int(width / 2, height / 2));
            walkerDirections.Add((Direction)Random.Range(0, 4));
            
        }
    }

    private void PlaceRooms()
    {
        roomCenters = new List<Vector2Int>();
        edges = new List<Edge>();


        for (int i = 0; i < numberOfRooms; i++)
        {
            // Generate random room size
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

            // Store the RectInt for the room (use (0, 0) as origin)
            RectInt roomRect = new RectInt(0, 0, roomWidth, roomHeight);
            roomBounds.Add(roomRect);

            // Generate random room position
            int posX = Random.Range(minRoomDistance, width - minRoomDistance - roomWidth);
            int posY = Random.Range(minRoomDistance, height - minRoomDistance - roomHeight);
            Vector2Int roomPosition = new Vector2Int(posX, posY);

            // Create a new GameObject for the room
            GameObject room = new GameObject("Room" + i);
            room.transform.parent = transform;

            // Instantiate floor tiles in the room (offset by roomPosition)
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(roomPosition.x + x, roomPosition.y + y);
                    Instantiate(floorPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity, room.transform);
                    visitedPositions.Add(pos);
                }
            }

            // Update the RectInt position to use roomPosition as origin
            roomRect.position = roomPosition;
            roomBounds[i] = roomRect;


            // Store the center position of the room
            Vector2Int roomCenter = new Vector2Int(posX + roomWidth / 2, posY + roomHeight / 2);
            roomCenters.Add(roomCenter);

            // Generate edges between the current room and all the previous rooms
            for (int j = 0; j < i; j++)
            {
                Vector2Int otherCenter = roomCenters[j];
                float distance = Vector2Int.Distance(roomCenter, otherCenter);
                edges.Add(new Edge(i, j, distance));
            }
        }
    }

    private void GenerateDungeon()
    {
        for (int i = 0; i < maxIterations; i++)
        {
            for (int j = 0; j < numberOfWalkers; j++)
            {
                Vector2Int currentPosition = walkerPositions[j];
                Direction currentDirection = walkerDirections[j];

                if (!visitedPositions.Contains(currentPosition))
                {
                    visitedPositions.Add(currentPosition);
                    PlaceFloor(currentPosition);
                }
                if (Random.value < chanceToChangeDirection)
                {
                    currentDirection = (Direction)Random.Range(0, 4);
                }

                Vector2Int nextPosition = Move(currentPosition, currentDirection);

                if (InBounds(nextPosition))
                {
                    walkerPositions[j] = nextPosition;
                    walkerDirections[j] = currentDirection;
                }
            }
        }
    }

    private void SmoothCorridors()
    {
        for (int i = 0; i < corridorSmoothingIterations; i++)
        {
            HashSet<Vector2Int> newVisitedPositions = new HashSet<Vector2Int>(visitedPositions);

            foreach (Vector2Int position in visitedPositions)
            {
                int adjacentCount = CountAdjacent(position);

                if (adjacentCount >= 3)
                {
                    newVisitedPositions.Remove(position);
                }
            }

            visitedPositions = newVisitedPositions;
        }
    }

    private void EnsureConnectivity()
    {

        List<Edge> mst = GenerateMST();

        foreach (Edge edge in mst)
        {
            int room1 = edge.Room1;
            int room2 = edge.Room2;
            Vector2Int center1 = roomCenters[room1];
            Vector2Int center2 = roomCenters[room2];

            List<Vector2Int> path = _pathFinder.FindPath(center1, center2, (nodeA, nodeB) =>
            {
                return new PathFind.PathCost
                {
                    Traversable = true,
                    Cost = 1
                };
            });

            foreach (Vector2Int pos in path)
            {
                visitedPositions.Add(pos);
                PlacePath(pos);
            }
        }
    }

    private List<Edge> GenerateMST()
    {
        List<Edge> mst = new List<Edge>();
        HashSet<int> visited = new HashSet<int>();
        visited.Add(0);

        while (visited.Count < numberOfRooms)
        {
            Edge minEdge = null;
            float minDistance = float.MaxValue;

            foreach (Edge edge in edges)
            {
                if (visited.Contains(edge.Room1) && !visited.Contains(edge.Room2))
                {
                    if (edge.Distance < minDistance)
                    {
                        minDistance = edge.Distance;
                        minEdge = edge;
                    }
                }
                else if (visited.Contains(edge.Room2) && !visited.Contains(edge.Room1))
                {
                    if (edge.Distance < minDistance)
                    {
                        minDistance = edge.Distance;
                        minEdge = edge;
                    }
                }
            }
            // Add the minimum edge to the MST and mark the connected room as visited
            mst.Add(minEdge);
            visited.Add(minEdge.Room1);
            visited.Add(minEdge.Room2);
        }

        return mst;
    }

    private void PlaceFloor(Vector2Int position)
    {
        Vector3 pos = new Vector3(position.x, 0, position.y);
        Instantiate(floorPrefab, pos, Quaternion.identity, transform);
    }

    private Vector2Int Move(Vector2Int position, Direction direction)
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
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool InBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    private int CountAdjacent(Vector2Int position)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int newPos = new Vector2Int(position.x + x, position.y + y);

                if (visitedPositions.Contains(newPos))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool HasAdjacent(Vector2Int position)
    {
        return visitedPositions.Contains(new Vector2Int(position.x + 1, position.y))
            || visitedPositions.Contains(new Vector2Int(position.x - 1, position.y))
            || visitedPositions.Contains(new Vector2Int(position.x, position.y + 1))
            || visitedPositions.Contains(new Vector2Int(position.x, position.y - 1));
    }

    private void PlacePath(Vector2Int position)
    {
        Vector3 pos = new Vector3(position.x, 0, position.y);
        Instantiate(pathPrefab, pos, Quaternion.identity, transform);
    }

}