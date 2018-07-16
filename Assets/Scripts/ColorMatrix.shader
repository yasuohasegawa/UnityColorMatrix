Shader "Custom/ColorMatrix"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float _ColorMats[20];

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 res = fixed4(0.0, 0.0, 0.0, 1.0);
				res.r = (_ColorMats[0] * col.r) + (_ColorMats[1] * col.g) + (_ColorMats[2] * col.b) + (_ColorMats[3] * col.a) + _ColorMats[4];
				res.g = (_ColorMats[5] * col.r) + (_ColorMats[6] * col.g) + (_ColorMats[7] * col.b) + (_ColorMats[8] * col.a) + _ColorMats[9];
				res.b = (_ColorMats[10] * col.r) + (_ColorMats[11] * col.g) + (_ColorMats[12] * col.b) + (_ColorMats[13] * col.a) + _ColorMats[14];
				res.a = (_ColorMats[15] * col.r) + (_ColorMats[16] * col.g) + (_ColorMats[17] * col.b) + (_ColorMats[18] * col.a) + _ColorMats[19];

				return res;
			}
			ENDCG
		}
	}
}
