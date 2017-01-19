// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Cube Shader" 
{
	Properties 
	{
		_SpriteTex ("RGBA Texture Image", 2D) = "white" {}
		_Size ("Size", Range(0, 30)) = 0.5
		_X("X",Range(0,1)) = 0.5
		_Y("Y",Range(0,1)) = 0.5
		_Z("Z",Range(0,1)) = 0.5
		_BrushSize("BrushSize",Float) = 0.05
		_MinD0("MinD0",Float) = 0
		_MaxD0("MaxD0",Float) = 0
		_MinD1("MinD1",Float) = 0
		_MaxD1("MaxD1",Float) = 0
		_Alpha("Alpha", Float) = 0
	}

	SubShader 
	{
		Pass
		{
			
			LOD 400

			Tags
			{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			}
						
			Blend SrcAlpha OneMinusSrcAlpha 
			//Blend One One
			ColorMaterial AmbientAndDiffuse
			Lighting Off
			ZWrite On
			ZTest [unity_GUIZTestMode]
            Cull Off
			AlphaTest Greater 0
			
		
			CGPROGRAM
				#pragma target 4.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				
		        struct VS_INPUT {
          		    float4 position : POSITION;
            		float4 color: COLOR;
					float4 normal:	NORMAL;
        		};
				
				struct GS_INPUT
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float2  tex0	: TEXCOORD0;
					float4  col		: COLOR;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4  col		: COLOR;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				//uniform int _Points_Length = 0;
				//uniform float3 _Points [20];		// (x, y, z) = position
				float myXArray[3];// = {25,50,500};	// x = radius, y = intensity
				int LengthArray;

				float _Size;
				float4 _IndexPos;
				float _X;
				float _Y;
				float _Z;
				float _BrushSize;

				//**************************
				// CUTTING PLANE COORDINATES
				//**************************
				float4 p0Temp; // temporary plane coordinate 0
				float4 p1Temp; // temporary plane coordinate 1
				float4 p2Temp; // temporary plane coordinate 2

				float3 p0;
				float3 p1;
				float3 p2;

				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;

				//*********************************
				// helper functions
				//*********************************
				
				//returns distance from point to plane (p0,p1,p2)
				float distanceToPlane(float3 vertexPosition)
				{
				//bbach: TODO
					return distance(p0,vertexPosition);
				}

				float normaliseValue(float value, float i0, float i1, float j0, float j1)
				{
				float L = (j0 - j1) / (i0 - i1);
				return (j0 - (L * i0) + (L * value));
				}

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(VS_INPUT v)
				{
					GS_INPUT output = (GS_INPUT)0;
					
					// calculates screen position for vertex
					output.pos =  mul(unity_ObjectToWorld, v.position);
					
					float4 colorV = v.color;
					// test the distance of the vertex to the plane
					p0 = float3(p0Temp.x,p0Temp.y,p0Temp.z);
					p1 = float3(p1Temp.x,p1Temp.y,p1Temp.z);
					p2 = float3(p2Temp.x,p2Temp.y,p2Temp.z);

					if(distanceToPlane(v.position)>0.2)
					colorV.a = 0;

					output.col = colorV;
					//if(v.position.z > 0.5) output.col = float4(1.0,0.0,0.0,1.0);
					return output;
				}


				void emitCube (float3 position, float4 color, float size,  inout TriangleStream<FS_INPUT> triStream)
				{
				    /*
					float3 look = _WorldSpaceCameraPos - p[0].pos;
					//look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);
					
					float halfS = 0.01f * _Size;
							
					float4 v[4];
					
					v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
					v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
					v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
					v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);
					*/
					
					float3 NEU = float3( size,  size,  size);
					float3 NED = float3( size, -size,  size);
					float3 NWU = float3( size,  size, -size);
					float3 NWD = float3( size, -size, -size);
					float3 SEU = float3(-size,  size,  size);
					float3 SED = float3(-size, -size,  size);
					float3 SWU = float3(-size,  size, -size);
					float3 SWD = float3(-size, -size, -size);

					float4 pNEU = float4(position + NEU, 1.0f);
					float4 pNED = float4(position + NED, 1.0f);
					float4 pNWU = float4(position + NWU, 1.0f);
					float4 pNWD = float4(position+ NWD, 1.0f);

					float4 pSEU = float4(position + SEU, 1.0f);
					float4 pSED = float4(position + SED, 1.0f);
					float4 pSWU = float4(position + SWU, 1.0f);
					float4 pSWD = float4(position + SWD, 1.0f);
					
					//the following commented line will 
					//float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
					float4x4 vp = UNITY_MATRIX_MVP;
					FS_INPUT pIn;
					
					// FACE 1


					pIn.col = color;
					
					pIn.pos = mul(vp, pNWU);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pNWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pNED);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();

					// FACE 2
					pIn.pos = mul(vp, pNED);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSED);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSEU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();

					// FACE 3
					pIn.pos = mul(vp, pNWU);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSWU);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSEU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();

					// FACE 4
					pIn.pos = mul(vp, pSWU);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pSEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSED);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();
					
					// FACE 5
					pIn.pos = mul(vp, pNWD);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pNED);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSED);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();

					// FACE 6
					pIn.pos = mul(vp, pNWD);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, pNWU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, pSWU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();
				}

				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(48)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					float ensize = 1.0;// p[0].col.x;

					float halfS = ensize* _Size/500.0;

					emitCube(p[0].pos, p[0].col, halfS, triStream);
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{

				float dx = input.tex0.x;// - 0.5f;
			    float dy = input.tex0.y;// - 0.5f;

				if(dx > 0.95 || dx < 0.05 || dy <0.05  || dy>0.95 ) return float4(0.0, 0.0, 0.0, 1.0);
				float dt = (dx -0.5) * (dx-0.5) + (dy-0.5) * (dy-0.5);

				return float4(input.col.x-dx/2,input.col.y-dx/2,input.col.z-dx/2,input.col.w);
				}
			

			ENDCG
		}
	}
	
		FallBack  "Transparent"

	 
}
