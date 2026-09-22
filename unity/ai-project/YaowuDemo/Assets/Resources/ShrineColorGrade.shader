Shader "Hidden/ShrineCameraGrade"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _ActTint ("Act Tint", Color) = (1,1,1,1)
        _Exposure ("Exposure", Float) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _ActTint;
            float _Exposure;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float3 color = tex2D(_MainTex, uv).rgb;
                float2 pixel = _MainTex_TexelSize.xy * 3.0;
                float3 nearby = 0;
                nearby += tex2D(_MainTex, uv + float2(pixel.x, 0)).rgb;
                nearby += tex2D(_MainTex, uv - float2(pixel.x, 0)).rgb;
                nearby += tex2D(_MainTex, uv + float2(0, pixel.y)).rgb;
                nearby += tex2D(_MainTex, uv - float2(0, pixel.y)).rgb;
                nearby *= 0.25;
                float brightness = dot(nearby, float3(0.2126, 0.7152, 0.0722));
                color += nearby * smoothstep(0.55, 1.1, brightness) * 0.075;
                color = (color - 0.5) * 1.025 + 0.5;
                color = max(color, 0) * _ActTint.rgb * _Exposure;
                float2 offset = (uv - 0.5) * float2(1.0, 0.85);
                float vignette = 1.0 - smoothstep(0.24, 0.68, length(offset)) * 0.08;
                return float4(color * vignette, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
