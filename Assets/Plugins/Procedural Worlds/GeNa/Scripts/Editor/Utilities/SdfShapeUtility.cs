using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class SdfShapeUtility
    {
        static SdfShapeUtility()
        {
            GeNaEvents.SDFDrawWireCylinder = DrawWireCylinder;
            GeNaEvents.SDFDrawWireCapsule = DrawWireCapsule;
            GeNaEvents.SDFDrawWireLine = DrawLine;
        }
        #region Methods
        private static bool DrawWireCylinder(Vector3 a, Vector3 b, float r, Color c = default)
        {
            if (c != default)
                Handles.color = c;
            Vector3 forward = b - a;
            Quaternion rotation = Quaternion.LookRotation(forward);
            float length = forward.magnitude;
            Vector3 center2 = new Vector3(0f, 0, length);
            Matrix4x4 angleMatrix = Matrix4x4.TRS(a, rotation, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, r);
                Handles.DrawWireDisc(center2, Vector3.forward, r);
                DrawLine(r, 0f, length);
                DrawLine(-r, 0f, length);
                DrawLine(0f, r, length);
                DrawLine(0f, -r, length);
            }
            return true;
        }
        private static bool DrawWireCapsule(Vector3 a, Vector3 b, float r, Color c = default)
        {
            if (c != default)
                Handles.color = c;
            Vector3 forward = b - a;
            Quaternion rotation = Quaternion.LookRotation(forward);
            float pointOffset = r / 2f;
            float length = forward.magnitude;
            Vector3 center2 = new Vector3(0f, 0, length);
            Matrix4x4 angleMatrix = Matrix4x4.TRS(a, rotation, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, r);
                Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.left * pointOffset, -180f, r);
                Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.down * pointOffset, -180f, r);
                Handles.DrawWireDisc(center2, Vector3.forward, r);
                Handles.DrawWireArc(center2, Vector3.up, Vector3.right * pointOffset, -180f, r);
                Handles.DrawWireArc(center2, Vector3.left, Vector3.up * pointOffset, -180f, r);
                DrawLine(r, 0f, length);
                DrawLine(-r, 0f, length);
                DrawLine(0f, r, length);
                DrawLine(0f, -r, length);
            }
            return true;
        }
        private static bool DrawLine(float arg1, float arg2, float forward)
        {
            Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward));
            return true;
        }
        #endregion
    }
}