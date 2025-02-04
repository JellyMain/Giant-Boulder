#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float2 DirectionalFlowUV(
	float2 uv, float3 flowVectorAndSpeed, float tiling, float time,
	out float2x2 rotation
) {
	float2 dir = normalize(flowVectorAndSpeed.xy); //make direction a unit vector
	rotation = float2x2(dir.y, dir.x, -dir.x, dir.y); //create a copy of the reverse rotation that we are about to perform. This is needed to account for the derivates after they have been rotated
	uv = mul(float2x2(dir.y, -dir.x, dir.x, dir.y), uv); //rotate the uvs based on the direction's rotation. 
	return uv;// *tiling; //tile to the uv's after the rotation
}


//Remap position into (0 to 1) range
void Remap(float3 value, float3 from1, float3 to1, float3 from2, float3 to2, out float3 o_output)
{
	o_output = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}

//Correct derivate map data
float3 UnpackDerivativeHeight(float4 textureData) {
	//derivate data, X is stored in Alpha, Y is stored in Green, height is stored in Blue
	float3 dh = textureData.agb;// swizzle the components to get X, Y and then Height in each RGB channel
	dh.xy = dh.xy * 2 - 1; //decode the derivate data
	return dh;
}

//Create random value from uv
float random(float2 uv)
{
	//return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
	float value = frac(sin(dot(uv, float2(12, 78))) * 43);
	return round(value * 100) / 100;
}

//sample flow vectors at each tile size, with relative offset
float3 FlowCell(float2 uv, float2 offset, float time, bool gridB, float2 worldUV)
{
	float2 shift = 1 - offset; //sample neighbour cells based off offset data.
	//we dont want a full shift by 1 tile unit, we want it shifted halfway, so correct this by pushing it back by half
	shift *= 0.5;
	offset *= 0.5;

	//correct this shift we are using a second grid
	if (gridB) {
		offset += 0.25;
		shift -= 0.25;
	}

	float2x2 derivRotation; //rotation matrix used to store the inverse rotation of the directional flow for the derivates
	float2 uvTiled = (floor(uv * _Extent.x + offset) + shift) / _Extent.x; //create the tile squares

	//Random directions
#ifdef _PW_RIVERHDRP
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);
#else
	float4 randomData = tex2D(_NoiseMap, uvTiled);
#endif

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV = mul(randomMat, worldUV);

#ifdef _PW_RIVERHDRP
	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#else
	float3 flow = tex2D(_FlowMap, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#endif

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	//float tiling = flow.z * _TilingModulated + _Tiling; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed
	float tiling = 1;

	worldUV.y *= lerp(1, 0.5, pow(flow.z, _SlopeStrength));
	time *= lerp(1, 2, pow(slopeMask, _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);



	uvFlow.y -= time;

	//correct derivate height data
#ifdef _PW_RIVERHDRP
	float3 dh = UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_MainTex, SamplerState_Linear_Repeat, uvFlow));
#else
	float3 dh = UnpackDerivativeHeight(tex2D(_MainTex, uvFlow));
#endif

	dh.xy = mul(derivRotation, dh.xy); //undo the rotation that the DirectionalFlowUV function did to the derivate map. 
	dh *= flow.z * _HeightScaleModulated + _HeightScale; //scale the height based on the speed, the normals are stronger the faster it is.
	return dh;
}

//Sample tile flows with neighbouring cells
void FlowGrid_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, out float3 o_output) {
	//sample current, above, top right, right flow cells
	float3 dhA = FlowCell(i_uv, float2(0, 0), i_time, i_gridB, i_worldUV);
	float3 dhB = FlowCell(i_uv, float2(1, 0), i_time, i_gridB, i_worldUV);
	float3 dhC = FlowCell(i_uv, float2(0, 1), i_time, i_gridB, i_worldUV);
	float3 dhD = FlowCell(i_uv, float2(1, 1), i_time, i_gridB, i_worldUV);


	//setup the gradients used to blend each flowcell offset with one another
	float2 t = i_uv * _Extent.x;
	//shift this gradient across if we are using gridB
	if (i_gridB) {
		t += 0.25;
	}

	//make the gradient a triangle wave
	t = abs(2 * frac(t) - 1);

	//setup the gradients for the flowcells
	float wA = (1 - t.x) * (1 - t.y);
	float wB = t.x * (1 - t.y);
	float wC = (1 - t.x) * t.y;
	float wD = t.x * t.y;

	//multiply each flowcell by their respective gradient and combine them together. 
	o_output = dhA *wA +dhB * wB + dhC * wC + dhD * wD;
}

//sample flow vectors at each tile size, with relative offset
float3 FlowCellFoam(float2 uv, float2 offset, float time, bool gridB, float2 worldUV)
{
	float2 shift = 1 - offset; //sample neighbour cells based off offset data.
	//we dont want a full shift by 1 tile unit, we want it shifted halfway, so correct this by pushing it back by half
	shift *= 0.5;
	offset *= 0.5;

	//correct this shift we are using a second grid
	if (gridB) {
		offset += 0.25;
		shift -= 0.25;
	}

	float2x2 derivRotation; //rotation matrix used to store the inverse rotation of the directional flow for the derivates
	float2 uvTiled = (floor(uv * (_Extent.x * _FoamGridResolution) + offset) + shift) / (_Extent.x * _FoamGridResolution); //create the tile squares

#ifdef _PW_RIVERHDRP
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);
#else
	float4 randomData = tex2D(_NoiseMap, uvTiled);
#endif

	//Random directions
	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV = mul(randomMat, worldUV);


#ifdef _PW_RIVERHDRP
	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#else
	float3 flow = tex2D(_FlowMap, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#endif

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	//float tiling = flow.z * _TilingModulated + _Tiling; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed
	float tiling = 1;

	worldUV.y *= lerp(1, 0.5, pow(flow.z, _SlopeStrength));
	time *= lerp(1, 2, pow(slopeMask, _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);

	uvFlow.y -= time;

	//correct derivate height data
#ifdef _PW_RIVERHDRP
	float3 dh = SAMPLE_TEXTURE2D(_FoamMap, SamplerState_Linear_Repeat, uvFlow);
#else
	float3 dh = tex2D(_FoamMap, uvFlow);
#endif


	return dh;

}

//Sample tile flows with neighbouring cells
void FlowGridFoam_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, out float3 o_output) {
	//sample current, above, top right, right flow cells
	float3 dhA = FlowCellFoam(i_uv, float2(0, 0), i_time, i_gridB, i_worldUV);
	float3 dhB = FlowCellFoam(i_uv, float2(1, 0), i_time, i_gridB, i_worldUV);
	float3 dhC = FlowCellFoam(i_uv, float2(0, 1), i_time, i_gridB, i_worldUV);
	float3 dhD = FlowCellFoam(i_uv, float2(1, 1), i_time, i_gridB, i_worldUV);

	//setup the gradients used to blend each flowcell offset with one another
	float2 t = i_uv * (_Extent.x * _FoamGridResolution);
	//shift this gradient across if we are using gridB
	if (i_gridB) {
		t += 0.25;
	}

	//make the gradient a triangle wave
	t = abs(2 * frac(t) - 1);

	//setup the gradients for the flowcells
	float wA = (1 - t.x) * (1 - t.y);
	float wB = t.x * (1 - t.y);
	float wC = (1 - t.x) * t.y;
	float wD = t.x * t.y;

	//multiply each flowcell by their respective gradient and combine them together. 
	o_output = dhA * wA + dhB * wB + dhC * wC + dhD * wD;
}

//sample flow vectors at each tile size, with relative offset
#ifdef _PW_RIVERHDRP
void FlowCellSurface(float2 uv, float2 offset, float time, bool gridB, float2 worldUV, float indexMask, UnityTexture2D i_albedo, UnityTexture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#else
void FlowCellSurface(float2 uv, float2 offset, float time, bool gridB, float2 worldUV, float indexMask, sampler2D i_albedo, sampler2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#endif
{
	float2 shift = 1 - offset; //sample neighbour cells based off offset data.
	//we dont want a full shift by 1 tile unit, we want it shifted halfway, so correct this by pushing it back by half
	shift *= 0.5;
	offset *= 0.5;

	//correct this shift we are using a second grid
	if (gridB) {
		offset += 0.25;
		shift -= 0.25;
	}

	float2x2 derivRotation; //rotation matrix used to store the inverse rotation of the directional flow for the derivates
	float2 uvTiled = (floor(uv * _Extent.x + offset) + shift) / _Extent.x; //create the tile squares

	//Random directions
#ifdef _PW_RIVERHDRP
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);
#else
	float4 randomData = tex2D(_NoiseMap, uvTiled);
#endif

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV += float2(randomData.xy * 2 - 1);
	worldUV = mul(randomMat, worldUV);

#ifdef _PW_RIVERHDRP
	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#else
	float3 flow = tex2D(_FlowMap, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#endif

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	float tiling = 1; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed

	worldUV.y *= lerp(1, 0.5, pow(flow.z, _SlopeStrength));
	time *= lerp(1, 2, pow(slopeMask, _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);
	uvFlow.y -= time;

	float4 normalMask;
	float4 albedoChannels;

	//correct derivate height data
#ifdef _PW_RIVERHDRP
	normalMask = SAMPLE_TEXTURE2D(i_normalMask, SamplerState_Linear_Repeat, uvFlow);
	albedoChannels = SAMPLE_TEXTURE2D(i_albedo, SamplerState_Linear_Repeat, uvFlow);
#else
	normalMask = tex2D(i_normalMask, uvFlow);
	albedoChannels = tex2D(i_albedo, uvFlow);


#endif
	normalMask.xy = normalMask.xy * 2 - 1;
	normalMask.xy = mul(derivRotation, normalMask.xy);

	o_albedo = albedoChannels;
	o_normalMask = normalMask;

}



//Sample tile flows with neighbouring cells
#ifdef _PW_RIVERHDRP
void FlowGridSurface_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, float indexMask, UnityTexture2D i_albedo, UnityTexture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#else
void FlowGridSurface_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, float indexMask, sampler2D i_albedo, sampler2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#endif

{
	float4 dhA_albedo, dhA_normalMask;
	float4 dhB_albedo, dhB_normalMask;
	float4 dhC_albedo, dhC_normalMask;
	float4 dhD_albedo, dhD_normalMask;

	//sample current, above, top right, right flow cells
	FlowCellSurface(i_uv, float2(0, 0), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhA_albedo, dhA_normalMask);
	FlowCellSurface(i_uv, float2(1, 0), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhB_albedo, dhB_normalMask);
	FlowCellSurface(i_uv, float2(0, 1), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhC_albedo, dhC_normalMask);
	FlowCellSurface(i_uv, float2(1, 1), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhD_albedo, dhD_normalMask);

	//setup the gradients used to blend each flowcell offset with one another
	float2 t = i_uv * _Extent.x;
	//shift this gradient across if we are using gridB
	if (i_gridB) {
		t += 0.25;
	}

	//make the gradient a triangle wave
	t = abs(2 * frac(t) - 1);

	//setup the gradients for the flowcells
	float wA = (1 - t.x) * (1 - t.y);
	float wB = t.x * (1 - t.y);
	float wC = (1 - t.x) * t.y;
	float wD = t.x * t.y;

	//multiply each flowcell by their respective gradient and combine them together. 
	o_albedo = dhA_albedo * wA + dhB_albedo * wB + dhC_albedo * wC + dhD_albedo * wD;
	o_normalMask = dhA_normalMask * wA + dhB_normalMask * wB + dhC_normalMask * wC + dhD_normalMask * wD;
}

//sample flow vectors at each tile size, with relative offset
#ifdef _PW_RIVERHDRP
void FlowCellSurfaceTexArray(float2 uv, float2 offset, float time, bool gridB, float2 worldUV, float indexMask, UnityTexture2D i_albedo, UnityTexture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#else
void FlowCellSurfaceTexArray(float2 uv, float2 offset, float time, bool gridB, float2 worldUV, float indexMask, sampler2D i_albedo, sampler2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#endif
{
	float2 shift = 1 - offset; //sample neighbour cells based off offset data.
	//we dont want a full shift by 1 tile unit, we want it shifted halfway, so correct this by pushing it back by half
	shift *= 0.5;
	offset *= 0.5;

	//correct this shift we are using a second grid
	if (gridB) {
		offset += 0.25;
		shift -= 0.25;
	}

	float2x2 derivRotation; //rotation matrix used to store the inverse rotation of the directional flow for the derivates
	float2 uvTiled = (floor(uv * _Extent.x + offset) + shift) / _Extent.x; //create the tile squares

	//Random directions
#ifdef _PW_RIVERHDRP
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);
#else
	float4 randomData = tex2D(_NoiseMap, uvTiled);
#endif

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV += float2(randomData.xy * 2 - 1);
	worldUV = mul(randomMat, worldUV);

#ifdef _PW_RIVERHDRP
	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#else
	float3 flow = tex2D(_FlowMap, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#endif

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	float tiling = 1; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed

	worldUV.y *= lerp(1, 0.5, pow(flow.z, _SlopeStrength));
	time *= lerp(1, 2, pow(slopeMask, _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);
	uvFlow.y -= time;

	float4 normalMask;
	float4 albedoChannels;

	//correct derivate height data
#ifdef _PW_RIVERHDRP

	normalMask = SAMPLE_TEXTURE2D_ARRAY(i_normalMask, SamplerState_Linear_Repeat, uvFlow, 0);
	albedoChannels = SAMPLE_TEXTURE2D_ARRAY(i_albedo, SamplerState_Linear_Repeat, uvFlow, 0);
#else

	normalMask = UNITY_SAMPLE_TEX2DARRAY(_TestTexNormal, float3(uvFlow, 0));
	albedoChannels = UNITY_SAMPLE_TEX2DARRAY(_TestTexAlbedo, float3(uvFlow, 0));

#endif
	normalMask.xy = mul(derivRotation, normalMask.xy);

	o_albedo = albedoChannels;
	o_normalMask = normalMask;

}



//Sample tile flows with neighbouring cells
#ifdef _PW_RIVERHDRP
void FlowGridSurfaceTexArray_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, float indexMask, UnityTexture2D i_albedo, UnityTexture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#else
void FlowGridSurfaceTexArray_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, float indexMask, sampler2D i_albedo, sampler2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
#endif

{
	float4 dhA_albedo, dhA_normalMask;
	float4 dhB_albedo, dhB_normalMask;
	float4 dhC_albedo, dhC_normalMask;
	float4 dhD_albedo, dhD_normalMask;

	//sample current, above, top right, right flow cells
	FlowCellSurfaceTexArray(i_uv, float2(0, 0), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhA_albedo, dhA_normalMask);
	FlowCellSurfaceTexArray(i_uv, float2(1, 0), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhB_albedo, dhB_normalMask);
	FlowCellSurfaceTexArray(i_uv, float2(0, 1), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhC_albedo, dhC_normalMask);
	FlowCellSurfaceTexArray(i_uv, float2(1, 1), i_time, i_gridB, i_worldUV, indexMask, i_albedo, i_normalMask, dhD_albedo, dhD_normalMask);

	//setup the gradients used to blend each flowcell offset with one another
	float2 t = i_uv * _Extent.x;
	//shift this gradient across if we are using gridB
	if (i_gridB) {
		t += 0.25;
	}

	//make the gradient a triangle wave
	t = abs(2 * frac(t) - 1);

	//setup the gradients for the flowcells
	float wA = (1 - t.x) * (1 - t.y);
	float wB = t.x * (1 - t.y);
	float wC = (1 - t.x) * t.y;
	float wD = t.x * t.y;

	//multiply each flowcell by their respective gradient and combine them together. 
	o_albedo = dhA_albedo * wA + dhB_albedo * wB + dhC_albedo * wC + dhD_albedo * wD;
	o_normalMask = dhA_normalMask * wA + dhB_normalMask * wB + dhC_normalMask * wC + dhD_normalMask * wD;
}


//Normal Intensity Whiteout Method
float3 NormalIntensity(float3 normal, float intensity)
{
	return float3(normal.x * intensity, normal.y * intensity, normal.z);
}

//Correct Hardware Y Axis
float2 AlignWithGrabTexel(float2 uv) {
#if UNITY_UV_STARTS_AT_TOP
	if (_CameraDepthTexture_TexelSize.y < 0) {
		uv.y = 1 - uv.y;
	}
#endif

	return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
}

//Blur
float3 blur(float2 uv, sampler2D tex, float distance, float steps)
{
	float3 CurColor = float3(0, 0, 0);
	float2 NewUV = uv;
	int incre = 0;
	float StepSize = distance / (int)steps;
	float CurDistance = 0;
	float2 CurOffset = 0;
	float SubOffset = 0;
	float TwoPi = 6.283185;
	float accumdist = 0;
	float RadialSteps = 8;
	float RadialOffset = 0.618;
	float KernelPower = 1;

	if (steps < 1)
	{
#ifdef _PW_RIVERHDRP
		//REPLACE NEEDED
		return float3(0, 0, 0);
#else
		return float3(tex2D(tex, uv).xyz);
#endif

	}
	else
	{
		while (incre < (int)steps)
		{
			CurDistance += StepSize;
			for (int j = 0; j < (int)RadialSteps; j++)
			{
				SubOffset += 1;
				CurOffset.x = cos(TwoPi * (SubOffset / RadialSteps));
				CurOffset.y = sin(TwoPi * (SubOffset / RadialSteps));
				NewUV.x = uv.x + CurOffset.x * CurDistance;
				NewUV.y = uv.y + CurOffset.y * CurDistance;
				float distpow = pow(CurDistance, KernelPower);

#ifdef _PW_RIVERHDRP
				//REPLACE NEEDED
				CurColor += float3(0, 0, 0);
#else
				CurColor += tex2D(tex, NewUV) * distpow;
#endif

				accumdist += distpow;
			}
			SubOffset += RadialOffset;
			incre++;
		}
		CurColor = CurColor;
		CurColor /= accumdist;

#ifdef _PW_RIVERHDRP
		//REPLACE NEEDED
		return float3(0, 0, 0);
#else
		return float4(CurColor.xyz, tex2D(tex, NewUV).a);
#endif

	}
}

void getFoamMask_float(float3 foamData, float3 normal, out float3 foam)
{
	foam = saturate(pow(foamData, _FoamContrast));
	foam *= _FoamBrightness;
	float3 slope = saturate(normal.y * _FoamSlopeStrength);
	foam *= slope;
	foam = float3(foam.x, foam.x, foam.x);
}

void getDepthAtSurface_float(float4 screenPos, out float depth, float heightOffset = 0)
{
#ifdef _PW_RIVERHDRP
	#ifdef _PW_RIVERURP
		float depthDifference = 0;
	#else
		//REPLACE NEEDED
		//float depthDifference = SampleCameraDepth(screenPos.xy);
		float depthDifference = 0;
		//depthDifference = Linear01Depth(depthDifference, _ZBufferParams);
	#endif

#else
	float2 screenUV = AlignWithGrabTexel((screenPos.xy) / screenPos.w);
	float backgroundDepth = 1; //LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
	//float backgroundDepth = 1;
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z) + heightOffset;
	float depthDifference = backgroundDepth - surfaceDepth;
#endif


	depth = depthDifference;
}

void getSeaLevelBlend_float(float2 uv, float3 worldPosition, out float seaLevelBlend, out float edgeFalloff)
{
	float seaLevelBlendIn = (worldPosition.y - _PWOceanHeight) * _SeaLevelBlend;
	float uvWidth = (saturate(uv.xx) - 0.5f) * 2.0f;
	edgeFalloff = 1.0f - dot(uvWidth, uvWidth);
	seaLevelBlend = saturate(lerp(edgeFalloff * seaLevelBlendIn, saturate(1 + edgeFalloff) * seaLevelBlendIn, seaLevelBlendIn * seaLevelBlendIn));
}

void getLightDirectionMask_float(float4 screenPos, out float mask)
{
	float2 screenUV = AlignWithGrabTexel((screenPos.xy) / screenPos.w);

#ifdef _PW_RIVERHDRP
	#ifdef _PW_RIVERURP
		float3 belowSurfaceNormalsWS = float3(0, 0, 0);
	#else
		//REPLACE NEEDED
		struct NormalData
		{
			float3 normalWS;
			float  perceptualRoughness;
		};
		NormalData normalData;
		DecodeFromNormalBuffer(screenPos.xy, normalData);

		float3 belowSurfaceNormalsWS = normalData.normalWS;
	#endif

#else
	float3 belowSurfaceNormalsWS = tex2D(_CameraGBufferTexture2, screenUV);
#endif

	mask = abs(dot(belowSurfaceNormalsWS, _WorldSpaceLightPos0.xyz));
}

void getRefractionAngle_float(float3 worldPosition, float causticsSurfaceDepth, out float refractionDepth)
{
	float3 myViewDir = normalize(_WorldSpaceCameraPos - worldPosition);
	myViewDir.y = myViewDir.y * causticsSurfaceDepth;
	float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
	refractionDepth = myViewDir.y / max(0.0001, lightDir.y);
}

void calculateRefractionVector_float(float refractionDepth, float3 worldNormal, out float3 refractVector)
{
#ifdef _PW_RIVERHDRP
	refractVector = (refract(normalize(_WorldSpaceLightPos0), NormalIntensity(worldNormal, (_CausticsNormalIntensity)) + float3(0, 0, _CausticsHeightIntensity), 1.01));
#else
	refractVector = (refract(normalize(_WorldSpaceLightPos0), NormalIntensity(worldNormal, _CausticsNormalIntensity) + float3(0, 0, _CausticsHeightIntensity), 1.01));
#endif
	
	refractVector = (refractionDepth / _CausticsDepth) * refractVector;
}

void getCaustics_float(float2 causticsWorldUV, float3 refractVector, float causticsLightMask, float refractionDepth, out float3 caustics)
{

	float focus = (abs(refractionDepth - _FocalLength)) * _Aperture;
	
#ifdef _PW_RIVERHDRP
	//REPLACE NEEDED
	//caustics = SAMPLE_TEXTURE2D(_CausticsMap, SamplerState_Linear_Repeat, causticsWorldUV + refractVector.xz).x* _CausticsBrightness;
	caustics = SAMPLE_TEXTURE2D_LOD(_CausticsMap, SamplerState_Linear_Repeat, causticsWorldUV + refractVector.xz, focus).rgb * (_CausticsBrightness * 2);
	caustics = caustics * causticsLightMask;
#else


	caustics = tex2Dbias(_CausticsMap, float4(causticsWorldUV + refractVector.xz, 0, focus)).rgb * _CausticsBrightness;
	//caustics = tex2D(_CausticsMap, causticsWorldUV + refractVector.xz).a * _CausticsBrightness;
	caustics = caustics * causticsLightMask;
	caustics = caustics * lerp(_LightColor0, float3(0, 0, 0), saturate(refractionDepth / 3));
	caustics = caustics * saturate(pow(refractionDepth, 1.5));
	caustics *= saturate(refractionDepth);
#endif


}

void colorBelowWater_float(float2 screenUV, float depth, float3 caustics, out float3 colorBelowWater)
{
	//create fog density
	float fogFactor = exp2(-_WaterFogDensity * depth);

	//sample and blur background color based on depth
#ifdef _PW_RIVERHDRP
	#ifdef _PW_RIVERURP
		float3 backgroundColor = float3(0, 0, 0);
	#else
		float3 backgroundColor = SampleCameraColor(screenUV).xyz;
	#endif

#else
	float3 backgroundColor = blur(screenUV, _CameraOpaqueTexture, lerp(0.0003, _FogBlurAmount, (1 - depth) * _FogBlurDepth), 16);
#endif

	//make the background wet
	backgroundColor *= _Wetness;

	//add the caustics to the background color
	backgroundColor += caustics;

	//transition between clear and depth water
	colorBelowWater = lerp(_WaterFogColor, backgroundColor, fogFactor);
}

//Create uvs ready for flipbook function
void prepareFlipbookUVs_float(float2 uv, float tiling, float subImages, float texelSize, float edgePadding, out float2 o_outUV)
{
	float2 newUV = frac(uv * tiling);
	float texel = (texelSize / subImages);
	newUV = newUV * (texel / (texel + edgePadding));
	newUV = newUV + (((1 / texel) * edgePadding) * 0.5);
	o_outUV = newUV;
}

//Flipbook with padding function
#ifdef _PW_RIVERHDRP
void flipbookPadding_float(UnityTexture2D tex, float2 uv, float2 subImages, float frame, float2 seamlessUVs, float mipBias, out float3 o_output)
#else
void flipbookPadding_float(sampler2D tex, float2 uv, float2 subImages, float frame, float2 seamlessUVs, float mipBias, out float3 o_output)
#endif
{
	float fracFrame = frac(frame);
	float baseFrame = frame - fracFrame;
	float2 baseUVTile = float2(1, 1) / subImages;

	float uvA_mod = baseFrame % subImages.x;
	float uvA_floor = floor(baseFrame * baseUVTile.x);
	float2 uvA = float2(uvA_mod, -uvA_floor);
	uvA += uv;
	uvA *= baseUVTile;

	float uvB_mod = (1 + baseFrame) % subImages.x;
	float uvB_floor = floor((1 + baseFrame) * baseUVTile.x);
	float2 uvB = float2(uvB_mod, -uvB_floor);
	uvB += uv;
	uvB *= baseUVTile;

	float2 derivX = ddx(seamlessUVs) * mipBias;
	float2 derivY = ddy(seamlessUVs) * mipBias;


#ifdef _PW_RIVERHDRP
	float3 texA = SAMPLE_TEXTURE2D_GRAD(tex, SamplerState_Linear_Repeat, uvA, derivX, derivY);
	float3 texB = SAMPLE_TEXTURE2D_GRAD(tex, SamplerState_Linear_Repeat, uvB, derivX, derivY);
#else
	float3 texA = tex2Dgrad(tex, uvA, derivX, derivY);
	float3 texB = tex2Dgrad(tex, uvB, derivX, derivY);
#endif
	float3 final = lerp(texA, texB, fracFrame);

	o_output = final;
}

void createSurfaceStruct_float(in float3 i_albedo, in float i_alpha, in float3 i_normal, in float i_height, in float i_metallic, in float i_AO, in float i_emission, in float i_smoothness,
	out float4 o_albedo, out float4 o_normal, out float4 o_mask)
{
	o_albedo = float4(i_albedo, i_alpha);
	o_normal = float4(i_normal, i_height);
	o_mask = float4(i_metallic, i_AO, i_emission, i_smoothness);
}

void splitSurfaceStruct_float(in float4 i_albedo, in float4 i_normal, in float4 i_mask, out float3 o_albedo, out float o_alpha, out float3 o_normal, out float o_height, out float o_metallic,
	out float o_AO, out float o_emission, out float o_smoothness)
{
	o_albedo = i_albedo.rgb;
	o_alpha = i_albedo.a;
	o_normal = i_normal.rgb;
	o_height = i_normal.a;
	o_metallic = i_mask.r;
	o_AO = i_mask.g;
	o_emission = i_mask.b;
	o_smoothness = i_mask.a;
}

void blendSurfaceStructAlpha_float(
	in float4 a_albedo, in float4 a_normal, in float4 a_mask,
	in float4 b_albedo, in float4 b_normal, in float4 b_mask,
	in float alpha,
	out float4 o_albedo, out float4 o_normal, out float4 o_mask)
{

	o_albedo = lerp(a_albedo, b_albedo, alpha);
	o_normal = lerp(a_normal, b_normal, alpha);
	o_mask = lerp(a_mask, b_mask, alpha);
}


//sample flow vectors at each tile size, with relative offset
#ifdef _PW_RIVERHDRP
float3 FlowCellCaustics(float2 uv, float2 offset, float time, bool gridB, UnityTexture2D tex, float2 worldUV)
#else
float3 FlowCellCaustics(float2 uv, float2 offset, float time, bool gridB, sampler2D tex, float2 worldUV)
#endif
{
	float2 shift = 1 - offset; //sample neighbour cells based off offset data.
	//we dont want a full shift by 1 tile unit, we want it shifted halfway, so correct this by pushing it back by half
	shift *= 0.5;
	offset *= 0.5;

	//correct this shift we are using a second grid
	if (gridB) {
		offset += 0.25;
		shift -= 0.25;
	}

	float2x2 derivRotation; //rotation matrix used to store the inverse rotation of the directional flow for the derivates
	float2 uvTiled = (floor(uv * _Extent.x + offset) + shift) / _Extent.x; //create the tile squares

	//Random directions
#ifdef _PW_RIVERHDRP
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);
#else
	float4 randomData = tex2D(_NoiseMap, uvTiled);
#endif

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV = mul(randomMat, worldUV);

#ifdef _PW_RIVERHDRP
	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#else
	float3 flow = tex2D(_FlowMap, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors
#endif

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	//float tiling = flow.z * _TilingModulated + _Tiling; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed
	float tiling = 1;

	worldUV.y *= lerp(1, 0.5, pow(flow.z, _SlopeStrength));
	time *= lerp(1, 2, pow(slopeMask, _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);

	uvFlow.y -= time;


	float2 causticsFlipUV = float2(0, 0);
	prepareFlipbookUVs_float(uvFlow, 1, 8, 4096, 4, causticsFlipUV);
	float3 flipCaustics = float3(0, 0, 0);
	flipbookPadding_float(tex, causticsFlipUV, float2(8, 8), _Time.y * 64, worldUV, 0.1, flipCaustics);

	return flipCaustics;
}

//Sample tile flows with neighbouring cells
#ifdef _PW_RIVERHDRP
void FlowGridCaustics_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, UnityTexture2D tex, out float3 o_output)
#else
void FlowGridCaustics_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, sampler2D tex, out float3 o_output)
#endif
{
	//sample current, above, top right, right flow cells
	float3 dhA = FlowCellCaustics(i_uv, float2(0, 0), i_time, i_gridB, tex, i_worldUV);
	float3 dhB = FlowCellCaustics(i_uv, float2(1, 0), i_time, i_gridB, tex, i_worldUV);
	float3 dhC = FlowCellCaustics(i_uv, float2(0, 1), i_time, i_gridB, tex, i_worldUV);
	float3 dhD = FlowCellCaustics(i_uv, float2(1, 1), i_time, i_gridB, tex, i_worldUV);


	//setup the gradients used to blend each flowcell offset with one another
	float2 t = i_uv * _Extent.x;
	//shift this gradient across if we are using gridB
	if (i_gridB) {
		t += 0.25;
	}

	//make the gradient a triangle wave
	t = abs(2 * frac(t) - 1);

	//setup the gradients for the flowcells
	float wA = (1 - t.x) * (1 - t.y);
	float wB = t.x * (1 - t.y);
	float wC = (1 - t.x) * t.y;
	float wD = t.x * t.y;

	//multiply each flowcell by their respective gradient and combine them together. 
	o_output = dhA * wA + dhB * wB + dhC * wC + dhD * wD;
}



#endif