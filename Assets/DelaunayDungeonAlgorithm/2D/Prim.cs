using System;
using System.Collections.Generic;
using UnityEngine;
using Graphs;

public static class Prim
{
    public class Edge : Graphs.Edge
    {
        public float Distance { get; private set; }

        public Edge(Vertex u, Vertex v) : base(u, v)
        {
            Distance = Vector3.Distance(u.Position, v.Position);
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.X == right.X && left.Y == right.Y)
                || (left.X == right.Y && left.Y == right.X);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge e)
            {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e)
        {
            return this == e;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }

    public static List<Edge> MinimumSpanningTree(List<Edge> edges, Vertex start)
    {
        HashSet<Vertex> openSet = new HashSet<Vertex>();
        HashSet<Vertex> closedSet = new HashSet<Vertex>();

        foreach (var edge in edges)
        {
            openSet.Add(edge.X);
            openSet.Add(edge.Y);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        while (openSet.Count > 0)
        {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (var edge in edges)
            {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.X)) closedVertices++;
                if (!closedSet.Contains(edge.Y)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.Distance < minWeight)
                {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.Distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);
            openSet.Remove(chosenEdge.X);
            openSet.Remove(chosenEdge.Y);
            closedSet.Add(chosenEdge.X);
            closedSet.Add(chosenEdge.Y);
        }

        return results;
    }
}