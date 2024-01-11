Shader "Custom/FlatColor" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			uniform float4 _Color;

			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert( appdata_base v ) {
				v2f o;
				
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
				
				return o;
			}

			half4 frag( v2f i ) : SV_Target {
				return half4( _Color.xyz, 1.0 );
			}
			ENDCG
		}
	}
}
