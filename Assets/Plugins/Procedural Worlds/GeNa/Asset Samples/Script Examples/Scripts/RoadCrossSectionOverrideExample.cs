using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeNa.Core;
/// <summary>
/// This derrived class of the RoadCrossSection class is used to override
/// the cross section used by a GeNa Road Extention to create Road Meshes.
/// Place an instance of this component onto a GeNa Spline GameObject.
/// </summary>
public class RoadCrossSectionOverrideExample : RoadCrossSectionOverride
{
    RoadCrossSection roadCrossSection = new RoadCrossSection(_points, _normals);

    private static readonly Vector2[] _points = new Vector2[]
    {
            new Vector2(-0.5f, 0.04f),
            new Vector2(-0.48f, 0.04f),
            new Vector2(-0.47f, 0.026f),
            new Vector2(-0.4f, 0.025f),
            new Vector2(0.4f, 0.025f),
            new Vector2(0.47f, 0.026f),
            new Vector2(0.48f, 0.04f),
            new Vector2(0.5f, 0.04f)
    };
    private static readonly Vector2[] _normals = new Vector2[]
    {
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(0.707f, 0.707f),
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(-0.707f, 0.707f),
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 1.0f)
    };

    /// <summary>
    /// Return a RoadCrossSection instance for overriding
    /// the cross section used by the GeNa Road Extension
    /// to generate the road meshe(s).
    /// Note that the number of points and normals must be even, and the same size.
    /// </summary>
    /// <returns></returns>
    public override RoadCrossSection GetRoadCrossSection()
    {
        return roadCrossSection;
    }
}
