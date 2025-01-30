Shader "Custom/UnlitCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}    // Основная текстура
        _Color ("Color", Color) = (1,1,1,1)     // Цвет
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // Порог отсечения
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Blend SrcAlpha OneMinusSrcAlpha    // Прозрачность
        ZWrite On                          // Запись в Z-буфер
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _Cutoff;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);  // Преобразование координат в пространство экрана
                o.uv = v.uv;                            // Передача UV-координат
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Получение цвета из текстуры и умножение на цвет
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;

                // Отсечение пикселей по порогу
                clip(texColor.a - _Cutoff);

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
