using System.Collections.Generic;
using UnityEngine;
using Graphs;
using System.Diagnostics;

public class Generator3D : MonoBehaviour {
    enum CellType {
        None,
        Room,
        Hallway,
        Stairs
    }

    class Room {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

    [SerializeField]
    Vector3Int size;
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector3Int roomMaxSize;
    [SerializeField]
    GameObject cubePrefab;


    [SerializeField]
    GameObject hallwayPrefab;

    [SerializeField]
    Material roomMat;
    [SerializeField]
    Material pathMat;


    public int seed = 0; 
    Grid3D<CellType> grid;
    List<Room> rooms;
    Delaunay3D delaunay;
    HashSet<Prim.Edge> selectedEdges;



    void Start() {

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Random.InitState(UnityEngine.Random.Range(0, 100));
        grid = new Grid3D<CellType>(size, Vector3Int.zero);
        rooms = new List<Room>();
        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken: " + stopwatch.Elapsed.TotalMilliseconds + " ms");
    
    }

    void PlaceRooms() {
        for (int i = 0; i < roomCount; i++)
        {
            Vector3Int location = new Vector3Int(
                Random.Range(0, size.x),
                Random.Range(0, size.y),
                Random.Range(0, size.z)
            );

            Vector3Int roomSize = new Vector3Int(
                Random.Range(1, roomMaxSize.x + 1),
                Random.Range(1, roomMaxSize.y + 1),
                Random.Range(1, roomMaxSize.z + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);

            // Check if the new room is too far from existing rooms
            foreach (var room in rooms)
            {
                if (Vector3.Distance(newRoom.bounds.center, room.bounds.center) < 10)
                {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z)
            {
                add = false;
            }

            if (add)
            {
                rooms.Add(newRoom);
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin)
                {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay3D.Triangulate(vertices);
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
           
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].X);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (Random.value < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }

    
    void PathfindHallways()
    {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

        foreach (var edge in selectedEdges)
        {

           
            var startRoom = (edge.X as Vertex<Room>).Item;
            var endRoom = (edge.Y as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0)
                {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position] == CellType.Room)
                    {
                        pathCost.cost += 5;
                    }
                    else if (grid[b.Position] == CellType.None)
                    {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                }
                else
                {
                    //vertical path
                    pathCost.cost = 10 + Vector3Int.Distance(b.Position, endPos); // base cost + heuristic
                    pathCost.traversable = true;
                }

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.None)
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];
                        //Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
                    }
                }

                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway || grid[pos] == CellType.Room)
                    {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

    void PlaceCube(Vector3Int location, Vector3Int size, Material material) {
        GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceRoom(Vector3Int location, Vector3Int size) {
        PlaceCube(location, size, roomMat);
    }

    void PlaceHallway(Vector3Int location) {
        PlaceCube(location, new Vector3Int(1, 1, 1), pathMat);
    }

}
