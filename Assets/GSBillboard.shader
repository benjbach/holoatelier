// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Outline Dots" 
{
	Properties 
	{
		_SpriteTex ("Base (RGB)", 2D) = "White" {}
		_Size ("Size", Range(0, 30)) = 0.5
		_IndexPos ("IndexPos", Vector) = (15.0,15.0,15.0,15.0)
		_X("X",Float) = 15.0
		_Y("Y",Float) = 15.0
		_Z("Z",Float) = 15.0
		_BrushSize("BrushSize",Float) = 0.05
		_MinD0("MinD0",Float) = 0
		_MaxD0("MaxD0",Float) = 0
		_MinD1("MinD1",Float) = 0
		_MaxD1("MaxD1",Float) = 0

		_tl("Top Left", Vector) = (-1,1,0,0)
		_tr("Top Right", Vector) = (1,1,0,0)
		_bl("Bottom Left", Vector) = (-1,-1,0,0)
		_br("Bottom Right", Vector) = (1,-1,0,0)

	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" }
			//Blend func : Blend Off : turns alpha blending off
			Blend SrcAlpha OneMinusSrcAlpha
			//Lighting On
			Zwrite On
			//ZTest NotEqual    
            //Cull Front
			LOD 200
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 
		//		#include "Distort.cginc"

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
					float4  color		: COLOR;
					float	isBrushed : FLOAT;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4  color		: COLOR;
					float	isBrushed : FLOAT;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				float _Size;
				float4 _IndexPos;
				float _X;
				float _Y;
				float _Z;
				float _BrushSize;
				
				//*******************
				// DIMENSIONS
				//*******************

				float _MinX;
				float _MaxX;
				float _MinY;
				float _MaxY;
				float _MinZ;
				float _MaxZ;

				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;

				//*********************************
				// helper functions
				//*********************************

				float normaliseValue(float value, float i0, float i1, float j0, float j1)
				{
				float L = (j0 - j1) / (i0 - i1);
				return (j0 - (L * i0) + (L * value));
				}

				// ************************
				// PROJECTION FUNCTIONS
				// ************************

				//if(v.position.x < _X + _BrushSize && v.position.x > _X - _BrushSize && v.position.y < _Y + _BrushSize && v.position.y > _Y - _BrushSize && v.position.z < _Z + _BrushSize && v.position.z > _Z - _BrushSize)
				//	//output.col = float4(1.0,1.0,1.0,1.0);
				//	{
				//		output.isBrushed = 1.0;
				//		float radius = _BrushSize; // (_BrushSize+ abs(output.pos.y));
				//		// project the point onto a sphere
				//		float xS = _X + radius*(output.pos.x - _X)/sqrt((_X-output.pos.x)*(_X-output.pos.x)+(_Y-output.pos.y)*(_Y-output.pos.y)+ (_Z-output.pos.z)*(_Z-output.pos.z));
				//		float yS = _Y + radius*(output.pos.y - _Y)/sqrt((_X-output.pos.x)*(_X-output.pos.x)+(_Y-output.pos.y)*(_Y-output.pos.y)+ (_Z-output.pos.z)*(_Z-output.pos.z));
				//		float zS = _Z + radius*(output.pos.z - _Z)/sqrt((_X-output.pos.x)*(_X-output.pos.x)+(_Y-output.pos.y)*(_Y-output.pos.y)+ (_Z-output.pos.z)*(_Z-output.pos.z));
						 
				//		output.pos.x = xS;
				//		output.pos.y = yS;
				//		output.pos.z = zS;
						
						
				//	}
				//	else
				//	{
				//		output.isBrushed = 0.0;
				//	}
					

				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(VS_INPUT v)
				{
					GS_INPUT output = (GS_INPUT)0;
					
					output.pos = mul(unity_ObjectToWorld, v.position);

					output.normal = float3(0.0,0.0,0.0);
					output.tex0 = float2(0, 0);

					output.color = v.color;

					_MinX = 0;
					_MaxX = 1;

					if(v.position.x < _MinX || v.position.x > _MaxX)
					output.color.w = 0;

					return output;
				}



				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					float3 up = float3(0, 1, 0);
					
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

					float4x4 vp = UNITY_MATRIX_VP;
					
					FS_INPUT pIn;
					
					pIn.isBrushed = p[0].isBrushed;
					pIn.color = p[0].color;
					
					pIn.pos = mul(vp, v[0]);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul(vp, v[1]);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, v[2]);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(vp, v[3]);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
					
					triStream.RestartStrip();
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
					float dx = input.tex0.x - 0.5f;
					float dy = input.tex0.y - 0.5f;
					float dt = dx * dx + dy * dy;
					if(input.color.w == 0)
					{ discard;	return float4(0.0, 0.0, 0.0, 0.0f);}
					 else{
					if( dt <= 0.2f)
						return float4(input.color.x-dt*0.25,input.color.y-dt*0.25,input.color.z-dt*0.25,1.0);
					else
					if(dx * dx + dy * dy <= 0.25f)
					return float4(0.0, 0.0, 0.0, 1.0);
					else
					{ discard;	return float4(0.0, 0.0, 0.0, 0.0f);
					}
					}
				}

			ENDCG
		}
	} 
}