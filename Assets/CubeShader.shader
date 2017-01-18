// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Cube Shader" 
{
	Properties 
	{
		_SpriteTex ("RGBA Texture Image", 2D) = "white" {}
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
					float	isBrushed : FLOAT;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4  col		: COLOR;
					float	isBrushed : FLOAT;
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

				//******************
				// RANGE VALUES
				//******************

				float minRX;
				float minRY;
				float minRZ;

				float maxRX;
				float maxRY;
				float maxRZ;

				//*******************
				// DIMENSIONS
				//*******************

				float _MinD0;
				float _MaxD0;
				float _MinD1;
				float _MaxD1;
				float _Alpha;

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

				bool brushTest(VS_INPUT v, float x, float y, float z)
				{
					bool brushTest = false;

					bool XBrush = !(x < 0.0);
					bool YBrush = !(y < 0.0);
					bool ZBrush = !(z < 0.0);
								
					bool pixX = (v.position.x < x + maxRX && v.position.x > x - minRX);
					bool pixY = (v.position.y < y + maxRY && v.position.y > y - minRY);
					bool pixZ = (v.position.z < z + maxRZ && v.position.z > z - minRZ);
					
					if(XBrush && !YBrush && !ZBrush)
					{
						brushTest =(pixX);// output.isBrushed = 1.0;
					}
					else if(!XBrush && YBrush && !ZBrush)
					{
						brushTest = (pixY);// output.isBrushed = 1.0;
					}
					else if(!XBrush && !YBrush && ZBrush)
					{
						brushTest = (pixZ);// output.isBrushed = 1.0;
					}
					else if(XBrush && YBrush && !ZBrush)
					{
						brushTest = (pixX && pixY);// output.isBrushed = 1.0;
					}
					else if(XBrush && !YBrush && ZBrush)
					{
						brushTest = (pixX && pixZ);// output.isBrushed = 1.0;
					}
					else if(!XBrush && YBrush && ZBrush)
					{
						brushTest = (pixY && pixZ) ;//output.isBrushed = 1.0;
					}
					else if(XBrush && YBrush && ZBrush)
					{
						brushTest = (pixX && pixY && pixZ);// output.isBrushed = 1.0;
					}

					return brushTest;
				}

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(VS_INPUT v)
				{
					GS_INPUT output = (GS_INPUT)0;
					
					output.pos =  mul(unity_ObjectToWorld, v.position);

					//output.isBrushed = 0.0;
					//for(int i=0; i< 3; i++)
					//{
					//_X_Array[0] = 0.75; //, 0.2, 0.3};

					//bool brushTested = brushTest(v,_X,_Y,_Z);

					//if(brushTested) 
					//{
					//	output.isBrushed = 1.0;
					//	//output.pos.x += 1.0;
					//}
					

					output.col = v.color;
					//if(v.position.z > 0.5) output.col = float4(1.0,0.0,0.0,1.0);
					return output;
				}


				void emitCube (float3 position, float4 color, float size, float isBrushed,  inout TriangleStream<FS_INPUT> triStream)
				{
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
					
					float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
					
					FS_INPUT pIn;
					
					// FACE 1

					pIn.isBrushed = isBrushed;
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
//					void emitCube (float3 position, float4 color, float size, float isBrushed,  inout TriangleStream<FS_INPUT> triStream)
					float isBrushed = p[0].isBrushed;

					//if(isBrushed == 1.0)
					//{
					//float3 p0 = p[0].pos;
					//p0.x = p0.x * 2.0 ;//+ 2.0;
					//p0.y = p0.y * 2.0;
					//p0.z = p0.z * 2.0;

					
					//emitCube(p[0].pos, p[0].col, halfS, 1.0, triStream);
					////emitCube(p0, p[0].col, halfS, 1.0, triStream);

					//}
					//else
					//{
					emitCube(p[0].pos, p[0].col, halfS, isBrushed, triStream);
					//}
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{

				//float blue = 0.0;
				//float green = 0.0;

				//if (input.isBrushed == 1.0)
				//{
				//	_Alpha=1.0;
				//	blue = 0.55;
				//	green = 1.0;
				//}
				float dx = input.tex0.x;// - 0.5f;
			    float dy = input.tex0.y;// - 0.5f;

				if(dx > 0.95 || dx < 0.05 || dy <0.05  || dy>0.95 ) return float4(0.0, 0.0, 0.0, 1.0);
				//else return float4(0.0, 0.0, 0.0, 0.0);
				//float4 colorReturn = float4(input.col.x, 0.0, 0.5-input.col.x, 1.0);				
				//return colorReturn; //ambient+diffuse*saturate(dot(Light,Norm));
				float dt = (dx -0.5) * (dx-0.5) + (dy-0.5) * (dy-0.5);

				return float4(input.col.x-dx/2,input.col.y-dx/2,input.col.z-dx/2,1.0);
				}
			

			ENDCG
		}
	}
	
		FallBack  "Transparent"

	 
}
