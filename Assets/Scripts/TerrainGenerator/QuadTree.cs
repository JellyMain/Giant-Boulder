using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace TerrainGenerator
{
    public class QuadTree
    {
        private int capacity = 50;
        private int maxDepth = 10;
        
        private Rect bounds;
        private List<Vector2> points;
        private QuadTree[] children;
        private int depth;


        public QuadTree(Rect bounds, int depth = 0)
        {
            this.bounds = bounds;
            this.depth = depth;
            points = new List<Vector2>();
            children = null;
        }


        public List<Vector2> Query(Rect area)
        {
            List<Vector2> foundPoints = new List<Vector2>();

            if (!bounds.Overlaps(area))
            {
                return foundPoints;
            }
            
            if (children == null)
            {
                foreach (Vector2 point in points)
                {
                    if (area.Contains(point))
                    {
                        foundPoints.Add(point);
                    }
                }
            }
            else
            {
                foreach (QuadTree child in children)
                {
                    foundPoints.AddRange(child.Query(area));
                }
            }

            return foundPoints;
        }
        
        
        private void Subdivide()
        {
            float halfWidth = bounds.width / 2;
            float halfHeight = bounds.height / 2;

            children = new QuadTree[4];

            children[0] = new QuadTree(new Rect(bounds.x, bounds.y, halfWidth, halfHeight), depth + 1);
            children[1] = new QuadTree(new Rect(bounds.x + halfWidth, bounds.y, halfWidth, halfHeight), depth + 1);
            children[2] = new QuadTree(new Rect(bounds.x, bounds.y + halfHeight, halfWidth, halfHeight), depth + 1);
            children[3] = new QuadTree(new Rect(bounds.x + halfWidth, bounds.y + halfHeight, halfWidth, halfHeight),
                depth + 1);

            foreach (Vector2 point in points)
            {
                foreach (QuadTree child in children)
                {
                    if (child.bounds.Contains(point))
                    {
                        child.Insert(point);
                        break;
                    }
                }
            }

            points.Clear();
        }


        public bool Insert(Vector2 point)
        {
            if (!bounds.Contains(point))
            {
                return false;
            }

            if (children != null)
            {
                foreach (QuadTree child in children)
                {
                    if (child.Insert(point))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (points.Count < capacity || depth >= maxDepth)
            {
                points.Add(point);
                return true;
            }

            Subdivide();

            foreach (QuadTree child in children)
            {
                if (child.bounds.Contains(point))
                {
                    return child.Insert(point);
                }
            }

            return false;
        }



        public Vector2 GetRandomPoint()
        {
            if (points.Count > 0)
            {
                int randomIndex = Random.Range(0, points.Count);
                return points[randomIndex];
            }

            if (children != null)
            {
                int randomChildIndex = Random.Range(0, 4);
                return children[randomChildIndex].GetRandomPoint();
            }

            return Vector2.zero;
        }
    }
}