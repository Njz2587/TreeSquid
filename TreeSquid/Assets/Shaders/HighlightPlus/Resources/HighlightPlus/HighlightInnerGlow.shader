Shader "HighlightPlus/Geometry/InnerGlow" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _Width ("Width", Float) = 1.0
}
    SubShader
    {
        Tags { "Queue"="Transparent+122" "RenderType"="Transparent" }
    
        // Inner Glow
        Pass
        {
           	Stencil {
                Ref 2
                Comp Equal
                Pass keep 
            }
            Blend SrcAlpha One
            ZWrite Off
            Offset -1, -1
            ZTest [_InnerGlowZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float3 wpos   : TEXCOORD1;
                float3 normal : NORMAL;
				UNITY_VERTEX_OUTPUT_STEREO
            };

      		fixed4 _Color;
      		fixed _Width;


            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	float3 viewDir = normalize(i.wpos - _WorldSpaceCameraPos.xyz);
            	fixed dx = saturate(_Width - abs(dot(viewDir, normalize(i.normal)))) / _Width;
                fixed4 col = _Color * dx;
				return col;
            }
            ENDCG
        }

    }
}