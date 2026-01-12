Shader "UI/GlowSprite"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowStrength ("Glow Strength", Range(0, 1)) = 0.5
        _GlowSize ("Glow Size", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Lighting Off
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv) * _Color;

                float glow = 0;
                for (float x = -_GlowSize; x <= _GlowSize; x++)
                {
                    for (float y = -_GlowSize; y <= _GlowSize; y++)
                    {
                        float2 offset = float2(x, y) / 512; // assuming 512px texture, adjust if needed
                        glow += tex2D(_MainTex, uv + offset).a;
                    }
                }

                glow = saturate(glow / ((_GlowSize * 2 + 1) * (_GlowSize * 2 + 1)));

                float alpha = col.a;
                col.rgb += _GlowColor.rgb * glow * _GlowStrength;
                col.a = alpha;

                return col;
            }
            ENDCG
        }
    }
}
