Shader "VertexPainting/BlendOverlay" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTex ("Secondary Texture (RGBA)",2D) = "white" {}
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
        
		sampler2D _MainTex;
		sampler2D _SecondTex;

        v2f vert (appdata v) 
		{
            v2f o;
            o.pos = mul( UNITY_MATRIX_MVP, v.vertex) + float4(_NormalDisplacementIntensity * v.color.b * v.normal , 0.0);
            o.color = v.color;
			o.uv = v.texcoord;
            return o;
        }
        
		float4 white = float4(1.0,1.0,1.0,1.0);
		
        fixed4 frag (v2f i) : COLOR0 
		{ 
			float4 base  = tex2D(_MainTex,i.uv.xy);
			float4 blend = tex2D(_SecondTex,i.uv.xy);//lerp(base,tex2D(_SecondTex,i.uv.xy),i.color.r);

			float4 lumCoef = float4(0.2125,0.7154,0.0721,1.0);
			float luminance = dot(lumCoef,base);
			
			if(luminance < 0.45)
				return 2.0 * blend * base;

			if(luminance > 0.55)
				return white - 2.0 * (white - blend) * (white - base);

			return lerp(2.0 * blend * base,
						white - 2.0 * (white - blend) * (white - base),
						(luminance * 0.45) * 10.0); 
		}
        ENDCG
    }
}
}