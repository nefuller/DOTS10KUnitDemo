using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct SpatialPartitionData : IDisposable
    {
        public NativeParallelMultiHashMap<int, PartitionData> spatialPartitionData;

        public NativeArray<FixedList64Bytes<int>> spatialPartitionAdjacencies;

        public NativeArray<int2> spatialPartitionDirections;

        public static SpatialPartitionData Create()
        {
            var spatialPartitionData = new SpatialPartitionData()
            {
                spatialPartitionData        = new NativeParallelMultiHashMap<int, PartitionData>(GameSettings.UnitMax, Allocator.Persistent),
                spatialPartitionAdjacencies = new NativeArray<FixedList64Bytes<int>>(GameSettings.SpatialPartitionsTotal, Allocator.Persistent),
                spatialPartitionDirections  = new NativeArray<int2>(GameSettings.SpatialPartitionDirectionsLength, Allocator.Persistent)
            };

            for (var i = 0; i < GameSettings.SpatialPartitionsTotal; ++i)
            {
                spatialPartitionData.spatialPartitionAdjacencies[i] = new FixedList32Bytes<int>();
            }

            NativeArray<int2>.Copy(GameSettings.SpatialPartitionDirections, spatialPartitionData.spatialPartitionDirections);

            IJobParallelForBatch_PrecalculateSpatialPartitionAdjacencies.ScheduleBatch(spatialPartitionData.spatialPartitionDirections, spatialPartitionData.spatialPartitionAdjacencies, default).Complete();

            return spatialPartitionData;
        }

        public void Clear()
        {
            spatialPartitionData.Clear();
        }

        public void Dispose()
        {
            spatialPartitionData.Dispose();
            spatialPartitionAdjacencies.Dispose();
            spatialPartitionDirections.Dispose();
        }
    }
}
