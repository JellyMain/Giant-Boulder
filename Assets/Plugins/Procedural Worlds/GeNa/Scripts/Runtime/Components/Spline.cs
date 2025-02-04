using UnityEngine;

namespace GeNa.Core
{
    public class Spline : GeNaSpline
    {      
        /// <summary>
        /// Creates a Spline with a given Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Spline CreateSpline(string name)
        {
            GameObject genaGo = new GameObject(name);
            // GeNa Spawner
            GameObject parent = GeNaUtility.GeNaSplinesTransform.gameObject;
            // Reparent it
            genaGo.transform.SetParent(parent.transform);
            // Add & Return Spline Component
            Spline spline = genaGo.AddComponent<Spline>();
            spline.Name = name;
            return spline;
        }
    }
}