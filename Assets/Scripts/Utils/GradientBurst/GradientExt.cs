using System;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace TerrainGenerator.GradientBurst
{
    internal static class GradientExt
    {
        private static readonly int m_PtrOffset;


        static GradientExt()
        {
            var m_PtrMember = typeof(Gradient).GetField("m_Ptr", BindingFlags.Instance | BindingFlags.NonPublic);

            m_PtrOffset = UnsafeUtility.GetFieldOffset(m_PtrMember);
        }


        private static unsafe IntPtr Ptr(this Gradient gradient)
        {
            var ptr = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(gradient, out var handle);
            var gradientPtr = *(IntPtr*)(ptr + m_PtrOffset);
            UnsafeUtility.ReleaseGCObject(handle);

            return gradientPtr;
        }


        /// <summary>
        /// Assigns a value to the original class from the copy.
        /// </summary>
        public static unsafe void SetData(this Gradient gradient, GradientStruct value)
        {
            *(GradientStruct*)gradient.Ptr() = value;
        }


        /// <summary>
        /// A copy of the gradient not bound to the original class.
        /// </summary>
        public static unsafe GradientStruct GetData(this Gradient gradient)
        {
            var ptr = gradient.Ptr();
            GradientStruct result = default;
            UnsafeUtility.MemCpy(&result, (void*)ptr, sizeof(GradientStruct));
            return result;
        }


        /// <summary>
        /// Direct access to the original class memory.
        /// </summary>
        public static unsafe GradientStruct* DirectAccess(this Gradient gradient)
        {
            return (GradientStruct*)gradient.Ptr();
        }


        /// <summary>
        /// Direct access to the memory of the original class. Read only. Suitable for multithreading.
        /// </summary>
        public static unsafe GradientStruct.ReadOnly DirectAccessReadOnly(this Gradient gradient)
        {
            return GradientStruct.AsReadOnly(gradient.DirectAccess());
        }
    }
}
