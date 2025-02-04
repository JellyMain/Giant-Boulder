#ifndef GENA_GPUNOISE_PARAMS
#define GENA_GPUNOISE_PARAMS

#include "Keyframes.cginc"

////////////////////////////////////////////////////////////////////////////////
//Externally available parameters
////////////////////////////////////////////////////////////////////////////////

//The computed noisemap
bool _NoisemapEnabled = true;
int _NoisemapType = 1;
float _NoisemapStrength = 1.0f;
float _NoisemapSeed = 1000;
int _NoisemapOctaves = 9;
float _NoisemapFrequency = 0.8;
float _NoisemapPersistence = 0.5;
float _NoisemapLacunarity = 2.0;
float _NoisemapAmplitude = 0.5;
float2 _NoisemapOffset = float2(0.0, 0.0);
float _NoisemapWarp = 0.0;
float _NoisemapWarp0 = 0.0;
float _NoisemapDamp = 0.0;
float _NoisemapDamp0 = 0.0;
float _NoisemapDampScale = 0.0;
float _NoisemapRidgedOffset = 0.0;
StructuredBuffer<Keyframe> _NoiseFalloff;
uint _NoiseFalloffCount = 0;

#endif