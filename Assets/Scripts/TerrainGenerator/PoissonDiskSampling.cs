using System.Collections.Generic;
using UnityEngine;


namespace TerrainGenerator
{
    public static class PoissonDiskSampling
    {
        public static List<Vector3> GetPoints(int boxSize, float radius, int attemptsCount)
        {
            List<Vector3> placedPoints = new List<Vector3>();
            List<Vector3> activePoints = new List<Vector3>();


            float cellSize = radius / Mathf.Sqrt(2);
            int gridSize = Mathf.CeilToInt(boxSize / cellSize);
            Vector3[,] grid = new Vector3[gridSize, gridSize];

            Vector3 startingPoint = new Vector3(boxSize / 2, 0, boxSize / 2);
            activePoints.Add(startingPoint);
            placedPoints.Add(startingPoint);
            int gridX = Mathf.FloorToInt(startingPoint.x / cellSize);
            int gridY = Mathf.FloorToInt(startingPoint.z / cellSize);
            grid[gridX, gridY] = startingPoint;

            while (activePoints.Count > 0)
            {
                Vector3 currentActivePoint = activePoints[Random.Range(0, activePoints.Count)];
                bool isValid = false;

                for (int i = 0; i < attemptsCount; i++)
                {
                    float randomDistance = Random.Range(radius, radius * 2);
                    Vector2 unitCirclePoint = Random.insideUnitCircle;
                    Vector3 randomDirection = new Vector3(unitCirclePoint.x, 0, unitCirclePoint.y).normalized;

                    Vector3 candidate = currentActivePoint + randomDirection * randomDistance;

                    if (IsValid(candidate, cellSize, grid, radius, gridSize))
                    {
                        activePoints.Add(candidate);
                        placedPoints.Add(candidate);
                        int candidateGridX = Mathf.FloorToInt(candidate.x / cellSize);
                        int candidateGridY = Mathf.FloorToInt(candidate.z / cellSize);
                        grid[candidateGridX, candidateGridY] = candidate;
                        isValid = true;

                        break;
                    }
                }

                if (!isValid)
                {
                    activePoints.Remove(currentActivePoint);
                }
            }

            return placedPoints;
        }


        private static bool IsValid(Vector3 candidatePoint, float cellSize, Vector3[,] grid, float radius, int gridSize)
        {
            int gridX = Mathf.FloorToInt(candidatePoint.x / cellSize);
            int gridY = Mathf.FloorToInt(candidatePoint.z / cellSize);


            if (gridX < 0 || gridY < 0 || gridX >= gridSize || gridY >= gridSize || grid[gridX, gridY] != Vector3.zero)
            {
                return false;
            }


            int searchingRadius = 2;

            for (int x = gridX - searchingRadius; x < gridX + searchingRadius; x++)
            {
                for (int y = gridY - searchingRadius; y < gridY + searchingRadius; y++)
                {
                    if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1))
                    {
                        Vector3 neighborPoint = grid[x, y];

                        if (neighborPoint != Vector3.zero &&
                            (candidatePoint - neighborPoint).sqrMagnitude < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
