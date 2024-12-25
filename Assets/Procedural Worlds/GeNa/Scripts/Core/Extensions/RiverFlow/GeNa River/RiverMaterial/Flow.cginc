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
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV = mul(randomMat, worldUV);

	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	//float tiling = flow.z * _TilingModulated + _Tiling; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed
	float tiling = 1;

	worldUV.y *= lerp(1, 0.5, pow(abs(flow.z), _SlopeStrength));
	time *= lerp(1, 2, pow(abs(slopeMask), _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);



	uvFlow.y -= time;

	//correct derivate height data
	float3 dh = UnpackDerivativeHeight(SAMPLE_TEXTURE2D(_MainTex, SamplerState_Linear_Repeat, uvFlow));

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

void Test_float(Texture2D Tex, SamplerState SS, float2 UV, out float4 value)
{
	value = SAMPLE_TEXTURE2D(Tex, SS, UV).rgba;
	//value = tex2D(Tex, UV);
	value = float4(0, 0, 0, 0);
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
void flipbookPadding_float(Texture2D tex, float2 uv, float2 subImages, float frame, float2 seamlessUVs, float mipBias, out float3 o_output)
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

	float3 texA = SAMPLE_TEXTURE2D_GRAD(tex, SamplerState_Linear_Repeat, uvA, derivX, derivY).xyz;
	float3 texB = SAMPLE_TEXTURE2D_GRAD(tex, SamplerState_Linear_Repeat, uvB, derivX, derivY).xyz;
	float3 final = lerp(texA, texB, fracFrame);

	o_output = final;
}

//sample flow vectors at each tile size, with relative offset
float3 FlowCellCaustics(float2 uv, float2 offset, float time, bool gridB, Texture2D tex, float2 worldUV)
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
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV = mul(randomMat, worldUV);

	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	//float tiling = flow.z * _TilingModulated + _Tiling; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed
	float tiling = 1;

	worldUV.y *= lerp(1, 0.5, pow(abs(flow.z), _SlopeStrength));
	time *= lerp(1, 2, pow(abs(slopeMask), _SlopeStrength));

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
void FlowGridCaustics_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, Texture2D tex, out float3 o_output)
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


//sample flow vectors at each tile size, with relative offset
void FlowCellSurface(float2 uv, float2 offset, float time, bool gridB, float2 worldUV, float indexMask, Texture2D i_albedo, Texture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)
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
	float4 randomData = SAMPLE_TEXTURE2D(_NoiseMap, SamplerState_Linear_Repeat, uvTiled);

	float randomGrid = randomData.a;
	randomGrid = randomGrid * 2 - 1;
	randomGrid *= _RandomAmount;
	float2 randomRot = float2(randomGrid, 1);
	randomRot = normalize(randomRot);
	float2x2 randomMat = float2x2(randomRot.y, -randomRot.x, randomRot.x, randomRot.y);
	worldUV += float2(randomData.xy * 2 - 1);
	worldUV = mul(randomMat, worldUV);

	float3 flow = SAMPLE_TEXTURE2D(_FlowMap, SamplerState_Linear_Repeat, uvTiled).rgb; //sample flowvectors with offset - this is for the square vectors

	float slopeMask = flow.z;
	flow.xy = flow.xy * 2 - 1; //correct the range of the flowmap vector

	flow.z = randomData.r;

	flow.z *= _FlowStrength; //adjust the strength of the flow
	float tiling = 1; //since only a small section of the texture gets sampled with the rotation, we can increase this size by modulating the tiling based on speed

	worldUV.y *= lerp(1, 0.5, pow(abs(flow.z), _SlopeStrength));
	time *= lerp(1, 2, pow(abs(slopeMask), _SlopeStrength));

	//align the uvs with each flow tile direction, get a copy of the inverse rotation matrix to fix the derivates later
	float2 uvFlow = DirectionalFlowUV(
		worldUV + offset, flow, tiling, time,
		derivRotation
	);
	uvFlow.y -= time;

	float4 normalMask;
	float4 albedoChannels;

	//correct derivate height data
	normalMask = SAMPLE_TEXTURE2D(i_normalMask, SamplerState_Linear_Repeat, uvFlow);
	albedoChannels = SAMPLE_TEXTURE2D(i_albedo, SamplerState_Linear_Repeat, uvFlow);
	normalMask.xy = normalMask.xy * 2 - 1;
	normalMask.xy = mul(derivRotation, normalMask.xy);

	o_albedo = albedoChannels;
	o_normalMask = normalMask;

}



//Sample tile flows with neighbouring cells
void FlowGridSurface_float(float2 i_uv, float i_time, bool i_gridB, float2 i_worldUV, float indexMask, Texture2D i_albedo, Texture2D i_normalMask, out float4 o_albedo, out float4 o_normalMask)

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

#endif