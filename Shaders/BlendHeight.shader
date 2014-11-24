Shader "VertexPainting/BlendHeight" 
{
	Properties 
	{
		_Color ("Color ",Color) = (1.0 , 1.0 , 1.0 , 1.0)
		_MainTex       ("Base (RGBA)"              , 2D) = "white" {}
		_SecondTex     ("Secondary Texture (RGBA)" , 2D) = "white" {}
		_MainBumpMap   ("Base Bump Map (RGB)"      , 2D) = "white" {}
		_SecondBumpMap ("Secondary Bump Map (RGB)" , 2D) = "white" {}
		_MainSpecTex   ("Base Spec (R) Diffuse (A)", 2D) = "white" {}
		_SecondSpecTex ("Base Spec (R) Diffuse (A)", 2D) = "white" {}
		_HeightMap     ("Height Map"               , 2D) = "white" {}
		_HeightMapPower("Height Map Power",float) = 20.0
		_NormalDisplacementIntensity("Normal Displacement Intensity",float) = 1.0
	}

	SubShader 
	{
        CGPROGRAM

        #pragma surface surf BlinnPhong vertex:vert

		float _NormalDisplacementIntensity;
		float _HeightMapPower;
                        
		sampler2D _MainTex;
		sampler2D _SecondTex;
		
		sampler2D _MainBumpMap;
		sampler2D _SecondBumpMap;
		
		sampler2D _MainSpecTex;
		sampler2D _SecondSpecTex;
		
		sampler2D _HeightMap;

		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 color : COLOR;
		};

		void vert (inout appdata_full v) 
		{
        	v.vertex.xyz += _NormalDisplacementIntensity * v.color.b * v.normal;
      	}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float4 mainTexColor        = tex2D(_MainTex  , IN.uv_MainTex.xy);
			float4 secondTexColor      = tex2D(_SecondTex, IN.uv_MainTex.xy);
			
			float3 _mainBumpNormal     = UnpackNormal(tex2D(_MainBumpMap,   IN.uv_BumpMap));
			float3 _secondBumpNormal   = UnpackNormal(tex2D(_SecondBumpMap, IN.uv_BumpMap));
			
			float4 mainSpecDiffColor   = tex2D(_MainSpecTex  , IN.uv_MainTex.xy);
			float4 secondSpecDiffColor = tex2D(_SecondSpecTex, IN.uv_MainTex.xy);
		
			float blendFactor = IN.color.r * (1.0-tex2D(_HeightMap,IN.uv_MainTex.xy).r) + 
								IN.color.r;
			
			blendFactor = clamp(pow(blendFactor,_HeightMapPower),0.0,1.0);
			
			o.Albedo   = _Color.rgb * lerp(mainTexColor,        secondTexColor,        blendFactor).rgb;
			o.Gloss    =              lerp(mainSpecDiffColor.a, secondSpecDiffColor.a, blendFactor);
			o.Alpha    = _Color.a *   lerp(mainTexColor,        secondTexColor,        blendFactor).a;
			o.Specular =              lerp(mainSpecDiffColor.r, secondSpecDiffColor.r, blendFactor);
			o.Normal   =              lerp(_mainBumpNormal,     _secondBumpNormal,     blendFactor);
		}
		
        ENDCG
	}
}