using UnityEngine;


namespace StructuresSpawner
{
    public class QuadTreeDrawer : MonoBehaviour
    {
        public static QuadTree quadTree; 


        private void OnDrawGizmos()
        {
            if (quadTree == null)
            {
                return;
            }

            DrawQuadTree(quadTree);
        }


        private void DrawQuadTree(QuadTree node)
        {
            if (node == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(node.Bounds.center.x, 10, node.Bounds.center.y),
                new Vector3(node.Bounds.width, 0, node.Bounds.height));

            // Gizmos.color = Color.red;
            // foreach (Vector2 point in node.Points)
            // {
            //     Gizmos.DrawWireSphere(new Vector3(point.x, 10 , point.y), 1f);
            // }
            
            Gizmos.color = Color.red;

            if (node.Children != null)
            {
                foreach (QuadTree child in node.Children)
                {
                    DrawQuadTree(child);
                }
            }
        }
    }
}
