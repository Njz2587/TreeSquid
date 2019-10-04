/*
Shader converted and modified by Kittenji

Find me:
Twitter - @Kitt3nji | https://twitter.com/kitt3nji
Twitch  - Kittenji  | https://www.twitch.tv/kittenji
VRChat  - Kittenji  | https://vrchat.net/home/user/usr_7ac745b8-e50e-4c9c-95e5-8e7e3bcde682
Discord - Kittenji

Original surface shader by thnewlands can be found here: https://github.com/thnewlands/unity-surfaceshader-flipbook
*/

Shader "Unlit/Kittenji/SpriteSheet"
{
	Properties
	{
		[Header(General)]
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Color Spritesheet", 2D) = "white" {}
		// _Cutoff ("Alpha Cutoff", Range(0,1)) = 1

		[Header(Spritesheet)]
		_Columns("Columns (int)", int) = 3
		_Rows("Rows (int)", int) = 3
		_FrameNumber ("Frame Number (int)", int) = 0
		_TotalFrames ("Total Number of Frames (int)", int) = 9

		_AnimationSpeed ("Animation Speed", float) = 0
	}
	SubShader
	{
		Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
		Cull Off

		Pass
		{
			AlphaToMask On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;

			float4 _Color;

			int _Columns;
			int _Rows;
			int _FrameNumber;
			int _TotalFrames;

			float _AnimationSpeed;
			// float _Cutoff;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				_FrameNumber += frac(_Time[0] * _AnimationSpeed) * _TotalFrames;

				float frame = clamp(_FrameNumber, 0, _TotalFrames);

				float2 offPerFrame = float2((1 / (float)_Columns), (1 / (float)_Rows));

				float2 spriteSize = i.uv;
				spriteSize.x = (spriteSize.x / _Columns);
				spriteSize.y = (spriteSize.y / _Rows);

				float2 currentSprite = float2(0,  1 - offPerFrame.y);
				currentSprite.x += frame * offPerFrame.x;
			
				float rowIndex;
				float mod = modf(frame / (float)_Columns, rowIndex);
				currentSprite.y -= rowIndex * offPerFrame.y;
				currentSprite.x -= rowIndex * _Columns * offPerFrame.x;
			
				float2 spriteUV = (spriteSize + currentSprite); //* _FrameScale

				fixed4 c = tex2D(_MainTex, spriteUV) * _Color;

				return c;
			}
			ENDCG
		}
	}
}
