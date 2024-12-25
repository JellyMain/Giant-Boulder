#ifndef GENA_BOUNDS
#define GENA_BOUNDS

struct Bounds
{
    float3 center;
    float3 size;
    float3 extents;
    float3 min;
    float3 max;
};

Bounds Expand(Bounds bounds, float3 amount)
{
    bounds.extents += amount;
    return bounds;
}

bool Intersects(Bounds boundsA, Bounds boundsB)
{
    return boundsA.min.x <= boundsB.max.x && boundsA.max.x >= boundsB.min.x &&
        boundsA.min.y <= boundsB.max.y && boundsA.max.y >= boundsB.min.y &&
        boundsA.min.z <= boundsB.max.z && boundsA.max.z >= boundsB.min.z;
}

bool Contains(Bounds bounds, float3 location)
{
    return location.x >= bounds.min.x && location.x <= bounds.max.x &&
        location.y >= bounds.min.y && location.y <= bounds.max.y &&
        location.z >= bounds.min.z && location.z <= bounds.max.z;
}

#endif
