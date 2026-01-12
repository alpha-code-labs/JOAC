Shader "Custom/PerlinNoiseTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 2)) = 1
        _Scale ("Scale", Range(0.01, 1)) = 0.1
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _NoiseColor ("Noise Color", Color) = (1, 1, 1, 1)
        _BlendMode ("Blend Mode", Float) = 0
        
        [HideInInspector] _SrcBlend ("Src Blend", Float) = 5
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 10
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        
        Pass
        {
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
            float _Intensity;
            float _Scale;
            float2 _Offset;
            fixed4 _NoiseColor;
            float _BlendMode;

            // Improved Perlin noise function
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            float perlinNoise(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                // Add multiple octaves for more detailed noise
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(uv);
                    uv *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 noiseUV = i.uv / _Scale + _Offset;
                float noiseValue = perlinNoise(noiseUV);
                
                fixed4 noiseColor = _NoiseColor * noiseValue * _Intensity;
                
                // Return the noise color with alpha for blending
                return fixed4(noiseColor.rgb, noiseColor.a * _Intensity);
            }
            ENDCG
        }
    }
}