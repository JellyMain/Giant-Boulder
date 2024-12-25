using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    ///   <para>A compound handle to edit a capsule-shaped bounding volume in the Scene view.</para>
    /// </summary>
    public class CylinderBoundsHandle : PrimitiveBoundsHandle
    {
        #region Definitons
        /// <summary>
        ///   <para>An enumeration for specifying which axis on a CapsuleBoundsHandle object maps to the CapsuleBoundsHandle.height parameter.</para>
        /// </summary>
        public enum HeightAxis
        {
            /// <summary>
            ///   <para>X-axis.</para>
            /// </summary>
            X,
            /// <summary>
            ///   <para>Y-axis.</para>
            /// </summary>
            Y,
            /// <summary>
            ///   <para>Z-axis.</para>
            /// </summary>
            Z,
        }
        #endregion
        #region Variables
        protected const int k_DirectionX = 0;
        protected const int k_DirectionY = 1;
        protected const int k_DirectionZ = 2;
        protected static readonly Vector3[] s_HeightAxes =
        {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };
        protected static readonly int[] s_NextAxis = new int[]
        {
            1,
            2,
            0
        };
        protected int m_HeightAxis = 1;
        #endregion
        #region Properties
        /// <summary>
        ///   <para>Returns or specifies the axis in the handle's space to which height maps. The radius maps to the remaining axes.</para>
        /// </summary>
        public HeightAxis heightAxis
        {
            get => (HeightAxis) m_HeightAxis;
            set
            {
                int index = (int) value;
                if (m_HeightAxis == index)
                    return;
                Vector3 size = Vector3.one * radius * 2f;
                size[index] = GetSize()[m_HeightAxis];
                m_HeightAxis = index;
                SetSize(size);
            }
        }
        /// <summary>
        ///   <para>Returns or specifies the height of the capsule bounding volume.</para>
        /// </summary>
        public float height
        {
            get => !IsAxisEnabled(m_HeightAxis) ? 0.0f : Mathf.Max(GetSize()[m_HeightAxis], 2f * radius);
            set
            {
                value = Mathf.Max(Mathf.Abs(value), 2f * radius);
                if (Math.Abs((double) height - (double) value) < .001f)
                    return;
                Vector3 size = GetSize();
                size[m_HeightAxis] = value;
                SetSize(size);
            }
        }
        /// <summary>
        ///   <para>Returns or specifies the radius of the capsule bounding volume.</para>
        /// </summary>
        public float radius
        {
            get
            {
                int radiusAxis;
                return GetRadiusAxis(out radiusAxis) || IsAxisEnabled(m_HeightAxis) ? 0.5f * GetSize()[radiusAxis] : 0.0f;
            }
            set
            {
                Vector3 size = GetSize();
                float b = 2f * value;
                for (int index = 0; index < 3; ++index)
                    size[index] = index == m_HeightAxis ? Mathf.Max(size[index], b) : b;
                SetSize(size);
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        ///   <para>Create a new instance of the CapsuleBoundsHandle class.</para>
        /// </summary>
        /// <param name="controlIDHint">An integer value used to generate consistent control IDs for each control handle on this instance. Avoid using the same value for all of your CapsuleBoundsHandle instances.</param>
        [Obsolete("Use parameterless constructor instead.")]
        public CylinderBoundsHandle(int controlIDHint) : base(controlIDHint)
        {
        }
        /// <summary>
        ///   <para>Create a new instance of the CapsuleBoundsHandle class.</para>
        /// </summary>
        public CylinderBoundsHandle()
        {
        }
        #endregion
        #region Methods
        /// <summary>
        ///   <para>Draw a wireframe capsule for this instance.</para>
        /// </summary>
        protected override void DrawWireframe()
        {
            CapsuleBoundsHandle.HeightAxis heightAxis1 = CapsuleBoundsHandle.HeightAxis.Y;
            CapsuleBoundsHandle.HeightAxis heightAxis2 = CapsuleBoundsHandle.HeightAxis.Z;
            switch (heightAxis)
            {
                case HeightAxis.Y:
                    heightAxis1 = CapsuleBoundsHandle.HeightAxis.Z;
                    heightAxis2 = CapsuleBoundsHandle.HeightAxis.X;
                    break;
                case HeightAxis.Z:
                    heightAxis1 = CapsuleBoundsHandle.HeightAxis.X;
                    heightAxis2 = CapsuleBoundsHandle.HeightAxis.Y;
                    break;
            }
            bool flag1 = IsAxisEnabled((int) heightAxis);
            bool flag2 = IsAxisEnabled((int) heightAxis1);
            bool flag3 = IsAxisEnabled((int) heightAxis2);
            Vector3 heightAx1 = s_HeightAxes[m_HeightAxis];
            Vector3 heightAx2 = s_HeightAxes[s_NextAxis[m_HeightAxis]];
            Vector3 heightAx3 = s_HeightAxes[s_NextAxis[s_NextAxis[m_HeightAxis]]];
            float radius = this.radius;
            float height = this.height;
            Vector3 center1 = center + heightAx1 * (height * 0.5f - radius);
            Vector3 center2 = center - heightAx1 * (height * 0.5f - radius);
            if (flag1)
            {
                if (flag3)
                {
                    Handles.DrawLine(center1 + heightAx3 * radius, center2 + heightAx3 * radius);
                    Handles.DrawLine(center1 - heightAx3 * radius, center2 - heightAx3 * radius);
                }
                if (flag2)
                {
                    Handles.DrawLine(center1 + heightAx2 * radius, center2 + heightAx2 * radius);
                    Handles.DrawLine(center1 - heightAx2 * radius, center2 - heightAx2 * radius);
                }
            }
            if (!(flag2 & flag3))
                return;
            Handles.DrawWireArc(center1, heightAx1, heightAx2, 360f, radius);
            Handles.DrawWireArc(center2, heightAx1, heightAx2, -360f, radius);
        }
        protected override Bounds OnHandleChanged(
            HandleDirection handle,
            Bounds boundsOnClick,
            Bounds newBounds)
        {
            int index1 = 0;
            switch (handle)
            {
                case HandleDirection.PositiveY:
                case HandleDirection.NegativeY:
                    index1 = 1;
                    break;
                case HandleDirection.PositiveZ:
                case HandleDirection.NegativeZ:
                    index1 = 2;
                    break;
            }
            Vector3 max = newBounds.max;
            Vector3 min = newBounds.min;
            if (index1 == m_HeightAxis)
            {
                int radiusAxis;
                GetRadiusAxis(out radiusAxis);
                float num = max[radiusAxis] - min[radiusAxis];
                if ((double) (max[m_HeightAxis] - min[m_HeightAxis]) < (double) num)
                {
                    if (handle == HandleDirection.PositiveX ||
                        handle == HandleDirection.PositiveY ||
                        handle == HandleDirection.PositiveZ)
                        max[m_HeightAxis] = min[m_HeightAxis] + num;
                    else
                        min[m_HeightAxis] = max[m_HeightAxis] - num;
                }
            }
            else
            {
                ref Vector3 local1 = ref max;
                int heightAxis1 = m_HeightAxis;
                double num1 = (double) boundsOnClick.center[m_HeightAxis];
                Vector3 vector3 = boundsOnClick.size;
                double num2 = 0.5 * (double) vector3[m_HeightAxis];
                double num3 = num1 + num2;
                local1[heightAxis1] = (float) num3;
                ref Vector3 local2 = ref min;
                int heightAxis2 = m_HeightAxis;
                vector3 = boundsOnClick.center;
                double num4 = (double) vector3[m_HeightAxis];
                vector3 = boundsOnClick.size;
                double num5 = 0.5 * (double) vector3[m_HeightAxis];
                double num6 = num4 - num5;
                local2[heightAxis2] = (float) num6;
                float b = (float) (0.5 * ((double) max[index1] - (double) min[index1]));
                float a = (float) (0.5 * ((double) max[m_HeightAxis] - (double) min[m_HeightAxis]));
                for (int index2 = 0; index2 < 3; ++index2)
                {
                    if (index2 != index1)
                    {
                        float num7 = index2 == m_HeightAxis ? Mathf.Max(a, b) : b;
                        ref Vector3 local3 = ref min;
                        int index3 = index2;
                        vector3 = center;
                        double num8 = (double) vector3[index2] - (double) num7;
                        local3[index3] = (float) num8;
                        ref Vector3 local4 = ref max;
                        int index4 = index2;
                        vector3 = center;
                        double num9 = (double) vector3[index2] + (double) num7;
                        local4[index4] = (float) num9;
                    }
                }
            }
            return new Bounds((max + min) * 0.5f, max - min);
        }
        private bool GetRadiusAxis(out int radiusAxis)
        {
            radiusAxis = s_NextAxis[m_HeightAxis];
            if (IsAxisEnabled(radiusAxis))
                return IsAxisEnabled(s_NextAxis[radiusAxis]);
            radiusAxis = s_NextAxis[radiusAxis];
            return false;
        }
        #endregion
    }
}