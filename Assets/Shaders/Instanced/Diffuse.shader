Shader "Instanced/Diffuse"
{
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct Unit
            {
                float4 color;
                float4 rotation;
                float3 position;
                float pad0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
                float4 diff : COLOR1;
                float4 vertex : SV_POSITION;
            };

            StructuredBuffer<Unit> _UnitBuffer;

            float4 QuaternionMultiply(float4 q1, float4 q2)
            {
                return float4(
                    q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                    q1.w * q2.w - dot(q1.xyz, q2.xyz));
            }

            float3 RotateVector(float4 quaternion, float3 vec)
            {
                float4 inversedRotation = quaternion * float4(-1, -1, -1, 1);
                return QuaternionMultiply(quaternion, QuaternionMultiply(float4(vec, 0), inversedRotation)).xyz;
            }

            v2f vert(appdata_full v, uint instanceId : SV_InstanceID)
            {
                Unit u = _UnitBuffer[instanceId];

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(RotateVector(u.rotation, v.vertex) + u.position, 1.0));
                o.uv = v.texcoord;

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

                o.diff = nl * _LightColor0;

                o.color = u.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(i.color, 1);
                 return col * i.diff;
            }

            ENDCG
        }
    }
}