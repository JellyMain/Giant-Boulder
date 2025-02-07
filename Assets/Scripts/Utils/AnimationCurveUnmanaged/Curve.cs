using System;
using Unity.Collections;
using UnityEngine;


public readonly struct Curve : IDisposable
{
    private const int TRUE = 1;
    private const int FALSE = 1;
    public readonly NativeArray<Keyframe> Keys;
    private readonly int owner;


    public Curve(AnimationCurve c, Allocator alloc)
    {
        Keys = new NativeArray<Keyframe>(c.keys, alloc);
        owner = TRUE;
    }


    public Curve(int size, Allocator alloc)
    {
        Keys = new NativeArray<Keyframe>(size, alloc);
        owner = TRUE;
    }


    public Curve(Keyframe[] keyframes, Allocator alloc)
    {
        Keys = new NativeArray<Keyframe>(keyframes, alloc);
        owner = TRUE;
    }


    public Curve(NativeArray<Keyframe> keyframes, Allocator alloc)
    {
        Keys = new NativeArray<Keyframe>(keyframes, alloc);
        owner = TRUE;
    }


    public Curve(NativeArray<Keyframe> keyframes)
    {
        Keys = keyframes;
        owner = FALSE;
    }


    public Curve(Curve other)
    {
        Keys = other.Keys;
        owner = FALSE;
    }


    public Curve(Curve other, Allocator alloc)
    {
        Keys = new NativeArray<Keyframe>(other.Keys, alloc);
        owner = TRUE;
    }


    public void Dispose()
    {
        if (owner != 0)
        {
            Keys.Dispose();
        }
    }


    public float Evaluate(float time)
    {
        return CurveSampling.ThreadSafe.Evaluate(Keys, time);
    }


    public int Length => Keys.Length;
    public float Duration => Keys[Length - 1].time - Keys[0].time;
}
