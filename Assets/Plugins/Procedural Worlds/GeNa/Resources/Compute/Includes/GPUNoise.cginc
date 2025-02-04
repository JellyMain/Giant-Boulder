#ifndef GENA_NOISE
#define GENA_NOISE

////////////////////////////////////////////////////////////////////////////////
//Includes
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
//Constants
////////////////////////////////////////////////////////////////////////////////
//
// static const uint NumThreads = 256;
// static const float PI = 3.14159265358979323846f;
// static const float DEGREES_TO_RADIANS = PI / 180.0f;

////////////////////////////////////////////////////////////////////////////////
// Noise Methods
////////////////////////////////////////////////////////////////////////////////

// The noise methods in this section have been adapted from the amazing open source turbulence 
// libary by Jérémie St-Amand - check it out here https://github.com/jesta88/Turbulence-Library
//
// The licence for the noise specific methods contained in this shader are open source and 
// the licence for them can be found here:
//
// https://github.com/jesta88/Turbulence-Library/blob/master/LICENSE

//
//	FAST32_hash
//	A very fast hashing function.  Requires 32bit support.
//	http://briansharpe.wordpress.com/2011/11/15/a-fast-and-simple-32bit-floating-point-hash-function/
//
//	The hash formula takes the form....
//	hash = mod( coord.x * coord.x * coord.y * coord.y, SOMELARGEFLOAT ) / SOMELARGEFLOAT
//	We truncate and offset the domain to the most interesting part of the noise.
//	SOMELARGEFLOAT should be in the range of 400.0->1000.0 and needs to be hand picked.  Only some give good results.
//	3D Noise is achieved by offsetting the SOMELARGEFLOAT value by the Z coordinate
//
void FAST32_hash_2D(float2 gridcell, out float4 hash_0, out float4 hash_1) //	generates 2 random numbers for each of the 4 cell corners
{
    //    gridcell is assumed to be an integer coordinate
    const float2 OFFSET = float2(26.0, 161.0);
    const float DOMAIN = 71.0;
    const float2 SOMELARGEFLOATS = float2(951.135664, 642.949883);
    float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
    P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;
    P += OFFSET.xyxy;
    P *= P;
    P = P.xzxz * P.yyww;
    hash_0 = frac(P * (1.0 / SOMELARGEFLOATS.x));
    hash_1 = frac(P * (1.0 / SOMELARGEFLOATS.y));
}

//
//	FAST32_hash
//	A very fast hashing function.  Requires 32bit support.
//	http://briansharpe.wordpress.com/2011/11/15/a-fast-and-simple-32bit-floating-point-hash-function/
//
//	The hash formula takes the form....
//	hash = mod( coord.x * coord.x * coord.y * coord.y, SOMELARGEFLOAT ) / SOMELARGEFLOAT
//	We truncate and offset the domain to the most interesting part of the noise.
//	SOMELARGEFLOAT should be in the range of 400.0->1000.0 and needs to be hand picked.  Only some give good results.
//	3D Noise is achieved by offsetting the SOMELARGEFLOAT value by the Z coordinate
//
float4 FAST32_hash_2D_Cell(float2 gridcell) //	generates 4 different random numbers for the single given cell point
{
    //	gridcell is assumed to be an integer coordinate
    const float2 OFFSET = float2(26.0, 161.0);
    const float DOMAIN = 71.0;
    const float4 SOMELARGEFLOATS = float4(951.135664, 642.949883, 803.202459, 986.973274);
    float2 P = gridcell - floor(gridcell * (1.0 / DOMAIN)) * DOMAIN;
    P += OFFSET.xy;
    P *= P;
    return frac((P.x * P.y) * (1.0 / SOMELARGEFLOATS.xyzw));
}

//
//	SimplexPerlin2D  ( simplex gradient noise )
//	Perlin noise over a simplex (triangular) grid
//	Return value range of -1.0->1.0
//	http://briansharpe.files.wordpress.com/2012/01/simplexperlinsample.jpg
//
//	Implementation originally based off Stefan Gustavson's and Ian McEwan's work at...
//	http://github.com/ashima/webgl-noise
//
float SimplexPerlin2D(float2 P)
{
    //	simplex math constants
    const float SKEWFACTOR = 0.36602540378443864676372317075294; // 0.5*(sqrt(3.0)-1.0)
    const float UNSKEWFACTOR = 0.21132486540518711774542560974902; // (3.0-sqrt(3.0))/6.0
    const float SIMPLEX_TRI_HEIGHT = 0.70710678118654752440084436210485; // sqrt( 0.5 )	height of simplex triangle
    const float3 SIMPLEX_POINTS = float3(1.0 - UNSKEWFACTOR, -UNSKEWFACTOR, 1.0 - 2.0 * UNSKEWFACTOR); //	vertex info for simplex triangle

    //	establish our grid cell.
    P *= SIMPLEX_TRI_HEIGHT; // scale space so we can have an approx feature size of 1.0  ( optional )
    const float2 Pi = floor(P + dot(P, float2(SKEWFACTOR, SKEWFACTOR)));

    //	calculate the hash.
    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    //	establish vectors to the 3 corners of our simplex triangle
    float2 v0 = Pi - dot(Pi, float2(UNSKEWFACTOR, UNSKEWFACTOR)) - P;
    float4 v1pos_v1hash = (v0.x < v0.y) ? float4(SIMPLEX_POINTS.xy, hash_x.y, hash_y.y) : float4(SIMPLEX_POINTS.yx, hash_x.z, hash_y.z);
    float4 v12 = float4(v1pos_v1hash.xy, SIMPLEX_POINTS.zz) + v0.xyxy;

    //	calculate the dotproduct of our 3 corner vectors with 3 random normalized vectors
    const float3 grad_x = float3(hash_x.x, v1pos_v1hash.z, hash_x.w) - 0.49999;
    const float3 grad_y = float3(hash_y.x, v1pos_v1hash.w, hash_y.w) - 0.49999;
    const float3 grad_results = rsqrt(grad_x * grad_x + grad_y * grad_y) * (grad_x * float3(v0.x, v12.xz) + grad_y * float3(v0.y, v12.yw));

    //	Normalization factor to scale the final result to a strict 1.0->-1.0 range
    //	x = ( sqrt( 0.5 )/sqrt( 0.75 ) ) * 0.5
    //	NF = 1.0 / ( x * ( ( 0.5 ? x*x ) ^ 4 ) * 2.0 )
    //	http://briansharpe.wordpress.com/2012/01/13/simplex-noise/#comment-36
    const float FINAL_NORMALIZATION = 99.204334582718712976990005025589;

    //	evaluate the surflet, sum and return
    float3 m = float3(v0.x, v12.xz) * float3(v0.x, v12.xz) + float3(v0.y, v12.yw) * float3(v0.y, v12.yw);
    m = max(0.5 - m, 0.0); //	The 0.5 here is SIMPLEX_TRI_HEIGHT^2
    m = m * m;
    m = m * m;
    return dot(m, grad_results) * FINAL_NORMALIZATION;
}

//
//	SimplexPerlin2D_Deriv
//	SimplexPerlin2D noise with derivatives
//	returns float3( value, xderiv, yderiv )
//
float3 SimplexPerlin2D_Deriv(float2 P)
{
    //	simplex math constants
    const float SKEWFACTOR = 0.36602540378443864676372317075294; // 0.5*(sqrt(3.0)-1.0)
    const float UNSKEWFACTOR = 0.21132486540518711774542560974902; // (3.0-sqrt(3.0))/6.0
    const float SIMPLEX_TRI_HEIGHT = 0.70710678118654752440084436210485; // sqrt( 0.5 )	height of simplex triangle
    const float3 SIMPLEX_POINTS = float3(1.0 - UNSKEWFACTOR, -UNSKEWFACTOR, 1.0 - 2.0 * UNSKEWFACTOR); //	vertex info for simplex triangle

    //	establish our grid cell.
    P *= SIMPLEX_TRI_HEIGHT; // scale space so we can have an approx feature size of 1.0  ( optional )
    const float2 Pi = floor(P + dot(P, float2(SKEWFACTOR, SKEWFACTOR)));

    //	calculate the hash.
    float4 hash_x, hash_y;
    FAST32_hash_2D(Pi, hash_x, hash_y);

    //	establish vectors to the 3 corners of our simplex triangle
    float2 v0 = Pi - dot(Pi, float2(UNSKEWFACTOR, UNSKEWFACTOR)) - P;
    float4 v1pos_v1hash = (v0.x < v0.y) ? float4(SIMPLEX_POINTS.xy, hash_x.y, hash_y.y) : float4(SIMPLEX_POINTS.yx, hash_x.z, hash_y.z);
    float4 v12 = float4(v1pos_v1hash.xy, SIMPLEX_POINTS.zz) + v0.xyxy;

    //	calculate the dotproduct of our 3 corner vectors with 3 random normalized vectors
    float3 grad_x = float3(hash_x.x, v1pos_v1hash.z, hash_x.w) - 0.49999;
    float3 grad_y = float3(hash_y.x, v1pos_v1hash.w, hash_y.w) - 0.49999;
    const float3 norm = rsqrt(grad_x * grad_x + grad_y * grad_y);
    grad_x *= norm;
    grad_y *= norm;
    const float3 grad_results = grad_x * float3(v0.x, v12.xz) + grad_y * float3(v0.y, v12.yw);

    //	evaluate the surflet
    float3 m = float3(v0.x, v12.xz) * float3(v0.x, v12.xz) + float3(v0.y, v12.yw) * float3(v0.y, v12.yw);
    m = max(0.5 - m, 0.0); //	The 0.5 here is SIMPLEX_TRI_HEIGHT^2
    const float3 m2 = m * m;
    const float3 m4 = m2 * m2;

    //	calc the deriv
    const float3 temp = 8.0 * m2 * m * grad_results;
    float xderiv = dot(temp, float3(v0.x, v12.xz)) - dot(m4, grad_x);
    float yderiv = dot(temp, float3(v0.y, v12.yw)) - dot(m4, grad_y);

    const float FINAL_NORMALIZATION = 99.204334582718712976990005025589; //	scales the final result to a strict 1.0->-1.0 range

    //	sum the surflets and return all results combined in a float3
    return float3(dot(m4, grad_results), xderiv, yderiv) * FINAL_NORMALIZATION;
}

float Cellular2D(float2 xy, int cellType, int distanceFunction)
{
    const int xi = int(floor(xy.x));
    const int yi = int(floor(xy.y));

    const float xf = xy.x - float(xi);
    const float yf = xy.y - float(yi);

    float dist1 = 9999999.0;
    float dist2 = 9999999.0;
    float dist3 = 9999999.0;
    float dist4 = 9999999.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 cell = FAST32_hash_2D_Cell(float2(xi + x, yi + y)).xy;
            cell.x += (float(x) - xf);
            cell.y += (float(y) - yf);
            float dist = 0.0;
            if (distanceFunction <= 1)
            {
                dist = sqrt(dot(cell, cell));
            }
            else if (distanceFunction > 1 && distanceFunction <= 2)
            {
                dist = dot(cell, cell);
            }
            else if (distanceFunction > 2 && distanceFunction <= 3)
            {
                dist = abs(cell.x) + abs(cell.y);
                dist *= dist;
            }
            else if (distanceFunction > 3 && distanceFunction <= 4)
            {
                dist = max(abs(cell.x), abs(cell.y));
                dist *= dist;
            }
            else if (distanceFunction > 4 && distanceFunction <= 5)
            {
                dist = dot(cell, cell) + cell.x * cell.y;
            }
            else if (distanceFunction > 5 && distanceFunction <= 6)
            {
                dist = pow(abs(cell.x * cell.x * cell.x * cell.x + cell.y * cell.y * cell.y * cell.y), 0.25);
            }
            else if (distanceFunction > 6 && distanceFunction <= 7)
            {
                dist = sqrt(abs(cell.x)) + sqrt(abs(cell.y));
                dist *= dist;
            }
            if (dist < dist1)
            {
                dist4 = dist3;
                dist3 = dist2;
                dist2 = dist1;
                dist1 = dist;
            }
            else if (dist < dist2)
            {
                dist4 = dist3;
                dist3 = dist2;
                dist2 = dist;
            }
            else if (dist < dist3)
            {
                dist4 = dist3;
                dist3 = dist;
            }
            else if (dist < dist4)
            {
                dist4 = dist;
            }
        }
    }

    float result;
    if (cellType <= 1) // F1
        result = dist1; //	scale return value from 0.0->1.333333 to 0.0->1.0  	(2/3)^2 * 3  == (12/9) == 1.333333
    else if (cellType > 1 && cellType <= 2) // F2
        result = dist2;
    else if (cellType > 2 && cellType <= 3) // F3
        result = dist3;
    else if (cellType > 3 && cellType <= 4) // F4
        result = dist4;
    else if (cellType > 4 && cellType <= 5) // F2 - F1 
        result = dist2 - dist1;
    else if (cellType > 5 && cellType <= 6) // F3 - F2 
        result = dist3 - dist2;
    else if (cellType > 6 && cellType <= 7) // F1 + F2/2
        result = dist1 + dist2 / 2.0;
    else if (cellType > 7 && cellType <= 8) // F1 * F2
        result = dist1 * dist2;
    else if (cellType > 8 && cellType <= 9) // Crackle
        result = max(1.0, 10 * (dist2 - dist1));
    else
        result = dist1;
    return result;
}

//Get simplex
float SimplexNormal(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        const float h = SimplexPerlin2D((p + offset) * frequency);
        sum += h * amplitude;
        frequency *= lacunarity;
        amplitude *= persistence;
    }
    return sum;
}

//Get multi fractal simplex noise
float SimplexMulti(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence, float ridgeOffset)
{
    float result = SimplexNormal(p, 3, offset, frequency, amplitude, lacunarity, persistence);
    if (result > ridgeOffset)
    {
        result = lerp(result, SimplexNormal(p, octaves, offset, frequency, amplitude, lacunarity, persistence), result - ridgeOffset);
    }

    return result;
}

//Get simplex billow
float SimplexBillowed(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        const float h = abs(SimplexPerlin2D((p + offset) * frequency));
        sum += h * amplitude;
        frequency *= lacunarity;
        amplitude *= persistence;
    }
    return sum;
}

//Get simplex ridged
float SimplexRidged(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence, float ridgeOffset)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        const float h = 0.5 * (ridgeOffset - abs(4 * SimplexPerlin2D((p + offset) * frequency)));
        sum += h * amplitude;
        frequency *= lacunarity;
        amplitude *= persistence;
    }
    return sum;
}

//Get simplex derived IQ
float SimplexDerivedIQ(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence)
{
    float sum = 0;
    float2 dsum = float2(0.0, 0.0);
    for (int i = 0; i < octaves; i++)
    {
        float3 n = SimplexPerlin2D_Deriv((p + offset) * frequency);
        dsum += n.yz;
        sum += amplitude * n.x / (1 + dot(dsum, dsum));
        frequency *= lacunarity;
        amplitude *= persistence;
    }
    return sum;
}

//Get simplex derived swiss
float SimplexDerivedSwiss(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence, float warp, float ridgeOffset)
{
    float sum = 0.0;
    float2 dsum = float2(0.0, 0.0);
    for (int i = 0; i < octaves; i++)
    {
        float3 n = 0.5 * (0 + (ridgeOffset - abs(SimplexPerlin2D_Deriv((p + offset + warp * dsum) * frequency))));
        sum += amplitude * n.x;
        dsum += amplitude * n.yz * -n.x;
        frequency *= lacunarity;
        amplitude *= persistence * saturate(sum);
    }
    return sum;
}

//Simpelx derived Jordan
float SimplexDerivedJordan(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence, float warp0, float warp, float damp0, float damp, float damp_scale)
{
    float3 n = SimplexPerlin2D_Deriv((p + offset) * frequency);
    float3 n2 = n * n.x;
    float sum = n2.x;
    float2 dsum_warp = warp0 * n2.yz;
    float2 dsum_damp = damp0 * n2.yz;
    float damped_amp = amplitude * persistence;
    for (int i = 0; i < octaves; i++)
    {
        n = SimplexPerlin2D_Deriv((p + offset) * frequency + dsum_warp.xy);
        n2 = n * n.x;
        sum += damped_amp * n2.x;
        dsum_warp += warp * n2.yz;
        dsum_damp += damp * n2.yz;
        frequency *= lacunarity;
        amplitude *= persistence * saturate(sum);
        damped_amp = amplitude * (1 - damp_scale / (1 + dot(dsum_damp, dsum_damp)));
    }
    return sum;
}

//Get cell noise
float CellNormal(float2 p, int octaves, float2 offset, float frequency, float amplitude, float lacunarity, float persistence, int cellType, int distanceFunction)
{
    float sum = 0;
    for (int i = 0; i < octaves; i++)
    {
        const float h = Cellular2D((p + offset) * frequency, cellType, distanceFunction);
        sum += h * amplitude;
        frequency *= lacunarity;
        amplitude *= persistence;
    }
    return sum;
}

////////////////////////////////////////////////////////////////////////////////
// Utility Methods
////////////////////////////////////////////////////////////////////////////////

//Get the distance beweent two points
float GetDistance(float2 point1, float2 point2)
{
    return sqrt(pow(point2.y - point1.y, 2) + pow(point2.x - point1.x, 2));
}

// Convert a 1D address into a 2D address assuming the dimensions supplied
uint2 Translate1DTo2D(uint address1D, uint dimensions)
{
    //Calculate location
    uint y = address1D / dimensions;
    uint x = address1D - (y * dimensions);

    //Return
    return uint2(x, y);
}

// Convert a 2D normalised address to a 1D non normalised address
uint Translate2DTo1DActual(float2 address2D, float dimensions)
{
    const uint x = address2D.x * (dimensions - 1.0);
    const uint y = address2D.y * (dimensions - 1.0);
    return (y * dimensions) + x;
}

// Convert a standard 2d address to a 1d address
uint Translate2DTo1D(uint x, uint y, float dimensions)
{
    return (y * dimensions) + x;
}

float GetNoise(float2 position)
{
    float result = 0.0f;
    if (_NoisemapEnabled)
    {
        const float2 address = position + _NoisemapSeed / 100.0f;
        switch (_NoisemapType)
        {
        case 0: // Perlin
            result = SimplexNormal(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence);
            break;
        case 1: // Billow
            result = SimplexBillowed(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence);
            break;
        case 2: // Ridged
            result = SimplexRidged(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence, _NoisemapRidgedOffset);
            break;
        case 3: // IQ
            result = SimplexDerivedIQ(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence);
            break;
        case 4: // Swiss
            result = SimplexDerivedSwiss(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence, _NoisemapWarp, _NoisemapRidgedOffset);
            break;
        case 5: // Jordan
            result = SimplexDerivedJordan(address, _NoisemapOctaves, _NoisemapOffset, _NoisemapFrequency, _NoisemapAmplitude, _NoisemapLacunarity, _NoisemapPersistence, _NoisemapWarp0, _NoisemapWarp, _NoisemapDamp0, _NoisemapDamp, _NoisemapDampScale);
            break;
        default:
            result = 0.0f;
            break;
        }
    }
    return result;
}

void GetNoise_float(float2 position, out float o_output)
{
    if (_NoisemapEnabled)
    {
        o_output = (GetNoise(position) * 2.0f - 1.0f) * _NoisemapStrength;
    }
    else
    {
        o_output = 0.0f;
    }
}

#endif
