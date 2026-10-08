#define NUM_TEX_COORD_INTERPOLATORS 1
#define NUM_MATERIAL_TEXCOORDS_VERTEX 1
#define NUM_CUSTOM_VERTEX_INTERPOLATORS 0

struct Input
{
	//float3 Normal;
	float2 uv_MainTex : TEXCOORD0;
	float2 uv2_Material_Texture2D_0 : TEXCOORD1;
	float4 color : COLOR;
	float4 tangent;
	//float4 normal;
	float3 viewDir;
	float4 screenPos;
	float3 worldPos;
	//float3 worldNormal;
	float3 normal2;
};
struct SurfaceOutputStandard
{
	float3 Albedo;		// base (diffuse or specular) color
	float3 Normal;		// tangent space normal, if written
	half3 Emission;
	half Metallic;		// 0=non-metal, 1=metal
	// Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
	// Everywhere in the code you meet smoothness it is perceptual smoothness
	half Smoothness;	// 0=rough, 1=smooth
	half Occlusion;		// occlusion (default 1)
	float Alpha;		// alpha for transparencies
};

//#define HDRP 1
#define URP 1
#define UE5
//#define HAS_CUSTOMIZED_UVS 1
#define MATERIAL_TANGENTSPACENORMAL 1
//struct Material
//{
	//samplers start
SAMPLER( SamplerState_Linear_Repeat );
SAMPLER( SamplerState_Linear_Clamp );
TEXTURE2D(       Material_Texture2D_0 );
SAMPLER(  samplerMaterial_Texture2D_0 );
float4 Material_Texture2D_0_TexelSize;
float4 Material_Texture2D_0_ST;
TEXTURE2D(       Material_Texture2D_1 );
SAMPLER(  samplerMaterial_Texture2D_1 );
float4 Material_Texture2D_1_TexelSize;
float4 Material_Texture2D_1_ST;
TEXTURE2D(       Material_Texture2D_2 );
SAMPLER(  samplerMaterial_Texture2D_2 );
float4 Material_Texture2D_2_TexelSize;
float4 Material_Texture2D_2_ST;
TEXTURE2D(       Material_Texture2D_3 );
SAMPLER(  samplerMaterial_Texture2D_3 );
float4 Material_Texture2D_3_TexelSize;
float4 Material_Texture2D_3_ST;
TEXTURE2D(       Material_Texture2D_4 );
SAMPLER(  samplerMaterial_Texture2D_4 );
float4 Material_Texture2D_4_TexelSize;
float4 Material_Texture2D_4_ST;
TEXTURE2D(       Material_Texture2D_5 );
SAMPLER(  samplerMaterial_Texture2D_5 );
float4 Material_Texture2D_5_TexelSize;
float4 Material_Texture2D_5_ST;
TEXTURE2D(       Material_Texture2D_6 );
SAMPLER(  samplerMaterial_Texture2D_6 );
float4 Material_Texture2D_6_TexelSize;
float4 Material_Texture2D_6_ST;
TEXTURE2D(       Material_Texture2D_7 );
SAMPLER(  samplerMaterial_Texture2D_7 );
float4 Material_Texture2D_7_TexelSize;
float4 Material_Texture2D_7_ST;
TEXTURE2D(       Material_Texture2D_8 );
SAMPLER(  samplerMaterial_Texture2D_8 );
float4 Material_Texture2D_8_TexelSize;
float4 Material_Texture2D_8_ST;
TEXTURE2D(       Material_Texture2D_9 );
SAMPLER(  samplerMaterial_Texture2D_9 );
float4 Material_Texture2D_9_TexelSize;
float4 Material_Texture2D_9_ST;
TEXTURE2D(       Material_Texture2D_10 );
SAMPLER(  samplerMaterial_Texture2D_10 );
float4 Material_Texture2D_10_TexelSize;
float4 Material_Texture2D_10_ST;
TEXTURE2D(       Material_Texture2D_11 );
SAMPLER(  samplerMaterial_Texture2D_11 );
float4 Material_Texture2D_11_TexelSize;
float4 Material_Texture2D_11_ST;
TEXTURE2D(       Material_Texture2D_12 );
SAMPLER(  samplerMaterial_Texture2D_12 );
float4 Material_Texture2D_12_TexelSize;
float4 Material_Texture2D_12_ST;
TEXTURE2D(       Material_Texture2D_13 );
SAMPLER(  samplerMaterial_Texture2D_13 );
float4 Material_Texture2D_13_TexelSize;
float4 Material_Texture2D_13_ST;
TEXTURE2D(       Material_Texture2D_14 );
SAMPLER(  samplerMaterial_Texture2D_14 );
float4 Material_Texture2D_14_TexelSize;
float4 Material_Texture2D_14_ST;

//};

#ifdef UE5
	#define UE_LWC_RENDER_TILE_SIZE			2097152.0
	#define UE_LWC_RENDER_TILE_SIZE_SQRT	1448.15466
	#define UE_LWC_RENDER_TILE_SIZE_RSQRT	0.000690533954
	#define UE_LWC_RENDER_TILE_SIZE_RCP		4.76837158e-07
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_PI		0.673652053
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_2PI	0.673652053
	#define INVARIANT(X) X
	#define PI 					(3.1415926535897932)

	#include "LargeWorldCoordinates.hlsl"
#endif
struct MaterialStruct
{
	float4 PreshaderBuffer[37];
	float4 ScalarExpressions[1];
	float VTPackedPageTableUniform[2];
	float VTPackedUniform[1];
};
static SamplerState View_MaterialTextureBilinearWrapedSampler;
static SamplerState View_MaterialTextureBilinearClampedSampler;
struct ViewStruct
{
	float GameTime;
	float RealTime;
	float DeltaTime;
	float PrevFrameGameTime;
	float PrevFrameRealTime;
	float MaterialTextureMipBias;	
	float4 PrimitiveSceneData[ 40 ];
	float4 TemporalAAParams;
	float2 ViewRectMin;
	float4 ViewSizeAndInvSize;
	float MaterialTextureDerivativeMultiply;
	uint StateFrameIndexMod8;
	float FrameNumber;
	float2 FieldOfViewWideAngles;
	float4 RuntimeVirtualTextureMipLevel;
	float PreExposure;
	float4 BufferBilinearUVMinMax;
};
struct ResolvedViewStruct
{
	#ifdef UE5
		FLWCVector3 WorldCameraOrigin;
		FLWCVector3 PrevWorldCameraOrigin;
		FLWCVector3 PreViewTranslation;
		FLWCVector3 WorldViewOrigin;
	#else
		float3 WorldCameraOrigin;
		float3 PrevWorldCameraOrigin;
		float3 PreViewTranslation;
		float3 WorldViewOrigin;
	#endif
	float4 ScreenPositionScaleBias;
	float4x4 TranslatedWorldToView;
	float4x4 TranslatedWorldToCameraView;
	float4x4 TranslatedWorldToClip;
	float4x4 ViewToTranslatedWorld;
	float4x4 PrevViewToTranslatedWorld;
	float4x4 CameraViewToTranslatedWorld;
	float4 BufferBilinearUVMinMax;
	float4 XRPassthroughCameraUVs[ 2 ];
};
struct PrimitiveStruct
{
	float4x4 WorldToLocal;
	float4x4 LocalToWorld;
};

static ViewStruct View;
static ResolvedViewStruct ResolvedView;
static PrimitiveStruct Primitive;
uniform float4 View_BufferSizeAndInvSize;
uniform float4 LocalObjectBoundsMin;
uniform float4 LocalObjectBoundsMax;
static SamplerState Material_Wrap_WorldGroupSettings;
static SamplerState Material_Clamp_WorldGroupSettings;

#include "UnrealCommon.cginc"

static MaterialStruct Material;
void InitializeExpressions()
{
	Material.PreshaderBuffer[0] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[1] = float4(1.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[2] = float4(255.000000,255.000000,1.000000,1.000000);//(Unknown)
	Material.PreshaderBuffer[3] = float4(255.000000,255.000000,0.150000,0.100000);//(Unknown)
	Material.PreshaderBuffer[4] = float4(0.001000,2.000000,0.300000,0.250000);//(Unknown)
	Material.PreshaderBuffer[5] = float4(0.200000,0.030000,-2000.000000,20000.000000);//(Unknown)
	Material.PreshaderBuffer[6] = float4(0.000050,0.001000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[7] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[8] = float4(1.000000,1.000000,1.000000,1.000000);//(Unknown)
	Material.PreshaderBuffer[9] = float4(1.000000,1.000000,1.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[10] = float4(0.000000,0.000000,0.000000,0.500000);//(Unknown)
	Material.PreshaderBuffer[11] = float4(1.000000,1.000000,1.000000,1.000000);//(Unknown)
	Material.PreshaderBuffer[12] = float4(1.000000,1.000000,1.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[13] = float4(0.000000,0.000000,0.000000,0.500000);//(Unknown)
	Material.PreshaderBuffer[14] = float4(1.000000,1.000000,1.000000,1.000000);//(Unknown)
	Material.PreshaderBuffer[15] = float4(1.000000,1.000000,1.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[16] = float4(0.000000,0.000000,0.000000,0.500000);//(Unknown)
	Material.PreshaderBuffer[17] = float4(0.480195,0.546875,0.464274,1.000000);//(Unknown)
	Material.PreshaderBuffer[18] = float4(0.480195,0.546875,0.464274,0.000000);//(Unknown)
	Material.PreshaderBuffer[19] = float4(0.519805,0.453125,0.535726,0.500000);//(Unknown)
	Material.PreshaderBuffer[20] = float4(1.000000,1.000000,2.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[21] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[22] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[23] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[24] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[25] = float4(4008096.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[26] = float4(4007936.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[27] = float4(4006416.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[28] = float4(4004096.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[29] = float4(4012896.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[30] = float4(4002416.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[31] = float4(4004896.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[32] = float4(4008016.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[33] = float4(4012496.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[34] = float4(4009936.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[35] = float4(4006336.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.PreshaderBuffer[36] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
}
#define WPO_PRECISE(T) T

MaterialFloat3 CustomExpression3(FMaterialPixelParameters Parameters,MaterialFloat GroundWeight,MaterialFloat GrassWeight,MaterialFloat RoadWeight,MaterialFloat ForestWeight,MaterialFloat Ground,MaterialFloat Grass,MaterialFloat Road,MaterialFloat Forest)
{
float  lerpres;
float  Local0;




float  AllWeightsAndHeights = GroundWeight.r + GrassWeight.r + RoadWeight.r + ForestWeight.r + 0.0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = float3(Divider,Divider,Divider) * float3(GroundWeight,GroundWeight,GroundWeight) * Ground;
float3  Layer1Contribution = float3(Divider,Divider,Divider) * float3(GrassWeight,GrassWeight,GrassWeight) * Grass;
float3  Layer2Contribution = float3(Divider,Divider,Divider) * float3(RoadWeight,RoadWeight,RoadWeight) * Road;
float3  Layer3Contribution = float3(Divider,Divider,Divider) * float3(ForestWeight,ForestWeight,ForestWeight) * Forest;
float3  Result = Layer0Contribution + Layer1Contribution + Layer2Contribution + Layer3Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression2(FMaterialPixelParameters Parameters,MaterialFloat GroundWeight,MaterialFloat GrassWeight,MaterialFloat RoadWeight,MaterialFloat ForestWeight,MaterialFloat Ground,MaterialFloat Grass,MaterialFloat Road,MaterialFloat3 Forest)
{
float  lerpres;
float  Local0;




float  AllWeightsAndHeights = GroundWeight.r + GrassWeight.r + RoadWeight.r + ForestWeight.r + 0.0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = float3(Divider,Divider,Divider) * float3(GroundWeight,GroundWeight,GroundWeight) * Ground;
float3  Layer1Contribution = float3(Divider,Divider,Divider) * float3(GrassWeight,GrassWeight,GrassWeight) * Grass;
float3  Layer2Contribution = float3(Divider,Divider,Divider) * float3(RoadWeight,RoadWeight,RoadWeight) * Road;
float3  Layer3Contribution = float3(Divider,Divider,Divider) * float3(ForestWeight,ForestWeight,ForestWeight) * Forest;
float3  Result = Layer0Contribution + Layer1Contribution + Layer2Contribution + Layer3Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression1(FMaterialPixelParameters Parameters,MaterialFloat GroundWeight,MaterialFloat GrassWeight,MaterialFloat RoadWeight,MaterialFloat ForestWeight,MaterialFloat3 Ground,MaterialFloat3 Grass,MaterialFloat3 Road,MaterialFloat3 Forest)
{
float  lerpres;
float  Local0;




float  AllWeightsAndHeights = GroundWeight.r + GrassWeight.r + RoadWeight.r + ForestWeight.r + 0.0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = float3(Divider,Divider,Divider) * float3(GroundWeight,GroundWeight,GroundWeight) * Ground;
float3  Layer1Contribution = float3(Divider,Divider,Divider) * float3(GrassWeight,GrassWeight,GrassWeight) * Grass;
float3  Layer2Contribution = float3(Divider,Divider,Divider) * float3(RoadWeight,RoadWeight,RoadWeight) * Road;
float3  Layer3Contribution = float3(Divider,Divider,Divider) * float3(ForestWeight,ForestWeight,ForestWeight) * Forest;
float3  Result = Layer0Contribution + Layer1Contribution + Layer2Contribution + Layer3Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression0(FMaterialPixelParameters Parameters,MaterialFloat GroundWeight,MaterialFloat GrassWeight,MaterialFloat RoadWeight,MaterialFloat ForestWeight,MaterialFloat3 Ground,MaterialFloat3 Grass,MaterialFloat3 Road,MaterialFloat3 Forest)
{
float  lerpres;
float  Local0;




float  AllWeightsAndHeights = GroundWeight.r + GrassWeight.r + RoadWeight.r + ForestWeight.r + 0.0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = float3(Divider,Divider,Divider) * float3(GroundWeight,GroundWeight,GroundWeight) * Ground;
float3  Layer1Contribution = float3(Divider,Divider,Divider) * float3(GrassWeight,GrassWeight,GrassWeight) * Grass;
float3  Layer2Contribution = float3(Divider,Divider,Divider) * float3(RoadWeight,RoadWeight,RoadWeight) * Road;
float3  Layer3Contribution = float3(Divider,Divider,Divider) * float3(ForestWeight,ForestWeight,ForestWeight) * Forest;
float3  Result = Layer0Contribution + Layer1Contribution + Layer2Contribution + Layer3Contribution + float3(0,0,0);
return Result;
}
WPO_PRECISE(float3) GetMaterialWorldPositionOffset(FMaterialVertexParameters Parameters)
{
	#if USE_INSTANCING || USE_INSTANCE_CULLING
		// skip if this instance is hidden
		if (Parameters.PerInstanceParams.z < 1.f)
		{
			return float3(0,0,0);
		}
	#endif
	return MaterialFloat3(0.00000000,0.00000000,0.00000000);;
}
void CalcPixelMaterialInputs(in out FMaterialPixelParameters Parameters, in out FPixelMaterialInputs PixelMaterialInputs)
{
	//WorldAligned texturing & others use normals & stuff that think Z is up
	Parameters.TangentToWorld[0] = Parameters.TangentToWorld[0].xzy;
	Parameters.TangentToWorld[1] = Parameters.TangentToWorld[1].xzy;
	Parameters.TangentToWorld[2] = Parameters.TangentToWorld[2].xzy;

	float3 WorldNormalCopy = Parameters.WorldNormal;

	// Initial calculations (required for Normal)
	MaterialFloat2 Local0 = Parameters.TexCoords[0].xy;
	MaterialFloat2 Local1 = (DERIV_BASE_VALUE(Local0) * ((MaterialFloat2)Material.PreshaderBuffer[1].x));
	MaterialFloat Local2 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local1), 14);
	MaterialFloat4 Local3 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_0,GetMaterialSharedSampler(samplerMaterial_Texture2D_0,View_MaterialTextureBilinearClampedSampler),DERIV_BASE_VALUE(Local1)));
	MaterialFloat Local4 = MaterialStoreTexSample(Parameters, Local3, 14);
	MaterialFloat4 Local5 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_1,GetMaterialSharedSampler(samplerMaterial_Texture2D_1,View_MaterialTextureBilinearClampedSampler),DERIV_BASE_VALUE(Local1)));
	MaterialFloat Local6 = MaterialStoreTexSample(Parameters, Local5, 14);
	MaterialFloat4 Local7 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_2,GetMaterialSharedSampler(samplerMaterial_Texture2D_2,View_MaterialTextureBilinearClampedSampler),DERIV_BASE_VALUE(Local1)));
	MaterialFloat Local8 = MaterialStoreTexSample(Parameters, Local7, 14);
	MaterialFloat4 Local9 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_3,GetMaterialSharedSampler(samplerMaterial_Texture2D_3,View_MaterialTextureBilinearClampedSampler),DERIV_BASE_VALUE(Local1)));
	MaterialFloat Local10 = MaterialStoreTexSample(Parameters, Local9, 14);
	MaterialFloat2 Local11 = (DERIV_BASE_VALUE(Local0) + MaterialFloat4(0.00784314,0.00784314,0.00784314,0.00784314).rg);
	MaterialFloat2 Local12 = (DERIV_BASE_VALUE(Local11) * Material.PreshaderBuffer[3].xy);
	MaterialFloat2 Local13 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[3].z));
	MaterialFloat Local14 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local13), 3);
	MaterialFloat4 Local15 = UnpackNormalMap(Texture2DSample(Material_Texture2D_4,GetMaterialSharedSampler(samplerMaterial_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local13)));
	MaterialFloat Local16 = MaterialStoreTexSample(Parameters, Local15, 3);
	MaterialFloat2 Local17 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[3].w));
	MaterialFloat Local18 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local17), 5);
	MaterialFloat4 Local19 = UnpackNormalMap(Texture2DSampleBias(Material_Texture2D_5,samplerMaterial_Texture2D_5,DERIV_BASE_VALUE(Local17),View.MaterialTextureMipBias));
	MaterialFloat Local20 = MaterialStoreTexSample(Parameters, Local19, 5);
	MaterialFloat2 Local21 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[4].x));
	MaterialFloat Local22 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local21), 13);
	MaterialFloat4 Local23 = Texture2DSampleBias(Material_Texture2D_6,samplerMaterial_Texture2D_6,DERIV_BASE_VALUE(Local21),View.MaterialTextureMipBias);
	MaterialFloat Local24 = MaterialStoreTexSample(Parameters, Local23, 13);
	MaterialFloat3 Local25 = (Local23.rgb * ((MaterialFloat3)Material.PreshaderBuffer[4].y));
	MaterialFloat3 Local26 = PositiveClampedPow(Local25,((MaterialFloat3)Material.PreshaderBuffer[4].z));
	MaterialFloat3 Local27 = saturate(Local26);
	MaterialFloat3 Local28 = lerp(Local15.rgb,Local19.rgb,Local27);
	MaterialFloat2 Local29 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[4].w));
	MaterialFloat Local30 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local29), 5);
	MaterialFloat4 Local31 = UnpackNormalMap(Texture2DSample(Material_Texture2D_5,GetMaterialSharedSampler(samplerMaterial_Texture2D_5,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local29)));
	MaterialFloat Local32 = MaterialStoreTexSample(Parameters, Local31, 5);
	MaterialFloat2 Local33 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[5].x));
	MaterialFloat Local34 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 11);
	MaterialFloat4 Local35 = UnpackNormalMap(Texture2DSampleBias(Material_Texture2D_7,samplerMaterial_Texture2D_7,DERIV_BASE_VALUE(Local33),View.MaterialTextureMipBias));
	MaterialFloat Local36 = MaterialStoreTexSample(Parameters, Local35, 11);
	MaterialFloat2 Local37 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[5].y));
	MaterialFloat Local38 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local37), 11);
	MaterialFloat4 Local39 = UnpackNormalMap(Texture2DSampleBias(Material_Texture2D_7,samplerMaterial_Texture2D_7,DERIV_BASE_VALUE(Local37),View.MaterialTextureMipBias));
	MaterialFloat Local40 = MaterialStoreTexSample(Parameters, Local39, 11);
	MaterialFloat Local41 = GetPixelDepth(Parameters);
	MaterialFloat Local42 = DERIV_BASE_VALUE(Local41).r;
	MaterialFloat Local43 = (DERIV_BASE_VALUE(Local42) + Material.PreshaderBuffer[5].z);
	MaterialFloat Local44 = (DERIV_BASE_VALUE(Local43) * Material.PreshaderBuffer[6].x);
	MaterialFloat Local45 = saturate(DERIV_BASE_VALUE(Local44));
	MaterialFloat3 Local46 = lerp(Local35.rgb,Local39.rgb,DERIV_BASE_VALUE(Local45));
	MaterialFloat2 Local47 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)Material.PreshaderBuffer[6].y));
	MaterialFloat Local48 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local47), 13);
	MaterialFloat4 Local49 = Texture2DSampleBias(Material_Texture2D_6,samplerMaterial_Texture2D_6,DERIV_BASE_VALUE(Local47),View.MaterialTextureMipBias);
	MaterialFloat Local50 = MaterialStoreTexSample(Parameters, Local49, 13);
	MaterialFloat3 Local51 = (Local49.rgb * ((MaterialFloat3)Material.PreshaderBuffer[4].y));
	MaterialFloat3 Local52 = PositiveClampedPow(Local51,((MaterialFloat3)Material.PreshaderBuffer[4].z));
	MaterialFloat3 Local53 = saturate(Local52);
	MaterialFloat3 Local54 = lerp(Local19.rgb,Local46,Local53);
	MaterialFloat3 Local55 = CustomExpression0(Parameters,Local3.r,Local5.r,Local7.r,Local9.r,Local15.rgb,Local28,Local31.rgb,Local54);

	// The Normal is a special case as it might have its own expressions and also be used to calculate other inputs, so perform the assignment here
	PixelMaterialInputs.Normal = Local55.rgb;


#if TEMPLATE_USES_STRATA
	Parameters.SharedLocalBases = StrataInitialiseSharedLocalBases();
#endif

	// Note that here MaterialNormal can be in world space or tangent space
	float3 MaterialNormal = GetMaterialNormal(Parameters, PixelMaterialInputs);

#if MATERIAL_TANGENTSPACENORMAL
#if SIMPLE_FORWARD_SHADING
	Parameters.WorldNormal = float3(0, 0, 1);
#endif

#if FEATURE_LEVEL >= FEATURE_LEVEL_SM4
	// Mobile will rely on only the final normalize for performance
	MaterialNormal = normalize(MaterialNormal);
#endif

	// normalizing after the tangent space to world space conversion improves quality with sheared bases (UV layout to WS causes shrearing)
	// use full precision normalize to avoid overflows
	Parameters.WorldNormal = TransformTangentNormalToWorld(Parameters.TangentToWorld, MaterialNormal);

#else //MATERIAL_TANGENTSPACENORMAL

	Parameters.WorldNormal = normalize(MaterialNormal);

#endif //MATERIAL_TANGENTSPACENORMAL

#if MATERIAL_TANGENTSPACENORMAL
	// flip the normal for backfaces being rendered with a two-sided material
	Parameters.WorldNormal *= Parameters.TwoSidedSign;
#endif

	Parameters.ReflectionVector = ReflectionAboutCustomWorldNormal(Parameters, Parameters.WorldNormal, false);

#if !PARTICLE_SPRITE_FACTORY
	Parameters.Particle.MotionBlurFade = 1.0f;
#endif // !PARTICLE_SPRITE_FACTORY

	// Now the rest of the inputs
	MaterialFloat3 Local56 = lerp(MaterialFloat3(0.00000000,0.00000000,0.00000000),Material.PreshaderBuffer[7].xyz,Material.PreshaderBuffer[6].z);
	MaterialFloat2 Local57 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)0.21340001));
	MaterialFloat Local58 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local57), 12);
	MaterialFloat4 Local59 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_8,samplerMaterial_Texture2D_8,DERIV_BASE_VALUE(Local57),View.MaterialTextureMipBias));
	MaterialFloat Local60 = MaterialStoreTexSample(Parameters, Local59, 12);
	MaterialFloat Local61 = (Local59.r + 0.50000000);
	MaterialFloat2 Local62 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)0.05341000));
	MaterialFloat Local63 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local62), 12);
	MaterialFloat4 Local64 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_8,samplerMaterial_Texture2D_8,DERIV_BASE_VALUE(Local62),View.MaterialTextureMipBias));
	MaterialFloat Local65 = MaterialStoreTexSample(Parameters, Local64, 12);
	MaterialFloat Local66 = (Local64.r + 0.50000000);
	MaterialFloat2 Local67 = (DERIV_BASE_VALUE(Local12) * ((MaterialFloat2)0.00200000));
	MaterialFloat Local68 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local67), 12);
	MaterialFloat4 Local69 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_8,samplerMaterial_Texture2D_8,DERIV_BASE_VALUE(Local67),View.MaterialTextureMipBias));
	MaterialFloat Local70 = MaterialStoreTexSample(Parameters, Local69, 12);
	MaterialFloat Local71 = (Local69.r + 0.50000000);
	MaterialFloat Local72 = (Local66 * Local71);
	MaterialFloat Local73 = (Local61 * Local72);
	MaterialFloat3 Local74 = lerp(MaterialFloat3(0.50000000,0.50000000,0.50000000),((MaterialFloat3)1.00000000),Local73);
	MaterialFloat Local75 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local13), 2);
	MaterialFloat4 Local76 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_9,GetMaterialSharedSampler(samplerMaterial_Texture2D_9,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local13)));
	MaterialFloat Local77 = MaterialStoreTexSample(Parameters, Local76, 2);
	MaterialFloat3 Local78 = (((MaterialFloat3)1.00000000) - Local76.rgb);
	MaterialFloat3 Local79 = (Local78 * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local80 = (Local79 * Material.PreshaderBuffer[10].xyz);
	MaterialFloat3 Local81 = (((MaterialFloat3)1.00000000) - Local80);
	MaterialFloat3 Local82 = (Local76.rgb * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local83 = (Local82 * Material.PreshaderBuffer[9].xyz);
	MaterialFloat Local84 = ((Local76.rgb.r >= 0.50000000) ? Local81.r : Local83.r);
	MaterialFloat Local85 = ((Local76.rgb.g >= 0.50000000) ? Local81.g : Local83.g);
	MaterialFloat Local86 = ((Local76.rgb.b >= 0.50000000) ? Local81.b : Local83.b);
	MaterialFloat3 Local87 = (MaterialFloat3(MaterialFloat2(Local84,Local85),Local86) * ((MaterialFloat3)Material.PreshaderBuffer[10].w));
	MaterialFloat Local88 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local17), 1);
	MaterialFloat4 Local89 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_10,samplerMaterial_Texture2D_10,DERIV_BASE_VALUE(Local17),View.MaterialTextureMipBias));
	MaterialFloat Local90 = MaterialStoreTexSample(Parameters, Local89, 1);
	MaterialFloat3 Local91 = (((MaterialFloat3)1.00000000) - Local89.rgb);
	MaterialFloat3 Local92 = (Local91 * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local93 = (Local92 * Material.PreshaderBuffer[13].xyz);
	MaterialFloat3 Local94 = (((MaterialFloat3)1.00000000) - Local93);
	MaterialFloat3 Local95 = (Local89.rgb * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local96 = (Local95 * Material.PreshaderBuffer[12].xyz);
	MaterialFloat Local97 = ((Local89.rgb.r >= 0.50000000) ? Local94.r : Local96.r);
	MaterialFloat Local98 = ((Local89.rgb.g >= 0.50000000) ? Local94.g : Local96.g);
	MaterialFloat Local99 = ((Local89.rgb.b >= 0.50000000) ? Local94.b : Local96.b);
	MaterialFloat3 Local100 = (MaterialFloat3(MaterialFloat2(Local97,Local98),Local99) * ((MaterialFloat3)Material.PreshaderBuffer[13].w));
	MaterialFloat3 Local101 = lerp(Local87,Local100,Local27);
	MaterialFloat Local102 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local29), 1);
	MaterialFloat4 Local103 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_10,GetMaterialSharedSampler(samplerMaterial_Texture2D_10,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local29)));
	MaterialFloat Local104 = MaterialStoreTexSample(Parameters, Local103, 1);
	MaterialFloat3 Local105 = (((MaterialFloat3)1.00000000) - Local103.rgb);
	MaterialFloat3 Local106 = (Local105 * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local107 = (Local106 * Material.PreshaderBuffer[16].xyz);
	MaterialFloat3 Local108 = (((MaterialFloat3)1.00000000) - Local107);
	MaterialFloat3 Local109 = (Local103.rgb * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local110 = (Local109 * Material.PreshaderBuffer[15].xyz);
	MaterialFloat Local111 = ((Local103.rgb.r >= 0.50000000) ? Local108.r : Local110.r);
	MaterialFloat Local112 = ((Local103.rgb.g >= 0.50000000) ? Local108.g : Local110.g);
	MaterialFloat Local113 = ((Local103.rgb.b >= 0.50000000) ? Local108.b : Local110.b);
	MaterialFloat3 Local114 = (MaterialFloat3(MaterialFloat2(Local111,Local112),Local113) * ((MaterialFloat3)Material.PreshaderBuffer[16].w));
	MaterialFloat Local115 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 9);
	MaterialFloat4 Local116 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_11,samplerMaterial_Texture2D_11,DERIV_BASE_VALUE(Local33),View.MaterialTextureMipBias));
	MaterialFloat Local117 = MaterialStoreTexSample(Parameters, Local116, 9);
	MaterialFloat Local118 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local37), 9);
	MaterialFloat4 Local119 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_11,samplerMaterial_Texture2D_11,DERIV_BASE_VALUE(Local37),View.MaterialTextureMipBias));
	MaterialFloat Local120 = MaterialStoreTexSample(Parameters, Local119, 9);
	MaterialFloat3 Local121 = lerp(Local116.rgb,Local119.rgb,DERIV_BASE_VALUE(Local45));
	MaterialFloat3 Local122 = (((MaterialFloat3)1.00000000) - Local121);
	MaterialFloat3 Local123 = (Local122 * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local124 = (Local123 * Material.PreshaderBuffer[19].xyz);
	MaterialFloat3 Local125 = (((MaterialFloat3)1.00000000) - Local124);
	MaterialFloat3 Local126 = (Local121 * ((MaterialFloat3)2.00000000));
	MaterialFloat3 Local127 = (Local126 * Material.PreshaderBuffer[18].xyz);
	MaterialFloat Local128 = ((Local121.r >= 0.50000000) ? Local125.r : Local127.r);
	MaterialFloat Local129 = ((Local121.g >= 0.50000000) ? Local125.g : Local127.g);
	MaterialFloat Local130 = ((Local121.b >= 0.50000000) ? Local125.b : Local127.b);
	MaterialFloat3 Local131 = (MaterialFloat3(MaterialFloat2(Local128,Local129),Local130) * ((MaterialFloat3)Material.PreshaderBuffer[19].w));
	MaterialFloat3 Local132 = lerp(Local100,Local131,Local53);
	MaterialFloat3 Local133 = CustomExpression1(Parameters,Local3.r,Local5.r,Local7.r,Local9.r,Local87,Local101,Local114,Local132);
	MaterialFloat3 Local134 = (Local74 * Local133.rgb);
	MaterialFloat Local135 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local13), 8);
	MaterialFloat4 Local136 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_12,GetMaterialSharedSampler(samplerMaterial_Texture2D_12,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local13)));
	MaterialFloat Local137 = MaterialStoreTexSample(Parameters, Local136, 8);
	MaterialFloat Local138 = (Local136.g * Material.PreshaderBuffer[20].x);
	MaterialFloat Local139 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local17), 4);
	MaterialFloat4 Local140 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_13,samplerMaterial_Texture2D_13,DERIV_BASE_VALUE(Local17),View.MaterialTextureMipBias));
	MaterialFloat Local141 = MaterialStoreTexSample(Parameters, Local140, 4);
	MaterialFloat Local142 = (Local140.g * 1.00000000);
	MaterialFloat Local143 = lerp(Local138,Local142,Local27.x);
	MaterialFloat Local144 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local29), 4);
	MaterialFloat4 Local145 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_13,GetMaterialSharedSampler(samplerMaterial_Texture2D_13,View_MaterialTextureBilinearWrapedSampler),DERIV_BASE_VALUE(Local29)));
	MaterialFloat Local146 = MaterialStoreTexSample(Parameters, Local145, 4);
	MaterialFloat Local147 = (Local145.g * Material.PreshaderBuffer[20].y);
	MaterialFloat Local148 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local33), 10);
	MaterialFloat4 Local149 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_14,samplerMaterial_Texture2D_14,DERIV_BASE_VALUE(Local33),View.MaterialTextureMipBias));
	MaterialFloat Local150 = MaterialStoreTexSample(Parameters, Local149, 10);
	MaterialFloat Local151 = MaterialStoreTexCoordScale(Parameters, DERIV_BASE_VALUE(Local37), 10);
	MaterialFloat4 Local152 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_14,samplerMaterial_Texture2D_14,DERIV_BASE_VALUE(Local37),View.MaterialTextureMipBias));
	MaterialFloat Local153 = MaterialStoreTexSample(Parameters, Local152, 10);
	MaterialFloat3 Local154 = lerp(Local149.rgb,Local152.rgb,DERIV_BASE_VALUE(Local45));
	MaterialFloat3 Local155 = (Local154 * ((MaterialFloat3)Material.PreshaderBuffer[20].z));
	MaterialFloat3 Local156 = lerp(((MaterialFloat3)Local142),Local155,Local53);
	MaterialFloat3 Local157 = CustomExpression2(Parameters,Local3.r,Local5.r,Local7.r,Local9.r,Local138,Local143,Local147,Local156);
	MaterialFloat Local158 = lerp(Local136.r,Local140.r,Local27.x);
	MaterialFloat Local159 = lerp(Local140.r,1.00000000,Local53.x);
	MaterialFloat3 Local160 = CustomExpression3(Parameters,Local3.r,Local5.r,Local7.r,Local9.r,Local136.r,Local158,Local145.r,Local159);

	PixelMaterialInputs.EmissiveColor = Local56;
	PixelMaterialInputs.Opacity = 1.00000000;
	PixelMaterialInputs.OpacityMask = 1.00000000;
	PixelMaterialInputs.BaseColor = Local134;
	PixelMaterialInputs.Metallic = 0.00000000;
	PixelMaterialInputs.Specular = 0.50000000;
	PixelMaterialInputs.Roughness = Local157.r;
	PixelMaterialInputs.Anisotropy = 0.00000000;
	PixelMaterialInputs.Normal = Local55.rgb;
	PixelMaterialInputs.Tangent = MaterialFloat3(1.00000000,0.00000000,0.00000000);
	PixelMaterialInputs.Subsurface = 0;
	PixelMaterialInputs.AmbientOcclusion = Local160.r;
	PixelMaterialInputs.Refraction = 0;
	PixelMaterialInputs.PixelDepthOffset = 0.00000000;
	PixelMaterialInputs.ShadingModel = 1;
	PixelMaterialInputs.FrontMaterial = GetInitialisedStrataData();


#if MATERIAL_USES_ANISOTROPY
	Parameters.WorldTangent = CalculateAnisotropyTangent(Parameters, PixelMaterialInputs);
#else
	Parameters.WorldTangent = 0;
#endif
}

#define UnityObjectToWorldDir TransformObjectToWorld

void SetupCommonData( int Parameters_PrimitiveId )
{
	View_MaterialTextureBilinearWrapedSampler = SamplerState_Linear_Repeat;
	View_MaterialTextureBilinearClampedSampler = SamplerState_Linear_Clamp;

	Material_Wrap_WorldGroupSettings = SamplerState_Linear_Repeat;
	Material_Clamp_WorldGroupSettings = SamplerState_Linear_Clamp;

	View.GameTime = View.RealTime = _Time.y;// _Time is (t/20, t, t*2, t*3)
	View.PrevFrameGameTime = View.GameTime - unity_DeltaTime.x;//(dt, 1/dt, smoothDt, 1/smoothDt)
	View.PrevFrameRealTime = View.RealTime;
	View.DeltaTime = unity_DeltaTime.x;
	View.MaterialTextureMipBias = 0.0;
	View.TemporalAAParams = float4( 0, 0, 0, 0 );
	View.ViewRectMin = float2( 0, 0 );
	View.ViewSizeAndInvSize = View_BufferSizeAndInvSize;
	View.MaterialTextureDerivativeMultiply = 1.0f;
	View.StateFrameIndexMod8 = 0;
	View.FrameNumber = (int)_Time.y;
	View.FieldOfViewWideAngles = float2( PI * 0.42f, PI * 0.42f );//75degrees, default unity
	View.RuntimeVirtualTextureMipLevel = float4( 0, 0, 0, 0 );
	View.PreExposure = 0;
	View.BufferBilinearUVMinMax = float4(
		View_BufferSizeAndInvSize.z * ( 0 + 0.5 ),//EffectiveViewRect.Min.X
		View_BufferSizeAndInvSize.w * ( 0 + 0.5 ),//EffectiveViewRect.Min.Y
		View_BufferSizeAndInvSize.z * ( View_BufferSizeAndInvSize.x - 0.5 ),//EffectiveViewRect.Max.X
		View_BufferSizeAndInvSize.w * ( View_BufferSizeAndInvSize.y - 0.5 ) );//EffectiveViewRect.Max.Y

	for( int i2 = 0; i2 < 40; i2++ )
		View.PrimitiveSceneData[ i2 ] = float4( 0, 0, 0, 0 );

	float4x4 LocalToWorld = transpose( UNITY_MATRIX_M );
    LocalToWorld[3] = float4(ToUnrealPos(LocalToWorld[3]), LocalToWorld[3].w);
	float4x4 WorldToLocal = transpose( UNITY_MATRIX_I_M );
	float4x4 ViewMatrix = transpose( UNITY_MATRIX_V );
	float4x4 InverseViewMatrix = transpose( UNITY_MATRIX_I_V );
	float4x4 ViewProjectionMatrix = transpose( UNITY_MATRIX_VP );
	uint PrimitiveBaseOffset = Parameters_PrimitiveId * PRIMITIVE_SCENE_DATA_STRIDE;
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 0 ] = LocalToWorld[ 0 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 1 ] = LocalToWorld[ 1 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 2 ] = LocalToWorld[ 2 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 3 ] = LocalToWorld[ 3 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 100.0 );//ObjectWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 6 ] = WorldToLocal[ 0 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 7 ] = WorldToLocal[ 1 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 8 ] = WorldToLocal[ 2 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 9 ] = WorldToLocal[ 3 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 10 ] = LocalToWorld[ 0 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 11 ] = LocalToWorld[ 1 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 12 ] = LocalToWorld[ 2 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 13 ] = LocalToWorld[ 3 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 0 );//ActorWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ] = LocalObjectBoundsMax - LocalObjectBoundsMin;//ObjectBounds
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 21 ] = mul( LocalToWorld, float3( 1, 0, 0 ) );
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 23 ] = LocalObjectBoundsMin;//LocalObjectBoundsMin 
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 24 ] = LocalObjectBoundsMax;//LocalObjectBoundsMax

#ifdef UE5
	ResolvedView.WorldCameraOrigin = LWCPromote( ToUnrealPos( _WorldSpaceCameraPos.xyz ) );
	ResolvedView.PreViewTranslation = LWCPromote( float3( 0, 0, 0 ) );
	ResolvedView.WorldViewOrigin = LWCPromote( float3( 0, 0, 0 ) );
#else
	ResolvedView.WorldCameraOrigin = ToUnrealPos( _WorldSpaceCameraPos.xyz );
	ResolvedView.PreViewTranslation = float3( 0, 0, 0 );
	ResolvedView.WorldViewOrigin = float3( 0, 0, 0 );
#endif
	ResolvedView.PrevWorldCameraOrigin = ResolvedView.WorldCameraOrigin;
	ResolvedView.ScreenPositionScaleBias = float4( 1, 1, 0, 0 );
	ResolvedView.TranslatedWorldToView		 = ViewMatrix;
	ResolvedView.TranslatedWorldToCameraView = ViewMatrix;
	ResolvedView.TranslatedWorldToClip		 = ViewProjectionMatrix;
	ResolvedView.ViewToTranslatedWorld		 = InverseViewMatrix;
	ResolvedView.PrevViewToTranslatedWorld = ResolvedView.ViewToTranslatedWorld;
	ResolvedView.CameraViewToTranslatedWorld = InverseViewMatrix;
	ResolvedView.BufferBilinearUVMinMax = View.BufferBilinearUVMinMax;
	Primitive.WorldToLocal = WorldToLocal;
	Primitive.LocalToWorld = LocalToWorld;
}
#define VS_USES_UNREAL_SPACE 1
float3 PrepareAndGetWPO( float4 VertexColor, float3 UnrealWorldPos, float3 UnrealNormal, float4 InTangent,
						 float4 UV0, float4 UV1 )
{
	InitializeExpressions();
	FMaterialVertexParameters Parameters = (FMaterialVertexParameters)0;

	float3 InWorldNormal = UnrealNormal;
	float4 tangentWorld = InTangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	
	#ifdef VS_USES_UNREAL_SPACE
		UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	#endif
	Parameters.WorldPosition = UnrealWorldPos;
	#ifdef VS_USES_UNREAL_SPACE
		Parameters.TangentToWorld[ 0 ] = Parameters.TangentToWorld[ 0 ].xzy;
		Parameters.TangentToWorld[ 1 ] = Parameters.TangentToWorld[ 1 ].xzy;
		Parameters.TangentToWorld[ 2 ] = Parameters.TangentToWorld[ 2 ].xzy;//WorldAligned texturing uses normals that think Z is up
	#endif

	Parameters.VertexColor = VertexColor;

#if NUM_MATERIAL_TEXCOORDS_VERTEX > 0			
	Parameters.TexCoords[ 0 ] = float2( UV0.x, UV0.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 1
	Parameters.TexCoords[ 1 ] = float2( UV1.x, UV1.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( UV0.x, UV0.y );
	}
#endif

	Parameters.PrimitiveId = 0;

	SetupCommonData( Parameters.PrimitiveId );

#ifdef UE5
	Parameters.PrevFrameLocalToWorld = MakeLWCMatrix( float3( 0, 0, 0 ), Primitive.LocalToWorld );
#else
	Parameters.PrevFrameLocalToWorld = Primitive.LocalToWorld;
#endif
	
	float3 Offset = float3( 0, 0, 0 );
	Offset = GetMaterialWorldPositionOffset( Parameters );
	#ifdef VS_USES_UNREAL_SPACE
		//Convert from unreal units to unity
		Offset /= float3( 100, 100, 100 );
		Offset = Offset.xzy;
	#endif
	return Offset;
}

void SurfaceReplacement( Input In, out SurfaceOutputStandard o )
{
	InitializeExpressions();

	float3 Z3 = float3( 0, 0, 0 );
	float4 Z4 = float4( 0, 0, 0, 0 );

	float3 UnrealWorldPos = float3( In.worldPos.x, In.worldPos.y, In.worldPos.z );

	float3 UnrealNormal = In.normal2;	

	FMaterialPixelParameters Parameters = (FMaterialPixelParameters)0;
#if NUM_TEX_COORD_INTERPOLATORS > 0			
	Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 1
	Parameters.TexCoords[ 1 ] = float2( In.uv2_Material_Texture2D_0.x, 1.0 - In.uv2_Material_Texture2D_0.y );
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
	}
#endif
	Parameters.VertexColor = In.color;
	Parameters.WorldNormal = UnrealNormal;
	Parameters.ReflectionVector = half3( 0, 0, 1 );
	Parameters.CameraVector = normalize( _WorldSpaceCameraPos.xyz - UnrealWorldPos.xyz );
	//Parameters.CameraVector = mul( ( float3x3 )unity_CameraToWorld, float3( 0, 0, 1 ) ) * -1;
	Parameters.LightVector = half3( 0, 0, 0 );
	//float4 screenpos = In.screenPos;
	//screenpos /= screenpos.w;
	Parameters.SvPosition = In.screenPos;
	Parameters.ScreenPosition = Parameters.SvPosition;

	Parameters.UnMirrored = 1;

	Parameters.TwoSidedSign = 1;


	float3 InWorldNormal = UnrealNormal;	
	float4 tangentWorld = In.tangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	//WorldAlignedTexturing in UE relies on the fact that coords there are 100x larger, prepare values for that
	//but watch out for any computation that might get skewed as a side effect
	UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	
	Parameters.AbsoluteWorldPosition = UnrealWorldPos;
	Parameters.WorldPosition_CamRelative = UnrealWorldPos;
	Parameters.WorldPosition_NoOffsets = UnrealWorldPos;

	Parameters.WorldPosition_NoOffsets_CamRelative = Parameters.WorldPosition_CamRelative;
	Parameters.LightingPositionOffset = float3( 0, 0, 0 );

	Parameters.AOMaterialMask = 0;

	Parameters.Particle.RelativeTime = 0;
	Parameters.Particle.MotionBlurFade;
	Parameters.Particle.Random = 0;
	Parameters.Particle.Velocity = half4( 1, 1, 1, 1 );
	Parameters.Particle.Color = half4( 1, 1, 1, 1 );
	Parameters.Particle.TranslatedWorldPositionAndSize = float4( UnrealWorldPos, 0 );
	Parameters.Particle.MacroUV = half4( 0, 0, 1, 1 );
	Parameters.Particle.DynamicParameter = half4( 0, 0, 0, 0 );
	Parameters.Particle.LocalToWorld = float4x4( Z4, Z4, Z4, Z4 );
	Parameters.Particle.Size = float2( 1, 1 );
	Parameters.Particle.SubUVCoords[ 0 ] = Parameters.Particle.SubUVCoords[ 1 ] = float2( 0, 0 );
	Parameters.Particle.SubUVLerp = 0.0;
	Parameters.TexCoordScalesParams = float2( 0, 0 );
	Parameters.PrimitiveId = 0;
	Parameters.VirtualTextureFeedback = 0;

	FPixelMaterialInputs PixelMaterialInputs = (FPixelMaterialInputs)0;
	PixelMaterialInputs.Normal = float3( 0, 0, 1 );
	PixelMaterialInputs.ShadingModel = 0;
	PixelMaterialInputs.FrontMaterial = 0;

	SetupCommonData( Parameters.PrimitiveId );
	//CustomizedUVs
	#if NUM_TEX_COORD_INTERPOLATORS > 0 && HAS_CUSTOMIZED_UVS
		float2 OutTexCoords[ NUM_TEX_COORD_INTERPOLATORS ];
		//Prevent uninitialized reads
		for( int i = 0; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
		{
			OutTexCoords[ i ] = float2( 0, 0 );
		}
		GetMaterialCustomizedUVs( Parameters, OutTexCoords );
		for( int i = 0; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
		{
			Parameters.TexCoords[ i ] = OutTexCoords[ i ];
		}
	#endif
	//<-
	CalcPixelMaterialInputs( Parameters, PixelMaterialInputs );

	#define HAS_WORLDSPACE_NORMAL 0
	#if HAS_WORLDSPACE_NORMAL
		PixelMaterialInputs.Normal = mul( PixelMaterialInputs.Normal, (MaterialFloat3x3)( transpose( Parameters.TangentToWorld ) ) );
	#endif

	o.Albedo = PixelMaterialInputs.BaseColor.rgb;
	o.Alpha = PixelMaterialInputs.Opacity;
	//if( PixelMaterialInputs.OpacityMask < 0.333 ) discard;

	o.Metallic = PixelMaterialInputs.Metallic;
	o.Smoothness = 1.0 - PixelMaterialInputs.Roughness;
	o.Normal = normalize( PixelMaterialInputs.Normal );
	o.Emission = PixelMaterialInputs.EmissiveColor.rgb;
	o.Occlusion = PixelMaterialInputs.AmbientOcclusion;

	//BLEND_ADDITIVE o.Alpha = ( o.Emission.r + o.Emission.g + o.Emission.b ) / 3;
}