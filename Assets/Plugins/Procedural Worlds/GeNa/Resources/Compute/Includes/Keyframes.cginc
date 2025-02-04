#ifndef GENA_KEYFRAMES
#define GENA_KEYFRAMES

struct Keyframe
{
    float time;
    float value;
    float inTangent;
    float outTangent;
};

float EvaluateKeyframes(float t, Keyframe keyframe0, Keyframe keyframe1)
{
    float dt = keyframe1.time - keyframe0.time;

    float m0 = keyframe0.outTangent * dt;
    float m1 = keyframe1.inTangent * dt;

    t = (t - keyframe0.time) / dt;

    float t2 = t * t;
    float t3 = t2 * t;

    float a = 2 * t3 - 3 * t2 + 1;
    float b = t3 - 2 * t2 + t;
    float c = t3 - t2;
    float d = -2 * t3 + 3 * t2;

    return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
}

float EvaluateFalloff(float t, StructuredBuffer<Keyframe> buffer, int count)
{
    float result = 0.0f;
    for (int i = 0; i < count - 1; i++)
    {
        Keyframe keyframe0 = buffer[i];
        Keyframe keyframe1 = buffer[i + 1];
        if (t >= keyframe0.time && t <= keyframe1.time)
        {
            return EvaluateKeyframes(t, keyframe0, keyframe1);
        }
    }
    return result;
}

#endif
