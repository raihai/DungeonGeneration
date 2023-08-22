using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using System.Diagnostics;

public class DTDungeonGenerator : MonoBehaviour
{
    enum CellType
    {
        None,
        Room,
        Hallway
    }
    class Room
    {
        public RectInt bounds;

        public Room(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }
    }

    [SerializeField] private Vector2Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;

    private Random random;
    private Grid2D<CellType> grid2D;
    private List<Room> rooms;
    private Delaunay delaunay;
    private HashSet<Prim.Edge> selectedEdges;

    private void Start()
    {
       
        Generate();

     


    }

    private void Generate()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        InitializeGenerator();
         PlaceRooms();
        TriangulateRooms();
        CreateHallways();
        PathfindHallways();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken: " + stopwatch.Elapsed.TotalMilliseconds + " ms");

    }

    private void InitializeGenerator()
    {

        random = new Random(UnityEngine.Random.Range(0,100));
        grid2D = new Grid2D<CellType>(size, Vector2Int.zero);
        rooms = new List<Room>();
    }

    private void PlaceRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1)
            );

            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            if (IsValidRoom(newRoom, buffer))
            {
                rooms.Add(newRoom);
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin)
                {
                    grid2D[pos] = CellType.Room;
                }
            }
        }
    }

    private bool IsValidRoom(Room newRoom, Room buffer)
    {
        bool isValid = true;

        foreach (var room in rooms)
        {
            if (Room.Intersect(room, buffer))
            {
                isValid = false;
                break;
            }
        }

        if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
            || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y)
        {
            isValid = false;
        }

        return isValid;
    }

    private void TriangulateRooms()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms)
        {
            
            vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay.Triangulate(vertices);
    }

    private void CreateHallways()
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));

           
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].X);

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges)
        {
           
            if (random.NextDouble() < 0.125)
            {
                selectedEdges.Add(edge);
               
            }
          

        }
    }
    
    //void DrawEdge2(Prim.Edge edge)
    //{
    //    
    //    var edgeObject = new GameObject("Edge");
    //    var line = edgeObject.AddComponent<LineRenderer>();

    //    
    //    line.startWidth = 0.1f;
    //    line.endWidth = 0.1f;

    // 
    //    line.positionCount = 2;

    //    line.SetPosition(0, new Vector3(edge.X.Position.x, 0, edge.X.Position.y));
    //    line.SetPosition(1, new Vector3(edge.Y.Position.x, 0, edge.Y.Position.y));
    //}


    private void PathfindHallways()
    {
        PathFind aStar = new PathFind(size);

        foreach (var edge in selectedEdges)
        {
            //DrawEdge2(edge);

            var startRoom = (edge.X as Vertex<Room>).Item;
            var endRoom = (edge.Y as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (PathFind.Node a, PathFind.Node b) =>
            {
                var pathCost = new PathFind.PathCost();

                pathCost.Cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid2D[b.Position] == CellType.Room)
                {
                    pathCost.Cost += 10;
                }
                else if (grid2D[b.Position] == CellType.None)
                {
                    pathCost.Cost += 5;
                }
                else if (grid2D[b.Position] == CellType.Hallway)
                {
                    pathCost.Cost += 1;
                }

                pathCost.Traversable = true;

                return pathCost;
            });

            if (path != null)
            {
                BuildHallway(path);
            }
        }
    }

    private void BuildHallway(List<Vector2Int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            var current = path[i];

            if (grid2D[current] == CellType.None)
            {
                grid2D[current] = CellType.Hallway;
            }
        }

        foreach (var pos in path)
        {
            if (grid2D[pos] == CellType.Hallway)
            {
                PlaceHallway(pos);
            }
        }
    }

    private void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x + size.x / 2f, 0, location.y + size.y / 2f), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }

    private void PlaceRoom(Vector2Int location, Vector2Int size)
    {
        PlaceCube(location, size, redMaterial);
    }

    private void PlaceHallway(Vector2Int location)
    {
        PlaceCube(location, new Vector2Int(1, 1), blueMaterial);
    }
}