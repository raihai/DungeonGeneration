using System;
using UnityEngine;

namespace Graphs
{
    public class Vertex : IEquatable<Vertex>
    {
        public Vector3 Position { get; }

        public Vertex(Vector3 position)
        {
            Position = position;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex v)
            {
                return Position == v.Position;
            }

            return false;
        }

        public bool Equals(Vertex other)
        {
            return Position == other.Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public class Vertex<T> : Vertex
    {
        public T Item { get; }

        public Vertex(Vector3 position, T item) : base(position)
        {
            Item = item;
        }
    }

    public class Edge : IEquatable<Edge>
    {
        public Vertex X { get; }
        public Vertex Y { get; }

        public Edge(Vertex x, Vertex y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.X == right.X || left.X == right.Y)
                && (left.Y == right.X || left.Y == right.Y);
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
}