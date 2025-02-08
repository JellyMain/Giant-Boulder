using System.Collections.Generic;
using UnityEngine;


namespace StructuresSpawner
{
    public class QuadTree
    {
        private int capacity = 50;
        private int maxDepth = 15;

        public Rect Bounds { get; private set; }
        public List<Vector2> Points { get; private set; }
        public QuadTree[] Children { get; private set; }
        private int depth;


        public QuadTree(Rect bounds,int mapSize ,int depth = 0)
        {
            this.depth = depth;
            Bounds = bounds;
            Points = new List<Vector2>();
            Children = null;

            // if (this.depth == 0)
            // {
            //     CreateChildrenFromChunks(mapSize);
            // }
        }
        
        private void CreateChildrenFromChunks(int mapSize)
        {
            Children = new QuadTree[mapSize * mapSize];

            float chunkWidth = Bounds.width / mapSize;
            float chunkHeight = Bounds.height / mapSize;
            
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Rect chunkBounds = new Rect(
                        Bounds.x + x * chunkWidth,
                        Bounds.y + y * chunkHeight,
                        chunkWidth,
                        chunkHeight
                    );
                    Children[y * mapSize + x] = new QuadTree(chunkBounds, mapSize, depth + 1); 
                }
            }
        }


        public List<Vector2> Query(Rect area)
        {
            List<Vector2> foundPoints = new List<Vector2>();

            if (!Bounds.Overlaps(area))
            {
                return foundPoints;
            }

            if (Children == null)
            {
                foreach (Vector2 point in Points)
                {
                    if (area.Contains(point))
                    {
                        foundPoints.Add(point);
                    }
                }
            }
            else
            {
                foreach (QuadTree child in Children)
                {
                    foundPoints.AddRange(child.Query(area));
                }
            }

            return foundPoints;
        }


        private void Subdivide()
        {
            float halfWidth = Bounds.width / 2;
            float halfHeight = Bounds.height / 2;

            Children = new QuadTree[4];

            Children[0] = new QuadTree(new Rect(Bounds.x, Bounds.y, halfWidth, halfHeight), depth + 1);
            Children[1] = new QuadTree(new Rect(Bounds.x + halfWidth, Bounds.y, halfWidth, halfHeight), depth + 1);
            Children[2] = new QuadTree(new Rect(Bounds.x, Bounds.y + halfHeight, halfWidth, halfHeight), depth + 1);
            Children[3] = new QuadTree(new Rect(Bounds.x + halfWidth, Bounds.y + halfHeight, halfWidth, halfHeight),
                depth + 1);

            foreach (Vector2 point in Points)
            {
                foreach (QuadTree child in Children)
                {
                    if (child.Bounds.Contains(point))
                    {
                        child.Insert(point);
                        break;
                    }
                }
            }

            Points.Clear();
        }


        public bool Insert(Vector2 point)
        {
            if (!Bounds.Contains(point))
            {
                return false;
            }

            if (Children != null)
            {
                foreach (QuadTree child in Children)
                {
                    if (child.Insert(point))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (Points.Count < capacity || depth >= maxDepth)
            {
                Points.Add(point);
                return true;
            }

            Subdivide();

            foreach (QuadTree child in Children)
            {
                if (child.Bounds.Contains(point))
                {
                    return child.Insert(point);
                }
            }

            return false;
        }

        
        


        public Vector2 GetRandomPoint()
        {
            if (Points.Count > 0)
            {
                int randomIndex = Random.Range(0, Points.Count);
                return Points[randomIndex];
            }

            if (Children != null)
            {
                int randomChildIndex = Random.Range(0, 4);
                return Children[randomChildIndex].GetRandomPoint();
            }

            return Vector2.zero;
        }
    }
}