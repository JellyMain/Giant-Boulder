using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public unsafe struct CurveData
{
    public CurveData(Curve curve, Allocator alloc)
    {
        array = curve.Keys.GetUnsafePtr();
        size = curve.Length;
        allocatorLabel = alloc;
    }


    public static CurveData CreateInvalid()
    {
        return new CurveData()
        {
            array = null,
            size = 0,
            allocatorLabel = Allocator.Invalid
        };
    }


    public bool IsValid => array != null;

    // public ref Keyframe this[int index] => ref UnsafeUtilityEx.ArrayElementAsRef<Keyframe>(array, index);


    public Curve ToCurve()
    {
        NativeArray<Keyframe> keys =
            NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Keyframe>(array, size, allocatorLabel);
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref keys, AtomicSafetyHandle.Create());
        return new Curve(keys);
    }


    public void* array;
    public int size;
    public Allocator allocatorLabel;
}
