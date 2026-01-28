Shader "Custom/Hologram"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0, 0.7, 1, 0.5)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _ScanlineSpeed ("Scanline Speed", Float) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // Using UnityCG for standard built-in support, or adapt to URP if needed. 
            // Note: Skill showed HLSL structure but URP macros differ from built-in. 
            // Assuming simplified HLSL/CG compatibility.
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            float4 _MainColor;
            float _RimPower;
            float _ScanlineSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target {
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);
                
                float rim = 1.0 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                
                float scanline = frac(i.worldPos.y * 20 + _Time.y * _ScanlineSpeed);
                scanline = step(0.5, scanline) * 0.2 + 0.8;
                
                return float4(_MainColor.rgb, _MainColor.a * rim * scanline);
            }
            ENDHLSL
        }
    }
}
