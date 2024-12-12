using System.Collections.Generic;
using UnityEngine;
using GeometryUtility = Utils.GeometryUtility;


namespace TerrainGenerator
{
    public class DelaunayTriangulation
    {
        private List<(Vector3, Vector3, Vector3)> triangles;

        private (Vector3, Vector3, Vector3) superTriangle;


        private void CreateSuperTriangle(int gridSize)
        {
            triangles = new List<(Vector3, Vector3, Vector3)>();

            Vector3 pointA = new Vector3(-gridSize * 10, 0, -gridSize * 10);
            Vector3 pointB = new Vector3(gridSize * 20, 0, -gridSize * 10);
            Vector3 pointC = new Vector3(0, 0, gridSize * 30);

            superTriangle = (pointA, pointB, pointC);
            triangles.Add(superTriangle);
        }


        public List<(Vector3, Vector3, Vector3)> Triangulate(int gridSize, List<Vector3> vertices)
        {
            CreateSuperTriangle(gridSize);

            foreach (Vector3 vertex in vertices)
            {
                List<(Vector3, Vector3, Vector3)> badTriangles = new List<(Vector3, Vector3, Vector3)>();

                foreach ((Vector3 pointA, Vector3 pointB, Vector3 pointC) in triangles)
                {
                    if (GeometryUtility.IsInCircumcircle(vertex, pointA, pointB, pointC))
                    {
                        badTriangles.Add((pointA, pointB, pointC));
                    }
                }

                foreach ((Vector3, Vector3, Vector3) badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle);
                }

                List<(Vector3, Vector3)> boundaryEdges = GetBoundaryEdges(badTriangles);

                foreach (var edge in boundaryEdges)
                {
                    triangles.Add(EnsureClockwiseOrder(edge.Item1, edge.Item2, vertex));
                }
            }

            RemoveSuperTriangle();

            return triangles;
        }



        private void RemoveSuperTriangle()
        {
            triangles.RemoveAll(triangle =>
                IsSuperTriangleVertex(triangle.Item1) ||
                IsSuperTriangleVertex(triangle.Item2) ||
                IsSuperTriangleVertex(triangle.Item3));
        }


        private bool IsSuperTriangleVertex(Vector3 vertex)
        {
            return vertex == superTriangle.Item1 ||
                   vertex == superTriangle.Item2 ||
                   vertex == superTriangle.Item3;
        }


        private List<(Vector3, Vector3)> GetBoundaryEdges(List<(Vector3, Vector3, Vector3)> badTriangles)
        {
            Dictionary<(Vector3, Vector3), int> edgeCount = new Dictionary<(Vector3, Vector3), int>();

            foreach ((Vector3 pointA, Vector3 pointB, Vector3 pointC) in badTriangles)
            {
                AddEdge(edgeCount, pointA, pointB);
                AddEdge(edgeCount, pointB, pointC);
                AddEdge(edgeCount, pointC, pointA);
            }

            List<(Vector3, Vector3)> boundaryEdges = new List<(Vector3, Vector3)>();

            foreach (var edge in edgeCount)
            {
                if (edge.Value == 1)
                {
                    boundaryEdges.Add(edge.Key);
                }
            }

            return boundaryEdges;
        }


        private void AddEdge(Dictionary<(Vector3, Vector3), int> edgeCount, Vector3 v1, Vector3 v2)
        {
            if (v1.x > v2.x || (v1.x == v2.x && v1.z > v2.z))
            {
                (v1, v2) = (v2, v1);
            }

            if (edgeCount.ContainsKey((v1, v2)))
            {
                edgeCount[(v1, v2)]++;
            }
            else
            {
                edgeCount[(v1, v2)] = 1;
            }
        }


        private (Vector3, Vector3, Vector3) EnsureClockwiseOrder(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;

            Vector3 cross = Vector3.Cross(ab, ac);

            if (cross.y < 0)
            {
                return (a, c, b);
            }

            return (a, b, c);
        }
    }
}
