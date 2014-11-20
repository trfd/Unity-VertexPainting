Shader "VertexPainting/BlendHeight" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTex ("Secondary Texture (RGBA)",2D) = "white" {}
		_HeightMap ("Height Map",2D) = "white" {}
		_HeightCutOff("_Height CutOff",float) = 0.25
		_NormalDisplacementIntensity("Normal Displacement Intensity",float) = 1.0
	}

SubShader 
{
    Pass 
    {    
        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag

        // vertex input: position, color
        struct appdata 
		{
            float4 vertex : POSITION;
            fixed4 color : COLOR;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
        };

        struct v2f 
		{
            float4 pos : SV_POSITION;
            fixed4 color : COLOR;
			float4 uv : TEXCOORD0;
        };

		float _NormalDisplacementIntensity;
		float _HeightCutOff;

                        
		sampler2D _MainTex;
		sampler2D _SecondTex;
		sampler2D _HeightMap;

        v2f vert (appdata v) 
		{
            v2f o;
            o.pos = mul( UNITY_MATRIX_MVP, v.vertex) + float4(_NormalDisplacementIntensity * v.color.b * v.normal , 0.0);
            o.color = v.color;
			o.uv = v.texcoord;
            return o;
        }
		
        fixed4 frag (v2f i) : COLOR0 
		{ 
			float blendFactor = i.color.r * (1.0-tex2D(_HeightMap,i.uv.xy).r) + i.color.r;
			
			blendFactor = clamp(pow(blendFactor,15.0),0.0,1.0);
			
			return lerp(tex2D(_MainTex,i.uv.xy),tex2D(_SecondTex,i.uv.xy),blendFactor);
		}
        ENDCG
    }
}
}