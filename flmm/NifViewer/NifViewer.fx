//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float4 g_MaterialAmbientColor;      // Material's ambient color
float4 g_MaterialDiffuseColor;      // Material's diffuse color
float4 g_MaterialSpecularColor;     // Material's specular color
float4 g_MaterialEmissiveColor;     // Material's emmisive color
float g_MaterialGloss;				// Material's glossiness
float g_MaterialAlpha;				// Material's alpha

float3 g_LightHalfVec;
float3 g_LightDir;                  // Light's direction in world space
float4 g_LightDiffuse;              // Light's diffuse color
float4 g_LightAmbient;              // Light's ambient color

float3 eyePos;						// Position of the camera in world space
float3 eyeVec;						// Direction of the camera in world space

texture colorMap;					// Color texture for mesh
texture glowMap;					// Glow map
texture normalMap;					// Normal map

float4x4 iWorld;					// inverse of the world matrix
float4x4 worldMatrix;               // World matrix for object
float4x4 worldView;					// World * View matrix
float4x4 normalMatrix;				// Transpose of the inverse of the worldView matrix
float4x4 viewProjection;		    // View * Projection matrix
float4x4 worldViewProj;				// world * view * proj

bool use_specular;					// True to use the alpha channel of the normal map as a specular map
bool use_glow;						// True to use a glow map instead of the materials emissive property
bool use_dvcolor;					// True to use diffuse vertex color (also effects ambient/specular)
bool use_evcolor;					// True to use emissive vertex color
bool use_alpha;						// True if the subset has alpha

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler sColorMap  =  sampler_state { Texture = <colorMap>;  };
sampler sGlowMap   =  sampler_state { Texture = <glowMap>;   };
sampler sNormalMap =  sampler_state { Texture = <normalMap>; };

//--------------------------------------------------------------------------------------
// Textureless
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_TEXTURELESS
{
    float4 Position   : POSITION;   // vertex position
    float3 Diffuse    : COLOR0;     // vertex diffuse color
};

VS_OUTPUT_TEXTURELESS TexturelessVS( 
                  float4 vPos       : POSITION, 
                  float3 Normal     : NORMAL,
                  float3 vColor     : COLOR0
                )
{
    VS_OUTPUT_TEXTURELESS Output;
    
    Output.Position = mul(vPos, worldViewProj);
    
    // Transform the normal from object space to world space    
    Normal = normalize(mul(Normal, worldMatrix));
    
    // No specular lighting for anything without a normal map
    float3 lightColor = g_LightDiffuse * g_MaterialDiffuseColor * saturate(dot(Normal, g_LightDir));
    lightColor += g_MaterialAmbientColor * g_LightAmbient;
    if(use_dvcolor) lightColor *= vColor;
    if(use_evcolor) lightColor += vColor;
    else lightColor += g_MaterialEmissiveColor;
    
    Output.Diffuse = saturate(lightColor);
    
    return Output;
}

float4 TexturelessPS( VS_OUTPUT_TEXTURELESS In )  : COLOR0
{ 
	if(use_alpha) return float4(In.Diffuse, g_MaterialAlpha);
    else return float4(In.Diffuse, 1);
}

//--------------------------------------------------------------------------------------
// Flat
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_FLAT
{
    float4 Position   : POSITION;   // vertex position
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords
    float3 Diffuse    : COLOR0;     // vertex diffuse color
};

VS_OUTPUT_FLAT FlatVS( 
                  float4 vPos       : POSITION, 
                  float3 Normal     : NORMAL,
                  float3 vColor     : COLOR0,
                  float2 vTexCoord0 : TEXCOORD0
                )
{
    VS_OUTPUT_FLAT Output;

    float4x4 worldViewProj = mul(worldMatrix, viewProjection);
    Output.Position = mul(vPos, worldViewProj);
    Output.TextureUV = vTexCoord0;
    
    // Transform the normal from object space to world space    
    Normal = normalize(mul(Normal, worldMatrix));
    
    // No specular lighting for anything without a normal map
    float3 lightColor = g_LightDiffuse * g_MaterialDiffuseColor * saturate(dot(Normal, g_LightDir));
    lightColor += g_MaterialAmbientColor * g_LightAmbient;
    if(use_dvcolor) lightColor *= vColor;
    if(!use_glow) {
		if(use_evcolor) lightColor += vColor;
		else lightColor += g_MaterialEmissiveColor;
	}
	Output.Diffuse = lightColor;
    
    return Output;
}

float4 FlatPS( VS_OUTPUT_FLAT In )  : COLOR0
{ 
    float4 tex = tex2D(sColorMap, In.TextureUV);
    
    float3 result = In.Diffuse * tex.rgb;
    if(use_glow) result += tex2D(sGlowMap, In.TextureUV) * g_MaterialEmissiveColor;

	if(use_alpha) return float4(result, g_MaterialAlpha*tex.a);
	else return float4(result, 1);
}

//--------------------------------------------------------------------------------------
// Normal
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_NORMAL
{
    float4 Position   : POSITION;   // vertex position 
    float2 TexUV      : TEXCOORD0;  // vertex texture coords 
    float3 ViewDir    : TEXCOORD1;
    float3 LightDir   : TEXCOORD2;
    float3 HalfVector : TEXCOORD3;
    float3 aColor     : COLOR0;
    float3 dColor     : COLOR1;		// vertex colour
};

float3 tspace(float3 v, float3 a, float3 b, float3 c) {
	return float3( dot( v, c ), dot( v, b ), dot( v, a ) );
}

VS_OUTPUT_NORMAL NormalVS( 
                    float4 vPos       : POSITION, 
                    float3 Normal     : NORMAL,
                    float3 vColor     : COLOR0,
                    float2 vTexCoord0 : TEXCOORD0,
                    float3 Tangent    : TANGENT,
                    float3 Binormal   : BINORMAL
                  )
{
    VS_OUTPUT_NORMAL Output;
    
    // Basic info
    Output.Position = mul(vPos, worldViewProj);
    Output.TexUV = vTexCoord0;
    
    float3 vPosWorld = mul(vPos, worldMatrix);
    float3 EyeVec    = normalize(vPosWorld-eyePos);
    float3 HalfVec   = normalize(EyeVec + g_LightDir);
    
    //Normal = mul(normalMatrix, Normal);
    //Tangent = mul(normalMatrix, Tangent);
    //Binormal = mul(normalMatrix, Binormal);
    Normal = mul(Normal, normalMatrix);
    Tangent = mul(Tangent, normalMatrix);
    Binormal = mul(Binormal, normalMatrix);
	
	Output.ViewDir = tspace( vPosWorld, Normal, Tangent, Binormal );
	Output.LightDir = tspace( g_LightDir, Normal, Tangent, Binormal );
	Output.HalfVector = tspace( HalfVec, Normal, Tangent, Binormal );
	
	Output.aColor = g_MaterialAmbientColor * g_LightAmbient;
	if(!use_glow) {
		if(use_evcolor) Output.aColor += vColor;
		else Output.aColor += g_MaterialEmissiveColor;
	}
	if(use_dvcolor) Output.aColor *= vColor;
	
	Output.dColor = g_MaterialDiffuseColor * g_LightDiffuse;
	if(use_dvcolor) Output.dColor *= vColor;
    
    return Output;
}

float4 NormalPS( VS_OUTPUT_NORMAL In )  : COLOR0
{ 

	float3 color = float4(In.aColor, 0);
	if(use_glow) color += tex2D( sGlowMap, In.TexUV );

	float4 normal = tex2D( sNormalMap, In.TexUV );
	normal.rgb = normal.rgb * 2.0 - 1.0;
	
	float NdotL = max( dot( normal.rgb, normalize( In.LightDir ) ), 0.0 );
	
	if ( NdotL > 0.0 )
	{
		color += In.dColor * NdotL;
		if(use_specular) {
			float NdotHV = max( dot( normal.rgb, normalize( In.HalfVector ) ), 0.0 );
			color += normal.a * g_MaterialSpecularColor * g_LightDiffuse * pow( NdotHV, g_MaterialGloss );
		}
	}
	
	color = min( color, 1.0 );
	float4 tex=tex2D(sColorMap, In.TexUV);
	color *= tex.rgb;
	
	if(use_alpha) return float4(color, g_MaterialAlpha*tex.a);
	else return float4(color, 1);
}

float4 ParallaxPS( VS_OUTPUT_NORMAL In )  : COLOR0
{ 
    float offset = 0.015 - tex2D( sColorMap, In.TexUV ).a * 0.03;
	float2 texco = In.TexUV + normalize( In.ViewDir ).xy * offset;
	
	float3 color = float4(In.aColor, 0);
	if(use_glow) color += tex2D( sGlowMap, texco );

	float4 normal = tex2D( sNormalMap, texco );
	normal.rgb = normal.rgb * 2.0 - 1.0;
	
	float NdotL = max( dot( normal.rgb, normalize( In.LightDir ) ), 0.0 );
	
	if ( NdotL > 0.0 )
	{
		color += In.dColor * NdotL;
		if(use_specular) {
			float NdotHV = max( dot( normal.rgb, normalize( In.HalfVector ) ), 0.0 );
			color += normal.a * g_MaterialSpecularColor * g_LightDiffuse * pow( NdotHV, g_MaterialGloss );
		}
	}
	
	color = saturate(color);
	color *= tex2D( sColorMap, texco );
	
	if(use_alpha) return float4(color, g_MaterialAlpha);
	else return float4(color, 1);
}


//--------------------------------------------------------------------------------------
// Relief
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_RELIEF
{
    float4 hpos : POSITION;
	float3 vpos : TEXCOORD0;
	float2 texcoord : TEXCOORD1;
	float3 view : TEXCOORD2;
	float3 light : TEXCOORD3;
	float4 scale : TEXCOORD4;
};

struct PS_OUTPUT_RELIEF
{
	float4 color : COLOR0;
	//float depth : DEPTH;
};

VS_OUTPUT_RELIEF ReliefVS( 
                    float4 ipos     : POSITION,
					float3 normal   : NORMAL,
					float2 texcoord : TEXCOORD0,
					float3 tangent  : TANGENT,
					float3 binormal : BINORMAL
                  )
{
    VS_OUTPUT_RELIEF OUT;
    
    float4x4 worldViewProj = mul(worldMatrix, viewProjection);

	// vertex position in object space
	float4 pos=float4(ipos.xyz, 1);

	// vertex position in clip space
	OUT.hpos=mul(ipos, worldViewProj);
	OUT.texcoord=texcoord;

	// vertex position in view space (with model transformations)
	OUT.vpos=mul(pos, worldMatrix);
	OUT.vpos-=eyePos;

	// tangent vectors in view space
	tangent=mul(tangent, normalMatrix);
	binormal=mul(binormal, normalMatrix);
	normal=mul(normal, normalMatrix);
	float3x3 tangentspace=float3x3(tangent,binormal,normal);

	//might need to change the 4
	//OUT.scale=float4(length(tangent),length(binormal),-0.04,1);
	OUT.scale=float4(1,1,0.04,1);

	// view and light in tangent space
	OUT.view=tspace(OUT.vpos, normal, tangent, binormal);
	OUT.light=tspace(g_LightDir, normal, tangent, binormal);

	return OUT;
}

void ray_intersect_rm_linear(
      in sampler2D reliefmap,
      inout float3 p, 
      inout float3 v)
{

	const int search_steps=40;

	v/=search_steps;

	for( int i=0;i<search_steps-1;i++ )
	{
		float t=tex2D(reliefmap,p.xy).w;
		t=1-t;
		if(p.z<t) p+=v;
	}
}

void ray_intersect_rm_binary(
      in sampler2D reliefmap,
      inout float3 p, 
      inout float3 v)
{
	const int binary_search_steps=12;
   
	for( int i=0;i<binary_search_steps;i++ )
	{
		v*=0.5;
		float t=tex2D(reliefmap,p.xy).w;
		t=1-t;
		if(p.z<t) p+=2*v;
		p-=v;
	}
}

PS_OUTPUT_RELIEF ReliefPS( VS_OUTPUT_RELIEF IN )
{ 
    PS_OUTPUT_RELIEF OUT;

	// view vector in eye space
	float3 view=normalize(IN.view);
	view.z=-view.z;

	// scale view vector to texture space using depth factor
//Depth bias
	float depth_bias=(2*view.z-view.z*view.z);
	view.xy*=depth_bias;
//
	view*=IN.scale.z/(IN.scale.xyz*view.z);

	// ray intersect depth map 
	float3 p=float3(IN.texcoord,0);
	ray_intersect_rm_linear(sColorMap,p,view);
	ray_intersect_rm_binary(sColorMap,p,view);

//border clamp
	//if (p.x<0) clip(-1);
	//if (p.y<0) clip(-1);
	//if (p.x>IN.scale.w) clip(-1);
	//if (p.y>IN.scale.w) clip(-1);
//

	// get normal and color
	float3 n=tex2D(sNormalMap,p.xy);
	float3 c=tex2D(sColorMap,p.xy);
	
	//OUT.color=float4(c.rgb, 1);
	//return OUT;

	// expand normal from normal map
	n=normalize(n-0.5);

	// restore view vector
	view=normalize(IN.view);

//Depth correction
	// a=-far/(far-near)
	// b=-far*near/(far-near)
	// Z=(a*z+b)/-z
	
	//float3 pos=IN.vpos-normalize(IN.vpos)*p.z*IN.scale.z/view.z;
	//OUT.depth=((planes.x*pos.z+planes.y)/-pos.z);
//

	float3 light=normalize(IN.light);

	// compute diffuse and specular terms
	float diff=saturate(dot(light,n));
	//float spec=saturate(dot(normalize(light-view),n));

	// ambient term
	float3 finalcolor=g_MaterialAmbientColor * g_LightAmbient * c;

//Shadows
	// compute light ray vector in texture space
	/*float light_depth_bias=(2*light.z-light.z*light.z);
	light.xy*=light_depth_bias;
	light.z=-light.z;
	light*=IN.scale.z/(IN.scale*light.z);

	// compute light ray entry point in texture space
	float3 lp=p-light*p.z;
	ray_intersect_rm_linear(sColorMap,lp,light);

	if (lp.z<p.z-0.05) // if pixel in shadow
	{
	  diff*=0.25;
	  //spec=0;
	}*/
//

	// compute final color
	finalcolor += c*g_MaterialDiffuseColor*g_LightDiffuse*diff;
	//finalcolor += c*g_MaterialSpecularColor*g_LightDiffuse*pow(spec,specular.w);

	OUT.color=float4(finalcolor, 1);
	OUT.color.a=1;
	return OUT;
}

//--------------------------------------------------------------------------------------
// Fur
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_FUR
{
    float4 Position	: POSITION;    
    float4 T0	    : TEXCOORD0;
    float3 normal   : TEXCOORD1;
};

static const float FurLength=60;
static const float numLayers=20;

VS_OUTPUT_FUR FurVS(
					float3 position		: POSITION,
					float3 normal		: NORMAL,
					float4 texCoord		: TEXCOORD0,
					uniform float Layer
				  )
{
    VS_OUTPUT_FUR Output;
	
	float3 P = position.xyz + (normal * FurLength * (Layer/numLayers));
	normal = mul(normal, normalMatrix);
	
	float3 vGravity = mul(float3(0,0,-0.01), worldMatrix);
	float k =  pow(Layer, 3);
	P = P + vGravity*k;
	Output.T0 = texCoord;

	Output.Position = mul(float4(P, 1.0f), worldViewProj);
    Output.normal = normal;
    return Output;
}

float4 FurPS( VS_OUTPUT_FUR In, uniform float Layer ): COLOR0
{
	float4 furcol =  tex2D( sColorMap, In.T0 );
	
	if(Layer==0) {
		furcol.a=1;
	} else {
		float alpha = Layer/numLayers;
		clip(furcol.a - alpha);
		furcol.a = saturate(0.1 + furcol.a - alpha);
	}
	
	furcol.rgb *= g_LightAmbient*g_MaterialAmbientColor + g_LightDiffuse*g_MaterialDiffuseColor*saturate(dot(g_LightDir,In.normal));
	
	return furcol;
}

//--------------------------------------------------------------------------------------
// the standard rendering techniques
//--------------------------------------------------------------------------------------
technique Textureless  { pass P0 { VertexShader = compile vs_2_0 TexturelessVS(); PixelShader = compile ps_2_0 TexturelessPS(); } }
technique Flat         { pass P0 { VertexShader = compile vs_2_0 FlatVS();        PixelShader = compile ps_2_0 FlatPS();        } }
technique Normal       { pass P0 { VertexShader = compile vs_2_0 NormalVS();      PixelShader = compile ps_2_0 NormalPS();      } }
technique Parallax     { pass P0 { VertexShader = compile vs_2_0 NormalVS();      PixelShader = compile ps_2_0 ParallaxPS();    } }

technique Relief       { pass P0 { VertexShader = compile vs_3_0 ReliefVS();      PixelShader = compile ps_3_0 ReliefPS();      } }
technique Fur
{
	pass P0 { VertexShader = compile vs_3_0 FurVS(0);    PixelShader = compile ps_3_0 FurPS(0); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(1);    PixelShader = compile ps_3_0 FurPS(1); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(2);    PixelShader = compile ps_3_0 FurPS(2); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(3);    PixelShader = compile ps_3_0 FurPS(3); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(4);    PixelShader = compile ps_3_0 FurPS(4); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(5);    PixelShader = compile ps_3_0 FurPS(5); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(6);    PixelShader = compile ps_3_0 FurPS(6); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(7);    PixelShader = compile ps_3_0 FurPS(7); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(8);    PixelShader = compile ps_3_0 FurPS(8); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(9);    PixelShader = compile ps_3_0 FurPS(9); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(10);   PixelShader = compile ps_3_0 FurPS(10); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(11);   PixelShader = compile ps_3_0 FurPS(11); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(12);   PixelShader = compile ps_3_0 FurPS(12); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(13);   PixelShader = compile ps_3_0 FurPS(13); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(14);   PixelShader = compile ps_3_0 FurPS(14); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(15);   PixelShader = compile ps_3_0 FurPS(15); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(16);   PixelShader = compile ps_3_0 FurPS(16); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(17);   PixelShader = compile ps_3_0 FurPS(17); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(18);   PixelShader = compile ps_3_0 FurPS(18); }
	pass P0 { VertexShader = compile vs_3_0 FurVS(19);   PixelShader = compile ps_3_0 FurPS(19); }
}

//--------------------------------------------------------------------------------------
// some extra stuff for rendering individual maps/materials/vertex properties
//--------------------------------------------------------------------------------------
float4 MapPS( VS_OUTPUT_FLAT In, uniform int map, uniform bool useAlpha ) : COLOR0
{ 
    float3 result;
    if(map==0) {
		if(useAlpha) result = tex2D(sColorMap, In.TextureUV).a;
		else result = tex2D(sColorMap, In.TextureUV);
    } else if(map==1) {
		if(useAlpha) result = tex2D(sNormalMap, In.TextureUV).a;
		else result = tex2D(sNormalMap, In.TextureUV);
	} else if(map==2) {
		result = tex2D(sGlowMap, In.TextureUV);
	}
    
    return float4(result, 1);
}

float4 MatPS( VS_OUTPUT_FLAT In, uniform int mat ) : COLOR0
{
	float3 result;
	if(mat==0) result = g_MaterialAmbientColor;
	else if(mat==1) result = g_MaterialDiffuseColor;
	else if(mat==2) result = g_MaterialSpecularColor;
	else if(mat==3) result = g_MaterialEmissiveColor;
	
	return float4(result, 1);
}

struct VS_OUTPUT_COLOR
{
    float4 Position : POSITION;   // vertex position 
    float3 vColor   : TEXCOORD0;  // vertex color
};

VS_OUTPUT_COLOR VPropVS(float4 vPos       : POSITION, 
					    float3 vNormal    : NORMAL,
                        float3 vColor     : COLOR0,
                        float2 vTexCoord0 : TEXCOORD0,
                        float3 Tangent    : TANGENT,
                        float3 BiNormal   : BINORMAL,
                        uniform int mat
                    )
{
	VS_OUTPUT_COLOR Output;
	
	float4x4 worldViewProjection = mul(worldMatrix, viewProjection);
    Output.Position = mul(vPos, worldViewProjection);
    
    if(mat==0) Output.vColor=saturate(vPos.rgb);
    else if(mat==1) Output.vColor=vNormal;
    else if(mat==2) Output.vColor=vColor;
    else if(mat==3) Output.vColor=float3(frac(vTexCoord0), 0);
    else if(mat==4) Output.vColor=Tangent;
    else if(mat==5) Output.vColor=BiNormal;
	
	return Output;
}
float4 VPropPS(VS_OUTPUT_COLOR In) : COLOR0
{
	return float4(In.vColor, 1);
}

technique ColorMap    { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MapPS(0, false); } }
technique HeightMap   { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MapPS(0, true ); } }
technique NormalMap   { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MapPS(1, false); } }
technique SpecularMap { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MapPS(1, true ); } }
technique GlowMap     { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MapPS(2, false); } }

technique AmbMat  { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MatPS(0); } }
technique DifMat  { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MatPS(1); } }
technique SpecMat { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MatPS(2); } }
technique GlowMat { pass P0 { VertexShader = compile vs_2_0 FlatVS(); PixelShader = compile ps_2_0 MatPS(3); } }

technique vPosition { pass P0 { VertexShader = compile vs_2_0 VPropVS(0); PixelShader = compile ps_2_0 VPropPS(); } }
technique vNormal   { pass P0 { VertexShader = compile vs_2_0 VPropVS(1); PixelShader = compile ps_2_0 VPropPS(); } }
technique vColor    { pass P0 { VertexShader = compile vs_2_0 VPropVS(2); PixelShader = compile ps_2_0 VPropPS(); } }
technique vUV       { pass P0 { VertexShader = compile vs_2_0 VPropVS(3); PixelShader = compile ps_2_0 VPropPS(); } }
technique vTangent  { pass P0 { VertexShader = compile vs_2_0 VPropVS(4); PixelShader = compile ps_2_0 VPropPS(); } }
technique vBinormal { pass P0 { VertexShader = compile vs_2_0 VPropVS(5); PixelShader = compile ps_2_0 VPropPS(); } }