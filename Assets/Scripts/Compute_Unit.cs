using Unity.Burst;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Compute shader representation of a unit.
    /// </summary>
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Compute_Unit
    {
        public float4 color;
        public float4 rotation;
        public float3 position;
        public float pad0;          // Pad struct to a multiple of float4 or 128 bits
                                    // to avoid spanning cache lines:
                                    // https://developer.nvidia.com/content/understanding-structured-buffer-performance
    }
}
