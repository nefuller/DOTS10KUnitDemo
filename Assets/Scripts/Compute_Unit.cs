using Unity.Burst;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Compute_Unit
    {
        public float4 color;
        public float4 rotation;
        public float3 position;
        public float pad0;
    }
}
