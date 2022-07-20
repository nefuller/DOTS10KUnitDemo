using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct SpatialPartition
    {
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GridHash(in float3 position, in WorldSize worldSize, int partitionsPerAxis)
        {
            var x = (int)math.floor((position.x + worldSize.halfSize.x) /
                        (worldSize.size.x / partitionsPerAxis));
            var z = (int)math.floor((position.z + worldSize.halfSize.y) /
                        (worldSize.size.y / partitionsPerAxis));

            return z * partitionsPerAxis + x;
        }
    }
}
